//------------------------------------------------------------------------------
// PSScaleTransition.cs
// Copyright 2019 2019/4/7 
// Created by CYM on 2019/4/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using UnityEngine;

namespace CYM.UI
{
    [System.Serializable]
    public class UIScaleShow : UIShow
    {
        #region prop
        public Vector2 From = Vector3.one * 0.5f;
        Vector2 To = Vector3.one;
        #endregion

        #region life
        public override void Init(UControl self)
        {
            base.Init(self);
            To = self.RectTrans.localScale;
        }
        public override void OnShow(bool b)
        {
            base.OnShow(b);
            if (RectTrans != null)
            {
                if (IsReset)
                {
                    RectTrans.localScale = b ? From : To;
                }
                tweener = RectTrans.DOScale(b ? To : From, Duration)
                    .SetEase(GetEase(b))
                    .OnComplete(OnTweenComplete)
                    .SetDelay(Delay);
            }
        }
        #endregion
    }
}