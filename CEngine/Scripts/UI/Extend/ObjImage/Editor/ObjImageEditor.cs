/*
 * TODO:
 * 1) Change to 'targets' and allow multi object editing
 */
#region Namespace Imports
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace CYM.UI
{
    [CustomEditor(typeof(ObjImage)), CanEditMultipleObjects]
    public class ObjImageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ObjImage objecImage = target as ObjImage;
            Dictionary<ObjImage, Transform> targetPrefabs = new Dictionary<ObjImage, Transform>();
            targetPrefabs = targets.ToDictionary(k => k as ObjImage, v => (v as ObjImage).ObjectPrefab);
            Dictionary<ObjImage, float> renderScales = targets.ToDictionary(k => k as ObjImage, v => (v as ObjImage).RenderScale);

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Force Render"))
            {
                foreach (var t in targetPrefabs)
                {
                    t.Key.HardUpdateDisplay();
                }
            }

            EditorGUILayout.Space();


            base.OnInspectorGUI();

            if (!EditorGUI.EndChangeCheck()) return;


            foreach (var t in targetPrefabs)
            {
                if (t.Key.ObjectPrefab != t.Value
                || renderScales[t.Key] != t.Key.RenderScale)
                {
                    t.Key.SetStarted();
                    t.Key.HardUpdateDisplay();
                    FrameTimer.AtEndOfFrame(() => t.Key.UpdateDisplay(), t.Key);
                }
                else
                {
                    t.Key.UpdateDisplay(true);
                }
            }

        }
    }
}
