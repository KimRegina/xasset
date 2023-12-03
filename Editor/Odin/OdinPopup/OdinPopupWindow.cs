using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 打开popup的选择界面
/// </summary>
public class OdinPopupWindow : PopupWindowContent
{
    string filter;

    public static OdinPopupItem[] s_DisplayItems;

    public OdinPopupWindow(OdinPopupItem[] options)
    {
        s_DisplayItems = options;
    }

    public static OdinPopupItem[] s_SelectedDisplayItems
    {
        get
        {
            List<OdinPopupItem> selects = new List<OdinPopupItem>();
            if (s_DisplayItems == null)
            {
                return null;
            }
            for (int i = 0; i < s_DisplayItems.Length; i++)
            {
                if(s_DisplayItems[i].IsToggled) selects.Add(s_DisplayItems[i]);
            }

            return selects.ToArray();
        }
    }

    Vector2 scrollPosition;

    public override void OnGUI(Rect rect)
    {
        editorWindow.minSize = new Vector2(200, 400);
        GUILayout.Label("搜索：");
        filter = EditorGUILayout.TextField(filter);
        GUILayout.Space(20);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < s_DisplayItems.Length; i++)
        {
            string info = s_DisplayItems[i].DisplayName;

            if (this.filter != null && this.filter.Length != 0)
            {
                if (!info.Contains(this.filter))
                {
                    continue;
                }
            }

            GUILayout.BeginHorizontal();
            // EditorGUI.BeginDisabledGroup(s_DisplayItems[i].IsToggled);
            s_DisplayItems[i].IsToggled = GUILayout.Toggle(s_DisplayItems[i].IsToggled, GUIContent.none,
                GUILayout.ExpandWidth(false));
            // EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(s_DisplayItems[i].DisplayName))
            {
                editorWindow.Close();
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    public override void OnOpen()
    {
        // hasopen = true;
        base.OnOpen();
    }
}