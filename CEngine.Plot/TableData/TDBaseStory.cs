//------------------------------------------------------------------------------
// TDBaseStory.cs
// Copyright 2021 2021/10/22 
// Created by CYM on 2021/10/22
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM.Plot
{
    public class TDBaseStoryData : TDBaseData
    {
        #region lua config
        public Anchor Anchor { get; set; } = Anchor.None;
        public Vector2 Offset { get; set; } = Vector2.zero;
        public string Talk { get; set; } // 子对话
        #endregion

        public bool TryGetTalk(int index, out string name)
        {
            string newkey = "";
            if (Talk.IsInv()) newkey = Group + index;
            else newkey = Talk + index;
            if (!BaseLangMgr.IsContain(newkey))
            {
                name = "";
                return false;
            }
            name = GetStr(newkey);
            return true;
        }
    }
}