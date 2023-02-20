using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TGS {
	
	[CustomEditor (typeof(TGSSnippetBase), true)]
	public class TGSSnippetEditor : Editor {

		SerializedProperty execute, order, delay, duration, easeType;

		void OnEnable() {
			execute = serializedObject.FindProperty ("execute");
			order = serializedObject.FindProperty ("order");
			delay = serializedObject.FindProperty ("delay");
			duration = serializedObject.FindProperty ("duration");
			easeType = serializedObject.FindProperty ("easeType");
		}

		public override void OnInspectorGUI () {
			EditorGUILayout.Separator ();

			TGSSnippetBase snippet = (TGSSnippetBase)target;

			if (snippet != null && !string.IsNullOrEmpty (snippet.instructions)) {
				EditorGUILayout.HelpBox (snippet.instructions, MessageType.Info);
			}

			if (snippet.hideOptions)
				return;
			
			serializedObject.Update ();

			EditorGUILayout.Separator ();
			EditorGUILayout.LabelField ("Snippet Parameters", EditorStyles.boldLabel);
			DrawPropertiesExcluding (serializedObject, "m_Script");

			EditorGUILayout.Separator ();
			EditorGUILayout.LabelField ("Execution Options", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PropertyField (execute);
			if (GUILayout.Button ("Run Now")) {
				snippet.Run ();
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.PropertyField (order);
			if (execute.intValue == (int)ExecutionEvent.OnStart) {
				EditorGUILayout.PropertyField (delay);
				if (snippet.supportsTweening) {
					EditorGUILayout.PropertyField (duration);
					if (duration.floatValue > 0) {
						EditorGUILayout.PropertyField (easeType);
					}
				}
			}
			if (serializedObject.ApplyModifiedProperties ()) {
				if (snippet.execute == ExecutionEvent.Immediate || (!Application.isPlaying && snippet.execute == ExecutionEvent.OnlyInEditMode)) {
					snippet.PingSnippets ();
				}
			}

		}
	}

}

