﻿using System.IO;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities;
using System.Collections;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Unity.EditorCoroutines.Editor;
using com.regina.fUnityTools.Editor;

namespace xasset.editor.Odin
{
    public class OdinBuildWindow : OdinMenuEditorWindow
    {
        private static OdinMenuTree odinMenuTree;

        private static Dictionary<string, object>
            cacheMenuDic; //不能直接缓存OdinMenuTree 内部设计不方便缓存使用，故每次打开重新new OdinMenuTree 利用缓存的cacheMenuDic 减少打开window时间

        [MenuItem("Tools/Odin Editor Window")]
        public static void OpenReginaWindow()
        {
            OdinBuildWindow window = GetWindow<OdinBuildWindow>();
            // Nifty little trick to quickly position the window in the middle of the editor.
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1500, 800);
            window.titleContent = new GUIContent("ROX");
            window.OpenWindow();
        }

        private void OpenWindow()
        {
            odinMenuTree = new OdinMenuTree(false);
            if (!OdinExtension.IsBuildConfigChanged())
            {
                UpdateMenuTree();
                return;
            }

            EditorCoroutineUtility.StartCoroutine(InitBuildConfig(), this);
        }

        private void UpdateMenuTree()
        {
            foreach (var item in cacheMenuDic)
            {
                odinMenuTree.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitBuildConfig()
        {
            odinMenuTree.Add("Settings", new OdinBuildSettings());
            OdinExtension.ClearCacheBuildDic();
            if (cacheMenuDic == null) cacheMenuDic = new Dictionary<string, object>();
            cacheMenuDic.Clear();
            var list = EditorFileUtils.FindAllAssets<Build>("Assets/xasset/Config/Builds");
            for (int i = 0; i < list.Length; i++)
            {
                Build build = list[i] as Build;
                if (build == null)
                {
                    Debug.LogError($"{list[i].name} is not {typeof(Build)}");
                    continue;
                }

                List<OdinBuildGroup> odinBuildGroups = new List<OdinBuildGroup>();
                OdinExtension.CacheBuildDic(build,odinBuildGroups);
                AddGroupsMenu(build);
            }

            EditorUtility.ClearProgressBar();
            yield return 0;
        }


        /// <summary>
        /// 添加GroupMenu
        /// </summary>
        /// <param name="build"></param>
        private static void AddGroupsMenu(Build build)
        {
            for (int i = 0; i < build.groups.Length; i++)
            {
                var group = build.groups[i];
                group.owner = build;
                string parentMenuName = $"{build.name}/{group.name}";
                OdinBuildGroup odinBuildGroup = new OdinBuildGroup(group);
                OdinBuildGroupEditor menuGroup = new OdinBuildGroupEditor(odinBuildGroup);
                OdinExtension.cacheBuildDic[build].Add(odinBuildGroup);
                odinMenuTree.Add(parentMenuName, menuGroup);
                if (!cacheMenuDic.ContainsKey(parentMenuName))
                    cacheMenuDic.Add(parentMenuName, menuGroup);
                for (int j = 0; j < group.assets.Length; j++)
                {
                    BuildEntry buildEntry = group.assets[j];
                    EditorUtility.DisplayProgressBar($"正在搜集{build.name}/{group.name}下所有文件夹",
                        $"正在收集{buildEntry.asset}", (float)j / group.assets.Length);
                    OdinBuildFolder odinBuildFolder = new OdinBuildFolder(buildEntry, odinBuildGroup);
                    AddSubDirectoryMenu(odinBuildFolder, menuGroup);
                }
            }
        }

        /// <summary>
        /// 添加文件夹 Menu
        /// </summary>
        /// <param name="odinBuildFolder"></param>
        /// <param name="groupEditor"></param>
        private static void AddSubDirectoryMenu(OdinBuildFolder odinBuildFolder, OdinBuildGroupEditor groupEditor)
        {
            if (!EditorFileUtils.IsDirectory(odinBuildFolder.buildEntry.asset)) return;
            string entryMenuName = odinBuildFolder.GetMenuName();
            OdinBuildFolder odinFolderBuildFolder =
                new OdinBuildFolder(odinBuildFolder.buildEntry, groupEditor.odinBuildGroup);
            groupEditor.odinBuildGroup.AddSubOinBuildFolder(odinBuildFolder.buildEntry, odinBuildFolder);
            OdinBuildFolderEditor menuFolder = new OdinBuildFolderEditor(odinFolderBuildFolder);
            odinMenuTree.Add(entryMenuName, menuFolder);
            if (!cacheMenuDic.ContainsKey(entryMenuName))
                cacheMenuDic.Add(entryMenuName, menuFolder);
            // return;
            var subDirectories =
                EditorFileUtils.GetTopDirectoryPaths(odinBuildFolder.buildEntry.asset);
            for (int i = 0; i < subDirectories.Length; i++)
            {
                string subDirectoryPath = subDirectories[i];
                BuildEntry subBuildEntry =
                    OdinExtension.CreateBuildEntry(subDirectoryPath, odinBuildFolder.buildEntry);
                OdinBuildFolder subOdinBuildFolder = new OdinBuildFolder(subBuildEntry, groupEditor.odinBuildGroup);
                AddSubDirectoryMenu(subOdinBuildFolder, groupEditor);
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            return odinMenuTree;
        }

        public void AddMenuName(string menuName, OdinBuildFolderEditor folderEditor)
        {
            if (!cacheMenuDic.ContainsKey(menuName)) cacheMenuDic.Add(menuName, folderEditor);
        }

        public void DeleteBuildMenu(string menuName)
        {
            if (cacheMenuDic.ContainsKey(menuName)) cacheMenuDic.Remove(menuName);
        }

        public void RefreshMenu()
        {
            odinMenuTree = new OdinMenuTree(false);
            UpdateMenuTree();
            ForceMenuTreeRebuild();
        }
    }
}