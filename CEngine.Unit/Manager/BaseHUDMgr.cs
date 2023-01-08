//**********************************************
// Class Name	: BaseHUDMgr
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using CYM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace CYM.Unit
{
    public struct JumpFontData
    {
        public string Text;
        public string Prefab;
        public Color Color;
        public void SetText(string val)=> Text = val;
        public void SetPrefab(string val)=> Prefab = val;
        public void SetColor(Color col)=> Color = col;
    }
    public class BaseHUDMgr<T> : BaseMgr, IHUDMgr 
        where T : UHUDBar
    {
        #region must override text
        protected virtual string ChatBubble => "UTextBubble";
        protected virtual string DamageText => "UDamageJumpFont";
        protected virtual string TreatmentText => "UTreatmentJumpFont";
        protected virtual string StateJumpText => "UStateJumpText";
        protected virtual string BarItemPrefab => SysConst.STR_Inv;
        protected virtual UHUDView HUDView => BaseHUDUIMgr.CommonHUDView;
        protected virtual UHUDView JumpHUDView => BaseHUDUIMgr.CommonHUDView;
        protected virtual Vector3 JumpOffset => Vector3.zero;
        #endregion

        #region prop
        UHUDText CurChatBubble;
        protected List<JumpFontData> jumpList = new List<JumpFontData>();
        protected Timer jumpFontTimer = new Timer();
        protected float NextInterval = 0.0f;
        public T BarItem { get; protected set; }
        #endregion

        #region Life
        public sealed override MgrType MgrType => MgrType.All;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            if (IsGlobal)
            {
                BaseGlobal.LoaderMgr.Callback_OnAllLoadEnd2 += OnAllLoadEnd2;
                BaseGlobal.BattleMgr.Callback_OnLoaded += OnBattleLoaded;
                BaseGlobal.BattleMgr.Callback_OnUnLoaded += OnBattleUnLoaded;
            }
        }
        public override void OnBirth3()
        {
            base.OnBirth3();
            if (!BarItemPrefab.IsInv())
                BarItem = SpawnDurableHUD<T>(BarItemPrefab);
        }
        public override void OnDeath()
        {
            base.OnDeath();
            if (BarItem != null)
            {
                BarItem.DoDestroy();
                BarItem = null;
            }
        }
        public override void OnInit()
        {
            base.OnInit();
            jumpFontTimer.Restart();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            UpdateJumpFontList();
        }
        public override void OnDisable()
        {
            base.OnDisable();
            jumpList.Clear();
        }
        void UpdateJumpFontList()
        {
            if (jumpFontTimer.Elapsed() > NextInterval)
            {
                jumpFontTimer.Restart();
                if (jumpList.Count > 0)
                {
                    JumpFont(jumpList[0]);
                    var go = BaseGlobal.RsUI.Get(jumpList[0].Prefab);
                    var temp = go.GetComponent<UHUDItem>();
                    if (go != null && temp != null)
                    {
                        NextInterval = temp.LifeTime * 0.5f;
                    }
                    else
                    {
                        NextInterval = 0.5f;
                    }
                    jumpList.RemoveAt(0);
                }
            }
        }
        #endregion

        #region is
        protected virtual bool IsCanJump(bool needPlayer = false)
        {
            if (!SelfBaseUnit.IsPlayer() && needPlayer) return false;
            if (!BaseGlobal.IsUnReadData) return false;
            return true;
        }
        #endregion

        #region Jump font
        protected JumpFontData? AddToJumpFontList(string text, string prefab, Color col)
        {
            JumpFontData tempData = new JumpFontData();
            tempData.Text = text;
            tempData.Prefab = prefab;
            tempData.Color = col;
            jumpList.Add(tempData);
            return tempData;
        }
        private void JumpFont(JumpFontData data)
        {
            GameObject tempGO = BaseGlobal.RsUI.Get(data.Prefab);
            if (tempGO != null)
            {
                var temp = JumpHUDView.JumpText(tempGO, SelfBaseUnit, data.Text);
                temp.Color = data.Color;
                temp.InputOffset = JumpOffset;
                temp.SetFollowObj(SelfBaseUnit.GetNode(temp.NodeType));
            }
        }
        #endregion

        #region Jump damage 负面跳字
        public JumpFontData? JumpDamageStr(string str, [DefaultValue(nameof(Color.red))] Color col, bool needPlayer = false)
        {
            if (!IsCanJump(needPlayer)) return null;
            return AddToJumpFontList(str, DamageText, col);
        }
        public JumpFontData? JumpDamage(string key, [DefaultValue(nameof(Color.red))] Color col, bool needPlayer = false, params string[] objs) => JumpDamageStr(BaseLangMgr.Get(key, objs), col, needPlayer);
        public JumpFontData? JumpDamage(float val, bool needPlayer = false) => JumpDamageStr(UIUtil.RoundD(val), Color.red, needPlayer);
        public JumpFontData? JumpDamage(string key, bool needPlayer = false, params string[] objs) => JumpDamageStr(BaseLangMgr.Get(key, objs), Color.red, needPlayer);
        #endregion

        #region Jump treat 正面跳字
        public JumpFontData? JumpTreatStr(string str, [DefaultValue(nameof(Color.green))] Color col, bool needPlayer = false)
        {
            if (!IsCanJump(needPlayer)) return null;
            return AddToJumpFontList(str, TreatmentText, col);
        }
        public JumpFontData? JumpTreat(string key, [DefaultValue(nameof(Color.green))] Color col, bool needPlayer = false, params string[] objs) => JumpTreatStr(BaseLangMgr.Get(key, objs), col, needPlayer);
        public JumpFontData? JumpTreat(float val, bool needPlayer = false) => JumpTreatStr(UIUtil.RoundD(val), Color.green, needPlayer);
        public JumpFontData? JumpTreat(string key, bool needPlayer = false, params string[] objs) => JumpDamageStr(BaseLangMgr.Get(key, objs), Color.green, needPlayer);
        #endregion

        #region Jump state 状态
        public JumpFontData? JumpStateStr(string str, [DefaultValue(nameof(Color.white))] Color col, bool needPlayer = false)
        {
            if (!IsCanJump(needPlayer)) return null;
            return AddToJumpFontList(str, StateJumpText, col);
        }
        public JumpFontData? JumpStateStr(string str, bool needPlayer = false) => JumpStateStr(str,Color.white,needPlayer);
        public JumpFontData? JumpState(string key, [DefaultValue(nameof(Color.white))] Color col, bool needPlayer = false) => JumpStateStr(BaseLangMgr.Get(key), col, needPlayer);
        public JumpFontData? JumpState(string key, bool needPlayer = false) => JumpStateStr(BaseLangMgr.Get(key), Color.white, needPlayer);
        #endregion

        #region Chat bubble 聊天气泡
        public UHUDText JumpChatBubbleStr(string str)
        {
            SpawnTempHUD(ChatBubble,ref CurChatBubble);
            CurChatBubble.SetText(str);
            return CurChatBubble;
        }
        public UHUDText JumpChatBubble(string key)
        {
            if (key.IsInv())
                return null;
            return JumpChatBubbleStr(BaseLangMgr.Get(key));
        }
        #endregion

        #region Spawn
        //创建一个HUDitem,HUDitem会在单位死亡后会自动销毁(DoDestroy)
        public THUD SpawnDurableHUD<THUD>(string prefabName, BaseUnit target=null) 
            where THUD : UHUDBar
        {
            if (prefabName.IsInv()) return null;
            GameObject tempGO = BaseGlobal.RsUI.Get(prefabName);
            if (tempGO != null)
            {
                var temp = HUDView.Jump(tempGO, target==null?SelfBaseUnit:target);
                if (temp == null) return null;
                temp.SetFollowObj(SelfBaseUnit.GetNode(temp.NodeType));
                return (temp as THUD);
            }
            return null;
        }
        //创建临时性的HUDItem
        public void SpawnTempHUD<THUD>(string prefab,ref THUD hud)
            where THUD : UHUDItem
        {
            if (prefab.IsInv()) return;
            var newPrefab = hud;
            newPrefab?.DoDestroy(0.01f);
            GameObject tempGO = BaseGlobal.RsUI.Get(prefab);
            if (tempGO != null)
            {
                hud = JumpHUDView.Jump(tempGO, SelfBaseUnit) as THUD;
                if (hud == null) return;
                hud.SetFollowObj(SelfBaseUnit.GetNode(hud.NodeType));
                return;
            }
        }
        #endregion

        #region Callback
        protected virtual void OnAllLoadEnd2()
        {
            
        }
        protected virtual void OnBattleLoaded()
        {

        }
        protected virtual void OnBattleUnLoaded()
        {
            
        }
        #endregion
    }
}