//------------------------------------------------------------------------------
// PSColorTransition.cs
// Copyright 2019 2019/4/7 
// Created by CYM on 2019/4/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    [System.Serializable]
    public class UIColorShow : UIShow
    {
        #region prop
        public Color From = Color.gray;
        public Color To = Color.white;
        #endregion

        public override void OnShow(bool b)
        {
            base.OnShow(b);
            if (RectTrans != null)
            {
                if (IsReset)
                {
                    Graphic.color = b ? From : To;
                }
                tweener = DOTween.To(() => Graphic.color, x => Graphic.color = x, b ? To : From, Duration)
                    .SetEase(GetEase(b))
                    .OnComplete(OnTweenComplete)
                    .SetDelay(Delay);
            }
        }

    }
}