//------------------------------------------------------------------------------
// UIConfig.cs
// Copyright 2018 2018/11/29 
// Created by CYM on 2018/11/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Video;

namespace CYM
{
    [Serializable]
    public class LogoData
    {
        public LogoType Type = LogoType.Image;
        [HideIf("IsVideo")]
        public Color BGColor = Color.black;
        [HideIf("IsVideo")]
        public Sprite Logo;
        [HideIf("IsImage")]
        public VideoClip Video;
        [HideIf("IsVideo")]
        public float WaitTime = 1.0f;
        [HideIf("IsVideo")]
        public float InTime = 0.5f;
        [HideIf("IsVideo")]
        public float OutTime = 0.5f;

        public bool IsImage()
        {
            return Type == LogoType.Image;
        }
        public bool IsVideo()
        {
            return Type == LogoType.Video;
        }
    }
    [Serializable]
    public class PresenterStateColor
    {
        public Color Normal = Color.white;
        public Color Enter = Color.white;
        public Color Press = Color.white;
        public Color Disable = Color.white;
        public Color Selected = Color.white;

        public PresenterStateColor()
        { 
        
        }
        public PresenterStateColor(PresenterStateColor self,Color sourceCol)
        {
            Normal = self.Normal;
            Enter = self.Enter;
            Press = self.Press;
            Disable = self.Disable;
            Selected = self.Selected;

            Normal *= sourceCol;
            Enter *= sourceCol;
            Press *= sourceCol;
            Disable *= sourceCol;
            Selected *= sourceCol;            
        }
    }
    [Serializable]
    public sealed class FontStyleData
    {
        public float Size = 1.0f;
        public Font Default;
        public SerializableDic<LanguageType, Font> Override = new SerializableDic<LanguageType, Font>();
        public Font Get(LanguageType lang)
        {
            if (Override.ContainsKey(lang)) 
                return Override[lang];
            else 
                return Default;
        }
    }
    public sealed class UIConfig : ScriptableObjectConfig<UIConfig>
    {
        #region prop
        public bool IsEditorMode()
        {
            if (Application.isEditor)
                return IsShowLogo;
            return true;
        }
        #endregion

        #region inspector
        [FoldoutGroup("Resulution"), SerializeField]
        public int Width = 1920;
        [FoldoutGroup("Resulution"), SerializeField]
        public int Height = 1080;

        [FoldoutGroup("Fonts"), SerializeField]
        public FontStyleData DynamicFont;
        [FoldoutGroup("Fonts"), SerializeField]
        public FontStyleData NormalFont;
        [FoldoutGroup("Fonts"), SerializeField]
        public FontStyleData TitleFont;
        [FoldoutGroup("Fonts"), SerializeField]
        public Dictionary<string, FontStyleData> ExtraFonts = new Dictionary<string, FontStyleData>();

        public Dictionary<string, FontStyleData> AllFonts { get; private set; } = new Dictionary<string, FontStyleData>();

        [FoldoutGroup("Logo"), SerializeField]
        public bool IsShowLogo;
        [FoldoutGroup("Logo"), SerializeField]
        public Sprite StartLogo;
        [FoldoutGroup("Logo"), SerializeField]
        public List<LogoData> Logos = new List<LogoData>();

        [FoldoutGroup("Progress"), SerializeField]
        public float ProgressWidth = 100;
        [FoldoutGroup("Progress"), SerializeField]
        public float ProgressHeight = 5;
        [FoldoutGroup("Progress"), SerializeField]
        public Texture2D ProgressBG;
        [FoldoutGroup("Progress"), SerializeField]
        public Texture2D ProgressFill;

        [FoldoutGroup("Audio"), SerializeField]
        public AudioClip Error;
        #endregion

        #region Misc
        [FoldoutGroup("Misc"), SerializeField]
        public PresenterStateColor ButtonPresenterStateColor = new PresenterStateColor();
        [FoldoutGroup("Misc"), SerializeField]
        public PresenterStateColor TextPresenterStateColor = new PresenterStateColor();
        [FoldoutGroup("Misc"), SerializeField]
        public Dictionary<string, PresenterStateColor> CustomStateColors = new Dictionary<string, PresenterStateColor>();
        [FoldoutGroup("Misc"), ReadOnly, SerializeField, HideInInspector]
        public Dictionary<string, PresenterStateColor> PresenterStateColors = new Dictionary<string, PresenterStateColor>();
        [FoldoutGroup("Misc"), SerializeField]
        public List<SpriteConfig> SpriteGroupConfigs;
        #endregion

        #region get
        public PresenterStateColor GetStateColor(string key)
        {
            if (CustomStateColors.ContainsKey(key))
                return CustomStateColors[key];
            if (PresenterStateColors.ContainsKey(key))
                return PresenterStateColors[key];
            CLog.Error("没有这个StateColor:{0}", key);
            return new PresenterStateColor();
        }
        public Font GetFont(string key,LanguageType language = LanguageType.Chinese)
        {
            if (!AllFonts.ContainsKey(key))
                return null;
            FontStyleData data = AllFonts[key];
            return data.Get(language);
        }
        public FontStyleData GetFontData(string key)
        {
            if (!AllFonts.ContainsKey(key))
                return null;
            return AllFonts[key];
        }
        #endregion

        #region life
        public override void OnInited()
        {
            base.OnInited();
            PresenterStateColors["Text"] = TextPresenterStateColor;
            PresenterStateColors["Button"] = ButtonPresenterStateColor;
            PresenterStateColors[SysConst.STR_Custom] = new PresenterStateColor();

            AllFonts.Clear();
            AllFonts[nameof(FontType.Normal)] = NormalFont;
            AllFonts[nameof(FontType.Title)] = TitleFont;
            AllFonts[nameof(FontType.Dynamic)] = DynamicFont;
            foreach (var item in ExtraFonts)
            {
                AllFonts.Add(item.Key,item.Value);
            }
        }

        #endregion
    }
}