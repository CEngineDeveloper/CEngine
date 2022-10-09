using DG.Tweening;
using UnityEngine;

namespace CYM.UI
{
    public class UITweenScale : UITween
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
                RectTrans.localScale = Target;
            }
            else
            {
                RectTrans.localScale = SourceLocalScale;
            }
        }

        public override void DoTween()
        {
            if (tween != null)
                tween.Kill();
            if (Inverse)
            {
                Vector3 newTarget = SourceLocalScale;
                RectTrans.localScale = Target;
                tween = DOTween.To(() => Trans.localScale, (x) => Trans.localScale = x, newTarget, Duration)
                    .SetLoops(LoopCount, LoopType)
                    .SetEase(Ease)
                    .SetDelay(Delay);
            }
            else
            {
                tween = DOTween.To(() => Trans.localScale, (x) => Trans.localScale = x, Target, Duration)
                    .SetLoops(LoopCount, LoopType)
                    .SetEase(Ease)
                    .SetDelay(Delay);
            }
        }

    }
}
