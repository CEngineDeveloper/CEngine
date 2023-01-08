//------------------------------------------------------------------------------
// XenoTemplateInspector.cs
//
// Copyright 2015 Xenobrain Games LLC 
//
// Created by Habib Loew on 5/8/2015
// Owner: Habib Loew
//
// 
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
    // Custom inspector for XenoTemplates
    //
    //==============================================================================

    [CustomEditor(typeof(Template))]
    public class TemplateInspector : Editor {

        //
        // Private data
        //

        private Rect         m_elementSelectorRect;
        private Rect         m_elementDetailsRect;
        private Vector2      m_scrollState         = Vector2.zero;
        private int          m_selectedElement     = 0;
        private Template m_targetTemplate      = null;

        private bool m_guiStylesSet = false;
        private GUIStyle m_elementSelectionBgStyle;
        private GUIStyle m_selectedElementContainerStyle;
        private GUIStyle m_notSelectedElementContainerStyle;
        private GUIStyle m_elementNameStyle;
        private GUIStyle m_removeButtonStyle;


        //
        // Editor methods
        //

        //------------------------------------------------------------------------------
        private void OnEnable () {

            m_guiStylesSet = false;

        }

        //------------------------------------------------------------------------------
        public override void OnInspectorGUI () {

            m_targetTemplate = target as Template;
            Undo.RecordObject(m_targetTemplate, "Modify template");

            if (m_selectedElement >= m_targetTemplate.TemplateElements.Count) {
                SelectLastElement();
            }

            SetupGuiStyles();

            //SaveAndRefreshGUI();

            TemplateElementsGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            TemplateDetailsGUI();

            if (GUI.changed) {
                EditorUtility.SetDirty(target);
            }

            HandleDragAndDrop(m_elementSelectorRect, m_elementDetailsRect);

        }


        //
        // Private methods
        //

        //------------------------------------------------------------------------------
        private TemplateElement AddTemplateElement (String name) {

            String finalName = UniquifyElementName(name);

            TemplateElement element = new TemplateElement(finalName, "{0}", String.Empty, String.Empty);
            m_targetTemplate.TemplateElements.Add(element);

            return element;

        }

        //------------------------------------------------------------------------------
        private String FilterElementName (String rawString) {

            String filtered = System.Text.RegularExpressions.Regex.Replace(rawString, @"\s", "_");
            filtered = System.Text.RegularExpressions.Regex.Replace(filtered, @"[^\w_]", "");
            return filtered;

        }

        //------------------------------------------------------------------------------
        private String FilterExtension (String rawString) {

            String filtered = System.Text.RegularExpressions.Regex.Replace(rawString, @"\s", "_");
            filtered = System.Text.RegularExpressions.Regex.Replace(filtered, @"[^\w_.]", "");
            return filtered;

        }

        //------------------------------------------------------------------------------
        private String FilterFileNameFormat (String rawString) {

            String filtered = System.Text.RegularExpressions.Regex.Replace(rawString, @"\s", "_");
            filtered = System.Text.RegularExpressions.Regex.Replace(filtered, @"[^\w_{}]", "");
            return filtered;

        }

        //------------------------------------------------------------------------------
        private void HandleDragAndDrop (Rect elementListDropArea, Rect elementDetailsDropArea) {

            if (Event.current.type == EventType.DragExited) {
                // Clean up if drag was canceled preemptively (e.g. using escape)
                DragAndDrop.PrepareStartDrag();
            }

            bool mouseInElementList = elementListDropArea.Contains(Event.current.mousePosition);
            bool mouseInElementDetails = elementDetailsDropArea.Contains(Event.current.mousePosition);

            if (!mouseInElementList && !mouseInElementDetails) {
                return;
            }

            switch (Event.current.type) {
                case EventType.DragUpdated:
                    if (IsDraggedDataTextAsset()) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    } else {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }

                    Event.current.Use();
                    break;

                case EventType.DragPerform:

                    // Only single text assets can be dragged into the element details
                    if (mouseInElementDetails && DragAndDrop.objectReferences.Length > 1) {
                        EditorUtility.DisplayDialog(
                            "Error!", 
                            "Cannot import multiple objects into an existing template element.\n\n" + 
                            "If you wish to create multiple template elements from dragged objects please drop them in the Template Elements area.\n\n" + 
                            "Single objects can be dropped into the Element Details area to update an existing element.", 
                            "Ok"
                        );
                        DragAndDrop.PrepareStartDrag();
                        return;
                    }

                    DragAndDrop.AcceptDrag();
                    if (mouseInElementList) {
                        ImportDraggedTextAssetAsNewElement();
                    } 
                    else if (mouseInElementDetails) {
                        ImportDraggedTextAssetIntoCurrentElement();
                    }
                    Event.current.Use();
                    break;

                case EventType.MouseUp:
                    // Clean up
                    DragAndDrop.PrepareStartDrag();
                    break;
            }

        }

        //------------------------------------------------------------------------------
        private void ImportDraggedTextAssetAsNewElement () {

            for (int index = 0; index < DragAndDrop.objectReferences.Length; ++index) {

                TextAsset asset = DragAndDrop.objectReferences[index] as TextAsset;
                String path = DragAndDrop.paths[index];

                if (asset == null) {
                    continue;
                }

                TemplateElement newElement = AddTemplateElement(asset.name);
                newElement.Extension = System.IO.Path.GetExtension(path);
                newElement.TemplateTokenFormat = String.Concat("{0}", newElement.ElementName);
                newElement.Contents = asset.text;

                SelectLastElement();
            }

        }

        //------------------------------------------------------------------------------
        private void ImportDraggedTextAssetIntoCurrentElement () {

            for (int index = 0; index < DragAndDrop.objectReferences.Length; ++index) {

                TextAsset asset = DragAndDrop.objectReferences[index] as TextAsset;
                String path = DragAndDrop.paths[index];

                if (asset == null) {
                    continue;
                }

                TemplateElement newElement = m_targetTemplate.TemplateElements[m_selectedElement];
                newElement.Extension = System.IO.Path.GetExtension(path);
                newElement.Contents = asset.text;
            }

        }

        //------------------------------------------------------------------------------
        private bool IsDraggedDataTextAsset () {

            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences) {
                TextAsset asset = obj as TextAsset;

                if (asset != null) {
                    return true;
                }
            }

            return false;

        }

        //------------------------------------------------------------------------------
        private String NormalizeLineEndings (String text, String lineEnding) {

            String normalizedUnixLineEnding = text.Replace("\r\n", "\n");
            if (lineEnding == "\n") {
                return normalizedUnixLineEnding;
            }

            String normalizedUserLineEnding = normalizedUnixLineEnding.Replace("\n", lineEnding);

            return normalizedUserLineEnding;

        }

        //------------------------------------------------------------------------------
        private void SaveAndRefreshGUI () {

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIContent saveButtonContent = new GUIContent("Save & Refresh", "Saves all unsaved assets and refreshes the Xenotemplates menu. Refreshing is only required when the name of the template changes.");

            bool saveAndRefreshMenu = GUILayout.Button(saveButtonContent, EditorStyles.miniButton);
            if (saveAndRefreshMenu) {
                AssetDatabase.SaveAssets();
                TemplateTool.RefreshTemplates();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

        }

        //------------------------------------------------------------------------------
        private void SelectLastElement () {

            m_selectedElement = Math.Max(0, m_targetTemplate.TemplateElements.Count - 1);

        }

        //------------------------------------------------------------------------------
        private void SetupGuiStyles () {

            if (m_guiStylesSet) {
                return;
            }

            m_guiStylesSet = true;

            m_elementSelectionBgStyle = new GUIStyle("ObjectPickerBackground");
            m_elementSelectionBgStyle.padding = new RectOffset(4, 4, 4, 4);

            m_selectedElementContainerStyle = new GUIStyle("TL SelectionButton PreDropGlow");
            m_selectedElementContainerStyle.alignment = TextAnchor.MiddleLeft;
            m_selectedElementContainerStyle.padding = new RectOffset(4, 0, 0, 2);
            m_selectedElementContainerStyle.margin = new RectOffset(0, 0, 2, 2);

            m_notSelectedElementContainerStyle = new GUIStyle(m_selectedElementContainerStyle);
            m_notSelectedElementContainerStyle.normal.background = null;

            m_elementNameStyle = new GUIStyle("ObjectPickerSmallStatus");
            m_elementNameStyle.alignment = TextAnchor.MiddleLeft;

            m_removeButtonStyle = new GUIStyle("OL Minus");

        }

        //------------------------------------------------------------------------------
        private void TemplateDetailsGUI () {

            if (m_targetTemplate.TemplateElements.Count <= 0) {
                return;
            }


            EditorGUILayout.LabelField("Element Details", new GUIStyle("OL Title"));

            Rect elementDetailsRect = EditorGUILayout.BeginVertical(m_elementSelectionBgStyle);

            TemplateElement element = m_targetTemplate.TemplateElements[m_selectedElement];

            String elementNameTooltip = "Token for use in the %ELEMENT_NAME()% substition when referring to other elements in a template.";
            GUIContent elementNameContent = new GUIContent("Element Name", elementNameTooltip);
            element.ElementName = FilterElementName(EditorGUILayout.TextField(elementNameContent, element.ElementName));

            String templateTokenFormatTooltip = "C# style Format string used to generate the template token and file name for the output of this element. {0} will be replaced by the user supplied token when the template is instantiated.";
            GUIContent templateTokenFormatContent = new GUIContent("Template Token Format", templateTokenFormatTooltip);
            element.TemplateTokenFormat = FilterFileNameFormat(EditorGUILayout.TextField(templateTokenFormatContent, element.TemplateTokenFormat));

            String fileExtensionTooltip = "Optional file extension used when generating the file name for the output of this element.";
            GUIContent fileExtensionContent = new GUIContent("File Extension", fileExtensionTooltip);
            element.Extension = FilterExtension(EditorGUILayout.TextField(fileExtensionContent, element.Extension));

            EditorGUILayout.Space();

            try {
                String exampleFileName = String.Format(element.TemplateTokenFormat, "Example");
                String exampleHelpText = String.Format("Example generated file name: \"{0}{1}\"", exampleFileName, element.Extension);
                EditorGUILayout.HelpBox(exampleHelpText, MessageType.Info);
            } catch (Exception) {
                EditorGUILayout.HelpBox("Invalid file name format.  Please use {0} to insert the user specified name into the file name.", MessageType.Error);
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Contents", EditorStyles.largeLabel);
            GUILayout.FlexibleSpace();

            String importTooltip = "Import a file to populate this template element.";
            GUIContent importContent = new GUIContent("Import...", importTooltip);

            bool importFile = GUILayout.Button(importContent, EditorStyles.miniButton);
            if (importFile) {
                String path = EditorUtility.OpenFilePanel("Import code file", "", "");
                if (!String.IsNullOrEmpty(path)) {
                    using (System.IO.StreamReader fileReader = System.IO.File.OpenText(path)) {
                        element.Contents = fileReader.ReadToEnd();
                    }
                    GUI.FocusControl(String.Empty);
                }
            }
            EditorGUILayout.EndHorizontal();

            m_scrollState = EditorGUILayout.BeginScrollView(m_scrollState, GUILayout.ExpandHeight(true));

            String uiNormalizedContents = EditorUtil.NormalizeLineEndings(element.Contents, EditorUtil.EolType.Unix);
            if (uiNormalizedContents.Length <= EditorUtil.MaxTextAreaStringLength) {
                // Our template is at an editable length
                String rawContents = EditorGUILayout.TextArea(uiNormalizedContents, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

                // If editing has made the template too long then we trim it and force the
                // TextArea to update by removing focus
                if (rawContents.Length > EditorUtil.MaxTextAreaStringLength) {
                    rawContents = rawContents.Substring(0, EditorUtil.MaxTextAreaStringLength);
                    GUI.FocusControl(String.Empty);
                }

                if (GUI.changed) {
                    element.Contents = EditorUtil.NormalizeLineEndings(rawContents, TemplatePrefs.Eol);
                }
            }
            else {
                // Template is not at an editable length
                EditorGUILayout.HelpBox("Template is too long for editing.  Please import a shorter template.", MessageType.Error);
            }

            
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();

            // Cache the rect for this UI area for later use in drag & drop operations
            if (Event.current.type == EventType.Repaint) {
                m_elementDetailsRect = elementDetailsRect;
            }

        }

        //------------------------------------------------------------------------------
        private void TemplateElementsGUI () {

            String titleTooltip = "Each template element corresponds to a separate file which will be created when this template is used.  Click an element name to view or edit it.";
            GUIContent titleContent = new GUIContent("Template Elements", titleTooltip);
            EditorGUILayout.LabelField(titleContent, new GUIStyle("OL Title"));

            Rect elementSelectorRect = EditorGUILayout.BeginVertical(m_elementSelectionBgStyle);

            for (int i = 0; i < m_targetTemplate.TemplateElements.Count; ++i) {
                EditorGUILayout.BeginHorizontal((i == m_selectedElement) ? m_selectedElementContainerStyle : m_notSelectedElementContainerStyle, GUILayout.ExpandWidth(true));

                bool selectElement = GUILayout.Button(m_targetTemplate.TemplateElements[i].ElementName, m_elementNameStyle);
                if (selectElement) {
                    m_selectedElement = i;
                    GUI.FocusControl(String.Empty);
                }

                GUILayout.FlexibleSpace();

                if (m_targetTemplate.TemplateElements.Count > 1) {
                    GUIContent removeElementContent = new GUIContent("", "Delete this element.");
                    bool removeElement = GUILayout.Button(removeElementContent, m_removeButtonStyle);
                    if (removeElement) {
                        m_selectedElement = 0;
                        m_targetTemplate.TemplateElements.RemoveAt(i);
                        GUI.FocusControl(String.Empty);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIContent addElementContent = new GUIContent("Add element", "Adds a new template element. Each element corresponds to a file that will be created when the template is used.");
            bool addSection = GUILayout.Button(addElementContent, EditorStyles.miniButton);
            if (addSection) {
                AddTemplateElement("Element");
                GUI.FocusControl(String.Empty);
                SelectLastElement();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // Cache the rect for this UI area for later use in drag & drop operations
            if (Event.current.type == EventType.Repaint) {
                m_elementSelectorRect = elementSelectorRect;
            }
            
        }

        //------------------------------------------------------------------------------
        private String UniquifyElementName (String name) {

            String finalName = name;
            int offsetIndex = 0;
            bool validated = false;

            while (!validated) {

                validated = true;

                for (int i = 0; i < m_targetTemplate.TemplateElements.Count; ++i) {
                    if (m_targetTemplate.TemplateElements[i].ElementName == finalName) {
                        ++offsetIndex;
                        finalName = String.Format("{0}{1}", name, offsetIndex);
                        validated = false;
                        break;
                    }
                }
            }

            return finalName;

        }

    }

}