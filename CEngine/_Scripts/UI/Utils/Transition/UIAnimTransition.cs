//------------------------------------------------------------------------------
// PresenterAnimTransition.cs
// Copyright 2019 2019/4/7 
// Created by CYM on 2019/4/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
namespace CYM.UI
{
    [System.Serializable]
    public class UIAnimTransition : UITransition
    {
        #region inspector
        public string Normal = "Normal";
        public string Pressed = "Pressed";
        #endregion

        #region prop
        Animator animator;
        #endregion

        #region life
        public override void Init(UControl self)
        {
            base.Init(self);
            animator = Target.GetComponent<Animator>();
            if (animator == null)
            {
                CLog.Error("必须需要Animator组件:{0}", self.GOName);
            }
        }
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (animator == null) return;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (animator == null) return;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable) return;
            if (animator == null) return;
            animator.SetTrigger(Pressed);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable) return;
            if (animator == null) return;
            animator.SetTrigger(Normal);
        }

        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
        }
        #endregion
    }
}