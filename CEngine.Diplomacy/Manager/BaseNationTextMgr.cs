//------------------------------------------------------------------------------
// BaseNationTextMgr.cs
// Copyright 2019 2019/3/13 
// Created by CYM on 2019/3/13
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CYM.Diplomacy
{
    public class BaseNationTextMgr<TUnit> : BaseUFlowMgr<TUnit>
        where TUnit : BaseUnit
    {
        #region prop
        BaseCameraMgr CameraMgr => BaseGlobal.CameraMgr;
        TextMeshPro TextMesh;
        object mapAlphaTween;
        object colorTween;
        bool IsShow = false;
        Vector3 sourceScale = Vector3.one;
        BoxCollider Collider;
        #endregion

        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            TextMesh = SelfBaseUnit.GetComponentInChildren<TextMeshPro>();
            Collider = SelfBaseUnit.GetComponentInChildren<BoxCollider>();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            CameraMgr.Callback_OnIsTopHight += OnIsTopHight;
            CameraMgr.Callback_OnIsMostHight += OnIsMostHight;
            SelfBaseUnit.Callback_OnMouseDown += OnMouseDown;
            SelfBaseUnit.Callback_OnMouseEnter += OnMouseEnter;
            SelfBaseUnit.Callback_OnMouseExit += OnMouseExit;
            SelfBaseUnit.Callback_OnMouseUp += OnMouseUp;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            CameraMgr.Callback_OnIsTopHight -= OnIsTopHight;
            CameraMgr.Callback_OnIsMostHight -= OnIsMostHight;
            SelfBaseUnit.Callback_OnMouseDown -= OnMouseDown;
            SelfBaseUnit.Callback_OnMouseEnter -= OnMouseEnter;
            SelfBaseUnit.Callback_OnMouseExit -= OnMouseExit;
            SelfBaseUnit.Callback_OnMouseUp -= OnMouseUp;
        }
        public override void OnBirth3()
        {
            base.OnBirth3();
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            Show(false,true);
        }
        #endregion

        #region overrride
        protected virtual bool HideCondition()
        {
            return false;
        }
        protected virtual Vector3 CenterPos()
        {
            throw new System.NotImplementedException("此函数必须实现");
        }
        protected virtual string TextName()
        {
            return "None";
        }
        protected virtual Color TextColor()
        {
            return Color.black;
        }
        protected virtual Color MostTopTextColor()
        {
            return Color.black + Color.white * 0.1f;
        }
        protected virtual float HightModify()
        {
            return 1.0f;
        }
        protected virtual float ScaleModify()
        {
            return 1.0f;
        }
        #endregion

        #region set
        public void Show(bool b,bool isForce=false)
        {
            if (TextMesh == null) return;
            if (HideCondition()) return;
            if (IsShow == b && !isForce) return;
            IsShow = b;
            if (mapAlphaTween != null)
                DOTween.Kill(mapAlphaTween);
            if (b)
            {
                TextMesh.color = TextColor();
                TextMesh.color = new Color(TextMesh.color.r, TextMesh.color.g, TextMesh.color.b, 0.0f);
                TextMesh.text = TextName();
                Vector3 textPos = CenterPos();
                textPos.y += Mathf.Min(10.0f, HightModify());
                SelfBaseUnit.Pos = textPos;
                float scalefaction = Mathf.Min(5.0f, ScaleModify());
                sourceScale = SelfBaseUnit.LocalScale = Vector3.one * scalefaction;
            }
            else
            {

            }
            mapAlphaTween = DOTween.To(
                () => TextMesh.color.a,
                x => TextMesh.color = new Color(TextMesh.color.r, TextMesh.color.g, TextMesh.color.b, x),
                b ? 1.0f : 0.0f,
                0.3f).OnComplete(OnComplete);
        }
        void ShowOnMostHight(bool b)
        {
            if (BaseGlobal.TerrainGridMgr != null &&
                BaseGlobal.TerrainGridMgr.IsHaveTSG)
            {
                if (HideCondition())
                    return;
                if (colorTween != null)
                    DOTween.Kill(colorTween);
                colorTween = DOTween.To(
                    () => TextMesh.color,
                    x => TextMesh.color = x,
                    b ? MostTopTextColor() : TextColor(),
                    0.1f).SetEase(Ease.Linear);
            }
        }

        private void OnComplete()
        {
            if (!IsShow)
            {
                SelfBaseUnit.Pos = SysConst.VEC_FarawayPos;
            }
        }
        #endregion

        #region Callback
        private void OnIsTopHight(bool arg1)
        {
            Show(arg1);
        }
        private void OnIsMostHight(bool arg1)
        {
            if (Collider == null) return;
            Collider.enabled = !arg1;
            ShowOnMostHight(arg1);
        }
        private void OnMouseDown()
        {
        }
        private void OnMouseUp()
        {
            if (BaseInputMgr.IsStayInUI)
                return;
            if (BaseInputMgr.IsSamePt())
            {
                OnClicked();
            }
        }
        private void OnMouseEnter()
        {

        }
        private void OnMouseExit()
        {

        }
        protected virtual void OnClicked()
        {
            BaseGlobal.AudioMgr.PlayUI("Tabclick");
        }
        #endregion
    }
}