//------------------------------------------------------------------------------
// InspectorBaseGlobalMonoMgr.cs
// Copyright 2018 2018/6/1 
// Created by CYM on 2018/6/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM
{
    [CustomEditor(typeof(GlobalMonoManager), true)]
    public class IPTGlobalMonoManager : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            EditorGUILayout.BeginVertical();
            GlobalMonoManager.ToggleUpdate = EditorGUILayout.Toggle("ToggleUpdate",GlobalMonoManager.ToggleUpdate);
            GlobalMonoManager.ToggleFixedUpdate = EditorGUILayout.Toggle("ToggleFixedUpdate",GlobalMonoManager.ToggleFixedUpdate);
            GlobalMonoManager.ToggleLateUpdate = EditorGUILayout.Toggle("ToggleLateUpdate",GlobalMonoManager.ToggleLateUpdate);
            EditorGUILayout.EndVertical();

            UpdateUI("Normal", GlobalMonoManager.Normal);
            UpdateUI("Unit", GlobalMonoManager.Unit);
            UpdateUI("Global", GlobalMonoManager.Global);
            UpdateUI("View", GlobalMonoManager.View);
        }

        void UpdateUI(string tite, MonoUpdateData updateData)
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(tite + " Update:", updateData.UpdateIns.Count.ToString());
                EditorGUILayout.LabelField(tite + " FixedUpdate:", updateData.FixedUpdateIns.Count.ToString());
                EditorGUILayout.LabelField(tite + " LateUpdate:", updateData.LateUpdateIns.Count.ToString());
                EditorGUILayout.LabelField(tite + " GUI:", updateData.GUIIns.Count.ToString());
                EditorGUILayout.EndVertical();
            }
        }
    }
}