//------------------------------------------------------------------------------
// PresenterColorTransition.cs
// Copyright 2019 2019/4/7 
// Created by CYM on 2019/4/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    [System.Serializable]
    public class UIColorTransition : UITransition
    {
        #region Inspector
        [ValueDropdown("Inspector_StatePresets")]
        public string StateColorPreset = SysConst.STR_Custom;
        [HideIf("Inspector_IsHideStateColor")]
        public PresenterStateColor StateColor = new PresenterStateColor();
        #endregion

        #region prop
        private TweenerCore<Color, Color, ColorOptions> colorTween;
        PresenterStateColor TextStateColor = null;
        PresenterStateColor ImageStateColor = null;
        #endregion

        #region LIFE        
        public override void Init(UControl self)
        {
            base.Init(self);
            if (!StateColorPreset.IsInv() && StateColorPreset != SysConst.STR_Custom)
                StateColor = UIConfig.Ins.GetStateColor(StateColorPreset);
            if (Text != null)
            {
                TextStateColor = new PresenterStateColor(StateColor, Text.color);
                Text.color = TextStateColor.Normal;
            }
            else if (Image != null)
            {
                ImageStateColor = new PresenterStateColor(StateColor, Image.color);
                Image.color = ImageStateColor.Normal;
            }
            else if (Graphic != null)
                Graphic.color = Color.white;
        }
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsInited) return;
            if (UseCheck) return;
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null) colorTween.Kill();
            if (Text != null)
                colorTween = DOTween.To(() => Text.color, x => Text.color = x, TextStateColor.Enter, Duration).SetDelay(Delay);
            else if (Image != null)
                colorTween = DOTween.To(() => Image.color, x => Image.color = x, ImageStateColor.Enter, Duration).SetDelay(Delay);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!IsInited) return;
            if (UseCheck) return;
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null) colorTween.Kill();
            if (Text != null)
                colorTween = DOTween.To(() => Text.color, x => Text.color = x, TextStateColor.Normal, Duration).SetDelay(Delay);
            if (Image != null)
                colorTween = DOTween.To(() => Image.color, x => Image.color = x, ImageStateColor.Normal, Duration).SetDelay(Delay);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInited) return;
            if (UseCheck) return;
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null) colorTween.Kill();
            if (Text != null)
                colorTween = DOTween.To(() => Text.color, x => Text.color = x, TextStateColor.Press, Duration).SetDelay(Delay);
            if (Image != null)
                colorTween = DOTween.To(() => Image.color, x => Image.color = x, ImageStateColor.Press, Duration).SetDelay(Delay);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInited) return;
            if (UseCheck) return;
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Text != null)
                colorTween = DOTween.To(() => Text.color, x => Text.color = x, TextStateColor.Normal, Duration).SetDelay(Delay);
            if (Image != null)
                colorTween = DOTween.To(() => Image.color, x => Image.color = x, ImageStateColor.Normal, Duration).SetDelay(Delay);
        }

        public override void OnInteractable(bool b)
        {
            if (!IsInited) return;
            base.OnInteractable(b);
            if (Graphic == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Text != null)
            {
                if (b)
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, TextStateColor.Normal, Duration).SetDelay(Delay);
                else
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, TextStateColor.Disable, Duration).SetDelay(Delay);
            }
            else if (Image != null)
            {
                if (b)
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, ImageStateColor.Normal, Duration).SetDelay(Delay);
                else
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, ImageStateColor.Disable, Duration).SetDelay(Delay);
            }
            else
            {
                if (b)
                    Graphic.CrossFadeColor(StateColor.Normal, Duration, true, true);
                else
                    Graphic.CrossFadeColor(StateColor.Disable, Duration, true, true);
            }
        }
        public override void OnSelected(bool b)
        {
            if (!IsInited) return;
            if (UseCheck) return;
            if (!IsInteractable) return;
            base.OnSelected(b);
            if (Graphic == null) return;
            if (colorTween != null) colorTween.Kill();
            if (Text != null)
            {

                if (b)
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, TextStateColor.Selected, Duration).SetDelay(Delay);
                else
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, TextStateColor.Normal, Duration).SetDelay(Delay);
            }
            else if (Image != null)
            {

                if (b)
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, ImageStateColor.Selected, Duration).SetDelay(Delay);
                else
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, ImageStateColor.Normal, Duration).SetDelay(Delay);
            }
            else
            {
                if (b)
                    Graphic.CrossFadeColor(StateColor.Selected, Duration, true, true);
                else
                    Graphic.CrossFadeColor(StateColor.Normal, Duration, true, true);
            }
        }
        public override void OnChecked(bool b)
        {
            if (!IsInited) return;
            if (!UseCheck) return;
            base.OnChecked(b);
            if (Graphic == null) return;
            if (colorTween != null) colorTween.Kill();
            if (Text != null)
            {
                if (b)
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, TextStateColor.Checked, Duration).SetDelay(Delay);
                else
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, TextStateColor.Normal, Duration).SetDelay(Delay);
            }
            else if (Image != null)
            {

                if (b)
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, ImageStateColor.Checked, Duration).SetDelay(Delay);
                else
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, ImageStateColor.Normal, Duration).SetDelay(Delay);
            }
            else
            {
                if (b)
                    Graphic.CrossFadeColor(StateColor.Checked, Duration, true, true);
                else
                    Graphic.CrossFadeColor(StateColor.Normal, Duration, true, true);
            }
        }
        #endregion

        #region inspector editor
        protected override bool Inspector_IsHideStateColor()
        {
            return !StateColorPreset.IsInv() && StateColorPreset != SysConst.STR_Custom;
        }
        protected string[] Inspector_StatePresets()
        {
            List<string> data = new List<string>();
            data.AddRange(UIConfig.Ins.PresenterStateColors.Keys);
            data.AddRange(UIConfig.Ins.CustomStateColors.Keys);
            return data.ToArray();
        }
        #endregion
    }
}