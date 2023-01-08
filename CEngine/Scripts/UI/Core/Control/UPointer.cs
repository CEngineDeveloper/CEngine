//------------------------------------------------------------------------------
// UPointer.cs
// Copyright 2021 2021/4/14 
// Created by CYM on 2021/4/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using CYM.UI;
using Sirenix.OdinInspector;

namespace CYM
{
    public class UPointer : UControl 
    {
        [SerializeField, ChildGameObjectsOnly]
        GameObject IPointer;

        public override bool IsAtom => true;


        #region set
        public void SetPosAndRot(Vector3 pos,Vector3 rot)
        {
            Pos = pos;
            IPointer.transform.rotation = Quaternion.Euler(rot);
        }
        #endregion
    }
}