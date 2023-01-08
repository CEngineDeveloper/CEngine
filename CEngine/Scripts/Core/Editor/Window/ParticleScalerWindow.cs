//	Simple Particle Scaler v1.5
//	Copyright Unluck Software	
//	www.chemicalbliss.com																			

using UnityEngine;
using UnityEditor;
namespace CYM
{
	[System.Serializable]
	public class ParticleScalerWindow : EditorWindow
	{
		public float scaleMultiplier = 1.0f;
		public float originalScale = 1.0f;
		public bool autoRename;


		public static void ShowWindow()
		{
			EditorWindow win = EditorWindow.GetWindow(typeof(ParticleScalerWindow));
			win.titleContent = new GUIContent("Simple Particle Scaler");
			win.minSize = new Vector2(200.0f, 130.0f);
			win.maxSize = new Vector2(200.0f, 130.0f);
		}

		public void OnEnable()
		{
		}

		public void OnGUI()
		{
			GUILayout.Space(20);
			Color32 colorBlueLight = new Color32((byte)200, (byte)255, (byte)255, (byte)255);
			GUIStyle styleBigButton = null;
			styleBigButton = new GUIStyle(GUI.skin.button);
			styleBigButton.fixedWidth = 90.0f;
			styleBigButton.fixedHeight = 20.0f;
			styleBigButton.fontSize = 9;
			GUIStyle styleToggle = null;
			styleToggle = new GUIStyle(GUI.skin.toggle);
			styleToggle.fontSize = 9;
			GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
			titleStyle.fixedWidth = 200.0f;
			EditorGUILayout.Space();
			scaleMultiplier = EditorGUILayout.Slider(scaleMultiplier, 0.01f, 4.0f);
			EditorGUILayout.Space();
			GUI.color = colorBlueLight;
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Scale", styleBigButton))
			{
				foreach (GameObject gameObj in Selection.gameObjects)
				{
					if (autoRename)
					{
						string[] s = gameObj.name.Split('¤');
						if (s.Length == 1)
						{
							gameObj.name += " ¤" + scaleMultiplier;
						}
						else
						{
							float i = float.Parse(s[s.Length - 1]);
							gameObj.name = s[0] + "¤" + scaleMultiplier * i;
						}
					}
					ParticleSystem[] pss = null;
					pss = gameObj.GetComponentsInChildren<ParticleSystem>();
					foreach (ParticleSystem ps in pss)
					{
						ps.Stop();
						ScaleParticles(gameObj, ps);
						ps.Play();
					}
				}
			}
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Save Prefabs", styleBigButton))
			{
				CreatePrefabs();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("", GUILayout.Width(10.0f));
			autoRename = GUILayout.Toggle(autoRename, "Automatic rename", styleToggle);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			GUI.color = colorBlueLight;
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Play", EditorStyles.miniButtonLeft))
			{
				ParticleCalls("Play");
			}
			if (GUILayout.Button("Pause", EditorStyles.miniButtonMid))
			{
				ParticleCalls("Pause");
			}
			if (GUILayout.Button("Stop", EditorStyles.miniButtonRight))
			{
				ParticleCalls("Stop");
			}
			EditorGUILayout.EndHorizontal();
		}

		public void CreatePrefabs()
		{
			if (Selection.gameObjects.Length > 0)
			{
				string path = EditorUtility.SaveFolderPanel("Select Folder ", "Assets/", "");
				if (path.Length > 0)
				{
					if (path.Contains("" + Application.dataPath))
					{
						string s = "" + path + "/";
						string d = "" + Application.dataPath + "/";
						string p = "Assets/" + s.Remove(0, d.Length);
						GameObject[] objs = Selection.gameObjects;
						bool cancel = false;
						foreach (GameObject go in objs)
						{
							if (!cancel)
							{
								if (AssetDatabase.LoadAssetAtPath(p + go.gameObject.name + ".prefab", typeof(GameObject)) != null)
								{
									int option = EditorUtility.DisplayDialogComplex("Are you sure?", "" + go.gameObject.name + ".prefab" + " already exists. Do you want to overwrite it?", "Yes", "No", "Cancel");
									switch (option)
									{
										case 0:
											//CreateNew(go.gameObject, p + go.gameObject.name + ".prefab");
											goto case 1;
										case 1:
											break;
										case 2:
											cancel = true;
											break;
										default:
											Debug.LogError("Unrecognized option.");
											break;
									}
								}
								//else CreateNew(go.gameObject, p + go.gameObject.name + ".prefab");
							}
						}
					}
					else
					{
						Debug.LogError("Prefab Save Failed: Can't save outside project: " + path);
					}
				}
			}
			else
			{
				Debug.LogWarning("No GameObjects Selected");
			}
		}

		//public static void CreateNew(GameObject obj, string localPath) {
		//	Object prefab = PrefabUtility.CreateEmptyPrefab(localPath);
		//	PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
		//}

		public void UpdateParticles()
		{
			foreach (GameObject gameObj in Selection.gameObjects)
			{
				ParticleSystem[] pss = null;
				pss = gameObj.GetComponentsInChildren<ParticleSystem>();
				foreach (ParticleSystem ps in pss)
				{
					ps.Stop();
					ps.Play();
				}
			}
		}

		public void ParticleCalls(string call)
		{
			foreach (GameObject gameObj in Selection.gameObjects)
			{
				ParticleSystem[] pss = null;
				pss = gameObj.GetComponentsInChildren<ParticleSystem>();
				foreach (ParticleSystem ps in pss)
				{
					if (call == "Pause") ps.Pause();
					else if (call == "Play") ps.Play();
					else if (call == "Stop")
					{
						ps.Stop();
						ps.Clear();
					}
				}
			}
		}


		public void ScaleParticles(GameObject __parent_cs1, ParticleSystem __particles_cs1)
		{

			if (__parent_cs1 != __particles_cs1.gameObject)
			{
				__particles_cs1.transform.localPosition *= scaleMultiplier;
			}
			SerializedObject serializedParticles = new SerializedObject(__particles_cs1);

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
		serializedParticles.FindProperty("InitialModule.gravityModifier").floatValue *= scaleMultiplier;
#else
			serializedParticles.FindProperty("InitialModule.gravityModifier.scalar").floatValue *= scaleMultiplier;
#endif

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
#else
			serializedParticles.FindProperty("NoiseModule.strength.scalar").floatValue *= scaleMultiplier;
			serializedParticles.FindProperty("LightsModule.rangeCurve.scalar").floatValue *= scaleMultiplier;
#endif

			serializedParticles.FindProperty("InitialModule.startSize.scalar").floatValue *= scaleMultiplier;
			serializedParticles.FindProperty("InitialModule.startSpeed.scalar").floatValue *= scaleMultiplier;
#if UNITY_5
		serializedParticles.FindProperty("ShapeModule.boxX").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("ShapeModule.boxY").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("ShapeModule.boxZ").floatValue *= scaleMultiplier;
#else
			serializedParticles.FindProperty("ShapeModule.m_Scale").vector3Value *= scaleMultiplier;
#endif

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
		serializedParticles.FindProperty("ShapeModule.radius").floatValue *= scaleMultiplier;
#else
			serializedParticles.FindProperty("ShapeModule.radius.value").floatValue *= scaleMultiplier;
#endif

			serializedParticles.FindProperty("VelocityModule.x.scalar").floatValue *= scaleMultiplier;
			serializedParticles.FindProperty("VelocityModule.y.scalar").floatValue *= scaleMultiplier;
			serializedParticles.FindProperty("VelocityModule.z.scalar").floatValue *= scaleMultiplier;
			ScaleCurve(serializedParticles.FindProperty("VelocityModule.x.minCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("VelocityModule.x.maxCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("VelocityModule.y.minCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("VelocityModule.y.maxCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("VelocityModule.z.minCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("VelocityModule.z.maxCurve").animationCurveValue);
			serializedParticles.FindProperty("ClampVelocityModule.x.scalar").floatValue *= scaleMultiplier;
			serializedParticles.FindProperty("ClampVelocityModule.y.scalar").floatValue *= scaleMultiplier;
			serializedParticles.FindProperty("ClampVelocityModule.z.scalar").floatValue *= scaleMultiplier;
			serializedParticles.FindProperty("ClampVelocityModule.magnitude.scalar").floatValue *= scaleMultiplier;
			ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.x.minCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.x.maxCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.y.minCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.y.maxCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.z.minCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.z.maxCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.magnitude.minCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.magnitude.maxCurve").animationCurveValue);
			serializedParticles.FindProperty("ForceModule.x.scalar").floatValue *= scaleMultiplier;
			serializedParticles.FindProperty("ForceModule.y.scalar").floatValue *= scaleMultiplier;
			serializedParticles.FindProperty("ForceModule.z.scalar").floatValue *= scaleMultiplier;
			ScaleCurve(serializedParticles.FindProperty("ForceModule.x.minCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ForceModule.x.maxCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ForceModule.y.minCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ForceModule.y.maxCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ForceModule.z.minCurve").animationCurveValue);
			ScaleCurve(serializedParticles.FindProperty("ForceModule.z.maxCurve").animationCurveValue);
			serializedParticles.ApplyModifiedProperties();
		}

		public void ScaleCurve(AnimationCurve curve)
		{
			for (int i = 0; i < curve.keys.Length; i++)
			{
				var tmp_cs1 = curve.keys[i];
				tmp_cs1.value *= scaleMultiplier;
				curve.keys[i] = tmp_cs1;
			}
		}
	}
}