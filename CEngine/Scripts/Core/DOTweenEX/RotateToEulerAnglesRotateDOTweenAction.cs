﻿
using System;
using UnityEngine;

namespace CYM.DOTweenEx
{
    [Serializable]
    public sealed class RotateToEulerAnglesRotateDOTweenAction : RotateDOTweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);

        public override string DisplayName => "Rotate to Euler Angles";

        [SerializeField]
        private Vector3 eulerAngles;
        public Vector3 EulerAngles
        {
            get => eulerAngles;
            set => eulerAngles = value;
        }

        protected override Vector3 GetRotation()
        {
            return eulerAngles;
        }
    }
}
