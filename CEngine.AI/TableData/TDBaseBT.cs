//**********************************************
// Class Name	: TDBaseBT
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using CYM.AI.BT;
using System;
using UnityEngine;

namespace CYM.AI
{
    [Serializable]
    public class TDBaseBTData : TDBaseData
    {
        #region 属性
        public AI.BT.Tree Tree { get;private set; }
        bool IsDirtyReset = false;
        #endregion

        #region 生命周期
        protected virtual void InitParam()
        {

        }
        public override void OnBeAdded(BaseCoreMono mono, params object[] obj)
        {
            base.OnBeAdded(mono, obj);
            InitParam();
            if (Tree == null)
            {
                RebiuldTree();
            }
            else
            {
                CLog.Error("没有移除:Tree");
            }
        }
        public override void OnBeRemoved()
        {
            base.OnBeRemoved();
            Tree = null;
        }
        public override void ManualUpdate()
        {
            base.ManualUpdate();
            Tree.Update();
            if (CheckNeedResetTree())
            {
                SetTreeDirty();
            }
            if (IsDirtyReset)
            {
                Tree.Reset();
                IsDirtyReset = false;
            }
        }
        #endregion

        #region set
        private void RebiuldTree()
        {
            var temp = CreateTreeNode();
            if (temp == null)
            {
                CLog.Error("没有创建BTTree!!!!");
                return;
            }
            Tree = new AI.BT.Tree(temp);
        }
        public void ResetTree()
        {
            Tree.Reset();
        }
        public void SetTreeDirty()
        {
            IsDirtyReset = true;
        }
        protected virtual Node CreateTreeNode()
        {
            return null;
        }
        #endregion

        #region is
        protected virtual bool CheckNeedResetTree()
        {
            return false;
        }
        #endregion
    }
}