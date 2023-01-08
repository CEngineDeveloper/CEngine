using CYM.Pool;
using System;
using System.Collections.Generic;
using UnityEngine;
//**********************************************
// Class Name	: CYMUIBaseHUD
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM.UI
{
    public class UHUDView : UUIView
    {
        #region member variable
        protected PoolItem spawnPool => BaseGlobal.PoolHUD;
        public HashList<UHUDItem> Data { get; protected set; } = new HashList<UHUDItem>();
        private List<UHUDItem> ClearList = new List<UHUDItem>();
        private List<UHUDItem> AddList = new List<UHUDItem>();
        #endregion

        #region mgr
        IBattleMgr<TDBaseBattleData> BattleMgr=> BaseGlobal.BattleMgr;
        ILevelMgr<TDBaseLevelData> SubBattleMgr=> BaseGlobal.LevelMgr;
        #endregion

        #region life
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            if (BattleMgr != null)
                BattleMgr.Callback_OnUnLoad += OnBattleUnLoad;
            if (SubBattleMgr != null)
                SubBattleMgr.Callback_OnUnLoad += OnSubBattleUnLoad;
        }

        public override void OnDestroy()
        {
            if (BattleMgr != null)
                BattleMgr.Callback_OnUnLoad -= OnBattleUnLoad;
            if (SubBattleMgr != null)
                SubBattleMgr.Callback_OnUnLoad -= OnSubBattleUnLoad;
            base.OnDestroy();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            foreach (var item in Data)
            {
                item.OnUpdate();
            }
            foreach (var item in AddList)
            {
                Data.Add(item);
            }
            foreach (var item in ClearList)
            {
                Data.Remove(item);
            }
            ClearList.Clear();
            AddList.Clear();
        }
        public override void Refresh()
        {
            base.Refresh();
            foreach (var item in Data)
                item.Refresh();
        }
        #endregion

        #region methon
        public UHUDText JumpText(GameObject prefab, BaseUnit unit,string text, Transform node = null)
        {
            UHUDText hudText = Jump(prefab, unit, node) as UHUDText;
            if (hudText == null)
            {
                CLog.Error("错误！JumpText的HUD对象必须继承UHUDText");
                return null;
            }
            hudText.SetText(text);
            return hudText;
        }
        public UHUDItem Jump(GameObject prefab, BaseUnit unit, Transform node = null)
        {
            if (prefab == null)
            {
                CLog.Error("没有这个prefab");
                return null;
            }

            if (spawnPool != null && spawnPool != null)
            {
                Transform temp = spawnPool.SpawnTrans(prefab, null, null, Trans);
                if (temp != null)
                {
                    UHUDItem tempItem = temp.GetComponent<UHUDItem>();
                    if (tempItem != null)
                    {
                        tempItem.Init(unit, node);
                        tempItem.OnLifeOver = OnLifeOver;
                        tempItem.PUIView = this;
                        AddList.Add(tempItem);
                    }
                    return tempItem;
                }
            }
            return null;
        }
        #endregion

        #region Callback
        void OnLifeOver(UHUDItem item,float delay)
        {
            if (item == null) return;
            if (item.GO == null) return;
            spawnPool.Despawn(item.GO, delay);
            ClearList.Add(item);
        }
        protected virtual void OnBattleUnLoad()
        {

        }
        protected virtual void OnSubBattleUnLoad()
        {

        }
        #endregion
    }
}