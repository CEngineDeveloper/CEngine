
using System;
using UnityEditor;
using UnityEngine;

namespace CYM.UI.Sequencer
{
    [CustomPropertyDrawer(typeof(AnimationStepBase), true)]
    public class AnimationStepBasePropertyDrawer : PropertyDrawer
    {
        protected void DrawBaseGUI(Rect position, SerializedProperty property, GUIContent label, params string[] excludedPropertiesNames)
        {
            float originY = position.y;

            position.height = EditorGUIUtility.singleLineHeight;
            
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, EditorStyles.foldout);

            if (property.isExpanded)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUI.indentLevel++;
                position = EditorGUI.IndentedRect(position);
                EditorGUI.indentLevel--;
                
                position.height = EditorGUIUtility.singleLineHeight;
                position.y +=  EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                float lastHight = 0;
                //int totalCount = property.GetChildren()
                foreach (SerializedProperty serializedProperty in property.GetChildren())
                {
                    bool shouldDraw = true;
                    for (int i = 0; i < excludedPropertiesNames.Length; i++)
                    {
                        string excludedPropertyName = excludedPropertiesNames[i];
                        if (serializedProperty.name.Equals(excludedPropertyName, StringComparison.Ordinal))
                        {
                            shouldDraw = false;
                            break;
                        }
                    }

                    if (!shouldDraw)
                        continue;
                    
                    EditorGUI.PropertyField(position, serializedProperty);
                    lastHight = EditorGUI.GetPropertyHeight(serializedProperty);
                    position.y += lastHight + EditorGUIUtility.standardVerticalSpacing;
                    //index++;
                }
                position.y -=  EditorGUIUtility.standardVerticalSpacing+ lastHight;
                if (EditorGUI.EndChangeCheck())
                    property.serializedObject.ApplyModifiedProperties();
            }
            
            property.SetPropertyDrawerHeight(position.y - originY + EditorGUIUtility.singleLineHeight);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawBaseGUI(position, property, label);
        }
    
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.GetPropertyDrawerHeight();
        }
    }
}
