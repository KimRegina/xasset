using System;
using UnityEngine;

namespace xasset.editor.Odin
{
    public class OdinLabelsAttribute : PropertyAttribute
    {
    }

    [Serializable]
    public struct OdinLabelsEnum
    {
        public string[] Labels;

        private static string[] _s_Labels;

        public static string[] s_Labels
        {
            get
            {
                if (_s_Labels == null) return Array.Empty<string>();
                return _s_Labels;
            }
        }
        
        public OdinLabelsEnum(string[] labels)
        {
            Labels = labels;
        }
    }
}