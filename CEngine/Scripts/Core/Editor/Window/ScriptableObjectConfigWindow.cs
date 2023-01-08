using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CYM
{
    public class ScriptableObjectConfigWindow : EditorWindow
    {
        private Editor editor;
        public static void ShowConfigWindow(SerializedScriptableObject config)
        {
            var window = EditorWindow.GetWindow<ScriptableObjectConfigWindow>(true, "ConfigWindow", true);
            // 直接根据ScriptableObject构造一个Editor
            window.editor = Editor.CreateEditor(config);
        }

        private void OnGUI()
        {
            // 直接调用Inspector的绘制显示
            this.editor.OnInspectorGUI();
        }
    }
}
