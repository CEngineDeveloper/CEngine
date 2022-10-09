using DG.Tweening;
using UnityEngine;

namespace CYM.UI
{
    public class UITweenPosition : UITween
    {
        #region inspector
        [SerializeField]
        protected Vector3 Target;
        #endregion

        public override void DoReset()
        {
            if (tween != null)
                tween.Kill();
            if (Inverse)
            {
                RectTrans.anchoredPosition3D = Target;
            }
            else
            {
                RectTrans.anchoredPosition3D = SourceAnchorPos3D;
            }
        }
        public override void DoTween()
        {
            if (tween != null)
                tween.Kill();
            if (Inverse)
            {
                Vector3 newTarget = SourceAnchorPos3D;
                RectTrans.anchoredPosition3D = Target;
                tween = DOTween.To(() => RectTrans.anchoredPosition3D, (x) => RectTrans.anchoredPosition3D = x, newTarget, Duration)
                    .SetLoops(LoopCount, LoopType)
                    .SetEase(Ease)
                    .SetDelay(Delay);
            }
            else
            {
                tween = DOTween.To(() => RectTrans.anchoredPosition3D, (x) => RectTrans.anchoredPosition3D = x, Target, Duration)
                    .SetLoops(LoopCount, LoopType)
                    .SetEase(Ease)
                    .SetDelay(Delay);
            }
        }
    }
}
