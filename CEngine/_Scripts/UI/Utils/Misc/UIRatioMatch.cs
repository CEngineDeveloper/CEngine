//------------------------------------------------------------------------------
// UIObjMatch.cs
// Copyright 2021 2021/5/20 
// Created by CYM on 2021/5/20
// Owner: CYM
// 可以使一个游戏对象根据屏幕的缩放保持在指定的比例
//------------------------------------------------------------------------------
using UnityEngine;
namespace CYM.UI
{
    public class UIRatioMatch : BaseCoreMono 
    {
        #region inspector
        [SerializeField]
        float DefaultHeight=1920;
        [SerializeField]
        float DefaultScale = 0.8f;
        [SerializeField]
        float DefaultXOffset = -16;
        [SerializeField]
        float DefaultYOffset = -200;
        #endregion

        #region prop
        RectTransform RectTrans;
        #endregion

        #region life
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void Awake()
        {
            base.Awake();
            RectTrans = transform as RectTransform;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            float percent = Screen.height / DefaultHeight;
            RectTrans.localScale = new Vector3(percent, percent, 1) * DefaultScale;
            RectTrans.anchoredPosition = new Vector3(percent * DefaultXOffset, percent * DefaultYOffset,1);
        }
        #endregion
    }
}