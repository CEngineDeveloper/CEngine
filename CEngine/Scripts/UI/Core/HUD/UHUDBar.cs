//------------------------------------------------------------------------------
// BaseHUDBar.cs
// Copyright 2019 2019/2/8 
// Created by CYM on 2019/2/8
// Owner: CYM
// HUDBar用于血条之类的,实时跟随在物体上的UI
//------------------------------------------------------------------------------

using CYM.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class UHUDBar : UHUDItem
    {
        #region prop
        protected BaseCameraMgr BaseCameraMgr => BaseGlobal.CameraMgr;
        #endregion

        #region life
        public override void Init(BaseUnit unit, Transform followObj = null)
        {
            base.Init(unit, followObj);
            Data.OnClick = OnBgButtonClick;
            Data.OnEnter = OnBgButtonEnter;
            Data.OnExit = OnBgButtonExit;
            LifeTime = 0;
            CurveChange = false;
            SetNeedDestroyWhenTimeOut(false);
            SetDefaultShowType();
        }
        /// <summary>
        /// 根据单位世界坐标的位置实时更新当前血条的映射点
        /// </summary>
        protected override void UpdateAnchoredPosition()
        {
            if (BaseGlobal.MainCamera == null) return;
            if (IsDestroy ||
                RectTrans == null ||
                PUIView == null
                ) return;
            if (GetFollowObj() != null)
            {
                if (HideCondition())
                {
                    Show(false);
                }
                else
                {
                    Show(true);
                }

                if (GroupAlpha > 0.0f || IsShow)
                {
                    Vector2 a = RectTransformUtility.WorldToScreenPoint(BaseGlobal.MainCamera, GetFollowObj().position + GetOffset());
                    RectTrans.localPosition = new Vector3(a.x / PUIView.RootView.HUDItemOffset, a.y / PUIView.RootView.HUDItemOffset, 0.0f);
                }
            }
            else
            {
                Show(false);
            }
        }
        public override void OnShow(bool isShow)
        {
            base.OnShow(isShow);
            if (BaseInputMgr.HoverHUDBar == this && !isShow)
            {
                PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
                OnPointerExit(eventDataCurrentPosition);
            }
        }
        protected virtual bool HideCondition()
        {
            if (!SelfUnit.IsLive)
                return true;
            if (!SelfUnit.IsRendered)
                return true;
            if (BaseCameraMgr.IsEndHight)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            BaseInputMgr.SetHoverHUDBar(this);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            BaseInputMgr.SetHoverHUDBar(null);
        }
        private void OnBgButtonClick(UControl arg1, PointerEventData arg2)
        {
            if (arg2.button == PointerEventData.InputButton.Left)
                BaseGlobal.InputMgr?.LeftClick(SelfUnit, true);
            else if (arg2.button == PointerEventData.InputButton.Right)
                BaseGlobal.InputMgr?.RightClick(SelfUnit, true);
        }
        private void OnBgButtonEnter(UControl arg1, PointerEventData arg2)
        {
            //BaseGlobal.InputMgr?.DoEnterUnit(SelfUnit);
        }
        private void OnBgButtonExit(UControl arg1, PointerEventData arg2)
        {
            //BaseGlobal.InputMgr?.DoExitUnit(SelfUnit);
        }
        #endregion

        #region inspector
        protected override bool Inspector_HideLifeTime()
        {
            return true;
        }
        #endregion
    }
}