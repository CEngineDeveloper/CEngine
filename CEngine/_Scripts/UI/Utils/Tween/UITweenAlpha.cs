//------------------------------------------------------------------------------
// UITweenAlpha.cs
// Copyright 2021 2021/4/19 
// Created by CYM on 2021/4/19
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using DG.Tweening;
namespace CYM.UI
{
    public class UITweenAlpha : UITween
    {
        #region inspector
        [SerializeField]
        protected float Target=1.0f;
        #endregion

        #region prop
        CanvasGroup CanvasGroup;
        float SourceAlpha;
        #endregion

        #region life
        public override void Awake()
        {
            base.Awake();
            CanvasGroup = gameObject.SafeAddComponet<CanvasGroup>();
            SourceAlpha = CanvasGroup.alpha;
            if (Inverse)
            {
                CanvasGroup.alpha = Target;
            }
            else
            {
                CanvasGroup.alpha = SourceAlpha;
            }
        }
        public override void DoReset()
        {
            if (tween != null)
                tween.Kill();
            if (Inverse)
            {
                CanvasGroup.alpha = Target;
            }
            else
            {
                CanvasGroup.alpha = SourceAlpha;
            }
        }
        public override void DoTween()
        {
            if (tween != null)
                tween.Kill();
            if (Inverse)
            {
                float newTarget = SourceAlpha;
                CanvasGroup.alpha = Target;
                tween = DOTween.To(() => CanvasGroup.alpha, (x) => CanvasGroup.alpha = x, newTarget, Duration)
                    .SetLoops(LoopCount, LoopType)
                    .SetEase(Ease)
                    .SetDelay(Delay);
            }
            else
            {
                CanvasGroup.alpha = SourceAlpha;
                tween = DOTween.To(() => CanvasGroup.alpha, (x) => CanvasGroup.alpha = x, Target, Duration)
                    .SetLoops(LoopCount, LoopType)
                    .SetEase(Ease)
                    .SetDelay(Delay);
            }
        }
        #endregion
    }
}