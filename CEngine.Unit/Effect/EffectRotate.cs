//------------------------------------------------------------------------------
// EffectRotate.cs
// Copyright 2018 2018/4/17 
// Created by CYM on 2018/4/17
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM.Unit
{
    public class EffectRotate : BaseCoreMono
    {
        public override LayerData LayerData => null;

        [SerializeField]
        float UpSpeed = 10.1f;
        [SerializeField]
        float ForwardSpeed = 0.0f;
        [SerializeField]
        float RightSpeed = 0.0f;
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            if (Trans == null)
                return;
            transform.Rotate(Vector3.up, UpSpeed * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.forward, ForwardSpeed * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.right, RightSpeed * Time.deltaTime, Space.Self);
        }

    }
}