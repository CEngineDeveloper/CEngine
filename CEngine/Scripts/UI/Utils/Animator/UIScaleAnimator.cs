//------------------------------------------------------------------------------
// ScaleAnimator.cs
// Copyright 2021 2021/2/27 
// Created by CYM on 2021/2/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using DG.Tweening;
namespace CYM
{
    [System.Serializable]
    public class UIScaleAnimator : UIAnimator 
    {
        #region prop
        public Vector2 From = Vector3.one * 0.1f;
        Vector2 To = Vector3.one;
        #endregion

        public override void OnShow(bool b)
        {
            base.OnShow(b);
            Vector2 realTweenVec = Vector2.zero;
            if (RectTrans != null)
            {
                if (b)
                {
                    RectTrans.localScale = From;
                    realTweenVec = To;
                }
                else
                {
                    RectTrans.localScale = To;
                    realTweenVec = From;
                }

                if (b || IsEffClose)
                    tweener = RectTrans.DOScale(realTweenVec, Duration)
                    .SetEase(GetEase(b))
                    .SetDelay(Delay);
            }
        }
    }
}