using UnityEngine;
using UnityEditor;

namespace CYM
{

    /// <summary>
    /// Color picker window, makes color picking much easier.
    /// </summary>
    public class ColorPickerWindow : EditorWindow
	{
		static ColorPickerWindow window;
		protected Color color = Color.white;
		protected Color32 color32 = new Color32 ( 255, 255, 255, 255 );
		protected string hexCode = "FFFFFF";

		public static void ShowWindow ()
		{
			window = EditorWindow.GetWindow<ColorPickerWindow> ( "Color Picker" );
			window.minSize = new Vector2 ( 300f, 150f );
			window.Show ();
		}

        protected virtual void OnEnable()
        {
            this.color.r = EditorPrefs.GetFloat("ColorPicker.color.r", 1f);
            this.color.g = EditorPrefs.GetFloat("ColorPicker.color.g", 1f);
            this.color.b = EditorPrefs.GetFloat("ColorPicker.color.b", 1f);
            this.color.a = EditorPrefs.GetFloat("ColorPicker.color.a", 1f);
            this.color32 = this.color;
            this.hexCode = ColorUtility.ToHtmlStringRGB(this.color);
        }

        protected virtual void OnDisable()
        {
            EditorPrefs.SetFloat("ColorPicker.color.r", this.color.r);
            EditorPrefs.SetFloat("ColorPicker.color.g", this.color.g);
            EditorPrefs.SetFloat("ColorPicker.color.b", this.color.b);
            EditorPrefs.SetFloat("ColorPicker.color.a", this.color.a);
        }

        protected virtual void OnGUI()
        {
            if (window != null)
            {
                this.color = EditorGUILayout.ColorField("Color", this.color);
                if (GUI.changed)
                {
                    this.color32 = this.color;
                    this.hexCode = ColorUtility.ToHtmlStringRGB(this.color);
                }
                this.hexCode = EditorGUILayout.TextField("Hex Code", this.hexCode);
                if (GUI.changed)
                {
                    ColorUtility.TryParseHtmlString(this.hexCode, out this.color);
                }
                this.color32.r = (byte)EditorGUILayout.IntSlider("Red", this.color32.r, 0, 255);
                this.color32.g = (byte)EditorGUILayout.IntSlider("Green", this.color32.g, 0, 255);
                this.color32.b = (byte)EditorGUILayout.IntSlider("Blue", this.color32.b, 0, 255);
                this.color32.a = (byte)EditorGUILayout.IntSlider("Alpha", this.color32.a, 0, 255);
                if (GUI.changed)
                {
                    this.color = this.color32;
                    this.hexCode = ColorUtility.ToHtmlStringRGB(this.color);
                }
                EditorGUILayout.BeginHorizontal();
                string colorCode = EditorGUILayout.TextField(
                                       "Color Code",
                                       string.Format(
                                           "new Color ( {0}f, {1}f, {2}f, {3}f )",
                                           this.color.r,
                                           this.color.g,
                                           this.color.b,
                                           this.color.a));
                if (GUILayout.Button("Copy", GUILayout.Width(60f)))
                {
                    EditorGUIUtility.systemCopyBuffer = colorCode;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                string color32Code = EditorGUILayout.TextField(
                                         "Color32 Code",
                                         string.Format(
                                             "new Color32 ( {0}, {1}, {2}, {3} )",
                                             this.color32.r,
                                             this.color32.g,
                                             this.color32.b,
                                             this.color32.a));
                if (GUILayout.Button("Copy", GUILayout.Width(60f)))
                {
                    EditorGUIUtility.systemCopyBuffer = color32Code;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Made with ❤️ by Yiming Games", EditorStyles.centeredGreyMiniLabel);
            }
        }

    }
	
}