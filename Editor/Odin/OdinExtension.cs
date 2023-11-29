using System.Collections.Generic;
using UnityEditor;
using com.regina.fUnityTools.Editor;
using UnityEngine;

namespace xasset.editor.Odin
{
    public class OdinExtension
    {
        public static UnityEngine.Object[] GetAllBuildConfig => EditorFileUtils.GetAllAssetsByAssetDirectoryPath("Assets/xasset/Config/Builds");

        public static bool IsBuildConfigChanged()
        {
            if (cacheBuildDic == null) return true;
            var files = GetAllBuildConfig;
            if (files.Length != cacheBuildDic.Keys.Count) return true;
            for (int i = 0; i < files.Length; i++)
            {
                Build fileBuild = files[i] as Build;
                if (fileBuild == null)
                {
                    Debug.LogError($"find not {typeof(Build)} file: {files[i].name}");
                    continue;
                }

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
            var array = EditorFileUtils.GetAllAssetsByAssetDirectoryPath("Assets/xasset/Config/Builds");
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
            entry.tag = 0;
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
            entry.tag = parent.tag;
            entry.filter = parent.filter;
            entry.owner = parent.owner;
            entry.addressMode = parent.addressMode;
            entry.bundleMode = parent.bundleMode;
            return entry;
        }

        public static void SaveChanges()
        {
            // if (OdinBuildWindow.cacheBuildDic == null) return;
            // foreach (var item in OdinBuildWindow.cacheBuildDic)
            // {
            //     List<OdinBuildGroup> list = item.Value;
            //     for (int i = 0; i < list.Count; i++)
            //         list[i].SaveModifies();
            //     EditorUtility.SetDirty(item.Key);
            // }

            AssetDatabase.Refresh();
        }

        public static string GetBuildEntryName(BuildEntry buildEntry)
        {
            int lastIndex = buildEntry.asset.LastIndexOf('/');
            return buildEntry.asset.Substring(lastIndex);
        }
    }
}