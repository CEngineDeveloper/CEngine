//------------------------------------------------------------------------------
// PlanAction.cs
// Copyright 2020 2020/5/14 
// Created by CYM on 2020/5/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

namespace CYM.AI.Planner
{
    public class PlanAction<TUnit> 
        where TUnit : BaseUnit 
    {
        #region prop
        protected TUnit SelfUnit { get; private set; }
        protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        #endregion

        #region set
        public void Init(TUnit unit)
        {
            SelfUnit = unit;
        }
        public bool Do()
        {
            var ret = OnDo();
            if (SysConsole.Ins.IsOnlyPlayerAI)
                OnLog(ret);
            return ret;
        }
        protected void Log(string str, params object[] obj)
        {
            if (SysConsole.Ins.IsOnlyPlayerAI)
                CYM.CLog.Cyan(str, obj);
        }
        #endregion

        #region is
        public virtual bool IsValid()
        {
            return true;
        }
        #endregion

        #region Callback
        protected virtual bool OnDo()
        {
            return true;
        }
        protected virtual void OnLog(bool b)
        {

        }
        #endregion
    }
}