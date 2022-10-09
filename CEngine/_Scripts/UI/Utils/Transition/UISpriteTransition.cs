//------------------------------------------------------------------------------
// PresenterSpriteTransition.cs
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
    public class UISpriteTransition : UITransition
    {
        #region Inspector
        public Sprite Normal = null;
        public Sprite Enter = null;
        public Sprite Press = null;
        public Sprite Disable = null;
        public Sprite Selected = null;
        #endregion

        #region life
        public override void Init(UControl self)
        {
            base.Init(self);
            if (Image != null)
            {
                Image.overrideSprite = Normal;
            }
            if (Image == null)
            {
                CLog.Error("必须需要Image组件:{0}", self.GOName);
            }
        }
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (Image != null)
            {
                Image.overrideSprite = Enter;
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (Image != null)
            {
                Image.overrideSprite = Normal;
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (Image != null)
            {
                Image.overrideSprite = Press;
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (Image != null)
            {
                Image.overrideSprite = Normal;
            }
        }

        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
            if (Graphic == null) return;
            if (Image != null)
            {
                if (b)
                    Image.overrideSprite = Normal;
                else
                    Image.overrideSprite = Disable;
            }
        }
        public override void OnSelected(bool b)
        {
            if (!IsInteractable) return;
            base.OnSelected(b);
            if (Graphic == null) return;
            if (Image != null)
            {
                if (b)
                    Image.overrideSprite = Selected;
                else
                    Image.overrideSprite = Normal;
            }
        }
        #endregion
    }
}