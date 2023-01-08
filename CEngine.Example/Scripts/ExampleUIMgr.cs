//------------------------------------------------------------------------------
// TutorialUIMgr.cs
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
    public class ExampleUIMgr : BaseMainUIMgr 
    {
        public static UExampleView TutorialView { get; private set; }

        protected override void OnCreateUIView1()
        {
            base.OnCreateUIView1();
            TutorialView = CreateView<UExampleView>("Editor:Assets/Plugins/CEngine.Example/UExampleView");
        }
    }
}