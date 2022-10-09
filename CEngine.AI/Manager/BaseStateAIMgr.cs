//------------------------------------------------------------------------------
// BaseStateAIMgr.cs
// Copyright 2021 2021/1/28 
// Created by CYM on 2021/1/28
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using CYM.AI.FStateMachine;

namespace CYM.AI
{
    public class BaseStateAIMgr<TData> : BaseTableDataAIMgr<TData>
        where TData : TDBaseSMData, new()
    {
        #region get
        public IState CurState
        {
            get
            {
                return CurData?.RootState?.CurState;
            }
        }
        #endregion

        #region set
        public void TriggerEvent(string name)
        {
            CurData?.TriggerEvent(name);
        }
        public void ChangeState(StrHash name)
        {
            CurData?.ChangeState(name);
        }
        #endregion

        #region is
        public bool IsHaveState(StrHash stateName)
        {
            return CurData.IsHaveState(stateName);
        }
        public bool IsInState(StrHash name)
        {
            return CurData.IsInState(name);
        }
        public bool IsInState(params StrHash[] name)
        {
            foreach (var item in name)
            {
                if (!IsInState(item))
                    return false;
            }
            return true;
        }
        public bool NotInState(params StrHash[] name)
        {
            foreach (var item in name)
            {
                if (IsInState(item))
                    return false;
            }
            return true;
        }
        public bool IsInOneState(params StrHash[] name)
        {
            foreach (var item in name)
            {
                if (IsInState(item))
                    return true;
            }
            return false;
        }

        #endregion
    }


}