//------------------------------------------------------------------------------
// XenoTemplateUtilityWindow.cs
//
// Copyright 2015 Xenobrain Games LLC 
//
// Created by Habib Loew on 4/24/2015
// Owner: Habib Loew
//
// A window where users enter the name of the template instance they wish to 
// create. This window also shows a preview of the files which will be created
// as a part of the template instance.
//
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace CYM.Template
{

    //==============================================================================
    //
    // Window class for the template instantiation window
    //
    //==============================================================================

    public class TemplateUtilityWindow : EditorWindow {

        //
        // Private data
        //

        private bool   m_setFocus;
        private bool   m_setSize;
        private String m_templateFilePath;
        private String m_userSuppliedNameText;
        private String m_targetPath;

        private List<String> m_generatedFileList = new List<String>();


        //
        // Private static data
        //

        private static float s_width = 400.0f;


        //
        // Public methods
        //

        //------------------------------------------------------------------------------
        public static void ShowWindow (String templateFilePath, String defaultName, String targetPath) {

            TemplateUtilityWindow window = TemplateUtilityWindow.CreateInstance<TemplateUtilityWindow>();
            String title = String.Format("Template: {0}", defaultName);

#if UNITY_5_0
            window.title = title;
#else
            window.titleContent = new GUIContent(title);
#endif

            window.Initialize(templateFilePath, defaultName, targetPath);
            window.ShowUtility();
            window.Focus();

        }

        //------------------------------------------------------------------------------
        public void Initialize (String templateFilePath, String defaultName, String targetPath) {

            m_setFocus             = true;
            m_setSize              = true;
            m_targetPath           = targetPath;
            m_templateFilePath     = templateFilePath;
            m_userSuppliedNameText = defaultName;

        }


        //
        // EditorWindow methods
        //

        //------------------------------------------------------------------------------
        private void OnGUI () {

            HandleKeyEvents();

            Rect contentRect = EditorGUILayout.BeginVertical();

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label("Template", titleStyle);

            GUI.SetNextControlName("NameTokenTextField");
            String newName = EditorGUILayout.TextField(m_userSuppliedNameText);
            String error = String.Empty;
            if (newName != m_userSuppliedNameText || m_generatedFileList.Count == 0) {
                TemplateTool.GenerateFileNames(m_templateFilePath, newName, ref m_generatedFileList, ref error);
                m_userSuppliedNameText = newName;
            }

            EditorGUILayout.LabelField("Files to create", String.Format("{0}", m_generatedFileList.Count));

            if (m_generatedFileList.Count == 0) {
                if (!String.IsNullOrEmpty(error)) {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
                else {
                    EditorGUILayout.HelpBox("Template malformed. No files will be created.", MessageType.Error);
                }
            }

            GUIStyle fileNameStyle = new GUIStyle(EditorStyles.label);
            fileNameStyle.alignment = TextAnchor.LowerRight;
            fileNameStyle.contentOffset = new Vector2(-15.0f, 0.0f);

            foreach (String fileName in m_generatedFileList) {
                GUILayout.Label(fileName, fileNameStyle);
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            bool cancelClicked = GUILayout.Button("Cancel", GUILayout.ExpandWidth(false));
            if (cancelClicked) {
                ActionCancel();
            }

            GUILayout.FlexibleSpace();

            if (m_generatedFileList.Count > 0) {
                bool proceedClicked = GUILayout.Button("Proceed", GUILayout.ExpandWidth(false));
                if (proceedClicked) {
                    ActionCreate();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();

            GUIStyle helpStyle = new GUIStyle(EditorStyles.miniLabel);
            helpStyle.fontStyle = FontStyle.Italic;
            helpStyle.alignment = TextAnchor.LowerCenter;
            Rect helpLabelRect = contentRect;
            helpLabelRect.height = EditorGUIUtility.singleLineHeight;
            helpLabelRect.y = contentRect.height - helpLabelRect.height;

            String helpString = "Esc. to cancel; Enter to create";
            if (m_generatedFileList.Count == 0) {
                helpString = "Esc. to cancel";
            }

            GUI.Label(helpLabelRect, helpString, helpStyle);



            if (m_setSize && Event.current.type == EventType.Repaint) {
                ResizeWindow(contentRect);
                m_setSize = false;
            }

            if (m_setFocus && Event.current.type == EventType.Repaint) {
                EditorGUI.FocusTextInControl("NameTokenTextField");
                m_setFocus = false;
            }


        }


        //
        // Private methods
        //

        //------------------------------------------------------------------------------
        private void ActionCancel () {

            this.Close();

        }

        //------------------------------------------------------------------------------
        private void ActionCreate () {

            if (m_generatedFileList.Count > 0) {
                TemplateTool.GenerateFiles(m_templateFilePath, m_userSuppliedNameText, m_targetPath);
            }

            Close();

        }

        //------------------------------------------------------------------------------
        private void HandleKeyEvents () {

            if (Event.current.type == EventType.KeyDown) {

                switch (Event.current.keyCode) {
                    case KeyCode.Return:
                        ActionCreate();
                        return;

                    case KeyCode.Escape:
                        ActionCancel();
                        return;
                }

            }

        }

        //------------------------------------------------------------------------------
        private void ResizeWindow (Rect contentRect) {

            Vector2 templateWindowSize = new Vector2(
                s_width,
                contentRect.height
            );

            this.minSize = templateWindowSize;
            this.maxSize = templateWindowSize;

            this.Focus();

        }

    }

}