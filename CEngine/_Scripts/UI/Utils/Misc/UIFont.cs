//------------------------------------------------------------------------------
// UFont.cs
// Created by CYM on 2022/5/23
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
namespace CYM.UI
{
    [HideMonoScript]
    public class UIFont : MonoBehaviour
    {
        [ValueDropdown("Inspector_FontPresets")]
        public string Type =nameof(FontType.None);

        #region Editor
        protected string[] Inspector_FontPresets()
        {
            List<string> data = new List<string>();
            data.Add(nameof(CYM.FontType.None));
            data.Add(nameof(CYM.FontType.Normal));
            data.Add(nameof(CYM.FontType.Title));
            data.Add(nameof(CYM.FontType.Dynamic));
            data.AddRange(UIConfig.Ins.ExtraFonts.Keys);
            return data.ToArray();
        }
        #endregion
    }
}