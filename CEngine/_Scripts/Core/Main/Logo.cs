//------------------------------------------------------------------------------
// BaseLogoView.cs
// Copyright 2018 2018/3/17 
// Created by CYM on 2018/3/17
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
#endif

namespace CYM.UI
{
    public sealed class Logo : MonoBehaviour
    {
        [SerializeField]
        public Image BG;
        [SerializeField]
        public Image Image;
        [SerializeField]
        public UVideo Video;
    }
}