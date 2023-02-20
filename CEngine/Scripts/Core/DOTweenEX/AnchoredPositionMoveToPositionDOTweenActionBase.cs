using System;
using UnityEngine;

namespace CYM.DOTweenEx
{
    [Serializable]
    public sealed class AnchoredPositionMoveToPositionDOTweenActionBase : AnchoredPositionMoveDOTweenActionBase
    {
        [SerializeField]
        private Vector2 position;

        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public override string DisplayName => "Move To Anchored Position";

        protected override Vector2 GetPosition()
        {
            return position;
        }
    }
}
