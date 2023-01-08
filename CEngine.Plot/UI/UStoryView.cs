//------------------------------------------------------------------------------
// UStoryView.cs
// Copyright 2021 2021/10/20 
// Created by CYM on 2021/10/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using UnityEngine.EventSystems;
using DG.Tweening;
using CYM.Plot;

namespace CYM.UI
{
    public class UStoryView : UUIView
    {
        #region Inspector
        [SerializeField]
        RectTransform Dlg;
        [SerializeField]
        UImage SelfBG;
        [SerializeField]
        UImage Face;
        [SerializeField]
        UText Name;
        [SerializeField]
        UText Desc;
        #endregion

        #region prop
        Tween Tween;
        TDBaseStoryData CurStoryData;
        #endregion

        #region life
        protected virtual IStoryMgr<TDBaseStoryData> StoryMgr => BaseGlobal.StoryMgr;
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            SelfBG.Init(new UImageData { OnClick = OnClickSelfBG });
            StoryMgr.Callback_OnStart += OnStoryStart;
            StoryMgr.Callback_OnNext += OnStoryNext;
            StoryMgr.Callback_OnSubNext += OnSubNext;
            StoryMgr.Callback_OnEnd += OnStoryEnd;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            StoryMgr.Callback_OnStart -= OnStoryStart;
            StoryMgr.Callback_OnNext -= OnStoryNext;
            StoryMgr.Callback_OnSubNext -= OnSubNext;
            StoryMgr.Callback_OnEnd -= OnStoryEnd;
        }
        public override void Refresh()
        {
            base.Refresh();
            if (CurStoryData == null)
                return;
            SetAnchorPosition(CurStoryData.Anchor,CurStoryData.Offset);
            if (Face) Face.IconSprite = CurStoryData.GetIcon();
            if (Name) Name.NameText = CurStoryData.GetName();
            if (Desc)
            {
                //Desc.NameText = CurStoryData.TDID.GetName();
                //if (Tween != null)
                //    Tween.Kill();
                //Tween = DOTween.To(() =>
                //    Desc.NameText, (x) => Desc.NameText = x, CurStoryData.TDID.GetName(), 0.5f)
                //    .SetDelay(0.5f)
                //    .OnComplete(OnTypeEnd)
                //    .OnStart(OnTweenStart);
            }
        }

        public void SetAnchorPosition(Anchor anchor,Vector2 offset)
        {
            UIUtil.SetAnchorPosition(Dlg, anchor,offset);
        }
        #endregion

        #region Callback
        private void OnStoryStart(TDBaseStoryData arg1, string arg3)
        {
            CurStoryData = arg1;
            Show();
            Show(true, true, true);
            if (Desc) Desc.NameText = arg3;
        }
        private void OnStoryNext(TDBaseStoryData arg1, int arg2, string arg3)
        {
            CurStoryData = arg1;
            Refresh();
            if (Desc) Desc.NameText = arg3;
        }
        private void OnSubNext(TDBaseStoryData arg1, int arg2, string arg3)
        {
            if (Desc) Desc.NameText = arg3;
        }
        private void OnStoryEnd()
        {
            Close();
        }
        private void OnClickSelfBG(UControl arg1, PointerEventData arg2)
        {
            StoryMgr.Next();
        }
        private void OnTweenStart()
        {

        }

        private void OnTypeEnd()
        {

        }
        #endregion
    }
}