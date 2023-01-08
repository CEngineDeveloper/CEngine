//------------------------------------------------------------------------------
// GlobalUITextMgr.cs
// Copyright 2020 2020/7/19 
// Created by CYM on 2020/7/19
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM.UI;
using System.Collections.Generic;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace CYM
{
    [HideMonoScript]
    public sealed class GlobalUITextMgr : MonoBehaviour 
    {
        #region font text
        static Dictionary<Text, string> Texts = new Dictionary<Text, string>();
        static Dictionary<Text, int> TextSizes = new Dictionary<Text, int>();
        public static void AddText(Text text, string fontType)
        {
            UIFont uiFont = text.GetComponent<UIFont>();
            if (uiFont != null)
                fontType = uiFont.Type;
            FontStyleData fontStyleData = UIConfig.Ins.GetFontData(fontType);
            if (fontStyleData == null)
                return;
            if (!Texts.ContainsKey(text))
            {
                Texts.Add(text, fontType);
                TextSizes.Add(text,text.fontSize);
            }
            text.resizeTextForBestFit = false;
            text.fontStyle = FontStyle.Normal;
            var newFont = fontStyleData.Get(BaseLangMgr.LanguageType);
            if (text.font != newFont)
            {
                text.font = newFont;
                text.fontSize = (int)(TextSizes[text] * fontStyleData.Size);
            }
        }
        public static void RemoveText(Text text)
        {
            Texts.Remove(text);
        }
        public static void RefreshFont()
        {
            foreach (var item in Texts)
            {
                if (item.Key == null) 
                    continue;
                FontStyleData fontStyleData = UIConfig.Ins.GetFontData(item.Value);
                if (fontStyleData == null)
                    return;
                if (fontStyleData.Size == 0)
                {
                    CLog.Error("fontStyleData size == {0}");
                    return;
                }
                var newFont = fontStyleData.Get(BaseLangMgr.LanguageType);
                if (item.Key.font != newFont)
                {
                    item.Key.font = newFont;
                    item.Key.fontSize = (int)(TextSizes[item.Key] * fontStyleData.Size);
                }

            }
        }
        #endregion
    }
}