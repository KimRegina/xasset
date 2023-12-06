using System;
using UnityEditor;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.regina.fUnityTools.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace xasset.editor.Odin
{

    public enum SettingType
    {
        SearchAssets = 0,
        SearchBundles = 1,
    }
    
    public class OdinBuildSettings
    {
        private SettingType mSettingType;

        [TitleGroup("Settings",Order = -1), HideLabel, ShowInInspector, EnumToggleButtons]
        public SettingType settingType
        {
            get => mSettingType;
            set
            {
                mSettingType = value;
                if (mSettingType == SettingType.SearchBundles) TryCollect();
            }
        }
        
        [ShowIf("settingType",SettingType.SearchAssets)]
        [HorizontalGroup("Settings/Tools", Width = 100,Order = 1)] [ShowInInspector] [HideLabel]
        private static string search;

        [ShowIf("settingType",SettingType.SearchAssets)]
        [HorizontalGroup("Settings/Tools", Width = 100,Order = 1)]
        [ShowInInspector]
        private void SearchAssets()
        {
            if (string.IsNullOrEmpty(search)) return;
            EditorCoroutineUtility.StartCoroutine(SearchBuildEntries(), this);
        }

        private bool ShowResults => odinBuildEntries != null && odinBuildEntries.Count > 0;

        [VerticalGroup("Settings/Search"), HideLabel, ShowInInspector,
         ShowIf("@this.ShowResults && this.settingType == SettingType.SearchAssets")]
        [TableList(IsReadOnly = true, ShowIndexLabels = true, AlwaysExpanded = true, ShowPaging = true,
            NumberOfItemsPerPage = 35)]
        private List<OdinBuildSearch> odinBuildEntries;

        private IEnumerator SearchBuildEntries()
        {
            if (odinBuildEntries == null) odinBuildEntries = new List<OdinBuildSearch>();
            odinBuildEntries.Clear();

            string[] results = SearchAssetsPath();

            UnityEngine.Object[] array = OdinExtension.GetAllBuildConfig;

            List<BuildEntry> buildEntries = new List<BuildEntry>();

            for (int i = 0; i < array.Length; i++)
            {
                Build build = array[i] as Build;
                if (build == null) continue;
                for (int j = 0; j < build.groups.Length; j++)
                {
                    buildEntries.AddRange(build.groups[i].assets);
                }
            }

            buildEntries.Sort((a, b) => { return String.Compare(a.asset, b.asset, StringComparison.Ordinal); });
            List<OdinBuildSearch> retList = new List<OdinBuildSearch>();
            for (int i = 0; i < results.Length; i++)
            {
                string resultAssetPath = results[i];
                EditorUtility.DisplayProgressBar($"搜索中...",
                    $"已搜集：{resultAssetPath}", (float) i / results.Length);
                for (int j = 0; j < buildEntries.Count; j++)
                {
                    BuildEntry configBuildEntry = buildEntries[j];
                    if (resultAssetPath.Contains(configBuildEntry.asset))
                    {
                        BuildEntry target = OdinExtension.CreateBuildEntry(resultAssetPath, configBuildEntry);
                        retList.Add(new OdinBuildSearch(target));
                        break;
                    }
                }
            }

            odinBuildEntries = retList;
            EditorUtility.ClearProgressBar();
            yield return 0;
        }

        private string[] SearchAssetsPath()
        {
            string[] results = AssetDatabase.FindAssets(search);
            string[] assetsPath = new string[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                assetsPath[i] = AssetDatabase.GUIDToAssetPath(results[i]);
            }

            return assetsPath;
        }

        private List<BuildEntry> SearchAssetsInGroup(BuildGroup group)
        {
            List<BuildEntry> list = new List<BuildEntry>();
            for (int i = 0; i < group.assets.Length; i++)
            {
                BuildEntry buildEntry = group.assets[i];
            }

            return list;
        }

        private void TryCollect()
        {
            var builds = Settings.FindAssets<Build>();
            if (builds.Length == 0)
            {
                Logger.I("Nothing to build.");
                return;
            }

            var assets = new List<BuildEntry>();
            foreach (var build in builds)
            {
                var item = build.parameters;
                item.name = build.name;
                var task = BuildTask.StartNew(build, new CollectAssets(), new OptimizeDependencies());
                if (!string.IsNullOrEmpty(task.error)) return;

                foreach (var asset in task.assets) assets.Add(asset);
            }

            var assetWithGroups = new Dictionary<string, HashSet<BuildEntry>>();
            foreach (var entry in assets)
            {
                if (!assetWithGroups.TryGetValue(entry.bundle, out var refs))
                {
                    refs = new HashSet<BuildEntry>();
                    assetWithGroups.Add(entry.bundle, refs);
                }

                refs.Add(entry);
            }

            if (results == null) results = new List<OdinBundleResult>();
            // var sb = new StringBuilder();
            foreach (var pair in assetWithGroups)
            {
                bool isExisted = false;
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].BundleName.Equals(pair.Key))
                    {
                        isExisted = true;
                        break;
                    }
                }

                if(isExisted) continue;
                OdinBundleResult result = new OdinBundleResult(pair.Key,pair.Value.ToList());
                results.Add(result);
            }
        }

        private bool showCollectResult => results != null && results.Count > 0;

        [HideLabel, ShowInInspector, ShowIf("@this.showCollectResult && this.settingType == SettingType.SearchBundles")]
        [TableList(IsReadOnly = true, ShowIndexLabels = true, AlwaysExpanded = true, ShowPaging = true,
             NumberOfItemsPerPage = 35), VerticalGroup("Settings/Results",Order =  1)]
        public List<OdinBundleResult> results;
    }

    public class OdinBuildSearch
    {
        private BuildEntry entry;

        public OdinBuildSearch(BuildEntry entry)
        {
            this.entry = entry;
            asset = entry.asset;
        }

        [HorizontalGroup("Assets/Item", Width = 150), VerticalGroup("Assets")]
        [ReadOnly, HideLabel, ObjectReference, ShowInInspector]
        private string asset;

        [HorizontalGroup("Assets/Item", Width = 200)]
        [ReadOnly, HideLabel, ShowInInspector]
        public BundleMode bundleMode => entry.bundleMode;

        [HorizontalGroup("Assets/Item", Width = 210)]
        [ReadOnly, HideLabel, ShowInInspector]
        public AddressMode addressMode => entry.addressMode;

        [HorizontalGroup("Assets/Item", Width = 100)]
        [ReadOnly, HideLabel, ShowInInspector]
        public TagEnum tag => (TagEnum) Enum.ToObject(typeof(TagEnum), entry.tag);

        [HorizontalGroup("Assets/Item", MinWidth = 400)]
        [ReadOnly, HideLabel, ShowInInspector]
        public string path => entry.asset;
    }

    public class OdinBundleResult
    {
        [HideInInspector]
        public string BundleName;

        public OdinBundleResult(string bundleName, List<BuildEntry> buildEntries)
        {
            BundleName = bundleName;
            List<OdinBundleItem> list = new List<OdinBundleItem>();
            for (int i = 0; i < buildEntries.Count; i++)
            {
                OdinBundleItem item = new OdinBundleItem(buildEntries[i]);
                list.Add(item);
            }

            assets = list;
        }

        [FoldoutGroup("$BundleName")]
        [HideLabel, ShowInInspector]
        [TableList(IsReadOnly = true, ShowIndexLabels = true, AlwaysExpanded = true, ShowPaging = true,
            NumberOfItemsPerPage = 35)]
        private List<OdinBundleItem> assets;

        private class OdinBundleItem
        {
            public OdinBundleItem(BuildEntry buildEntry)
            {
                asset = buildEntry.asset;
                path = buildEntry.asset;
            }

            [HorizontalGroup("Item", Width = 100)] [ReadOnly, HideLabel, ObjectReference, ShowInInspector]
            private string asset;

            [HorizontalGroup("Item", MinWidth = 400)] [ReadOnly, HideLabel, ShowInInspector]
            public string path;
        }
    }
}