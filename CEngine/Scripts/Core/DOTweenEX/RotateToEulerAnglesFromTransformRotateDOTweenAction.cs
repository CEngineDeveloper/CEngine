﻿
using System;
using UnityEngine;

namespace CYM.DOTweenEx
{
    [Serializable]
    public sealed class RotateToEulerAnglesFromTransformRotateDOTweenAction : RotateDOTweenActionBase
    {
        public override string DisplayName => "Rotate To Transform Euler Angles";

        [SerializeField]
        private Transform target;
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        [SerializeField]
        private bool useLocalEulerAngles;
        public bool UseLocalEulerAngles
        {
            get => useLocalEulerAngles;
            set => useLocalEulerAngles = value;
        }
        
        protected override Vector3 GetRotation()
        {
            if (!useLocalEulerAngles)
                return target.eulerAngles;
            return target.localEulerAngles;
        }
    }
}
