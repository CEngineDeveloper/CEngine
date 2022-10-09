//------------------------------------------------------------------------------
// BaseStaticUIView.cs
// Copyright 2019 2019/2/12 
// Created by CYM on 2019/2/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CYM.UI
{
    public class UStaticUIView<T> : UUIView 
        where T : UStaticUIView<T>
    {
        public static T Default { get; protected set; }

        [FoldoutGroup("Data"), SerializeField, Tooltip("是否为默认")]
        protected bool IsDefault = false;

        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            if (Default == null)
                Default = this as T;
            if (IsDefault)
                SetAsDefault();
        }

        /// <summary>
        /// 设置为默认的全局View
        /// </summary>
        public void SetAsDefault()
        {
            Default = this as T;
        }


    }
}