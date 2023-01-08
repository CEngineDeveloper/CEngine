using CYM.UI;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace CYM
{
    [InitializeOnLoad]
    public static class HierarchyDrawer
    {
        static HierarchyDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= Draw;
            EditorApplication.hierarchyWindowItemOnGUI += Draw;
        }



        private static void Draw(int id, Rect rect)
        {
            GameObject gameObject = EditorUtility.InstanceIDToObject(id) as GameObject;

            if (gameObject == null)
            {
                return;
            }

            Rect extended = new Rect(rect);
            //extended.xMin = extended.xMin - 2;
            DrawUIControl(gameObject, extended);
        }

        static void DrawUIControl(GameObject gameObject, Rect rect)
        {
            Rect rectToUse = rect;
            var com = gameObject.GetComponent<UControl>();
            if (com != null)
            {
                var key = com.GetType().Name;
                GUIStyle style = EditorStyles.helpBox;
                style = new GUIStyle(style);
                style.alignment = TextAnchor.MiddleRight;
                style.normal.textColor = Color.yellow;
                GUIHelper.PushColor(Color.white);
                string textToDraw = key;
                EditorGUI.LabelField(rectToUse, textToDraw, style);
                GUIHelper.PopColor();
            }
        }
    }
}