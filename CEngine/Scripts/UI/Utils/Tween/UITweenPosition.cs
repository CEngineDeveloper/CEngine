using DG.Tweening;
using UnityEngine;

namespace CYM.UI
{
    public class UITweenPosition : UITween
    {
        #region inspector
        [SerializeField]
        protected Vector2 Target;
        #endregion

        public override void Awake()
        {
            base.Awake();
            if (Target.x == 0)
                Target.x = SourceAnchorPos3D.x;
            if (Target.y == 0)
                Target.y = SourceAnchorPos3D.y;
        }
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
                Vector3 newTarget = Target;
                tween = DOTween.To(() => RectTrans.anchoredPosition3D, (x) => RectTrans.anchoredPosition3D = x, newTarget, Duration)
                    .SetLoops(LoopCount, LoopType)
                    .SetEase(Ease)
                    .SetDelay(Delay);
            }
        }
    }
}
