//------------------------------------------------------------------------------
// BaseHUDItem.cs
// Copyright 2018 CopyrightHolderName 
// Created by CYM on 2018/2/27
// Owner: CYM
// 所有HUD物件的基类,血条和跳字的HUD都会继承这个类
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UHUDItem : UPres<UData>
    {
        #region member variable
        [FoldoutGroup("HUD"), SerializeField]
        public NodeType NodeType = NodeType.Center;
        [FoldoutGroup("HUD"), SerializeField]
        public Vector3 Offset = Vector3.zero;
        #endregion

        #region 曲线
        [FoldoutGroup("Curve"), SerializeField,HideIf("Inspector_HideLifeTime")]
        public float LifeTime = 1;
        [FoldoutGroup("Curve"), SerializeField, HideIf("Inspector_HideDestroyWhenTimeOut"), HideIf("Inspector_HideLifeTime")]
        public bool CurveChange = true;
        [FoldoutGroup("Curve"), SerializeField, HideIf("Inspector_HideDestroyWhenTimeOut"), HideIf("Inspector_HideLifeTime")]
        public bool ShowEffect = false;
        [FoldoutGroup("Curve"), SerializeField,HideIf("Inspector_HideCurve"), HideIf("Inspector_HideLifeTime")]
        public Color Color;
        [FoldoutGroup("Curve"),HideIf("Inspector_HideCurve"), HideIf("Inspector_HideLifeTime")]
        public AnimationCurve OffsetCurveY = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) });
        [FoldoutGroup("Curve"), HideIf("Inspector_HideCurve"), HideIf("Inspector_HideLifeTime")]
        public AnimationCurve OffsetCurveX = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) });
        [FoldoutGroup("Curve"), HideIf("Inspector_HideCurve"), HideIf("Inspector_HideLifeTime")]
        public AnimationCurve AlphaCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) });
        [FoldoutGroup("Curve"), HideIf("Inspector_HideCurve"), HideIf("Inspector_HideLifeTime")]
        public AnimationCurve ScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) });
        #endregion

        #region Callback
        public Callback<UHUDItem, float> OnLifeOver;
        #endregion

        #region property
        public bool NeedDestroyWhenTimeOut { get; private set; } = true;
        public Vector3 InputOffset { get; set; } = Vector3.zero;
        public BaseUnit SelfUnit { get; private set; }
        protected CanvasScaler CanvasScaler;
        protected Transform followObj;
        protected float CurTime;
        protected Vector3 TempPos;
        protected float Offset_y = 0f;
        protected float Offset_x = 0f;
        protected float CurLifePercent
        {
            get
            {
                if (LifeTime <= 0)
                    return 0;
                return CurTime / LifeTime; 
            }
        }

        #endregion

        #region is
        public bool IsLifeOver => CurTime >= LifeTime && LifeTime > 0;
        public bool IsDestroy { get; private set; } = false;
        public override bool IsAutoInit => true;
        #endregion

        #region life
        /// <summary>
        /// 跳字创建的时候需要调用此函数
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pool"></param>
        /// <param name="follow"></param>        
        public virtual void Init(BaseUnit unit, Transform followObj = null)
        {
            SetNeedDestroyWhenTimeOut(true);
            this.followObj = followObj; 
            SelfUnit = unit;
            IsDestroy = false;
            CurTime = 0.0f;
            Offset_y = 0f;
            Offset_x = 0f;
            Color.a = 0.0f;
            RectTrans.localScale = Vector3.one;
        }
        public override void OnUpdate()
        {
            if (IsDestroy)return;
            if (CanvasScaler == null)
            {
                if (PUIView != null && PUIView.RootView != null)
                    CanvasScaler = PUIView.RootView.CanvasScaler;
            }
            UpdateAnchoredPosition();
            if (LifeTime > 0)
            {
                UpdateLifeTime();
            }
            //关闭的时候如果单位死亡了,就自动销毁
            else if (
                SelfUnit != null && 
                !SelfUnit.IsLive)
            {
                DoDestroy();
            }
        }
        public virtual void DoDestroy(float delay=0)
        {
            if (IsDestroy) 
                return;
            IsDestroy = true;
            OnLifeOver?.Invoke(this, delay);
            SelfUnit = null;
            CancleInit();
        }
        #endregion

        #region set
        public void SetFollowObj(Transform followObj = null)
        {
            this.followObj = followObj;
        }
        public void SetNeedDestroyWhenTimeOut(bool b)
        {
            NeedDestroyWhenTimeOut = b;
        }
        #endregion

        #region get
        protected virtual Transform GetFollowObj()
        {
            if (followObj == null)
                return SelfUnit.Trans;
            return followObj;
        }
        protected virtual Vector3 GetOffset()
        {
            return Offset + InputOffset;
        }
        #endregion

        #region update
        protected virtual void UpdateLifeTime()
        {
            if (ShowEffect && CurLifePercent <= 0)
            {
                ShowDirect(true);
                if (!IsShowComplete)
                    return;
            }
            Camera cacheCamera = BaseGlobal.MainCamera;
            if (cacheCamera == null) return;
            CurTime += Time.deltaTime;
            if (CurveChange)
            {
                Offset_y = OffsetCurveY.Evaluate(CurLifePercent);
                Offset_x = OffsetCurveX.Evaluate(CurLifePercent);
                Color.a = AlphaCurve.Evaluate(CurLifePercent);
                Trans.localScale = ScaleCurve.Evaluate(CurLifePercent) * Vector3.one;
            }
            if (CurTime > LifeTime &&
                NeedDestroyWhenTimeOut)
            {
                if (ShowEffect && CurLifePercent >= 1)
                {
                    ShowDirect(false);
                    if (!IsShowComplete)
                        return;
                }
                DoDestroy();
            }
        }
        protected virtual void UpdateAnchoredPosition()
        {
            Camera cacheCamera = BaseGlobal.MainCamera;
            if (cacheCamera == null) return;
            if (followObj != null)
            {
                TempPos = followObj.position + GetOffset();
            }
            Trans.position = cacheCamera.WorldToScreenPoint(TempPos);
            Trans.position += new Vector3(Offset_x, Offset_y, 0f);
        }
        #endregion

        #region Inspector
        bool Inspector_HideCurve()
        {
            return LifeTime <= 0 || !CurveChange;
        }
        bool Inspector_HideDestroyWhenTimeOut()
        {
            return LifeTime <= 0;
        }
        protected virtual bool Inspector_HideLifeTime()
        {
            return false;
        }
        #endregion
    }
}