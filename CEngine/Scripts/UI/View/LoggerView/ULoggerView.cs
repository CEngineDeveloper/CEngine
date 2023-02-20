//------------------------------------------------------------------------------
// BaseLoggerView.cs
// Copyright 2020 2020/2/29 
// Created by CYM on 2020/2/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.Pool;
using CYM.UI;
using UnityEngine;

namespace CYM.UI
{
    public class ULoggerView : UUIView
    {
        [SerializeField]
        bool IsInverseSort = false;
        [SerializeField]
        RectTransform DataBar;
        [SerializeField]
        UDupplicate DP_Points;
        [SerializeField]
        GameObject Prefab;
        [SerializeField]
        RectTransform StartPos;

        #region prop
        GOPool GOPool;
        HashList<BaseLogData> DatasSet = new HashList<BaseLogData>();
        HashList<ULogItem> ActiveItems = new HashList<ULogItem>();
        #endregion

        #region mgr
        BaseLoggerMgr LoggerMgr => BaseGlobal.LoggerMgr;
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
           
            GOPool = new GOPool(Prefab, DataBar);
            LoggerMgr.Callback_OnAddedLog += OnAddedLog;
            LoggerMgr.Callback_OnRemoveLog += OnRemoveLog;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            LoggerMgr.Callback_OnAddedLog -= OnAddedLog;
            LoggerMgr.Callback_OnRemoveLog -= OnRemoveLog;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateAlertPoint();
        }
        void UpdateAlertPoint()
        {
            int IndexPoint = 0;
            for (int i = 0; i < ActiveItems.Count; ++i)
            {
                var item = ActiveItems[i];
                if (!item.IsShow) continue;
                if (IndexPoint < DP_Points.GOCount)
                {
                    var lastIndex = IndexPoint;
                    if (IsInverseSort) lastIndex = DP_Points.GOCount - 1 - IndexPoint;
                    item.RectTrans.position = Vector3.LerpUnclamped(item.RectTrans.position, DP_Points.GOs[lastIndex].transform.position, 0.1f);
                    IndexPoint++;
                }
            }
        }
        #endregion

        #region set
        void Add(BaseLogData data)
        {
            if (DatasSet.Contains(data))
                return;
            GameObject go = GOPool.Spawn();
            ULogItem logItem = go.GetComponent<ULogItem>();
            logItem.CancleInit();
            logItem.Init(new UButtonData
            {
                OnShowActive = OnAlertShow,
            });
            logItem.SetID(data.ID);
            logItem.Show(true, true);
            logItem.NameText = data.GetDesc();
            logItem.RectTrans.position = StartPos.position;
            ActiveItems.Add(logItem);
            DatasSet.Add(data);
        }

        void Remove(BaseLogData data)
        {
            ULogItem tempLogItem = ActiveItems.Find((x) => { return x.ID == data.ID; });
            if (tempLogItem == null) return;
            tempLogItem.Show(false);
            ActiveItems.Remove(tempLogItem);
            DatasSet.Remove(data);
        }
        #endregion

        #region Callback
        private void OnAddedLog(BaseLogData arg1)
        {
            Add(arg1);
        }
        private void OnRemoveLog(BaseLogData arg1)
        {
            Remove(arg1);
        }
        private void OnAlertShow(UControl p, bool arg1)
        {
            if (arg1 == false)
            {
                p.GO.transform.position = StartPos.position;
                GOPool.Despawn(p.GO);
            }
        }
        #endregion
    }
}