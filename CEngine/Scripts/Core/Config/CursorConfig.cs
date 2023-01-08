//------------------------------------------------------------------------------
// CursorConfig.cs
// Copyright 2018 2018/11/1 
// Created by CYM on 2018/11/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public sealed class CursorConfig : ScriptableObjectConfig<CursorConfig>
    {
        #region inspector
        [SerializeField]
        public List<Texture2D> Normal = new List<Texture2D>();
        [SerializeField]
        public List<Texture2D> Wait = new List<Texture2D>();
        [SerializeField]
        public List<Texture2D> Press = new List<Texture2D>();
        [SerializeField]
        public List<Texture2D> Unit = new List<Texture2D>();
        [SerializeField]
        public Dictionary<string, List<Texture2D>> AnimCursor = new Dictionary<string, List<Texture2D>>();
        [SerializeField]
        public AudioClip PressSound;
        #endregion

        #region life
        public override void OnInited()
        {
        }
        #endregion

        #region get
        public List<Texture2D> GetTexs(string key)
        {
            if (AnimCursor.ContainsKey(key))
                return AnimCursor[key];
            return null;
        }
        #endregion
    }
}