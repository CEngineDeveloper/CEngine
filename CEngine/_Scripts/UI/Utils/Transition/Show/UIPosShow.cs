//------------------------------------------------------------------------------
// PSPositionTransition.cs
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
    public class UIPosShow : UIShow
    {
        #region prop
        public Vector2 From = Vector2.one * 0.5f;
        Vector2 To = Vector2.one;
        #endregion

        public override void OnShow(bool b)
        {
            base.OnShow(b);
            if (ShowCount == 0)
            {
                To = SelfControl.SourceAnchoredPosition;
            }
            if (RectTrans != null)
            {
                if (From.x == -1)
                    From.x = RectTrans.anchoredPosition.x;
                if (From.y == -1)
                    From.y = RectTrans.anchoredPosition.y;

                if (IsReset)
                {
                    RectTrans.anchoredPosition = b ? From : To;
                }
                tweener = DOTween.To(() => RectTrans.anchoredPosition, (x) => RectTrans.anchoredPosition = x, b ? To : From, Duration)
                    .SetEase(GetEase(b))
                    .OnComplete(OnTweenComplete)
                    .SetDelay(Delay);
            }
        }
    }
}