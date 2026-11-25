using System;
using UnityEditor;
using UnityEngine;

namespace MyMotion.Animation.Editor
{
    internal static class GUIHelper
    {
        public static Texture2D GetComponentIcon(Type type)
        {
            // HACK: AssetPreview.GetMiniTypeThumbnail は TMP_Text で動作しないため、別に設定する。
            if (type.FullName == "TMPro.TMP_Text")
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.unity.ugui/Editor Resources/Gizmos/TMP - Text Component Icon.psd");
            }
            else
            {
                return AssetPreview.GetMiniTypeThumbnail(type);
            }
        }
    }
}