//------------------------------------------------------------------------------
// BaseState.cs
// Copyright 2019 2019/7/25 
// Created by CYM on 2019/7/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM
{
    /// <summary>
    /// None=-1
    /// Yes=0
    /// No=1
    /// </summary>
    public class UStateData : UData
    {
        public List<string> States = new List<string>();
        public Func<int> GetIndex = () => -1;
    }

    //Index:-1为空，0同意，1拒绝，2未知
    [AddComponentMenu("UI/Control/UState")]
    [HideMonoScript]
    public class UState : UPres<UStateData>
    {
        #region Inspector
        [SerializeField, Required, SceneObjectsOnly]
        Image IActiveIcon;
        [SerializeField, SceneObjectsOnly]
        Image IHoverIcon;
        [SerializeField, AssetsOnly]
        List<Sprite> List;
        #endregion

        #region prop
        public int CurIndex { get; private set; } = -1;
        public bool IsChecked => CurIndex == 0;
        #endregion

        #region life
        public override bool IsAtom => true;
        protected override void Start()
        {
            base.Start();
            SetState(CurIndex);
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Data.GetIndex != null)
                SetState(Data.GetIndex());
        }
        #endregion

        #region set
        public void SetState(int index)
        {
            IHoverIcon?.CrossFadeAlpha(0.0f, 0.05f, true);
            CurIndex = index;
            if (CurIndex == -1)
            {
                IActiveIcon.CrossFadeAlpha(0.0f, 0.1f, true);
                return;
            }
            else
            {
                if (Data.States == null || Data.States.Count <= CurIndex)
                {
                    if (List == null || CurIndex >= List.Count) return;
                    IActiveIcon.sprite = List[CurIndex];
                }
                else
                {
                    IActiveIcon.sprite = Data.States[CurIndex].GetIcon();
                }
                IActiveIcon.CrossFadeAlpha(1.0f, 0.1f, true);
            }
        }
        #endregion

        #region Callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (IHoverIcon != null && CurIndex == -1)
                IHoverIcon.CrossFadeAlpha(0.5f, 0.1f, true);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (IHoverIcon != null)
                IHoverIcon.CrossFadeAlpha(0.0f, 0.1f, true);
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            //如果不是ToggleGroup 则执行以下操作
            if (CheckCanClick() &&
                IsInteractable)
            {
                SetState(IsChecked ? -1 : 0);
            }
            IHoverIcon?.CrossFadeAlpha(0.0f, 0.1f, true);
            base.OnPointerClick(eventData);
        }
        #endregion
    }
}