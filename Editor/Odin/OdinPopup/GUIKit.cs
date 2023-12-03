using UnityEditor;
using UnityEngine;

namespace xasset.editor.Odin
{
    public class GUIKit
    {
        public static OdinPopupItem[] ShowPopup(OdinPopupItem[] displayedOptions)
        {
            int contrelId = GUIUtility.GetControlID(FocusType.Passive);

            if (GUILayout.Button("添加标签"))
            {
                OdinPopupWindow popup = new OdinPopupWindow(displayedOptions);
                PopupWindow.Show(OdinPopupStyle.Get(contrelId).rect, popup);
            }
        
            if (Event.current.type == EventType.Repaint)
            {
                OdinPopupStyle style = new OdinPopupStyle();
                style.rect = GUILayoutUtility.GetLastRect();
                OdinPopupStyle.Set(contrelId, style);
            }
        
            // CustomPopupInfo.Get(contrelId, 0);
            return OdinPopupWindow.s_SelectedDisplayItems;
        }
    }
}