//------------------------------------------------------------------------------
// BaseTableDataAIMgr.cs
// Copyright 2021 2021/1/30 
// Created by CYM on 2021/1/30
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM.AI
{
    public class BaseTableDataAIMgr<TData> : BaseAIMgr
        where TData : TDBaseData, new()
    {
        #region prop
        ITDConfig ITDConfig;
        #endregion

        #region life
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
        }
        public override void OnRealDeath()
        {
            Remove();
            base.OnRealDeath();
        }
        #endregion

        #region Manual Update
        protected void ManualUpdateAI()
        {
            if (!IsActiveAI) return;
            if (CurData != null)
            {
                CurData.ManualUpdate();
            }
        }
        #endregion

        #region set
        /// <summary>
        /// 改变行为树
        /// </summary>
        public virtual void Change(string smKey)
        {
            if (smKey.IsInv())
                return;
            Remove();
            TData tempData = ITDConfig.Get<TData>(smKey);
            if (tempData != null)
            {
                CurData = tempData.Copy<TData>();
                CurData.OnBeAdded(SelfBaseUnit);
            }
            else
            {
                CLog.Error("错误,没有这个类型的AI:{0}", smKey);
            }
        }
        /// <summary>
        /// 移除
        /// </summary>
        protected virtual void Remove()
        {
            if (CurData != null)
                CurData.OnBeRemoved();
            CurData = null;
        }

        /// <summary>
        /// 是否拥有
        /// </summary>
        /// <returns></returns>
        public virtual bool IsHave()
        {
            return CurData != null;
        }
        #endregion

        #region must override
        public TData CurData { get; private set; }
        #endregion
    }

}