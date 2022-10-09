//------------------------------------------------------------------------------
// XenoTemplateMappingInspector.cs
//
// Copyright 2015 Xenobrain Games LLC 
//
// Created by Habib Loew on 5/18/2015
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
    // Custom inspector for XenoTemplateMappings
    //
    //==============================================================================

    [CustomEditor(typeof(TemplateMapping))]
    public class TemplateMappingInspector : Editor {

        //
        // Private data
        //

        private TemplateMapping m_targetMapping = null;
        private List<String> m_environmentVariables = new List<String>();

        private bool m_guiStylesSet = false;
        private GUIStyle m_elementNameStyle;
        private GUIStyle m_notSelectedElementContainerStyle;
        private GUIStyle m_removeButtonStyle;
        private GUIStyle m_rightArrowStyle;
        private GUIStyle m_sectionBackground;
        private GUIStyle m_selectedElementContainerStyle;

        private static readonly String m_arrow = '\u25B7'.ToString();


        //
        // Editor methods
        //

        //------------------------------------------------------------------------------
        private void OnEnable () {

            m_guiStylesSet = false;

        }

        //------------------------------------------------------------------------------
        public override void OnInspectorGUI() {
                
            m_targetMapping = target as TemplateMapping;
            Undo.RecordObject(m_targetMapping, "Modify mapping");

            SetupGuiStyles();

            CustomMappingGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EnvironmentMappingGUI();

            if (GUI.changed) {
                EditorUtility.SetDirty(target);
            }

        }


        //
        // Private methods
        //


        //------------------------------------------------------------------------------
        private String CustomMappingDelegateMenu (String selectedValue, MonoScript script) {

            if (script == null) {
                return String.Empty;
            }

            List<String> methods = TemplateTool.GetValidSubstitutionMethods(script);

            if (methods.Count == 0) {
                return String.Empty;
            }

            int selectedIndex = Math.Max(0, methods.IndexOf(selectedValue));

            int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, methods.ToArray());
            return methods[newSelectedIndex];

        }

        //------------------------------------------------------------------------------
        private void CustomMappingGUI () {

            GUIContent titleContent = new GUIContent(
                "Custom Mappings",
                "Custom mappings let you invoke your own code to generate the value for a substitution. Methods must match the signature \"public static string Method()\""
            );

            EditorGUILayout.LabelField(titleContent, new GUIStyle("OL Title"));
            EditorGUILayout.BeginVertical(m_sectionBackground);

            List<String> customMappingKeys = new List<String>(m_targetMapping.CustomMappings.Keys);

            int mappingCount = customMappingKeys.Count;

            for (int index = 0; index < mappingCount; ++index) {

                String key = customMappingKeys[index];
                MonoScript script = m_targetMapping.CustomMappings[key].TargetScript;
                String methodName = m_targetMapping.CustomMappings[key].MethodName;

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();

                String newKey = FilterKeyString(EditorGUILayout.TextField(key));

                GUILayout.FlexibleSpace();

                GUILayout.Label(m_arrow, m_rightArrowStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical();

                MonoScript newScript = (MonoScript)EditorGUILayout.ObjectField(script, typeof(MonoScript), false);

                String newMethodName = CustomMappingDelegateMenu(methodName, script);

                if (newScript != null && String.IsNullOrEmpty(newMethodName)) {
                    GUIStyle errorStyle = new GUIStyle(EditorStyles.boldLabel);
                    errorStyle.normal.textColor = Color.red;

                    GUIContent errorContent = new GUIContent("No suitable methods detected.", "Only public, static methods taking zero parameters and returning a string are mappable.");

                    EditorGUILayout.LabelField(errorContent, errorStyle);
                }

                EditorGUILayout.EndVertical();

                GUIContent removeContent = new GUIContent(String.Empty, "Delete mapping");
                bool removeElement = GUILayout.Button(removeContent, m_removeButtonStyle);
                if (removeElement) {
                    m_targetMapping.CustomMappings.Remove(key);
                    GUI.FocusControl(String.Empty);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (key != newKey) {
                    m_targetMapping.CustomMappings.Remove(key);
                    m_targetMapping.CustomMappings.Add(newKey, new TemplateCustomMappingData(newScript, newMethodName));
                }
                else if (script != newScript || methodName != newMethodName) {
                    m_targetMapping.CustomMappings[key] = new TemplateCustomMappingData(newScript, newMethodName);
                }

                if (index < mappingCount - 1) {
                    EditorGUILayout.Space();
                    EditorGUILayout.Separator();
                }

            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool addCustomMapping = GUILayout.Button("Add", EditorStyles.miniButton);
            if (addCustomMapping) {
                String newKey = GenerateUniqueKey("CUSTOM", m_targetMapping.CustomMappings);
                m_targetMapping.CustomMappings.Add(newKey, new TemplateCustomMappingData());
                GUI.FocusControl(String.Empty);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

        }

        //------------------------------------------------------------------------------
        private void EnvironmentMappingGUI () {

            UpdateEnvironmentVariablesList();

            GUIContent titleContent = new GUIContent(
                "Environment Mappings",
                "Environment mappings let you use the value of an environment variable for a substitution."
            );

            EditorGUILayout.LabelField(titleContent, new GUIStyle("OL Title"));
            EditorGUILayout.BeginVertical(m_sectionBackground);

            List<String> envVariableMappingKeys = new List<String>(m_targetMapping.EnvironmentMappings.Keys);

            EditorUtil.EolStringToEolType("\r\n");

            int mappingCount = envVariableMappingKeys.Count;
            for (int index = 0; index < mappingCount; ++index) {

                EditorGUILayout.Space();

                String key = envVariableMappingKeys[index];
                String value = m_targetMapping.EnvironmentMappings[key];

                EditorGUILayout.BeginHorizontal();

                String newKey = FilterKeyString(EditorGUILayout.TextField(key));

                GUILayout.FlexibleSpace();

                GUILayout.Label(m_arrow, m_rightArrowStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                GUILayout.FlexibleSpace();
                String newValue = EnvironmentVariableMenu(value);

                GUIContent removeContent = new GUIContent(String.Empty, "Delete mapping");
                bool removeElement = GUILayout.Button(removeContent, m_removeButtonStyle);
                if (removeElement) {
                    m_targetMapping.EnvironmentMappings.Remove(key);
                    GUI.FocusControl(String.Empty);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (key != newKey) {
                    m_targetMapping.EnvironmentMappings.Remove(key);
                    m_targetMapping.EnvironmentMappings.Add(newKey, newValue);
                }
                else if (value != newValue) {
                    m_targetMapping.EnvironmentMappings[key] = newValue;
                }

                if (index < mappingCount - 1) {
                    EditorGUILayout.Space();
                    EditorGUILayout.Separator();
                }

            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool addEnvMapping = GUILayout.Button("Add", EditorStyles.miniButton);
            if (addEnvMapping) {
                String newKey = GenerateUniqueKey("ENV", m_targetMapping.EnvironmentMappings);
                m_targetMapping.EnvironmentMappings.Add(newKey, "");
                GUI.FocusControl(String.Empty);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

        }

        //------------------------------------------------------------------------------
        private String EnvironmentVariableMenu (String selectedValue) {

            int selectedIndex = m_environmentVariables.IndexOf(selectedValue);

            List<String> menuItems = new List<string>(m_environmentVariables);

            if (selectedIndex < 0) {
                menuItems.Add(selectedValue);
                selectedIndex = menuItems.Count - 1;
            }

            int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, menuItems.ToArray());
            return menuItems[newSelectedIndex];

        }

        //------------------------------------------------------------------------------
        private String FilterKeyString (String rawString) {

            String filtered = System.Text.RegularExpressions.Regex.Replace(rawString, @"\s", "_");
            filtered = System.Text.RegularExpressions.Regex.Replace(filtered, @"[^\w_]", "");
            return filtered.ToUpper();

        }

        //------------------------------------------------------------------------------
        private String GenerateUniqueKey<T> (String baseKey, Dictionary<String, T> dictionary) {

            String key = baseKey;
            int retryCount = 0;

            while (dictionary.ContainsKey(key)) {
                ++retryCount;
                key = String.Format("{0}{1}", baseKey, retryCount);
            }

            return key;

        }

        //------------------------------------------------------------------------------
        private void SetupGuiStyles () {

            if (m_guiStylesSet) {
                return;
            }

            m_guiStylesSet = true;

            m_sectionBackground = new GUIStyle("ObjectPickerBackground");
            m_sectionBackground.padding = new RectOffset(4, 4, 4, 4);

            m_selectedElementContainerStyle = new GUIStyle("TL SelectionButton PreDropGlow");
            m_selectedElementContainerStyle.alignment = TextAnchor.MiddleLeft;
            m_selectedElementContainerStyle.padding = new RectOffset(4, 0, 0, 2);
            m_selectedElementContainerStyle.margin = new RectOffset(0, 0, 2, 2);

            m_notSelectedElementContainerStyle = new GUIStyle(m_selectedElementContainerStyle);
            m_notSelectedElementContainerStyle.normal.background = null;

            m_elementNameStyle = new GUIStyle("ObjectPickerSmallStatus");
            m_elementNameStyle.alignment = TextAnchor.MiddleLeft;

            m_removeButtonStyle = new GUIStyle("OL Minus");

            m_rightArrowStyle = new GUIStyle(EditorStyles.label);
            m_rightArrowStyle.alignment = TextAnchor.MiddleLeft;
            m_rightArrowStyle.fontSize = 20;

        }

        //------------------------------------------------------------------------------
        private void UpdateEnvironmentVariablesList () {

            System.Collections.ICollection envKeys = Environment.GetEnvironmentVariables().Keys;
            m_environmentVariables.Capacity = Math.Max(m_environmentVariables.Capacity, envKeys.Count);
            m_environmentVariables.Clear();

            foreach (object envObj in envKeys) {
                m_environmentVariables.Add(envObj as String);
            }

            m_environmentVariables.Sort();

        }

    }

}