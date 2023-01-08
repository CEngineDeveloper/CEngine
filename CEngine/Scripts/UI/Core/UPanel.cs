//------------------------------------------------------------------------------
// BaseContainer.cs
// Copyright 2019 2019/7/19 
// Created by CYM on 2019/7/19
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    //------------------------------------------------------------------------------
    //子窗口,类似窗口中的窗口
    //Panel是指较为复杂的控件集合,可以单独做成Prefab
    //UIview可以通过CreatePanel动态创建panel
    //Panel会自动添加到互斥组
    //会自动被添加到界面的Panel列表中
    //UIView会自动检测自生节点下的Panel,并且将第一个命名为“Main”的Panel添加到MainPanel中
    //和Control相比自带互斥,关闭回调
    //------------------------------------------------------------------------------
    [AddComponentMenu("UI/Control/UPanel")]
    public class UPanel : UControl
    {
        #region Inspector
        [FoldoutGroup("Inspector"), SerializeField]
        protected UButton BntClose;
        #endregion

        #region panel 组件互斥组,一次只能显示一个组件
        List<UMutexer> Mutexers { get; set; } = new List<UMutexer>();
        public UMutexer AddMutexer(bool isNeedReset, bool isShowOne, params UControl[] controls)
        {
            if (controls == null) return null;
            var temp = new UMutexer(controls, isNeedReset, isShowOne);
            Mutexers.Add(temp);
            foreach (var item in controls)
                StaticChilds.Remove(item);
            return temp;
        }
        #endregion

        #region life
        public override bool IsAutoInit => true;
        public override bool IsCollection => true;
        public override bool IsCanBeViewFetch => false;//不会被View获取，需要手动调用
        public override bool IsCanBeControlFetch => false;//不会被Control获取
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            BntClose?.Init(new UButtonData() { NameKey = "Bnt_Back", OnClick = OnClickClose });
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            foreach (var item in Mutexers)
                item.OnFixedUpdate();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            foreach (var item in Mutexers)
                item.OnUpdate();
        }
        public override void OnShow(bool isShow)
        {
            base.OnShow(isShow);
            if (isShow) { }
            else
            {
                foreach (var item in Mutexers)
                    item.TestReset();
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Mutexers.Clear();
        }
        #endregion

        #region dirty
        public override void SetDirtyShow()
        {
            base.SetDirtyShow();
            foreach (var item in Mutexers)
                item.SetDirtyShow();
        }
        public override void SetDirtyData()
        {
            base.SetDirtyData();
            foreach (var item in Mutexers)
                item.SetDirtyData();
        }
        public override void SetDirtyRefresh()
        {
            base.SetDirtyRefresh();
            foreach (var item in Mutexers)
                item.SetDirtyRefresh();
        }
        public override void SetDirtyAll()
        {
            base.SetDirtyAll();
            foreach (var item in Mutexers)
                item.SetDirtyAll();
        }
        #endregion

        #region set
        public override bool AddStaticChild(UControl child)
        {
            bool ret = base.AddStaticChild(child);
            if (ret) child.PPanel = this;
            return ret;
        }
        public void DettachFromPanelList()
        {
            PUIView?.RemovePanel(this);
        }
        #endregion

        #region Callback
        protected virtual void OnClickClose(UControl control, PointerEventData data) => Close();
        #endregion
    }
}