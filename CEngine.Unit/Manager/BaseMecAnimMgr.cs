using System;
using System.Collections.Generic;
using UnityEngine;
namespace CYM.Unit
{
    public interface IMecAnimMgr
    {
        void ChangeState(int state, int index = 0);
    }
    public class BaseMecAnimMgr<TUnit, TState> : BaseAnimMgr , IMecAnimMgr
        where TState : Enum 
        where TUnit : BaseUnit
    {
        #region Callback
        public event Callback<TState, TState, int> Callback_OnChangeState;
        #endregion

        #region member variable
        protected TUnit SelfUnit => SelfBaseUnit as TUnit;
        /// <summary>
        /// 动画播放器：注意这个对象是可以为空的，比如塔，水晶，基地等一些建筑物
        /// </summary>
        public Animator Animator { get; private set; }
        public AnimatorTransitionInfo BaseAnimatorTransitionInfo;
        public AnimatorStateInfo BaseAnimatorStateInfo;
        public RuntimeAnimatorController SourceAnimator { get; private set; }
        #endregion

        #region prop
        public StateMachine<State> Machine { get; private set; } = new StateMachine<State>();
        public Dictionary<int, BaseCharaState> States = new Dictionary<int, BaseCharaState>();
        public TState State { get; private set; }
        public int Index { get; private set; } = 0;
        public TState PreState { get; private set; }
        public int PreIndex { get; private set; } = 0;
        #endregion

        #region life
        protected virtual GameObject AnimatorObj => SelfMono.GO;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedFixedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            Animator = AnimatorObj.GetComponentInChildren<Animator>();
            if (Animator == null)
            {
                CLog.Error("错误 该对象没有Animator:" + SelfMono.name);
                return;
            }
            if (Animator.runtimeAnimatorController == null)
            {
                CLog.Error("错误 该对象没有runtimeAnimatorController:" + SelfMono.name);
                return;
            }
            Animator.cullingMode = AnimatorCullingMode.CullCompletely;
            SourceAnimator = Animator.runtimeAnimatorController;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            ClearState();
            Machine.Init(SelfBaseUnit as TUnit);

            EnumTool<TState>.For((x) =>
            {
                AddState(x, new BaseCharaState());
            });
        }
        public override void OnBirth()
        {
            base.OnBirth();
        }
        public override void OnInit()
        {
            base.OnInit();

        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Animator != null && 
                SelfMono.GO.activeSelf && 
                Animator.runtimeAnimatorController != null)
            {
                if (!Animator.isInitialized)
                    return;
                BaseAnimatorTransitionInfo = Animator.GetAnimatorTransitionInfo(0);
                BaseAnimatorStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            }
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            Machine.OnFixedUpdate();
        }
        #endregion

        #region set
        public override void EnableAnim(bool b)
        {
            Animator.enabled = b;
        }

        public virtual void SetTrigger(string name)
        {
            Animator.SetTrigger(name);
        }

        public override void Play(string animName, bool fixedTime = true)
        {
            if (Animator == null)
                return;
            else
            {
                if (fixedTime)
                    Animator.PlayInFixedTime(animName);
                else
                    Animator.Play(animName);
            }
        }
        public override void CrossFade(string animName, float transDuration = 0.05f, bool fixedTime = true)
        {
            if (Animator == null)
                return;
            else
            {
                if (fixedTime)
                    Animator.CrossFadeInFixedTime(animName, transDuration);
                else
                    Animator.CrossFade(animName, transDuration);
            }
        }
        /// <summary>
        ///直接改变一个状态
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(TState state, int index = 0)
        {
            ChangeState(EnumTool<TState>.Int(state),index);
        }
        public void ChangeState(int state, int index = 0)
        {
            int intState = EnumTool<TState>.Int(State);
            int intstate = state;
            if (intState == intstate && Index == index)
                return;

            PreIndex = Index;
            PreState = State;

            Index = index;
            State = EnumTool<TState>.Invert(state);

            Machine?.ChangeState(States[intstate]);
            Callback_OnChangeState?.Invoke(State, PreState, Index);
        }
        /// <summary>
        /// 添加一个状态
        /// </summary>
        /// <param name="type"></param>
        /// <param name="state"></param>
        public void AddState(TState type, BaseCharaState state)
        {
            int key = EnumTool<TState>.Int(type);
            state.Self = SelfBaseUnit as TUnit;
            if (States.ContainsKey(key))
            {
                States[key] = state;
            }
            else
            {
                States.Add(key, state);
            }

        }
        /// <summary>
        /// 清空状态
        /// </summary>
        void ClearState()
        {
            States.Clear();
        }
        public void SetSourceAndCurAnimator(RuntimeAnimatorController animator)
        {
            SetRuntimeAnimatorController(animator);
            SourceAnimator = animator;
        }
        public void SetAnimator(RuntimeAnimatorController animator)
        {
            if (animator != null)
            {
                SetRuntimeAnimatorController(animator);
            }
            else
            {
                RevertAnimator();
            }

        }
        public void RevertAnimator()
        {
            SetRuntimeAnimatorController(SourceAnimator);
        }
        private void SetRuntimeAnimatorController(RuntimeAnimatorController runtimeCtrl)
        {
            if (runtimeCtrl == null)
                return;
            Animator.runtimeAnimatorController = runtimeCtrl;
        }
        public void ApplyRootMotion(bool b)
        {
            Animator.applyRootMotion = b;
        }
        #endregion

        #region is
        public override bool IsName(string animName, int layer = 0)
        {
            base.IsName(animName);
            if (Animator == null)
                return false;
            else
            {
                return Animator.GetCurrentAnimatorStateInfo(layer).IsName(animName);
            }
        }
        public bool IsTag(string animTag, int layer = 0)
        {
            if (Animator == null)
                return false;
            else
            {
                return Animator.GetCurrentAnimatorStateInfo(layer).IsTag(animTag);
            }
        }
        public bool IsInEnd(string name)
        {
            if (IsName(name) && BaseAnimatorStateInfo.normalizedTime >= 1)
                return true;
            return false;
        }
        public bool IsInEnd()
        {
            if (BaseAnimatorStateInfo.normalizedTime >= 1)
                return true;
            return false;
        }
        public override bool IsTransitionName(string animName, int layer = 0)
        {
            base.IsTransitionName(animName);
            if (Animator == null)
                return false;
            else
            {
                return Animator.GetAnimatorTransitionInfo(layer).IsName(animName);
            }
        }
        public virtual bool IsNextName(string animName, int layer = 0)
        {
            if (Animator == null)
                return false;
            else
            {
                return Animator.GetNextAnimatorStateInfo(layer).IsName(animName);
            }
        }
        public override bool IsInTranslation(int layer = 0)
        {
            if (Animator == null)
                return false;

            return Animator.IsInTransition(layer);
        }
        /// <summary>
        /// 是否在指定状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool IsIn(TState state)
        {
            int intState = EnumTool<TState>.Int(State);
            int intstate = EnumTool<TState>.Int(state);
            return intState == intstate;
        }
        /// <summary>
        /// 判断上一个状态
        /// </summary>
        /// <param name="state"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsInPreState(TState state, int index)
        {
            int intState = EnumTool<TState>.Int(State);
            int intstate = EnumTool<TState>.Int(state);
            return intState == intstate && PreIndex == index;
        }
        #endregion

        #region get
        /// <summary>
        /// 转换hash
        /// </summary>
        /// <returns></returns>
        public static int StringToHash(string id)
        {
            return Animator.StringToHash(id);
        }
        public AnimatorStateInfo GetCurAnimatorStateInfo()
        {
            BaseAnimatorStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            return BaseAnimatorStateInfo;
        }
        #endregion

        public class BaseCharaState : State
        {
            public float Wait = 0.0f;
            public TUnit Self;
            public BaseCharaState() : base()
            {
            }
            public override void Update()
            {
                base.Update();
                if (UpdateTime >= Wait)
                {

                }
            }
        }
    }
}
