//------------------------------------------------------------------------------
// PSAlphaTransition.cs
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
    public class UIAlphaShow : UIShow
    {
        #region prop
        public float From = 0.5f;
        public float To = 1.0f;
        CanvasGroup CanvasGroup;
        #endregion

        public override void Init(UControl self)
        {
            base.Init(self);
            CanvasGroup = Target.GetComponent<CanvasGroup>();
            if (CanvasGroup == null)
            {
                CanvasGroup = Target.AddComponent<CanvasGroup>();
            }
        }

        public override void OnShow(bool b)
        {
            base.OnShow(b);
            if (RectTrans != null)
            {
                if (CanvasGroup == null)
                    CanvasGroup = RectTrans.GetComponent<CanvasGroup>();

                if (IsReset)
                {
                    CanvasGroup.alpha = b ? From : To;
                }
                tweener = DOTween.To(() => CanvasGroup.alpha, x => CanvasGroup.alpha = x, b ? To : From, Duration)
                    .SetEase(GetEase(b))
                    .OnComplete(OnTweenComplete)
                    .SetDelay(Delay);
            }
        }

    }
}