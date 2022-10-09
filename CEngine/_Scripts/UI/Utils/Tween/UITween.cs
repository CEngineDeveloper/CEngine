using DG.Tweening;
using UnityEngine;
/// <summary>
/// UI动画
/// 位置变化
/// 大小变化
/// 颜色变化等
/// </summary>
namespace CYM.UI
{
    public class UITween : BaseMono
    {
        #region inspector
        [SerializeField]
        protected Ease Ease = Ease.Linear;
        [SerializeField]
        protected LoopType LoopType = LoopType.Yoyo;
        [SerializeField]
        protected int LoopCount = -1;
        [SerializeField]
        protected float Duration = 1.0f;
        [SerializeField]
        protected float Delay = 0;
        [SerializeField]
        protected bool AutoTween = true;
        [SerializeField]
        protected bool Inverse = false;
        #endregion

        #region prop
        protected Tween tween;
        protected RectTransform RectTrans;
        protected Vector3 SourceAnchorPos3D;
        protected Vector3 SourceLocalScale;
        #endregion

        #region life
        public override void Awake()
        {
            base.Awake();
            RectTrans = Trans as RectTransform;
            SourceAnchorPos3D = RectTrans.anchoredPosition3D;
            SourceLocalScale = RectTrans.localScale;
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void Start()
        {
            base.Start();
            if (AutoTween)
            {
                DoTween();
            }
        }
        #endregion

        public void SetDelay(float val)
        {
            Delay = val;
        }
        public virtual void DoTween()
        {

        }
        public virtual void DoReset()
        { 
        
        }
        public virtual void Stop()
        {
            if (tween != null)
                tween.Kill();
        }
    }

}