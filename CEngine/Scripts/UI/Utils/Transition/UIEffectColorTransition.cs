//------------------------------------------------------------------------------
// PresenterEffectColorTransition.cs
// Copyright 2019 2019/4/7 
// Created by CYM on 2019/4/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
namespace CYM.UI
{
    [System.Serializable]
    public class UIEffectColorTransition : UITransition
    {
        #region Inspector
        public string StateColorPreset;
        [HideIf("Inspector_IsHideStateColor")]
        public PresenterStateColor StateColor = new PresenterStateColor();
        public bool IsShadow = true;
        public bool IsOutline = true;
        #endregion

        #region prop
        private TweenerCore<Color, Color, ColorOptions> colorShadowTween;
        private TweenerCore<Color, Color, ColorOptions> colorOutlineTween;
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (!IsInited) return;
            if (UseCheck) return;
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Effect == null) return;
            if (colorShadowTween != null) colorShadowTween.Kill();
            if (colorOutlineTween != null) colorOutlineTween.Kill();
            if (Shadow != null && IsShadow)
                colorShadowTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Enter, Duration).SetDelay(Delay);
            if (Outline != null && IsOutline)
                colorOutlineTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Enter, Duration).SetDelay(Delay);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (!IsInited) return;
            if (UseCheck) return;
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Effect == null) return;
            if (colorShadowTween != null) colorShadowTween.Kill();
            if (colorOutlineTween != null) colorOutlineTween.Kill();
            if (Shadow != null && IsShadow)
                colorShadowTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
            if (Outline != null && IsOutline)
                colorOutlineTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!IsInited) return;
            if (UseCheck) return;
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Effect == null) return;
            if (colorShadowTween != null) colorShadowTween.Kill();
            if (colorOutlineTween != null) colorOutlineTween.Kill();
            if (Shadow != null && IsShadow)
                colorShadowTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Press, Duration).SetDelay(Delay);
            if (Outline != null && IsOutline)
                colorOutlineTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Press, Duration).SetDelay(Delay);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (!IsInited) return;
            if (UseCheck) return;
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Effect == null) return;
            if (colorShadowTween != null) colorShadowTween.Kill();
            if (colorOutlineTween != null) colorOutlineTween.Kill();
            if (Shadow != null && IsShadow)
                colorShadowTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
            if (Outline != null && IsOutline)
                colorOutlineTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
        }

        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
            if (!IsInited) return;
            if (Effect == null) return;
            if (colorShadowTween != null) colorShadowTween.Kill();
            if (colorOutlineTween != null) colorOutlineTween.Kill();
            if (Shadow != null && IsShadow)
            {
                if (b)
                    colorShadowTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
                else
                    colorShadowTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Disable, Duration).SetDelay(Delay);
            }
            if (Outline != null && IsOutline)
            {

                if (b)
                    colorOutlineTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
                else
                    colorOutlineTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Disable, Duration).SetDelay(Delay);
            }
        }
        public override void OnSelected(bool b)
        {
            base.OnSelected(b);
            if (!IsInited) return;
            if (!IsInteractable) return;
            if (UseCheck) return;
            if (Effect == null) return;
            if (colorShadowTween != null)
                colorShadowTween.Kill();
            if (colorOutlineTween != null)
                colorOutlineTween.Kill();
            if (Shadow != null && IsShadow)
            {
                if (b)
                    colorShadowTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Selected, Duration).SetDelay(Delay);
                else
                    colorShadowTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            if (Outline != null && IsOutline)
            {
                if (b)
                    colorOutlineTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Selected, Duration).SetDelay(Delay);
                else
                    colorOutlineTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
        }
        public override void OnChecked(bool b)
        {
            if (!IsInited) return;
            if (!UseCheck) return;
            base.OnChecked(b);
            if (Effect == null) return;
            if (colorShadowTween != null)
                colorShadowTween.Kill();
            if (colorOutlineTween != null)
                colorOutlineTween.Kill();
            if (Shadow != null && IsShadow)
            {
                if (b)
                    colorShadowTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Checked, Duration).SetDelay(Delay);
                else
                    colorShadowTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            if (Outline != null && IsOutline)
            {
                if (b)
                    colorOutlineTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Checked, Duration).SetDelay(Delay);
                else
                    colorOutlineTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
        }
        #endregion


        #region inspector editor
        protected override bool Inspector_IsHideStateColor()
        {
            return !StateColorPreset.IsInv();
        }
        #endregion
    }
}