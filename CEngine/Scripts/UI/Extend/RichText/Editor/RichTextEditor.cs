using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif
using CYM.UI;

namespace CYM.UI
{
	[CustomEditor(typeof(RichText), true), CanEditMultipleObjects]
	public class RichTextEditor : GraphicEditor
	{
		private SerializedProperty m_Content;
		private SerializedProperty m_FontData;
        private SerializedProperty m_SpriteGroupsSize;
		private SerializedProperty m_UsedEffects;
		private SerializedProperty m_UnusedEffects;
		private SerializedProperty m_OnLinkProperty;
        private SerializedProperty m_EnableSprite;
        private SerializedProperty m_EnableRefStr;

        protected static string VerticalStyle = "HelpBox";

        protected override void OnEnable()
		{
			base.OnEnable();
			m_Content = serializedObject.FindProperty("m_Content");
			m_FontData = serializedObject.FindProperty("m_FontData");
			m_UsedEffects = serializedObject.FindProperty("UsedEffects");
			m_UnusedEffects = serializedObject.FindProperty("UnusedEffects");
            m_SpriteGroupsSize = serializedObject.FindProperty("SpriteSizeFaction");
			m_EnableSprite = serializedObject.FindProperty("EnableSprite");
            m_EnableRefStr = serializedObject.FindProperty("EnableRefStr");
            m_OnLinkProperty = serializedObject.FindProperty("onLink");
		}

		void SetHideFlags(SerializedProperty effects)
		{
			for (var i = 0; i < effects.arraySize; i++) {
				var sp = effects.GetArrayElementAtIndex(i);
				var effect = sp.objectReferenceValue as TextEffect;
				if (effect != null) {
					effect.hideFlags = HideFlags.HideInInspector;
				}
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(m_Content, new GUIContent("Text"), new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(m_FontData, new GUILayoutOption[0]);
			AppearanceControlsGUI();
			RaycastControlsGUI();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical(VerticalStyle);
            EditorGUILayout.PropertyField(m_EnableSprite, true, new GUILayoutOption[0]);
            if (m_EnableSprite.boolValue)
            {
                EditorGUILayout.PropertyField(m_SpriteGroupsSize, true, new GUILayoutOption[0]);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical(VerticalStyle);
            EditorGUILayout.PropertyField(m_EnableRefStr, true, new GUILayoutOption[0]);
            if (m_EnableRefStr.boolValue)
            {

            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
			EditorGUILayout.PropertyField(m_OnLinkProperty, new GUILayoutOption[0]);

			SetHideFlags(m_UsedEffects);
			SetHideFlags(m_UnusedEffects);

			serializedObject.ApplyModifiedProperties();
		}

		static void AddRichText(MenuCommand menuCommand)
		{
			var CreateUIElementRoot = typeof(DefaultControls).GetMethod("CreateUIElementRoot", BindingFlags.Static | BindingFlags.NonPublic);
			var gameObject = CreateUIElementRoot.Invoke(null, new object[] { "RichText", new Vector2(160f, 30f) }) as GameObject;
			var richText = gameObject.AddComponent<RichText>();
			richText.text = "New Text";

			Assembly assembly = Assembly.Load("UnityEditor.UI");
			var type = assembly.GetType("UnityEditor.UI.MenuOptions");
			var PlaceUIElementRoot = type.GetMethod("PlaceUIElementRoot", BindingFlags.Static | BindingFlags.NonPublic);
			PlaceUIElementRoot.Invoke(null, new object[] { gameObject, menuCommand });
		}
	}
}
