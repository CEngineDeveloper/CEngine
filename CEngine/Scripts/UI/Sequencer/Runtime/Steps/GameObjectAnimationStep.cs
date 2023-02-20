
using System;
using UnityEngine;

namespace CYM.UI.Sequencer
{
    [Serializable]
    public abstract class GameObjectAnimationStep : AnimationStepBase
    {
        [SerializeField]
        protected GameObject target;
        public GameObject Target
        {
            get => target;
            set => target = value;
        }

        [SerializeField]
        protected float duration = 1;
        public float Duration
        {
            get => duration;
            set => duration = value;
        }

        public void SetTarget(GameObject newTarget)
        {
            target = newTarget;
        }
    }
}
