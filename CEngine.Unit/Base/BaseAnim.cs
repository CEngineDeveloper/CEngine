using System;
using UnityEngine;

namespace CYM.Unit
{
    public class BaseAnim<TUnit, TState> : StateMachineBehaviour where TUnit : BaseUnit where TState : Enum
    {
        [SerializeField]
        public TState State;
        [SerializeField]
        public int Index;
        protected TUnit Self;
        protected float sourceSpeed = 1.0f;
        [SerializeField]
        bool IsApplyRootMotion = false;
        [SerializeField]
        [Tooltip("状态机会等待Transition后进入OnStateEntered")]
        bool IsIgnoreTransition = true;

        bool IsEnterTrigger = false;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (Self == null)
            {
                Self = animator.gameObject.GetComponent<TUnit>();
                if (Self == null)
                {
                    Self = animator.transform.parent.gameObject.GetComponent<TUnit>();
                }
            }
            animator.applyRootMotion = IsApplyRootMotion;
            IsEnterTrigger = true;
            if (Self.AnimMgr != null)
            {
                (Self.AnimMgr as IMecAnimMgr).ChangeState(EnumTool<TState>.Int(State), Index);
            }
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (IsEnterTrigger)
            {
                if (IsIgnoreTransition && animator.IsInTransition(0))
                    return;
                IsEnterTrigger = false;
                OnStateEntered();
            }
        }

        protected virtual void OnStateEntered()
        {

        }
    }
}
