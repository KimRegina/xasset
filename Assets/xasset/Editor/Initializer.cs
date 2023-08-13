using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    public static class Initializer
    {
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitializeOnLoad()
        {
            var settings = Settings.GetDefaultSettings();
            Assets.Platform = Settings.Platform;
            Assets.SimulationMode = settings.playMode == PlayMode.FastPlayWithoutBuild;
            Assets.MaxRetryTimes = settings.player.maxRetryTimes;
            Assets.MaxDownloads = settings.player.maxDownloads;
            Assets.Updatable = settings.playMode == PlayMode.PlayByUpdateWithRealtime ||
                               settings.playMode == PlayMode.PlayByUpdateWithSimulation;
            if (Assets.SimulationMode)
            {
                InitializeRequest.Initializer = InitializeAsyncWithoutBuild;
                AssetRequest.CreateHandler = EditorAssetHandler.CreateInstance;
                SceneRequest.CreateHandler = EditorSceneHandler.CreateInstance;
                // 编辑器仿真模式开启 通过引用计数回收资源优化内存 可能对性能有影响 
                ReferencesCounter.GetDependenciesFunc = Settings.GetDependencies;
                ReferencesCounter.Enabled = true;
            }
            else
            {
                if (Assets.Updatable)
                {
                    if (!File.Exists(Settings.GetCachePath(Versions.Filename)))
                    {
                        if (EditorUtility.DisplayDialog("Bundles not found.",
                                "Please selection xasset>Build Bundles before enter playmode.", "Build", "Cancel"))
                        {
                            EditorApplication.isPlaying = false;
                            MenuItems.BuildBundles();
                            return;
                        }
                    }

                    if (!File.Exists(Assets.GetPlayerDataPath(PlayerAssets.Filename)))
                    {
                        if (EditorUtility.DisplayDialog("Player Assets not found.",
                                "Please select xasset>Build Player Assets before enter playmode.", "Build", "Cancel"))
                        {
                            EditorApplication.isPlaying = false;
                            MenuItems.BuildPlayerAssetsWithSelection();
                            return;
                        }
                    }

                    if (settings.playMode == PlayMode.PlayByUpdateWithSimulation)
                    {
                        InitializeRequest.Initializer = InitializeAsyncWithSimulationUpdate;
                    }
                    else
                    {
                        InitializeRequest.Initializer = request => request.RuntimeInitializeAsync();
                    }
                }
                else
                {
                    InitializeRequest.Initializer = InitializeAsyncWithoutUpdate;
                }

                AssetRequest.CreateHandler = RuntimeAssetHandler.CreateInstance;
                SceneRequest.CreateHandler = RuntimeSceneHandler.CreateInstance;
            }
        }

        private static IEnumerator InitializeAsyncWithSimulationUpdate(InitializeRequest request)
        {
            yield return request.RuntimeInitializeAsync();
            DownloadRequest.Resumable = false;
            Assets.UpdateInfoURL = $"{Assets.Protocol}{Settings.GetCachePath(UpdateInfo.Filename)}";
            Assets.DownloadURL = $"{Assets.Protocol}{Settings.PlatformDataPath}";
        }

        private static IEnumerator InitializeAsyncWithoutUpdate(InitializeRequest request)
        {
            var file = Settings.GetCachePath(Versions.BundleFilename);
            if (!File.Exists(file))
            {
                var message = $"{file} not found! you can create it by build bundles before enter in playmode.";
                request.SetResult(Request.Result.Failed, message);
                EditorUtility.DisplayDialog("Notes", message, "Ok");
                EditorApplication.isPlaying = false;
            }

            Assets.DownloadDataPath = Settings.PlatformDataPath;
            Assets.PlayerAssets = Settings.GetDefaultSettings().GetPlayerAssets();
            yield return null;
            Assets.Versions = Utility.LoadFromFile<Versions>(Settings.GetCachePath(Versions.BundleFilename));
            yield return null;
            foreach (var version in Assets.Versions.data)
                version.Load(Settings.GetDataPath(version.file));
            request.SetResult(Request.Result.Success);
        }

        private static IEnumerator InitializeAsyncWithoutBuild(InitializeRequest request)
        {
            Assets.Versions = ScriptableObject.CreateInstance<Versions>();
            Assets.PlayerAssets = ScriptableObject.CreateInstance<PlayerAssets>();
            Assets.ContainsFunc = ContainsAsset;
            var builds = Settings.FindAssets<Build>();
            foreach (var build in builds)
            {
                if (!build.enabled) continue;
                foreach (var group in build.groups)
                {
                    if (!group.enabled) continue;
                    foreach (var entry in group.assets)
                    {
                        if (entry.addressMode != AddressMode.LoadByName &&
                            entry.addressMode != AddressMode.LoadByNameWithoutExtension) continue;
                        Func<string, string> addressFunc = Path.GetFileName;
                        if (entry.addressMode == AddressMode.LoadByNameWithoutExtension)
                            addressFunc = Path.GetFileNameWithoutExtension;
                        if (!Directory.Exists(entry.asset))
                        {
                            Assets.SetAddress(entry.asset, addressFunc(entry.asset));
                            continue;
                        }

                        var children = Settings.GetChildren(entry);
                        foreach (var child in children)
                            Assets.SetAddress(child, addressFunc(child));
                    }
                }

                yield return null;
            }

            request.SetResult(Request.Result.Success);
        }

        private static bool ContainsAsset(string path)
        {
            var result = File.Exists(path);
            if (result) return true;
            EditorUtility.DisplayDialog("File not found", path, "Ok");
            Logger.E(path);
            return false;
        }
    }
}