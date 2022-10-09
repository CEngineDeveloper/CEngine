//------------------------------------------------------------------------------
// EffectScale.cs
// Copyright 2019 2019/1/25 
// Created by CYM on 2019/1/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using UnityEngine;

namespace CYM.Unit
{
    public class EffectScale : BaseMono
    {
        [SerializeField]
        float Duration = 0.5f;
        [SerializeField]
        Ease Ease = Ease.Linear;

        Tween tween;
        Vector3 SourceScale = Vector3.one;

        #region life
        public override void Awake()
        {
            base.Awake();
            SourceScale = transform.localScale;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            Trans.localScale = Vector3.one * 0.01f;
            if (tween != null) tween.Kill();
            tween = Trans.DOScale(SourceScale, Duration).SetEase(Ease);
        }
        #endregion


    }
}