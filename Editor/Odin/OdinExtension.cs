using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using com.regina.fUnityTools.Editor;
using Unity.EditorCoroutines.Editor;

namespace xasset.editor.Odin
{
    public class OdinExtension
    {
        public static Build[] AllBuilds =>
            EditorFileUtils.FindAllAssets<Build>("*", "Assets/com.regina.xasset/Config/Builds");

        public static bool IsBuildConfigChanged()
        {
            if (cacheBuildDic == null) return true;
            var files = AllBuilds;
            if (files.Length != cacheBuildDic.Keys.Count) return true;
            for (int i = 0; i < files.Length; i++)
            {
                Build fileBuild = files[i];
                Build cacheBuild = null;
                foreach (var item in cacheBuildDic.Keys)
                {
                    if (item.name == fileBuild.name)
                    {
                        cacheBuild = item;
                        break;
                    }
                }

                if (cacheBuild == null) return true;

                if (cacheBuild.groups.Length != fileBuild.groups.Length) return true;
                for (int j = 0; j < cacheBuild.groups.Length; j++)
                {
                    BuildGroup cacheBuildGroup = cacheBuild.groups[j];
                    BuildGroup fileBuildGroup = fileBuild.groups[j];
                    if (cacheBuildGroup.assets.Length != fileBuildGroup.assets.Length) return true;
                }
            }

            return false;
        }

        public static Dictionary<Build, List<OdinBuildGroup>> cacheBuildDic;

        public static void ClearCacheBuildDic()
        {
            if (cacheBuildDic == null) cacheBuildDic = new Dictionary<Build, List<OdinBuildGroup>>();
            cacheBuildDic.Clear();
        }

        public static void CacheBuildDic(Build key, List<OdinBuildGroup> val)
        {
            cacheBuildDic.Add(key, val);
        }

        public static void SaveBuildConfig(Build build)
        {
            var array = AllBuilds;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].name == build.name)
                {
                    array[i] = build;
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    break;
                }
            }
        }

        public static BuildEntry CreateBuildEntry(string assetPath)
        {
            BuildEntry entry = new BuildEntry();
            entry.asset = assetPath;
            entry.filter = "";
            entry.owner = null;
            entry.addressMode = AddressMode.LoadByPath;
            entry.bundleMode = BundleMode.PackByRaw;
            return entry;
        }

        public static BuildEntry CreateBuildEntry(string assetPath, BuildEntry parent)
        {
            BuildEntry entry = new BuildEntry();
            entry.asset = assetPath;
            entry.filter = parent.filter;
            entry.owner = parent.owner;
            entry.addressMode = parent.addressMode;
            entry.bundleMode = parent.bundleMode;
            return entry;
        }

        public static string DefaultConfigPath
        {
            get
            {
                var path = AssetDatabase.GetAssetPath(Settings.GetDefaultSettings());
                return Path.GetDirectoryName(path);
            }
        }

        public static OdinBuildCache OdinBuildCache
        {
            get
            {
                OdinBuildCache odinBuildCache =
                    GetOrCreateAsset<OdinBuildCache>($"{DefaultConfigPath}/OdinBuildCache.asset");
                return odinBuildCache;
            }
        }

        public static T GetOrCreateAsset<T>(string path, Action<T> onCreate = null) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            Utility.CreateDirectoryIfNecessary(path);
            asset = UnityEngine.ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            onCreate?.Invoke(asset);
            return asset;
        }

        public static string GetBuildEntryName(BuildEntry buildEntry)
        {
            int lastIndex = buildEntry.asset.LastIndexOf('/');
            return buildEntry.asset.Substring(lastIndex);
        }

        public static OdinLabelsEnum GetOdinLabelsEnum(string assetPath)
        {
            string[] labels = EditorFileUtils.GetLabels(assetPath);
            OdinLabelsEnum labelsEnum = new OdinLabelsEnum(labels);
            return labelsEnum;
        }

        public static void CollectReference(UnityEngine.Object asset)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            Build[] builds = AllBuilds;
            BuildEntry target = null;
            for (int i = 0; i < builds.Length; i++)
            {
                BuildGroup[] groups = builds[i].groups;
                for (int j = 0; j < groups.Length; j++)
                {
                    for (int k = 0; k < groups[j].assets.Length; k++)
                    {
                        BuildEntry entry = groups[j].assets[k];
                        if (assetPath.Contains(entry.asset))
                        {
                            assetPath = entry.asset;
                            target = entry;
                        }
                    }
                }
            }

            if (target != null)
            {
                Debug.Log($"find asset bundle name: {target.bundle}");
            }
        }

        public static List<OdinBundleResult> CollectBundleResults()
        {
            var builds = Settings.FindAssets<Build>();
            List<OdinBundleResult> results = new List<OdinBundleResult>();
            if (builds.Length == 0)
            {
                Logger.I("Nothing to build.");
                return results;
            }

            foreach (var build in builds)
            {
                var item = build.parameters;
                item.name = build.name;
                var task = BuildTask.StartNew(build,
                    new CollectAssets(),
                    new OptimizeDependencies(),
                    new BuildBundles(),
                    new SaveOdinBuild());
            }

            return results;
        }
    }
}