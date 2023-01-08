//------------------------------------------------------------------------------
// BaseHUDBubble.cs
// Copyright 2018 2018/2/28 
// Created by CYM on 2018/2/28
// Owner: CYM
// 聊天气泡HUD类,介于跳字和血条之间,会显示一段时间,然后消失
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UHUDBubble : UHUDText
    {
        #region Inspector
        [SerializeField]
        Image Bg;
        #endregion

        #region prop
        CanvasGroup Group;
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
            Group = BaseMono.GetUnityComponet<CanvasGroup>(GO);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            Group.alpha = Color.a;
        }
        #endregion
    }
}