//------------------------------------------------------------------------------
// BaseLoginMgr.cs
// Copyright 2021 2021/3/12 
// Created by CYM on 2021/3/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

namespace CYM
{
    public class BaseLoginMgr : BaseGFlowMgr
    {
        //加载Login数据，专门提供给网游
        public void LoginInit(object data)
        {
            SelfBaseGlobal.OnLoginInit1(data);
            SelfBaseGlobal.OnLoginInit2(data);
            SelfBaseGlobal.OnLoginInit3(data);
        }
    }
}