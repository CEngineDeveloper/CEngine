//------------------------------------------------------------------------------
// UITweenString.cs
// Copyright 2019 2019/6/9 
// Created by CYM on 2019/6/9
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CYM
{
    public class UITweenString : UITween
    {
        #region inspector
        [SerializeField, TextArea]
        protected string Target;
        #endregion

        UnityEngine.UI.Text Text;

        #region life
        public override void Awake()
        {
            base.Awake();
            Text = Trans.GetComponent<UnityEngine.UI.Text>();
        }
        #endregion

        public override void DoTween()
        {
            if (tween != null)
                tween.Kill();
            tween = DOTween.To(() => Text.text, (x) => Text.text = x, Target, Duration)
                .SetLoops(LoopCount, LoopType)
                .SetEase(Ease);
        }
    }
}