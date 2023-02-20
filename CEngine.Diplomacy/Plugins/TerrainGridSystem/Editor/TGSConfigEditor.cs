using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TGS {

	[CustomEditor (typeof(TGSConfig))]
	public class TGSConfigEditor : Editor {

		SerializedProperty title, filterTerritories;
		TGSConfig config;

		void OnEnable() {
			title = serializedObject.FindProperty ("title");
			filterTerritories = serializedObject.FindProperty ("filterTerritories");
			config = (TGSConfig)target;
		}


		public override void OnInspectorGUI () {

			serializedObject.Update ();

			EditorGUILayout.Separator ();
			EditorGUILayout.HelpBox ("To load this configuration, just activate this component or call LoadConfiguration() method of this script.", MessageType.Info);
			EditorGUILayout.PropertyField (title);
			EditorGUILayout.PropertyField (filterTerritories);

			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Clear Grid")) {
				if (EditorUtility.DisplayDialog ("Clear Grid", "Remove any color/texture from cells and territories?", "Ok", "Cancel")) {
					config.Clear ();
				}
			}
			if (GUILayout.Button ("Store Config")) {
				if (EditorUtility.DisplayDialog ("Store Grid Configuration", "Current textures and cell configuration will be stored in this component. Any previous data will be overwritten. Continue?", "Ok", "Cancel")) {
					config.SaveConfiguration (TerrainGridSystem.instance);
				}
			}
			if (GUILayout.Button ("Reload Config")) {
				if (EditorUtility.DisplayDialog ("Reload Grid Configuration", "The stored cell settings will be loaded. Continue?", "Ok", "Cancel")) {
					config.LoadConfiguration ();
				}
			}
			EditorGUILayout.EndHorizontal ();

			serializedObject.ApplyModifiedProperties ();



		}
	}

}