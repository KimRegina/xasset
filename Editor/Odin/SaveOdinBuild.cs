using System.IO;
using UnityEditor;
using UnityEngine;

namespace xasset.editor.Odin
{
    public class SaveOdinBuild : IBuildStep
    {
        public void Start(BuildTask task)
        {
            OdinExtension.OdinBuildCache.Entries = task.assets;
            OdinExtension.OdinBuildCache.Bundles = task.bundles;
            var json = JsonUtility.ToJson(OdinExtension.OdinBuildCache);
            var path = Settings.GetCachePath($"{nameof(OdinBuildCache)}{task.parameters.name}.json");
            Utility.CreateDirectoryIfNecessary(path);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }
    }
}