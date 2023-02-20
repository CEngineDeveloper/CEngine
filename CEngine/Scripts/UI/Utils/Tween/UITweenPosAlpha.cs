using DG.Tweening;
using UnityEngine;

namespace CYM.UI
{
    public class UITweenPosAlpha : UITween
    {
        #region inspector
        [SerializeField]
        protected Vector2 From;
        [SerializeField]
        protected float FromAlpha = 0.0f;
        [SerializeField]
        protected float TargetAlpha = 1.0f;
        #endregion

        CanvasGroup CanvasGroup;
        float SourceAlpha;

        Tween tweenAlpha;

        public override void Awake()
        {
            base.Awake();
            if (From.x == 0)
                From.x = SourceAnchorPos3D.x;
            if (From.y == 0)
                From.y = SourceAnchorPos3D.y;
            CanvasGroup = gameObject.SafeAddComponet<CanvasGroup>();
            SourceAlpha = CanvasGroup.alpha = FromAlpha;
            if (Inverse)
            {
                CanvasGroup.alpha = TargetAlpha;
            }
            else
            {
                CanvasGroup.alpha = FromAlpha;
            }
        }
        public override void DoReset()
        {
            if (tween != null)
                tween.Kill();
            if (tweenAlpha != null)
                tweenAlpha.Kill();


            if (Inverse)
            {
                RectTrans.anchoredPosition3D = From;
                CanvasGroup.alpha = TargetAlpha;
            }
            else
            {
                RectTrans.anchoredPosition3D = SourceAnchorPos3D;
                CanvasGroup.alpha = FromAlpha;
            }
        }
        public override void DoTween()
        {
            if (tween != null)
                tween.Kill();
            if (tweenAlpha != null)
                tweenAlpha.Kill();

            if (Inverse)
            {
                Vector3 newTarget = From ;
                RectTrans.anchoredPosition3D = SourceAnchorPos3D;
                tween = DOTween.To(() => RectTrans.anchoredPosition3D, (x) => RectTrans.anchoredPosition3D = x, newTarget, Duration)
                    .SetLoops(LoopCount, LoopType)
                    .SetEase(Ease)
                    .SetDelay(Delay);
            }
            else
            {
                Vector3 newTarget = SourceAnchorPos3D;
                RectTrans.anchoredPosition3D = From;
                tween = DOTween.To(() => RectTrans.anchoredPosition3D, (x) => RectTrans.anchoredPosition3D = x, newTarget, Duration)
                    .SetLoops(LoopCount, LoopType)
                    .SetEase(Ease)
                    .SetDelay(Delay);
            }


            if (Inverse)
            {
                float newTarget = SourceAlpha;
                CanvasGroup.alpha = TargetAlpha;
                tweenAlpha = DOTween.To(() => CanvasGroup.alpha, (x) => CanvasGroup.alpha = x, newTarget, Duration)
                    .SetLoops(LoopCount, LoopType)
                    .SetEase(Ease)
                    .SetDelay(Delay);
            }
            else
            {
                CanvasGroup.alpha = FromAlpha;
                tweenAlpha = DOTween.To(() => CanvasGroup.alpha, (x) => CanvasGroup.alpha = x, TargetAlpha, Duration)
                    .SetLoops(LoopCount, LoopType)
                    .SetEase(Ease)
                    .SetDelay(Delay);
            }
        }
    }
}
