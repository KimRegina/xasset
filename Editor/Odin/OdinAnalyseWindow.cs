using UnityEngine;
using UnityEditor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using ObjectFieldAlignment = Sirenix.OdinInspector.ObjectFieldAlignment;

namespace xasset.editor.Odin
{
    public class OdinAnalyseWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Analyse Window")]
        public static void OpenWindow()
        {
            OdinAnalyseWindow window = GetWindow<OdinAnalyseWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 800);
            window.titleContent = new GUIContent("Analyse");
        }

        public static void OpenWindow(UnityEngine.Object target)
        {
            OpenWindow();
        }

        private UnityEngine.Object mTarget;

        [ShowInInspector, HideLabel, PreviewField(55, ObjectFieldAlignment.Left),
         HorizontalGroup("Item", Order = 0, Width = 60)]
        private UnityEngine.Object Target
        {
            get => mTarget;
            set
            {
                mTarget = value;
                if (mTarget != null)
                    Debug.Log($"onset target {mTarget.name}");
            }
        }

        [TableList(IsReadOnly = true, ShowIndexLabels = true, AlwaysExpanded = true, ShowPaging = true,
             NumberOfItemsPerPage = 35), VerticalGroup("Item/Reference", Order = 1, PaddingTop = 60)]
        public List<string> list;
    }
}