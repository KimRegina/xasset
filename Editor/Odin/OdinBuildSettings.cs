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

        [TitleGroup("Settings", Order = -1), HideLabel, ShowInInspector, EnumToggleButtons]
        public SettingType settingType
        {
            get => mSettingType;
            set
            {
                mSettingType = value;
                if (mSettingType == SettingType.SearchBundles)
                {
                    results = OdinExtension.CollectBundleResults();
                }
            }
        }

        [ShowIf("settingType", SettingType.SearchAssets)]
        [HorizontalGroup("Settings/Tools", Width = 100, Order = 1)]
        [ShowInInspector]
        [HideLabel]
        private static string search;

        [ShowIf("settingType", SettingType.SearchAssets)]
        [HorizontalGroup("Settings/Tools", Width = 100, Order = 1)]
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

            UnityEngine.Object[] array = OdinExtension.AllBuilds;

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

        private bool showCollectResult => results != null && results.Count > 0;

        [HideLabel, ShowInInspector, ShowIf("@this.showCollectResult && this.settingType == SettingType.SearchBundles")]
        [TableList(IsReadOnly = true, ShowIndexLabels = true, AlwaysExpanded = true, ShowPaging = true,
             NumberOfItemsPerPage = 35), VerticalGroup("Settings/Results", Order = 1)]
        public List<OdinBundleResult> results;
    }
}