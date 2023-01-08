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
        public string Checked = "Normal";
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
            if (!IsInited) return;
            if (UseCheck) return;
            if (animator == null) return;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!IsInited) return;
            if (UseCheck) return;
            if (animator == null) return;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInited) return;
            if (UseCheck) return;
            if (!IsInteractable) return;
            if (animator == null) return;
            animator.SetTrigger(Pressed);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInited) return;
            if (UseCheck) return;
            if (!IsInteractable) return;
            if (animator == null) return;
            animator.SetTrigger(Normal);
        }

        public override void OnInteractable(bool b)
        {
            if (!IsInited) return;
            base.OnInteractable(b);
        }
        public override void OnChecked(bool b)
        {
            if (!IsInited) return;
            if (!UseCheck) return;
            base.OnChecked(b);
            if (animator == null) return;
            animator.SetTrigger(Normal);
        }
        #endregion
    }
}