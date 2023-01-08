//------------------------------------------------------------------------------
// TDBaseEvent.cs
// Copyright 2019 2019/8/4 
// Created by CYM on 2019/8/4
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace CYM.Unit
{
    [Serializable]
    public class EventOption
    {
        #region config
        public BaseReward Reward1 { get; set; }
        public BaseReward Reward2 { get; set; }
        public BaseReward Reward3 { get; set; }
        #endregion

        #region prop
        public int Index { get; set; }
        #endregion

        public List<BaseReward> Rewards { get; private set; }

        public void Init()
        {
            Rewards = new List<BaseReward>();
            if (Reward1 != null) Rewards.Add(Reward1);
            if (Reward2 != null) Rewards.Add(Reward2);
            if (Reward3 != null) Rewards.Add(Reward3);
        }
    }
    [Serializable]
    public class TDBaseEventData : TDBaseData
    {
        #region Config
        // 奖励
        public EventOption Option1 { get; set; }
        public EventOption Option2 { get; set; }
        public EventOption Option3 { get; set; }
        public EventOption Option4 { get; set; }
        public BaseTarget Target1 { get; set; }
        public BaseTarget Target2 { get; set; }
        public BaseTarget Target3 { get; set; }
        public BaseTarget Target4 { get; set; }
        // 出现条件
        // 事件的触发概率
        public float Prob { get; set; } = 0.35f;
        // 事件冷却CD
        public int CD { get; set; } = 10;
        #endregion

        #region prop
        public List<BaseTarget> Targets { get;private set; } = new List<BaseTarget>();
        public List<EventOption> Options { get; private set; } = new List<EventOption>();
        #endregion

        #region life
        public virtual void DoReward(List<BaseReward> datas) => SelfBaseUnit?.AttrMgr?.DoReward(datas);
        public override void OnBeAdded(BaseCoreMono selfMono, params object[] obj)
        {
            base.OnBeAdded(selfMono, obj);
            AudioMgr.PlayPlUI(SFX, SelfBaseUnit);
            foreach (var item in Options)
            {
                foreach (var reward in item.Rewards)
                    reward.SetTarget(SelfBaseUnit);
            }
        }
        #endregion

        #region set
        public void DoSelOption(int index)
        {
            if (Options.Count == 0) return;
            if (index >= Options.Count) index = 0;
            DoReward(Options[index].Rewards);
        }
        #endregion

        #region get
        public string GetOpName(EventOption option)
        {
            string NameKey = TDID + SysConst.Suffix_Op + "_" + option.Index;
            return Util.GetStr(NameKey);
        }
        public string GetOpHintStr(EventOption option)
        {
            string finalStr = SysConst.Prefix_Lang_OptHintTrans + TDID + option.Index;
            if (!BaseLangMgr.IsContain(finalStr))
                finalStr = "CommonOptHint";
            return Util.GetStr(finalStr, Options[option.Index].Rewards.GetDesc());
        }
        public int GetMaxOption()
        {
            return Options.Count;
        }
        #endregion

        #region Callback
        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            if (Option1 != null) Options.Add(Option1);
            if (Option2 != null) Options.Add(Option2);
            if (Option3 != null) Options.Add(Option3);
            if (Option4 != null) Options.Add(Option4);

            if (Target1 != null) Targets.Add(Target1);
            if (Target2 != null) Targets.Add(Target2);
            if (Target3 != null) Targets.Add(Target3);
            if (Target4 != null) Targets.Add(Target4);

            int index = 0;
            foreach (var item in Options)
            {
                item.Init();
                item.Index = index;
                index++;
            }
        }
        #endregion
    }
}