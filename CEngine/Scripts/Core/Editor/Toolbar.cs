//------------------------------------------------------------------------------
// Toolbar.cs
// Copyright 2022 2022/12/27 
// Created by CYM on 2022/12/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;
namespace CYM
{
	[InitializeOnLoad]
	public class SceneSwitchLeftButton
	{
		static SceneSwitchLeftButton()
		{
			//BntCompile = AssetDatabase.GetBuiltinExtraResource<Texture>("");
			ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
		}

		static void OnToolbarGUI()
		{
			GUILayout.FlexibleSpace();

			if (GUILayout.Button(EditorGUIUtility.IconContent("d_Scene")))
			{
				BuildWindow.GoToStart();
			}
			if (GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool On")))
			{
				BuildWindow.Compilation();
			}
		}
	}
}