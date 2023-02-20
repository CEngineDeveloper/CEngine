//------------------------------------------------------------------------------
// UIAlphaAnimator.cs
// Copyright 2023 2023/1/18 
// Created by CYM on 2023/1/18
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using DG.Tweening;

namespace CYM
{
    [System.Serializable]
    public class UIAlphaAnimator : UIAnimator
    {
        #region inspector
        [SerializeField]
        protected float From = 0.0f;
        #endregion

        #region prop
        CanvasGroup CanvasGroup;
        float SourceAlpha;
        #endregion

        public override void Init(UUIView self)
        {
            base.Init(self);
            CanvasGroup = Target.SafeAddComponet<CanvasGroup>();
            SourceAlpha = CanvasGroup.alpha = 1;
            CanvasGroup.alpha = SourceAlpha;
        }

        public override void OnShow(bool b)
        {
            base.OnShow(b);
            if (tweener != null)
                tweener.Kill();
            float tarAlpha = 0;
            if (b)
            {
                tarAlpha = SourceAlpha;
                CanvasGroup.alpha = From;
            }
            else
            {
                tarAlpha = From;
                CanvasGroup.alpha = SourceAlpha;
            }

            tweener = DOTween.To(() => CanvasGroup.alpha, (x) => CanvasGroup.alpha = x, tarAlpha, Duration)
                .SetEase(GetEase(b))
                .SetDelay(Delay);
        }
    }
}