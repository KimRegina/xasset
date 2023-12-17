using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace xasset.editor.Odin
{
    public class OdinElements
    {
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
        [ReadOnly, HideLabel, ShowInInspector, OdinLabels]
        public OdinLabelsEnum labels;

        [HorizontalGroup("Assets/Item", MinWidth = 400)]
        [ReadOnly, HideLabel, ShowInInspector]
        public string path => entry.asset;
    }

    public class OdinBundleResult
    {
        [HideInInspector] public string BundleName;

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
    }

    public class OdinBundleItem
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