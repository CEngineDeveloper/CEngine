//------------------------------------------------------------------------------
// BaseUnitMgr.cs
// Copyright 2019 2019/1/20 
// Created by CYM on 2019/1/20
// Owner: CYM
// 单位组件的泛型基类
//------------------------------------------------------------------------------

namespace CYM
{
    public class BaseUFlowMgr<TUnit> : BaseMgr
        where TUnit : BaseUnit
    {
        #region prop
        public TUnit SelfUnit { get; private set; }
        #endregion

        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            SelfUnit = SelfBaseUnit as TUnit;
        }
        #endregion
    }
}