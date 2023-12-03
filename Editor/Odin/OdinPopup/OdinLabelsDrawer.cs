using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace xasset.editor.Odin
{
    [CustomPropertyDrawer(typeof(OdinLabelsAttribute))]
    public class OdinLabelsDrawer : PropertyDrawer
    {
        private static Texture2D mBackgound;

        private static Texture2D background
        {
            get
            {
                if (mBackgound == null)
                    mBackgound = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/xasset/Editor/label_background.png");
                return mBackgound;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!property.type.Equals($"{nameof(OdinLabelsEnum)}")) return;
            GUILayout.BeginHorizontal();
            List<OdinPopupItem> list = new List<OdinPopupItem>();

#if UNITY_2022
            OdinLabelsEnum boxed = (OdinLabelsEnum)property.boxedValue;
            for (int i = 0; i < boxed.Labels.Length; i++)
            {
                string labelStr = boxed.Labels[i];
                list.Add(new OdinPopupItem(labelStr, true));
                GUIStyle labelStyle = new GUIStyle(GUIStyle.none);
                labelStyle.normal.background = background;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.border = new RectOffset(4, 4, 4, 4);
                Vector2 size = labelStyle.CalcSize(new GUIContent(labelStr));
                float labelWidth = size.x > 25 ? size.x + 8 : 25;
                position.x += labelWidth + 5;
                GUILayout.Label(labelStr, labelStyle, GUILayout.Width(labelWidth),
                    GUILayout.ExpandWidth(false));
                GUILayout.Space(5);
            }
#else
            SerializedProperty labelProperty = property.FindPropertyRelative(property.propertyPath);
            for (int i = 0; i < labelProperty.arraySize; i++)
            {
                SerializedProperty item = labelProperty.GetArrayElementAtIndex(i);
                list.Add(new OdinPopupItem(item.stringValue, true));
                // Debug.Log($"{i}: {item.stringValue}");
                GUIStyle labelStyle = new GUIStyle(GUIStyle.none);
                labelStyle.normal.background = background;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.border = new RectOffset(4, 4, 4, 4);
                Vector2 size = labelStyle.CalcSize(new GUIContent(item.stringValue));
                float labelWidth = size.x > 25 ? size.x + 8 : 25;
                position.x += labelWidth + 5;
                GUILayout.Label(item.stringValue, labelStyle, GUILayout.Width(labelWidth),
                    GUILayout.ExpandWidth(false));
                GUILayout.Space(5);
            }
#endif

            for (int i = 0; i < OdinLabelsEnum.s_Labels.Length; i++)
            {
                bool isExisted = false;
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].DisplayName.Equals(OdinLabelsEnum.s_Labels[i]))
                    {
                        isExisted = true;
                        break;
                    }
                }

                if (isExisted) continue;
                list.Add(new OdinPopupItem(OdinLabelsEnum.s_Labels[i]));
            }

#if UNITY_2022
            OdinPopupItem[] selects = GUIKit.ShowPopup(list.ToArray());
            if (selects != null && selects.Length > 0)
            {
                boxed.Labels = new string[selects.Length];
                for (int i = 0; i < selects.Length; i++)
                {
                    boxed.Labels[i] = selects[i].DisplayName;
                }

                property.boxedValue = boxed;
                property.serializedObject.ApplyModifiedProperties();
            }
#else
       OdinPopupItem[] selects = GUIKit.ShowPopup(list.ToArray());
            if (selects != null && selects.Length > 0)
            {
                labelProperty.ClearArray();
                for (int i = 0; i < selects.Length; i++)
                {
                    labelProperty.arraySize += 1;
                    SerializedProperty item = labelProperty.GetArrayElementAtIndex(i);
                    item.stringValue = selects[i].DisplayName;
                    // Debug.Log($"{i}:  {selects[i].DisplayName}");
                }
                property.serializedObject.ApplyModifiedProperties();
            }
#endif
            GUILayout.EndHorizontal();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }
    }
}