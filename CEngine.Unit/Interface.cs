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
        // ƴ�����д����buff addtion ���ַ���
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
        //buff �ϲ�����
        BuffMergeType MergeType { get;} 
        //�Զ���buff���id,���ȼ�����BuffMergeType
        string BuffGroupID { get;}//������Buff��Ļ����,û������Buff��Ļ�ϲ�
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
        //����ʱ��MaxTime,RunTime MaxTime,���ֵ��������ֵ,�ǿ���ʵʱ�仯��
        float RTMaxTime { get; }
        #endregion

        #region is
        // buffʱ���Ƿ����
        bool IsTimeOver { get; }
        // �Ƿ�����
        public bool IsForever { get; }
        public bool IsHaveRTMaxTime { get; }
        public bool IsHaveMaxTime { get; }
        #endregion

        #region set
        // ���������޸�����
        void SetInput(float input);
        void SetValid(bool b);
        void SetCD(float cd);
        void SetRTMaxTime(float maxTime);
        #endregion

        #region get
        // ��ȡbuff�ļӳ������б� ����
        List<string> GetAdtStrs(float? inputVal = null);
        // ͨ��Layer ��üӳ��б�
        List<string> GetAdtStrsByLayer(int layer = 1);
        // ���buff�ļӳ������ַ������
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
    /// ������߼�������
    /// </summary>
    public enum ImmuneGainType
    {
        All = 0,
        Negative = 1,//����
        Positive = 2,//����
    }
    /// <summary>
    /// Ч������������
    /// </summary>
    public enum ImmuneType
    {
        All = 0,
        Normal = 1, //һ������:�չ�
        Magic = 2,  //ħ������:����
    }
    #endregion
}