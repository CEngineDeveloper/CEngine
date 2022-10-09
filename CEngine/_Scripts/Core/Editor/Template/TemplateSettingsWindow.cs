//------------------------------------------------------------------------------
// XenoTemplateSettingsWindow.cs
//
// Copyright 2015 Xenobrain Games LLC 
//
// Created by Habib Loew on 5/21/2015
// Owner: Habib Loew
// 
// Settings window for configuring XenoTemplates
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CYM.Template
{

    //==============================================================================
    //
    // Window class for displaying & editing XenoTemplate settings
    //
    //==============================================================================

    public class TemplateSettingsWindow : EditorWindow {

        //
        // Public methods
        //

        //------------------------------------------------------------------------------
        public static void ShowWindow () {

            TemplateSettingsWindow window = EditorWindow.GetWindow<TemplateSettingsWindow>("Settings");
            window.Show();
            window.Focus();

        }


        //
        // EditorWindow methods
        //

        //------------------------------------------------------------------------------
        private void OnGUI () {

            GUIStyle headerStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
            headerStyle.fixedHeight = EditorGUIUtility.singleLineHeight * 2.0f;

            EditorGUILayout.BeginVertical();

            String title = String.Format("Templates v{0} Settings", TemplateTool.Version.ToString("F1"));

            EditorGUILayout.LabelField(title, headerStyle);
            EditorGUILayout.Space();

            GUIContent eolLabelContent = new GUIContent("Normalize EOL", "Select which type of line ending you'd like generated files to use. If you don't know what this means it's safe to leave the default set.");
            EditorUtil.EolType eolType = (EditorUtil.EolType)EditorGUILayout.EnumPopup(eolLabelContent, TemplatePrefs.Eol);
            if (GUI.changed) {
                TemplatePrefs.Eol = eolType;
            }

            GUILayout.Space(40.0f);
            if (GUILayout.Button("CreateTemplate"))
            {
                TemplateTool.CreateTemplate();
            }
            if (GUILayout.Button("CreateMappingSet"))
            {
                TemplateTool.CreateMappingSet();
            }

            EditorGUILayout.EndVertical();

        }

    }

}
