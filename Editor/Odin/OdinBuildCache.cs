using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace xasset.editor.Odin
{
    public class OdinBuildCache : ScriptableObject, ISerializationCallbackReceiver
    {
        public List<BuildEntry> Entries = new List<BuildEntry>();

        private readonly Dictionary<string, BuildEntry> assetsDic = new Dictionary<string, BuildEntry>();

        public BuildEntry GetAsset(string asset)
        {
            if (assetsDic.TryGetValue(asset, out var result)) return result;
            result = new BuildEntry {asset = asset};
            assetsDic.Add(asset, result);
            Entries.Add(result);
            EditorUtility.SetDirty(this);
            return result;
        }

        public List<BuildBundle> Bundles = new List<BuildBundle>();

        private readonly Dictionary<string, BuildBundle> bundlesDic = new Dictionary<string, BuildBundle>();

        public BuildBundle GetBundle(string name)
        {
            if (bundlesDic.TryGetValue(name, out var result)) return result;
            return null;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            assetsDic.Clear();
            foreach (var entry in Entries)
                assetsDic[entry.asset] = entry;
            bundlesDic.Clear();
            foreach (var bundle in Bundles)
                bundlesDic[bundle.file] = bundle;
        }
    }
}