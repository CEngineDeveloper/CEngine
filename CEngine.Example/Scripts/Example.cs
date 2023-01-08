//------------------------------------------------------------------------------
// GlobalTutorial.cs
// Copyright 2022 2022/11/5 
// Created by CYM on 2022/11/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
namespace CYM.Example
{
    public class Example : BaseGlobalT<BaseUnit, SysConfig,DBBaseSettings, Example> 
    {
        protected override void OnAttachComponet()
        {
            base.OnAttachComponet();
            AddComponent<ExampleUIMgr>();
        }
    }
}