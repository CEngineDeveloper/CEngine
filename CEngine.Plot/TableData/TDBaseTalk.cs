
using System;
using System.Collections.Generic;
using UnityEngine;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM.Plot
{
    [Serializable]
    public class TalkFragment : TDBaseData
    {
        // 对话的方向
        public TalkType Type { get; set; } = TalkType.Left;
        // 语音
        public string Audio { get; set; } = SysConst.STR_None;

        #region Prop
        public int TalkIndex { get; set; } = 0;
        public string TalkId { get; set; }
        public bool IsLasted { get; set; } = false;
        public string TalkDescId { get; set; }
        #endregion

        public override string GetName()
        {
            return BaseLangMgr.Get(Name);
        }
        public override string GetDesc(params object[] ps)
        {
            if (!Desc.IsInv())
                return BaseLangMgr.Get(Desc, ps);
            return BaseLangMgr.Get(TalkDescId, ps);
        }
        public override Sprite GetIcon()
        {
            if (!Icon.IsInv())
                return BaseGlobal.RsIcon.Get(Icon);
            return BaseGlobal.RsIcon.Get(Name);
        }
    }
    [Serializable]
    public class TDBaseTalkData : TDBaseData
    {
        public const int Val_MaxTalkOptionCount = 4;
        public List<TalkFragment> Fragments { get; set; } = new List<TalkFragment>();

        // 选项
        public List<string> Option { get; set; } = new List<string>();

        // 是否有选项
        public bool IsHaveOption()
        {
            if (Option == null)
                return false;
            if (Option.Count == 0)
                return false;
            return true;
        }

        // 获得Option
        public string GetOption(int index)
        {
            if (Option.Count <= 0)
                return SysConst.STR_Inv;
            if (index < 0)
                return Option[0];
            if (index >= Option.Count)
                return Option[Option.Count - 1];
            return Option[index];
        }
        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            int index = 0;
            TalkFragment Lasted = null;
            foreach (var item in Fragments)
            {
                item.TalkIndex = index;
                index++;
                item.TalkId = TDID;
                Lasted = item;
                item.TalkDescId = item.TalkId + "_" + item.TalkIndex;
            }
            if (Lasted != null)
            {
                Lasted.IsLasted = true;
            }
            //获取Op
            for (int i = 0; i < Val_MaxTalkOptionCount; i++)
            {
                string opKey = TDID + SysConst.Suffix_Op + "_" + i;
                if (BaseLangMgr.IsContain(opKey))
                {
                    Option.Add(opKey);
                }
            }
        }
    }

}