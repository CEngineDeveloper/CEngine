//------------------------------------------------------------------------------
// UIAnimator.cs
// Copyright 2021 2021/2/27 
// Created by CYM on 2021/2/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using CYM.UI;
using Sirenix.OdinInspector;
using DG.Tweening;
namespace CYM
{
    [System.Serializable]
    public class UIAnimator  
    {
        #region Inspector
        [SerializeField,ChildGameObjectsOnly]
        public GameObject Target;
        [SerializeField]
        protected float Duration = 0.2f;
        [SerializeField, HideIf("Inspector_HideDelay")]
        protected float Delay = 0.0f;
        [SerializeField, HideIf("Inspector_HideEffClose")]
        protected bool IsEffClose = false;
        [SerializeField, HideIf("Inspector_HideInEase")]
        public Ease InEase = Ease.OutBack;
        [SerializeField, HideIf("Inspector_HideOutEase")]
        public Ease OutEase = Ease.InBack;
        #endregion

        #region prop
        protected RectTransform RectTrans;
        protected Tweener tweener;
        public Vector2 SourceAnchorMax { get; private set; }
        public Vector2 SourceAnchorMin { get; private set; }
        public Vector3 SourceLocalScale { get; private set; }
        public Vector2 SourceSizeData { get; private set; }
        public Vector2 SourceAnchoredPosition { get; private set; }
        public Vector3 SourceAnchoredPosition3D { get; private set; }

        protected UUIView SelfUIView;

        #endregion

        #region life
        public virtual void Init(UUIView self)
        {
            SelfUIView = self;
            if (SelfUIView == null)
            {
                CLog.Error($"{self.GOName}:UIView 没有");
            }
            if (Target == null)
            {
                CLog.Error($"{self.GOName}:Target 没有");
            }
            else
            {
                RectTrans = Target.transform as RectTransform;
                SourceAnchorMax = RectTrans.anchorMax;
                SourceAnchorMin = RectTrans.anchorMin;
                SourceLocalScale = RectTrans.localScale;
                SourceSizeData = RectTrans.sizeDelta;
                SourceAnchoredPosition = RectTrans.anchoredPosition;
                SourceAnchoredPosition3D = RectTrans.anchoredPosition3D;
            }
        }
        #endregion

        #region Callback
        public virtual void OnShow(bool b)
        {
            if (tweener != null) tweener.Kill();
        }
        #endregion

        #region get
        protected Ease GetEase(bool b)
        {
            if (b) return InEase;
            else return OutEase;
        }
        #endregion

        #region inspector

        protected virtual bool Inspector_HideSameDuration()
        {
            return false;
        }
        protected virtual bool Inspector_HideEffClose()
        {
            return false;
        }
        protected virtual bool Inspector_HideDelay()
        {
            return false;
        }
        protected virtual bool Inspector_HideInEase()
        {
            return false;
        }
        protected virtual bool Inspector_HideOutEase()
        {
            return !IsEffClose;
        }
        #endregion
    }
}