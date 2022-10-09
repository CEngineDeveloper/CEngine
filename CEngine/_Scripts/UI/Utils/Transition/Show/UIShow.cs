//------------------------------------------------------------------------------
// BasePSTransition.cs
// Copyright 2019 2019/4/7 
// Created by CYM on 2019/4/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using UnityEngine;

namespace CYM.UI
{
    public class UIShow : UITransition
    {
        #region inspector
        [SerializeField]
        public Ease InEase = Ease.OutBack;
        [SerializeField]
        public Ease OutEase = Ease.InBack;
        #endregion

        #region prop
        protected int ShowCount { get; set; } = 0;
        protected Tweener tweener;
        public bool IsComplete { get; private set; } = false;
        Callback OnFadeIn;
        Callback OnFadeOut;
        #endregion

        #region set
        public override void OnShow(bool b)
        {
            if (tweener != null) tweener.Kill();
            IsComplete = false;
            Duration = GetDuration(b);
            base.OnShow(b);
        }
        public void SetFadeInCompleteCallback(Callback callback)
        {
            OnFadeIn = callback;
        }
        public void SetFadeOutCompleteCallback(Callback callback)
        {
            OnFadeOut = callback;
        }
        #endregion

        #region get
        protected Ease GetEase(bool b)
        {
            if (b) return InEase;
            else return OutEase;
        }
        #endregion

        #region Callback
        // 手动设置,在Tween完毕的时候Dective
        protected void OnTweenComplete()
        {
            if (SelfControl != null)
            {
                if (!SelfControl.IsShow)
                {
                    OnFadeOut?.Invoke();
                    if (SelfControl.IsActiveByShow)
                        SelfControl.SetActive(false);
                }
                else
                {
                    OnFadeIn?.Invoke();
                }
            }
            IsComplete = true;
        }
        #endregion

        #region Inspector Editor
        protected override bool Inspector_HideTarget()
        {
            return true;
        }
        protected override bool Inspector_HideSameDuration()
        {
            return false;
        }
        protected override bool Inspector_HideReset()
        {
            return false;
        }
        protected override bool Inspector_HideDelay()
        {
            return false;
        }
        #endregion
    }
}