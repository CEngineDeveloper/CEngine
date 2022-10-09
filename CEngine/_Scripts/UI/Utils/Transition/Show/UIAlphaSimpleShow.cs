//------------------------------------------------------------------------------
// PSAlphaSimpleTransition.cs
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
    public class UIAlphaSimpleShow : UIShow
    {
        #region prop
        public float From = 0.5f;
        float To = 1.0f;
        Color Color;
        #endregion

        public override void Init(UControl self)
        {
            base.Init(self);
            Color = Graphic.color;
            To = Graphic.color.a;
        }

        public override void OnShow(bool b)
        {
            base.OnShow(b);
            if (RectTrans != null)
            {
                if (Graphic == null)
                    Graphic = RectTrans.GetComponent<Graphic>();

                if (IsReset)
                {
                    Color.a = b ? From : To;
                }
                tweener = DOTween.To(() => Color.a, x => Color.a = x, b ? To : From, Duration)
                    .SetEase(GetEase(b))
                    .OnComplete(OnTweenComplete)
                    .SetDelay(Delay)
                    .OnUpdate(OnAlphaTweenUpdate)
                    .OnStart(OnAlpahTweenStart);
            }
        }

        private void OnAlpahTweenStart()
        {
            Color = Graphic.color;
        }

        private void OnAlphaTweenUpdate()
        {
            Graphic.color = Color;
        }
    }
}