using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Playables;

/// <summary>
/// 自定义Popup的Style缓存可以有多个参数，不止是Rect，也可以自定义其他的
/// </summary>
public class OdinPopupStyle
{
    public Rect rect;

    static Dictionary<int, OdinPopupStyle> temp = new Dictionary<int, OdinPopupStyle>();

    public static OdinPopupStyle Get(int contrelId)
    {
        if (!temp.ContainsKey(contrelId))
        {
            return null;
        }

        OdinPopupStyle t = temp[contrelId];
        temp.Remove(contrelId);
        return t;
    }

    public static void Set(int contrelId,OdinPopupStyle style)
    {
        temp[contrelId] = style;
    }
}