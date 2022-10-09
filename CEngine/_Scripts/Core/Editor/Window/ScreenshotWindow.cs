using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace CYM
{
	[ExecuteInEditMode]
	public class ScreenshotWindow : EditorWindow
	{

		public enum Extension { EXR, JPG, PNG, TGA }

		public int CaptureWidth = 0;
		public int CaptureHeight = 0;
		public Camera CaptureCamera = null;
		public bool CaptureTransparent = true;
		public string CapturePath = null;
		public string CaptureLast = null;
		public Extension CaptureExtension = Extension.PNG;
		public bool FilenameCustomToggle = false;
		public string FilenameCustomValue = null;

		//[MenuItem("Window/Super Screenshot")]
		public static void ShowWindow()
		{
			EditorWindow window = EditorWindow.GetWindow<ScreenshotWindow>(false, "Super Screenshot", true);
			window.autoRepaintOnSceneChange = true;
			window.Show();
			window.Focus();
		}

		void OnGUI()
		{

			// Camera
			Camera[] CamerasScene = SceneView.GetAllSceneCameras();
			Camera CameraScene = CamerasScene.Length > 0 ? CamerasScene[0] : null;
			Camera CameraMain = Camera.main ?? FindObjectOfType<Camera>();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);
			if (CaptureCamera == null) CaptureCamera = CameraMain ?? CameraScene;
			CaptureCamera = EditorGUILayout.ObjectField(CaptureCamera, typeof(Camera), true, null) as Camera;
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(CameraMain == null);
			if (GUILayout.Button("Game Camera")) CaptureCamera = CameraMain;
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(CameraScene == null);
			if (GUILayout.Button("Scene Camera"))
			{
				CaptureWidth = (int)Handles.GetMainGameViewSize().x;
				CaptureHeight = (int)Handles.GetMainGameViewSize().y;
				CaptureCamera = CameraScene;
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			// Resolution
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Resolution", EditorStyles.boldLabel);
			if (CaptureWidth <= 0 || CaptureHeight <= 0)
			{
				CaptureWidth = (int)Handles.GetMainGameViewSize().x;
				CaptureHeight = (int)Handles.GetMainGameViewSize().y;
			}
			CaptureWidth = EditorGUILayout.IntField("Width", CaptureWidth);
			CaptureHeight = EditorGUILayout.IntField("Height", CaptureHeight);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("128x128")) CaptureWidth = CaptureHeight = 128;
			if (GUILayout.Button("256x256")) CaptureWidth = CaptureHeight = 256;
			if (GUILayout.Button("512x512")) CaptureWidth = CaptureHeight = 512;
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("1K")) CaptureWidth = CaptureHeight = 1024;
			if (GUILayout.Button("2K")) CaptureWidth = CaptureHeight = 2048;
			if (GUILayout.Button("4K")) CaptureWidth = CaptureHeight = 4096;
			if (GUILayout.Button("8K")) CaptureWidth = CaptureHeight = 8192;
			EditorGUILayout.EndHorizontal();

			// Settings
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			if (string.IsNullOrEmpty(CapturePath)) CapturePath = Application.dataPath;
			EditorGUILayout.TextField("Save Path", CapturePath, GUILayout.ExpandWidth(true));
			if (GUILayout.Button("Browse"))
			{
				string path = EditorUtility.SaveFolderPanel("Save path", Application.dataPath, "");
				if (!string.IsNullOrEmpty(path)) CapturePath = path;
			}
			EditorGUILayout.EndHorizontal();
			CaptureExtension = (Extension)EditorGUILayout.EnumPopup("Extension", CaptureExtension);
			if (CaptureExtension == Extension.JPG)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.Toggle("Transparent", false);
				EditorGUI.EndDisabledGroup();
			}
			else
			{
				CaptureTransparent = EditorGUILayout.Toggle("Transparent", CaptureTransparent);
			}
			FilenameCustomToggle = EditorGUILayout.Toggle("Custom Filename", FilenameCustomToggle);
			if (FilenameCustomToggle)
			{
				FilenameCustomValue = EditorGUILayout.TextField("Filename", FilenameCustomValue, GUILayout.ExpandWidth(true));
			}
			else
			{
				EditorGUILayout.LabelField("Filename", GetFilename());
			}

			// Capture
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Capture", EditorStyles.boldLabel);
			if (CaptureCamera == null) EditorGUILayout.HelpBox("Camera not found", MessageType.Warning);
			EditorGUI.BeginDisabledGroup(CaptureCamera == null);
			if (GUILayout.Button("Capture Screenshot", GUILayout.MinHeight(40)))
			{
				Capture();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Open save path"))
			{
				Application.OpenURL("file://" + CapturePath);
			}
			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(CaptureLast));
			if (GUILayout.Button("Open last screenshot"))
			{
				Application.OpenURL("file://" + CaptureLast);
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

		}

		private void Capture()
		{

			// HDRP - HDAdditionalCameraData
			Type HDRPCameraTypeA = Type.GetType(
				"UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData, " +
				"Unity.RenderPipelines.HighDefinition.Runtime",
				false, true
			);
			Type HDRPCameraTypeB = Type.GetType(
				"UnityEngine.Experimental.Rendering.HDPipeline.HDAdditionalCameraData, " +
				"Unity.RenderPipelines.HighDefinition.Runtime",
				false, true
			);
			Type HDRPCameraType = HDRPCameraTypeA ?? HDRPCameraTypeB;
			Component HDRPCameraInstance = HDRPCameraType == null ? null : CaptureCamera.gameObject.GetComponent(HDRPCameraType);
			FieldInfo HDRPCameraBackgroundColorHDR = HDRPCameraType == null ? null : HDRPCameraType.GetField("backgroundColorHDR");
			FieldInfo HDRPCameraClearMode = HDRPCameraType == null ? null : HDRPCameraType.GetField("clearColorMode");

			// HDRP - HDAdditionalCameraData.ClearColorMode
			Type HDRPClearModeEnumA = Type.GetType(
				"UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData+ClearColorMode, " +
				"Unity.RenderPipelines.HighDefinition.Runtime",
				false, true
			);
			Type HDRPClearModeEnumB = Type.GetType(
				"UnityEngine.Experimental.Rendering.HDPipeline.HDAdditionalCameraData+ClearColorMode, " +
				"Unity.RenderPipelines.HighDefinition.Runtime",
				false, true
			);
			Type HDRPClearModeEnum = HDRPClearModeEnumA ?? HDRPClearModeEnumB;
			object HDRPClearModeColor = null;
			if (HDRPClearModeEnum != null && HDRPClearModeEnum != null)
			{
				try
				{
					HDRPClearModeColor = Enum.Parse(HDRPClearModeEnum, "Color");
				}
				catch (Exception) { }
				if (HDRPClearModeColor == null)
				{
					try
					{
						HDRPClearModeColor = Enum.Parse(HDRPClearModeEnum, "BackgroundColor");
					}
					catch (Exception) { }
				}
			}

			// Temporary objects
			TextureFormat formatRGB = CaptureExtension == Extension.EXR ? TextureFormat.RGBAFloat : TextureFormat.RGB24;
			TextureFormat formatRGBA = CaptureExtension == Extension.EXR ? TextureFormat.RGBAFloat : TextureFormat.ARGB32;
			RenderTexture target = RenderTexture.GetTemporary(CaptureWidth, CaptureHeight, 24);
			Texture2D capture = null;

			if (CaptureTransparent && CaptureExtension != Extension.JPG)
			{

				// Remember current settings
				object preHDRPClearColorMode = HDRPCameraInstance == null ? null : HDRPCameraClearMode == null ? null : HDRPCameraClearMode.GetValue(HDRPCameraInstance);
				object preHDRPBackgroundColorHDR = HDRPCameraInstance == null ? null : HDRPCameraBackgroundColorHDR == null ? null : HDRPCameraBackgroundColorHDR.GetValue(HDRPCameraInstance);
				CameraClearFlags preClearFlags = CaptureCamera.clearFlags;
				RenderTexture preTargetTexture = CaptureCamera.targetTexture;
				RenderTexture preActiveTexture = RenderTexture.active;
				Color preBackgroundColor = CaptureCamera.backgroundColor;

				// Modify current settings
				if (HDRPCameraInstance != null && HDRPCameraClearMode != null) HDRPCameraClearMode.SetValue(HDRPCameraInstance, HDRPClearModeColor);
				CaptureCamera.clearFlags = CameraClearFlags.Color;
				CaptureCamera.targetTexture = target;
				RenderTexture.active = target;

				// Capture screenshot with black background
				if (HDRPCameraInstance != null && HDRPCameraBackgroundColorHDR != null) HDRPCameraBackgroundColorHDR.SetValue(HDRPCameraInstance, Color.black);
				CaptureCamera.backgroundColor = Color.black;
				CaptureCamera.Render();
				Texture2D captureBlack = new Texture2D(CaptureWidth, CaptureHeight, formatRGB, false);
				captureBlack.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0, false);
				captureBlack.Apply();

				// Capture screenshot with white background
				if (HDRPCameraInstance != null && HDRPCameraBackgroundColorHDR != null) HDRPCameraBackgroundColorHDR.SetValue(HDRPCameraInstance, Color.white);
				CaptureCamera.backgroundColor = Color.white;
				CaptureCamera.Render();
				Texture2D captureWhite = new Texture2D(CaptureWidth, CaptureHeight, formatRGB, false);
				captureWhite.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0, false);
				captureWhite.Apply();

				// Create capture with transparency
				capture = new Texture2D(CaptureWidth, CaptureHeight, formatRGBA, false);
				for (int x = 0; x < CaptureWidth; ++x)
				{
					for (int y = 0; y < CaptureHeight; ++y)
					{
						Color lColorWhenBlack = captureBlack.GetPixel(x, y);
						Color lColorWhenWhite = captureWhite.GetPixel(x, y);
						float alpha = 1 + lColorWhenBlack.r - lColorWhenWhite.r;
						Color color = new Color(lColorWhenBlack.r / alpha, lColorWhenBlack.g / alpha, lColorWhenBlack.b / alpha, alpha);
						capture.SetPixel(x, y, color);
					}
				}
				capture.Apply();

				// Cleanup
				DestroyImmediate(captureBlack);
				DestroyImmediate(captureWhite);

				// Revert settings
				if (HDRPCameraInstance != null && HDRPCameraBackgroundColorHDR != null) HDRPCameraBackgroundColorHDR.SetValue(HDRPCameraInstance, preHDRPBackgroundColorHDR);
				if (HDRPCameraInstance != null && HDRPCameraClearMode != null) HDRPCameraClearMode.SetValue(HDRPCameraInstance, preHDRPClearColorMode);
				RenderTexture.active = preActiveTexture;
				CaptureCamera.targetTexture = preTargetTexture;
				CaptureCamera.backgroundColor = preBackgroundColor;
				CaptureCamera.clearFlags = preClearFlags;

			}
			else
			{

				// Remember current settings
				RenderTexture preTargetTexture = CaptureCamera.targetTexture;
				RenderTexture preActiveTexture = RenderTexture.active;

				// Modify current settings
				CaptureCamera.targetTexture = target;
				RenderTexture.active = target;

				// Capture screenshot without transparency
				CaptureCamera.Render();
				capture = new Texture2D(CaptureWidth, CaptureHeight, formatRGB, false);
				capture.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0, false);
				capture.Apply();

				// Revert settings
				RenderTexture.active = preActiveTexture;
				CaptureCamera.targetTexture = preTargetTexture;

			}

			// Encode texture to data
			byte[] data = Encode(capture, CaptureExtension);
			if (data != null)
			{

				// Create filename path
				CaptureLast = GetRenamedFilename(Path.Combine(CapturePath, GetFilename()));
				string filename = Path.GetFileName(CaptureLast);

				// Write data to path
				using (FileStream stream = new FileStream(CaptureLast, FileMode.Create))
				{
					BinaryWriter writer = new BinaryWriter(stream);
					writer.Write(data);
				}

				// Log
				Debug.Log("Screenshot '" + filename + "' saved to '" + CapturePath + "'", this);

			}

			// Cleanup
			DestroyImmediate(capture);
			RenderTexture.ReleaseTemporary(target);

		}

		private string GetRenamedFilename(string filename)
		{
			int count = 1;
			string file = Path.GetFileNameWithoutExtension(filename);
			string extension = Path.GetExtension(filename);
			string path = Path.GetDirectoryName(filename);
			string renamed = filename;
			while (File.Exists(renamed))
			{
				string temp = string.Format("{0} ({1})", file, count++);
				renamed = Path.Combine(path, temp + extension);
			}
			return renamed;
		}

		private string GetFilename()
		{
			string extension = "." + CaptureExtension.ToString().ToLower();
			if (FilenameCustomToggle && !string.IsNullOrEmpty(FilenameCustomValue))
			{
				if (FilenameCustomValue.ToLower().EndsWith(extension))
				{
					return FilenameCustomValue;
				}
				else
				{
					return FilenameCustomValue + extension;
				}
			}
			else
			{
				return DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + extension;
			}

		}

		private static byte[] Encode(Texture2D texture, Extension extension)
		{
			if (extension == Extension.TGA)
			{
				MethodInfo EncodeToTGA = typeof(ImageConversion).GetMethod("EncodeToTGA", BindingFlags.Static | BindingFlags.Public);
				if (EncodeToTGA == null)
				{
					Debug.LogError("TGA Encoder not found.");
					return null;
				}
				else
				{
					return (byte[])EncodeToTGA.Invoke(null, new object[] { texture });
				}
			}
			else
			{
				switch (extension)
				{
					default:
					case Extension.PNG: return texture.EncodeToPNG();
					case Extension.EXR: return texture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
					case Extension.JPG: return texture.EncodeToJPG();
				}
			}
		}

	}
}