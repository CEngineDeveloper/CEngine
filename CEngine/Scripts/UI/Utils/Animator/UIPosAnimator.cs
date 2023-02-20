//------------------------------------------------------------------------------
// PosAnimator.cs
// Copyright 2021 2021/2/27 
// Created by CYM on 2021/2/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using DG.Tweening;
using CYM.UI;

namespace CYM
{
    [System.Serializable]
    public class UIPosAnimator : UIAnimator 
    {
        #region prop
        public Vector2 From = Vector2.zero;
        Vector2 To;
        #endregion

        public override void Init(UUIView self)
        {
            base.Init(self);
            if (From.x == 0)
                From.x = SourceAnchoredPosition.x;
            if (From.y == 0)
                From.y = SourceAnchoredPosition.y;
        }
        public override void OnShow(bool b)
        {
            base.OnShow(b);
            Vector2 realTweenVec = Vector2.zero;
            if (RectTrans != null)
            {
                To = SourceAnchoredPosition;
                if (b)
                {
                    RectTrans.anchoredPosition = From;
                    realTweenVec = To;
                }
                else 
                {
                    RectTrans.anchoredPosition = To;
                    realTweenVec = From;
                }
                if (b || IsEffClose)
                    tweener = DOTween.To(() => RectTrans.anchoredPosition, (x) => RectTrans.anchoredPosition = x, realTweenVec, Duration)
                    .SetEase(GetEase(b))
                    .SetDelay(Delay);
            }
        }
    }
}