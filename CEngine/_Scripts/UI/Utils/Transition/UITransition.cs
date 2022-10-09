//------------------------------------------------------------------------------
// BasePresenterTransition.cs
// Copyright 2019 2019/4/7 
// Created by CYM on 2019/4/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UITransition
    {
        #region Inspector
        [SerializeField, HideIf("Inspector_HideTarget")]
        public GameObject Target;
        [SerializeField, HideIf("Inspector_HideSameDuration")]
        protected bool IsSameDuration = true;
        [SerializeField, HideIf("Inspector_HideSubDuration")]
        protected float OpenDuration = 0.2f;
        [SerializeField, HideIf("Inspector_HideSubDuration")]
        protected float CloseDuration = 0.2f;
        [SerializeField, HideIf("Inspector_HideDuration")]
        protected float Duration = 0.2f;
        [SerializeField, HideIf("Inspector_HideDelay")]
        protected float Delay = 0.0f;
        [SerializeField, HideIf("Inspector_HideReset")]
        protected bool IsReset = true;
        #endregion

        #region prop
        protected RectTransform RectTrans;
        protected UControl SelfControl;
        protected BaseMeshEffect Effect;
        protected Shadow Shadow;
        protected Outline Outline;

        protected Graphic Graphic;
        protected Text Text;
        protected Image Image;
        protected bool IsInteractable => SelfControl.IsInteractable;
        protected bool IsSelected => SelfControl.IsSelected;
        #endregion

        #region life
        public virtual void Init(UControl self)
        {
            SelfControl = self;
            if (SelfControl == null)
            {
                CLog.Error("Presenter 没有");
            }
            if (Target == null)
                Target = SelfControl.GO;
            if (Target != null)
            {
                Graphic = Target.GetComponent<Graphic>();
                Effect = Target.GetComponent<BaseMeshEffect>();
            }
            if (Graphic != null)
            {
                if (Graphic is Text) Text = (Text)Graphic;
                if (Graphic is Image) Image = (Image)Graphic;
                RectTrans = Graphic.rectTransform;
            }
            if (Effect != null)
            {
                if (Effect is Shadow) Shadow = (Shadow)Effect;
                if (Effect is Outline) Outline = (Outline)Effect;
            }
            if (RectTrans == null)
            {
                RectTrans = Target.transform as RectTransform;
            }
        }
        #endregion

        #region callback
        public virtual void OnPointerEnter(PointerEventData eventData)
        {

        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {

        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {

        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {

        }

        public virtual void OnInteractable(bool b)
        {
            //IsInteractable = b;
        }
        public virtual void OnSelected(bool b)
        {
            //IsSelected = b;
        }
        public virtual void OnShow(bool b)
        {
            if (SelfControl.IsShow)
            {
                if (SelfControl.IsActiveByShow)
                    SelfControl.SetActive(true);
            }
        }
        #endregion

        #region get
        // 获得开/关闭的Duration
        public float GetDuration(bool b)
        {
            if (IsSameDuration) return Duration;
            if (b) return OpenDuration;
            else return CloseDuration;
        }
        public float GetTotalDuration()
        {
            return GetDuration(true) + GetDuration(false);
        }
        #endregion

        #region inspector

        protected virtual bool Inspector_HideDuration()
        {
            if (IsSameDuration)
                return false;
            return true;
        }
        protected virtual bool Inspector_HideSubDuration()
        {
            if (IsSameDuration)
                return true;
            return false;
        }
        protected virtual bool Inspector_IsHideStateColor()
        {
            return true;
        }
        protected virtual bool Inspector_HideTarget()
        {
            return false;
        }
        protected virtual bool Inspector_HideSameDuration()
        {
            return true;
        }
        protected virtual bool Inspector_HideReset()
        {
            return true;
        }
        protected virtual bool Inspector_HideDelay()
        {
            return true;
        }
        #endregion
    }
}