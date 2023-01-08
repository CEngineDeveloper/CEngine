//------------------------------------------------------------------------------
// BaseIMUIUtils.cs
// Copyright 2020 2020/7/10 
// Created by CYM on 2020/7/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public partial class IMUIUtil
    {
        const float slightlyGrayTone = 0.85f;
        const float lightGrayTone = 0.7f;
        const float grayTone = 0.55f;
        const float grayerTone = 0.4f;
        const float darkGrayTone = 0.25f;
        const float darkerGrayTone = 0.15f;

        public static readonly Color whiteColor = Color.white;
        public static readonly Color slightlyGrayColor = new Color(slightlyGrayTone, slightlyGrayTone, slightlyGrayTone, 1);
        public static readonly Color lightGrayColor = new Color(lightGrayTone, lightGrayTone, lightGrayTone, 1);
        public static readonly Color grayColor = new Color(grayTone, grayTone, grayTone, 1);
        public static readonly Color grayerColor = new Color(grayerTone, grayerTone, grayerTone, 1);
        public static readonly Color darkGrayColor = new Color(darkGrayTone, darkGrayTone, darkGrayTone, 1);
        public static readonly Color darkerGrayColor = new Color(darkerGrayTone, darkerGrayTone, darkerGrayTone, 1);
        public static readonly Color blackColor = Color.black;

        public static Texture2D whiteTexture;
        public static Texture2D slightlyGrayTexture;
        public static Texture2D lightGrayTexture;
        public static Texture2D grayTexture;
        public static Texture2D grayerTexture;
        public static Texture2D darkGrayTexture;
        public static Texture2D darkerGrayTexture;
        public static GUIStyle boxStyle;

        static GUIStyle _centeredButtonStyle;
        public static GUIStyle centeredButtonStyle
        {
            get
            {
                UpdateFont(_centeredButtonStyle);
                return _centeredButtonStyle;
            }
            set { _centeredButtonStyle = value; }
        }

        static GUIStyle _buttonStyle;
        public static GUIStyle buttonStyle
        {
            get
            {
                UpdateFont(_buttonStyle);
                return _buttonStyle;
            }
            set { _buttonStyle = value; }
        }

        static GUIStyle _textStyle;
        public static GUIStyle textStyle
        {
            get
            {
                UpdateFont(_textStyle);
                return _textStyle;
            }
            set { _textStyle = value; }
        }

        static GUIStyle _centeredTextStyle;
        public static GUIStyle centeredTextStyle
        {
            get
            {
                UpdateFont(_centeredTextStyle);
                return _centeredTextStyle;
            }
            set { _centeredTextStyle = value; }
        }

        static GUIStyle _rightTextStyle;
        public static GUIStyle rightTextStyle
        {
            get
            {
                UpdateFont(_rightTextStyle);
                return _rightTextStyle;
            }
            set { _rightTextStyle = value; }
        }

        static GUIStyle _inputStyle;
        public static GUIStyle inputStyle
        {
            get
            {
                UpdateFont(_inputStyle);
                _inputStyle.normal.textColor = Color.black;
                return _inputStyle;
            }
            set { _inputStyle = value; }
        }

        static IMUIUtil()
        {
            whiteTexture = Texture2D.whiteTexture;
            slightlyGrayTexture = CreateColorTexture(slightlyGrayColor);
            lightGrayTexture = CreateColorTexture(lightGrayColor);
            grayTexture = CreateColorTexture(grayColor);
            grayerTexture = CreateColorTexture(grayerColor);
            darkGrayTexture = CreateColorTexture(darkGrayColor);
            darkerGrayTexture = CreateColorTexture(darkerGrayColor);

            buttonStyle = new GUIStyle();
            buttonStyle.normal = new GUIStyleState() { background = whiteTexture };
            buttonStyle.hover = new GUIStyleState() { background = slightlyGrayTexture };
            buttonStyle.active = new GUIStyleState() { background = lightGrayTexture };
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.clipping = TextClipping.Clip;

            centeredButtonStyle = new GUIStyle(buttonStyle);
            centeredButtonStyle.alignment = TextAnchor.MiddleCenter;

            boxStyle = new GUIStyle();
            boxStyle.normal = new GUIStyleState() { background = whiteTexture };

            textStyle = new GUIStyle();
            textStyle.normal = new GUIStyleState() { textColor = slightlyGrayColor };
            textStyle.wordWrap = true;
            textStyle.richText = true;
            textStyle.clipping = TextClipping.Clip;

            centeredTextStyle = new GUIStyle(textStyle);
            centeredTextStyle.alignment = TextAnchor.MiddleCenter;

            rightTextStyle = new GUIStyle(textStyle);
            rightTextStyle.alignment = TextAnchor.UpperRight;

            inputStyle = new GUIStyle();
            inputStyle.alignment = TextAnchor.MiddleLeft;
            inputStyle.clipping = TextClipping.Clip;
        }

        static Texture2D CreateColorTexture(Color color)
        {
            return ApplyColorToTexture(whiteTexture, color);
        }

        public static Texture2D ApplyColorToTexture(Texture2D texture, Color color)
        {
            Texture2D copyTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            Color[] pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            copyTexture.SetPixels(pixels);
            copyTexture.Apply();
            return copyTexture;
        }

        static void UpdateFont(GUIStyle style)
        {
            //style.font = font.
            style.fontSize = 12;
        }

        public static bool DrawButton(Rect rect, GUIContent content, Color color)
        {
            return DrawButton(rect, content, color, buttonStyle);
        }

        public static bool DrawCenteredButton(Rect rect, GUIContent content, Color color)
        {
            return DrawButton(rect, content, color, centeredButtonStyle);
        }

        public static bool DrawButton(Rect rect, GUIContent content, Color color, GUIStyle style)
        {
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            bool result = GUI.Button(rect, content, style);
            GUI.backgroundColor = oldColor;
            return result;
        }

        public static bool DrawRepeatButton(Rect rect, GUIContent content, Color color)
        {
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            bool result = GUI.RepeatButton(rect, content, centeredButtonStyle);
            GUI.backgroundColor = oldColor;
            return result;
        }

        public static void DrawBox(Rect rect, Color color)
        {
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(rect, GUIContent.none, boxStyle);
            GUI.backgroundColor = oldColor;
        }
    }
}