//------------------------------------------------------------------------------
// BaseSaveOrLoadItem.cs
// Copyright 2019 2019/5/27 
// Created by CYM on 2019/5/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class USaveOrLoadItem : UCheck
    {
        #region life
        public override bool IsAtom => false;
        #endregion

        #region inspector
        [SerializeField, FoldoutGroup("Data")]
        public Image ArchiveIcon;
        [SerializeField, FoldoutGroup("Data")]
        public Text Time;
        [SerializeField, FoldoutGroup("Data")]
        public UnityEngine.UI.Text Duration;
        [SerializeField, FoldoutGroup("Data")]
        public UI.UButton BntClose;
        #endregion
    }
}