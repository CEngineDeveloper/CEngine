//------------------------------------------------------------------------------
// USelectView.cs
// Copyright 2021 2021/10/10 
// Created by CYM on 2021/10/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
namespace CYM.UI
{
    public class USelectView : UUIView
    {
        UButtonData[] ButtonDatas ;
        UButtonData CloseButtonData = new UButtonData { IsShow = (x) => false };
        List<UButton> Buttons = new List<UButton>();

        [SerializeField]
        RectTransform DPRoot;
        GridLayoutGroup GridLayout;

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            for (int i = 0; i < DPRoot.childCount; ++i)
            {
                var item = DPRoot.GetChild(i);
                var buton = item.GetComponent<UButton>();
                if (buton == null)
                    return;
                Buttons.Add(buton);
            }
            GridLayout = DPRoot.GetComponent<GridLayoutGroup>();
            if (GridLayout == null)
            {
                CLog.Error("错误SelectView的DPRoot没有添加GridLayoutGroup组件");
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            SetDirtyLayout(DPRoot);
        }
        #endregion

        #region set
        public void Show(params UButtonData[] data)
        {
            ButtonDatas = data;
            for (int i = 0; i < Buttons.Count; ++i)
            {
                var control = Buttons[i];
                if (control == null)
                    continue;
                if (ButtonDatas.Length > i)
                {
                    var bntdata = ButtonDatas[i];
                    bntdata.OnClick += (x, y) => Close();
                    control.SetData(bntdata);
                }
                else
                {
                    control.SetData(CloseButtonData);
                }
            }
            base.Show();
        }
        public USelectView SetConstraintCount(int count)
        {
            GridLayout.constraintCount = count;
            return this;
        }
        #endregion
    }
}