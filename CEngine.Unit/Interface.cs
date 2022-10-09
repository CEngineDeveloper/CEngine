using CYM.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Unit
{
    public interface IAttrMgr
    {
        void DoCost<TCostType>(List<Cost<TCostType>> datas, bool isReverse = false) where TCostType : Enum;
        void DoReward(List<BaseReward> rewards, bool isReverse = false);
        float GetVal(int type);
    }
    public interface IBuffMgr
    {
        ITDBuffData AddBase(string buffName);
        void Add(List<string> buffName);
        void Remove(List<string> buffName);
        void Remove(string buffName, RemoveBuffType type = RemoveBuffType.Once);

        string GetTableDesc(List<string> ids, bool newLine = false, string split = SysConst.STR_Indent, float? anticipationFaction = null, bool appendHeadInfo = false);
        // 拼接所有传入的buff addtion 的字符窜
        string GetTableDesc(string id, bool newLine = false, string split = SysConst.STR_Indent, float? inputVal = null, bool appendHeadInfo = false);
    }
    public interface IHUDMgr
    {
        THUD SpawnDurableHUD<THUD>(string prefabName, BaseUnit target = null) where THUD : UHUDBar;
        UHUDText JumpChatBubbleStr(string str);
        UHUDText JumpChatBubble(string key);
    }
    public interface ITDBuffData: ITDBaseData
    {
        #region config
        int MaxLayer { get; }
        bool IsHide { get; }
        float MaxTime { get; }
        int IntervalTime { get; }
        ImmuneGainType Immune { get; }
        float InputValStart { get;}
        //buff 合并类型
        BuffMergeType MergeType { get;} 
        //自定义buff组的id,优先级高于BuffMergeType
        string BuffGroupID { get;}//有配置Buff组的会叠加,没有配置Buff组的会合并
        List<string> Performs { get; }
        #endregion

        #region life
        void OnMerge(ITDBuffData newBuff, params object[] obj);
        #endregion

        #region runtime
        int MergeLayer { get; }
        float CurTime { get; }
        float Input { get; }
        float CurInterval { get; }
        bool Valid { get; }
        float PercentCD { get; }
        string PercentCDStr { get; }
        //运行时的MaxTime,RunTime MaxTime,这个值不是配置值,是可以实时变化的
        float RTMaxTime { get; }
        #endregion

        #region is
        // buff时间是否结束
        bool IsTimeOver { get; }
        // 是否永久
        public bool IsForever { get; }
        public bool IsHaveRTMaxTime { get; }
        public bool IsHaveMaxTime { get; }
        #endregion

        #region set
        // 设置属性修改因子
        void SetInput(float input);
        void SetValid(bool b);
        void SetCD(float cd);
        void SetRTMaxTime(float maxTime);
        #endregion

        #region get
        // 获取buff的加成描述列表 翻译
        List<string> GetAdtStrs(float? inputVal = null);
        // 通过Layer 获得加成列表
        List<string> GetAdtStrsByLayer(int layer = 1);
        // 获得buff的加成描述字符串组合
        string GetAdtDesc(bool isNewLine = true, string splite = SysConst.STR_Indent);
        string GetAdtDescH();
        string GetAdtDescByLayer(int layer = 1, bool isNewLine = false, string splite = SysConst.STR_Indent);
        string GetTipInfo();
        #endregion
    }
    public interface IBuffGroup
    {
        int Layer { get; }
        int MaxLayer { get; set; }
        ITDBuffData IBuff { get; }
        Sprite GetIcon();
    }
    public interface IAttrAdditon
    {
        AttrOpType AddType { get; set; }
        AttrFactionType FactionType { get; set; }
        float Val { get; set; }
        float Faction { get; set; }
        float Step { get; set; }
        float InputValStart { get; set; }
        float RealVal { get; }
        float InputVal { get; }
        float Min { get; }
        float Max { get; }
    }
    public interface IUpFactionData
    {
        UpFactionType FactionType { get; set; }
        float AnticipationVal(float? inputVal);
        float Val { get; }
        float RealVal { get; }
        float InputValStart { get; set; }
        float InputVal { get; }
        float Faction { get; set; }
        float Add { get; set; }
        float Percent { get; set; }
        string GetName();
        Sprite GetIcon();
        string GetDesc(bool isHaveSign = false, bool isHaveColor = false, bool isHeveAttrName = true);
    }
    #region Imm
    /// <summary>
    /// 增益或者减益类型
    /// </summary>
    public enum ImmuneGainType
    {
        All = 0,
        Negative = 1,//负面
        Positive = 2,//正面
    }
    /// <summary>
    /// 效果的免疫类型
    /// </summary>
    public enum ImmuneType
    {
        All = 0,
        Normal = 1, //一般类型:普攻
        Magic = 2,  //魔法类型:技能
    }
    #endregion
}