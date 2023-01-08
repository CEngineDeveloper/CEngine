//------------------------------------------------------------------------------
// SceneGO.cs
// Copyright 2021 2021/1/10 
// Created by CYM on 2021/1/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using Sirenix.OdinInspector;

namespace CYM
{
    [HideMonoScript]
    public class SceneObj<TMono,TIns> :MonoBehaviour 
        where TMono: Component
        where TIns : SceneObj<TMono, TIns>
    {
        public static TIns Ins { get; private set; }
        public static TMono Obj { get; private set; }
        public static GameObject GO { get; private set; }
        protected virtual void Awake()
        {
            Ins = this as TIns;
            Obj = GetComponentInChildren<TMono>();
            GO = Ins.gameObject;
        }
        private void OnDestroy()
        {
            Ins = null;
            Obj = null;
            GO = null;
        }
    }
}