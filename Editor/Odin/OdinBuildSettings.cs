using System;
using UnityEditor;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;

namespace xasset.editor.Odin
{
    public class OdinBuildSettings
    {
        [HorizontalGroup("Settings", Width = 100)] [ShowInInspector] [HideLabel]
        private static string search;

        [HorizontalGroup("Settings", Width = 100)]
        [ShowInInspector]
        private void SearchAssets()
        {
            if (string.IsNullOrEmpty(search)) return;
            EditorCoroutineUtility.StartCoroutine(SearchBuildEntries(), this);
        }

        private bool ShowResults => odinBuildEntries != null && odinBuildEntries.Count > 0;

        [VerticalGroup("Assets"), HideLabel, ShowInInspector, ShowIf("ShowResults")]
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
                    $"已搜集：{resultAssetPath}", (float)i / results.Length);
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
        public TagEnum tag => (TagEnum)Enum.ToObject(typeof(TagEnum), entry.tag);

        [HorizontalGroup("Assets/Item", MinWidth = 400)]
        [ReadOnly, HideLabel, ShowInInspector]
        public string path => entry.asset;
    }
}