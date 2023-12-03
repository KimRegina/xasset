using UnityEngine;

/// <summary>
/// 存储popup的信息如选择等
/// </summary>
public class OdinPopupItem
{
    public string DisplayName; // 显示的名字

    public bool IsToggled; //是否被选中

    public int _controlId; // 唯一 ID for a control
    public bool used;
    public static OdinPopupItem instance;

    public OdinPopupItem(int controlId, bool isToggled)
    {
        this._controlId = controlId;
        IsToggled = isToggled;
    }
    
    public OdinPopupItem(string displayName,bool isToggled = false)
    {
        IsToggled = isToggled;
        DisplayName = displayName;
    }

    public static int[] Get(int controlID, int selected)
    {
        // if (instance == null)
        // {
        //     return selected;
        // }

        if (instance._controlId == controlID && instance.used)
        {
            // GUI.changed = selected != instance.SelectIndex;
            // selected = instance.SelectIndex;
            instance = null;
        }

        return null;
    }

    public void Set(OdinPopupItem selectedItem)
    {
        // SelectIndex = selected;
        used = true;
    }
}