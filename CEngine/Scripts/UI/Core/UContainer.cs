//------------------------------------------------------------------------------
// Container.cs
// Copyright 2019 2019/8/20 
// Created by CYM on 2019/8/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CYM.UI
{
    //Control 容器,单纯的容器,不具备窗口的概念
    [AddComponentMenu("UI/Control/UContainer")]
    public class UContainer : UControl
    {
        #region Inspector
        [FoldoutGroup("Inspector"), SerializeField]
        UButton BntClose;
        #endregion

        public override bool IsCollection => true;
        public override bool IsCanBeControlFetch => false; //不会被Control获取
        public override bool IsAutoInit => true;
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            BntClose?.Init(new UButtonData { OnClick = (x, y) => Close() });
        }
        public override bool AddStaticChild(UControl child)
        {
            var ret = base.AddStaticChild(child);
            if (ret)
            {
                child.PContainer = this;
            }
            return ret;
        }
    }
}