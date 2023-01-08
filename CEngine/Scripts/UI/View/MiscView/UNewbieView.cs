//------------------------------------------------------------------------------
// BaseNewbieView.cs
// Copyright 2020 2020/9/29 
// Created by CYM on 2020/9/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
namespace CYM.UI
{
    public class UNewbieView : UUIView
    {
        #region Inspector
        [SerializeField]
        UText Desc;
        [SerializeField]
        UButton BntPrev;
        [SerializeField]
        UButton BntNext;
        #endregion

        #region prop
        List<string> PapeKeies = new List<string>();
        int PageIndex = 0;
        #endregion

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            BntNext.Init(new UButtonData { NameKey= "Bnt_下一个" ,OnClick=OnClickNext,IsInteractable= IsNextInteractable });
            BntPrev.Init(new UButtonData { NameKey = "Bnt_上一个", OnClick = OnClickPrev, IsInteractable = IsPrevInteractable });
        }
        protected override void OnShow()
        {
            base.OnShow();
            Desc.NameText = GetStr(GetInitPageKey());
        }
        #endregion

        #region show
        public void Show(string category)
        {
            PapeKeies = BaseLangMgr.GetCategoryKeyList(category);
            Show(true);
        }
        #endregion

        #region get
        public string GetInitPageKey()
        {
            if (PapeKeies.Count <= 0)
                return GetStr("Text_None");
            PageIndex = 0;
            return PapeKeies[0];
        }
        public string GetNextPageKey()
        {
            PageIndex++;
            if (PapeKeies.Count <= 0)
                return GetStr("Text_None");
            if (PageIndex >= PapeKeies.Count)
                PageIndex = PapeKeies.Count-1;
            string ret = PapeKeies[PageIndex];
            return ret;
        }
        public string GetPrevPageKey()
        {
            PageIndex--;
            if (PapeKeies.Count <= 0)
                return GetStr("Text_None");
            if (PageIndex <0)
                PageIndex = 0;
            string ret = PapeKeies[PageIndex];
            return ret;
        }
        #endregion

        #region Callback
        private void OnClickPrev(UControl arg1, PointerEventData arg2)
        {
            Desc.NameText = GetStr(GetPrevPageKey());
            SetDirtyAll();
        }
        private void OnClickNext(UControl arg1, PointerEventData arg2)
        {
            Desc.NameText = GetStr(GetNextPageKey()); 
            SetDirtyAll();
        }
        private bool IsPrevInteractable(int index)
        {
            return PageIndex > 0;
        }
        private bool IsNextInteractable(int index)
        {
            return PageIndex < PapeKeies.Count-1;
        }
        #endregion
    }
}