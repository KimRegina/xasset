using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自定义Popup的Style缓存可以有多个参数，不止是Rect，也可以自定义其他的
/// </summary>
public class OdinPopupStyle
{
    public Rect rect;

    static Dictionary<int, OdinPopupStyle> temp = new();

    public static OdinPopupStyle Get(int contrelId)
    {
        if (!temp.ContainsKey(contrelId))
        {
            return null;
        }
        OdinPopupStyle t;
        temp.Remove(contrelId,out t);
        return t;
    }

    public static void Set(int contrelId,OdinPopupStyle style)
    {
        temp[contrelId] = style;
    }
}