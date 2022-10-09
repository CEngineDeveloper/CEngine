using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
//**********************************************
// Class Name	: BasePerform
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM.Unit
{
    #region Perform
    public enum PerformFollowType
    {
        None = 0,//没有任何跟随
        Self = 1,//指定位置
        Follow = 3,//跟随
        Attach = 4,//附加在某个点
    }
    public enum PerformRotateType
    {
        None = 0,
        Normal = 1, //基于配置输入的角度
        Self = 2,   //基于载体角度
        Caster = 3, //基于释放者的角度
        CasterH = 4,//基于释放者的角度,适用于横版
    }
    public enum PerformScaleType
    {
        None = 0,//没有缩放
        Volume = 1,//更具单位的体积值缩放
        Width = 2,
        High = 3,

    }
    public enum PerformDispearType
    {
        None,
        Flicker,
        Scale,
    }
    #endregion

    public class BasePerform : BaseCoreMono, ITriggerObj
    {
        #region Inspector
        [FoldoutGroup("Condition"), SerializeField, Tooltip("缩放到最小,当粒子关闭的时候")]
        public bool IsScaleWhenCloseTime = false;
        [FoldoutGroup("Condition"), SerializeField, Tooltip("单位死亡的时候销毁")]
        public bool IsDetroyWhenUnitDeath = false;
        [FoldoutGroup("Condition"), SerializeField, Tooltip("否碰撞到之后销毁")]
        public bool IsDestroyWhenTriggerEnter = false;
        [FoldoutGroup("Condition"), SerializeField, Tooltip("是否FixedUpdate,和Update互斥,适合没有位移的物体,可以提高效率")]
        public bool IsFixedUpdate = false;

        [FoldoutGroup("Audio"), SerializeField, Tooltip("播放的音效")]
        public AudioClip AudioClip;
        [FoldoutGroup("Audio"), SerializeField, Tooltip("是否是2D音效"), ShowIf("Inspector_ShowAudioClip")]
        public bool Is2DClip = false;
        [FoldoutGroup("Audio"), SerializeField, Tooltip("是否循环音效"), ShowIf("Inspector_ShowAudioClip")]
        public bool IsLoopAudioClip = false;
        [FoldoutGroup("Audio"), SerializeField, Tooltip("是否缓存音效,缓存后在同一帧的时候不会播放相同的音效"), ShowIf("Inspector_ShowAudioClip")]
        public bool IsCacheAudioClip = true;
        [FoldoutGroup("Audio"), SerializeField, Tooltip("是否停止音效在特效结束的时候"), ShowIf("Inspector_ShowAudioClip")]
        public bool IsStopAudioOnDestroy = true;

        [FoldoutGroup("Time"), SerializeField, Tooltip("粒子关闭的时间")]
        public float CloseTime;
        [FoldoutGroup("Time"), SerializeField, Tooltip("生命周期")]
        public float LifeTime = FOREVER_LIFE_TIME;
        [FoldoutGroup("Time"), SerializeField, Tooltip("Collider生命周期")]
        public float ColliderLifeTime = FOREVER_LIFE_TIME;

        [FoldoutGroup("Transform"), SerializeField, Tooltip("跟随类型")]
        public PerformFollowType FollowType;
        [FoldoutGroup("Transform"), SerializeField, Tooltip("旋转类型")]
        public PerformRotateType RotateType;
        [FoldoutGroup("Transform"), SerializeField, Tooltip("缩放类型")]
        public PerformScaleType ScaleType;
        [FoldoutGroup("Transform"), SerializeField, Tooltip("所挂的节点")]
        public NodeType NodeType;
        [FoldoutGroup("Transform"), SerializeField, Tooltip("位置偏移")]
        public Vector3 Offset;
        [FoldoutGroup("Transform"), SerializeField, Tooltip("旋转偏移")]
        public Vector3 Rotate;

        [FoldoutGroup("Misc"), SerializeField, Tooltip("消失类型")]
        public PerformDispearType DispearType = PerformDispearType.None;
        [FoldoutGroup("Misc"), SerializeField, Tooltip("贴地层级")]
        public LayerMask TouchGroundLayer = 0;
        #endregion

        #region property
        protected AudioSource AudioSource;
        protected BaseCoreMono SelfMono;
        protected BaseCoreMono CastMono;
        protected BaseCoreMono TargetMono;
        protected BaseUnit Self;
        protected BaseUnit Cast;
        protected BaseUnit Target;
        protected ParticleSystem[] _particleSystems;
        protected MeshRenderer[] _meshRenders;
        protected TrailRenderer[] _trailRenders;
        protected SkinnedMeshRenderer[] _skinnedMeshRenders;
        protected float[] _particleSystemsSize;
        protected Animator[] _animators;
        protected Animation[] _animations;
        protected Collider[] _colliders;
        protected Timer lifeTimer = new Timer();
        protected Timer colliderLifeTimer = new Timer();
        protected Vector3 sourceScale = Vector3.one;
        protected bool isVisible = true;
        protected Vector3 MinScale = Vector3.one * 0.0001f;
        protected Tweener ScaleTweener;
        protected Transform FollowOwner;
        protected BaseAudioMgr AudioMgr => BaseGlobal.AudioMgr;
        public BasePerformMgr PerformMgr { get; set; }
        #endregion

        #region const
        static readonly float FOREVER_LIFE_TIME = 0.0f;
        #endregion

        #region Callback Val
        public Callback<Collider, BasePerform> Callback_OnTriggerIn { get; set; }
        public Callback<Collider, BasePerform> Callback_OnTriggering { get; set; }
        public Callback<Collider, BasePerform> Callback_OnTriggerOut { get; set; }
        public Callback<BasePerform> Callback_OnLifeOver { get; set; }
        public Callback<BasePerform> Callback_OnDoDestroy { get; set; }
        public Callback<BasePerform> Callback_OnCreated { get; set; }
        #endregion

        #region is
        /// <summary>
        /// 特效是否需要紧贴地面
        /// </summary>
        /// <returns></returns>
        public bool IsTouchGroundY() => TouchGroundLayer > 0;
        public bool IsLifeOver { get; protected set; } = false;
        public bool IsColliderLifeOver { get; protected set; } = false;
        public bool UseFollow { get; set; } = true;
        public Vector3 LastedPos { get; set; } = Vector3.zero;
        #endregion

        #region life
        public override LayerData LayerData => SysConst.Layer_Perform;
        public override void Awake()
        {
            base.Awake();
            _particleSystems = GO.GetComponentsInChildren<ParticleSystem>();
            _animators = GO.GetComponentsInChildren<Animator>();
            _animations = GO.GetComponentsInChildren<Animation>();
            _colliders = GO.GetComponentsInChildren<Collider>();
            _skinnedMeshRenders = GO.GetComponentsInChildren<SkinnedMeshRenderer>();
            _meshRenders = GO.GetComponentsInChildren<MeshRenderer>();
            _trailRenders = GO.GetComponentsInChildren<TrailRenderer>();
            for (int i = 0; i < _colliders.Length; ++i)
                _colliders[i].isTrigger = true;
            _storeSize();
        }
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = !IsFixedUpdate;
            NeedFixedUpdate = IsFixedUpdate;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            lifeTimer.Restart();
            colliderLifeTimer.Restart();
            IsLifeOver = false;
            IsColliderLifeOver = false;
            if (IsScaleWhenCloseTime)
                Trans.localScale = Vector3.one;
            //清空Trail
            ClearTrails();
        }
        public override void OnDisable()
        {
            base.OnDisable();
            IsLifeOver = true;
            IsColliderLifeOver = true;
            Callback_OnLifeOver?.Invoke(this);
            OnStopSFX();
        }

        public override void OnUpdate()
        {
            UpdateLifeTime();
            UpdateColliderLifeTime();
            UpdateEffect();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            UpdateLifeTime();
            UpdateColliderLifeTime();
            UpdateEffect();
        }
        //计算特效的生命周期
        void UpdateLifeTime()
        {
            if (LifeTime == FOREVER_LIFE_TIME)
            {
                if (!IsLifeOver)
                    LastedPos = Pos;
            }
            else
            {
                if (lifeTimer.Elapsed() > LifeTime && !IsLifeOver)
                    DoDestroy();
                else
                    LastedPos = Pos;
            }
        }
        void UpdateColliderLifeTime()
        {
            if (ColliderLifeTime != FOREVER_LIFE_TIME)
            {
                if (colliderLifeTimer.Elapsed() > ColliderLifeTime && !IsColliderLifeOver)
                    SetCollidersActive(false);
            }
        }
        void UpdateEffect()
        {
            if (FollowOwner != null)
            {
                Pos = FollowOwner.position + Offset;
            }
        }

        public void DoDestroy()
        {
            Callback_OnDoDestroy?.Invoke(this);
            IsLifeOver = true;
            PerformMgr.Despawn(this);
        }

        public virtual void OnCreate(BaseCoreMono self, BaseCoreMono cast, BaseCoreMono target)
        {
            SelfMono = self;
            CastMono = cast;
            TargetMono = target;
            Self = self as BaseUnit;
            Cast = cast as BaseUnit;
            Target = target as BaseUnit;
            if (UseFollow)
                SetFollow();
            TouchGround();
            SetLifeTime(LifeTime);
            OnPlaySFX();
            LastedPos = Pos;

            if (DispearType == PerformDispearType.Flicker)
            {
                StopFlick();
            }
            else if (DispearType == PerformDispearType.Scale)
            {
                Trans.localScale = Vector3.one;
            }
            Callback_OnCreated?.Invoke(this);
        }
        public virtual void OnClose()
        {
            if (IsScaleWhenCloseTime)
                Trans.localScale = MinScale;
            //flicker effect
            if (DispearType == PerformDispearType.Flicker && CloseTime > 0.0f)
            {
                StartFlick(0.0f, CloseTime);
            }
            else if (DispearType == PerformDispearType.Scale)
            {
                if (ScaleTweener != null)
                    ScaleTweener.Kill();
                ScaleTweener = Trans.DOScale(MinScale, CloseTime);
            }
            StopParticle();
        }
        protected virtual void OnPlaySFX()
        {
            if (AudioClip != null)
            {
                if (Is2DClip)
                {
                    AudioSource = AudioMgr.PlayUI(AudioClip, IsLoopAudioClip);
                }
                else
                {
                    AudioSource = AudioMgr.PlaySFX(AudioClip, Pos, IsLoopAudioClip, IsCacheAudioClip);
                }
            }
        }
        protected virtual void OnStopSFX()
        {
            if (IsStopAudioOnDestroy)
            {
                if (AudioSource != null)
                    AudioSource.Stop();
            }
        }
        #endregion

        #region 粒子操控
        public BasePerform PlayParticle()
        {
            if (_particleSystems != null)
            {
                for (int i = 0; i < _particleSystems.Length; ++i)
                {
                    if (_particleSystems[i] == null)
                        continue;
                    _particleSystems[i].Play();
                }
            }
            return this;
        }
        public BasePerform StopParticle()
        {
            if (_particleSystems != null)
            {
                for (int i = 0; i < _particleSystems.Length; ++i)
                {
                    if (_particleSystems[i] == null)
                        continue;
                    _particleSystems[i].Stop();
                }
            }
            return this;
        }
        public BasePerform SetParticleSpeed()
        {
            return this;
        }
        public virtual BasePerform SetParticleSize(float size)
        {
            if (_particleSystems != null)
            {
                for (int i = 0; i < _particleSystems.Length; ++i)
                {
                    ParticleSystem.MainModule main = _particleSystems[i].main;
                    main.startSizeMultiplier = size;
                }
            }
            return this;
        }
        public virtual BasePerform SetParticleScale(float size)
        {
            Trans.localScale = Vector3.one * size;
            return this;
        }
        public override void SetCollidersActive(bool enable)
        {
            base.SetCollidersActive(enable);
            IsColliderLifeOver = !enable;
        }
        private void _storeSize()
        {
            sourceScale = Trans.localScale;
        }
        private void _assignSize()
        {
            Trans.localScale = sourceScale;
        }
        //是否是附加在角色身上的特效
        public bool IsAttachedUnit => FollowType == PerformFollowType.Attach || FollowType == PerformFollowType.Follow;
        public virtual void Show(bool b) => SetVisible(b);
        /// <summary>
        /// 是否隐藏特效
        /// </summary>
        /// <param name="b"></param>
        public void SetVisible(bool b)
        {
            if (b == isVisible)
                return;
            isVisible = b;
            //粒子效果
            if (_particleSystems != null)
            {
                for (int i = 0; i < _particleSystems.Length; ++i)
                {
                    if (_particleSystems[i] == null)
                        continue;
                    if (b)
                        _particleSystems[i].Play();
                    else
                        _particleSystems[i].Stop();
                }
            }
            //静态网格
            if (_meshRenders != null)
            {
                for (int i = 0; i < _meshRenders.Length; ++i)
                {
                    if (_meshRenders[i] == null)
                        continue;
                    _meshRenders[i].enabled = b;
                }
            }
            //蒙皮网格
            if (_skinnedMeshRenders != null)
            {
                for (int i = 0; i < _skinnedMeshRenders.Length; ++i)
                {
                    if (_skinnedMeshRenders[i] == null)
                        continue;
                    _skinnedMeshRenders[i].enabled = b;
                }
            }
            //拖尾
            if (_trailRenders != null)
            {
                for (int i = 0; i < _trailRenders.Length; ++i)
                {
                    if (_trailRenders[i] == null)
                        continue;
                    _trailRenders[i].enabled = b;
                }
            }
        }
        public void ClearTrails()
        {
            if (_trailRenders != null)
            {
                for (int i = 0; i < _trailRenders.Length; ++i)
                {
                    if (_trailRenders[i] == null)
                        continue;
                    _trailRenders[i].Clear();
                }
            }
        }
        #endregion

        #region 设置属性
        public virtual BasePerform SetFollow(Transform customBone = null)
        {
            if (Trans == null)
            {
                CLog.Error("没有Trans:{0}", gameObject.name);
                return this;
            }
            Transform boneTrans = null;
            Vector3 bonePos = Vector3.one * 999.0f;
            {
                //自定义节点
                if (customBone != null)
                {
                    boneTrans = customBone;
                    bonePos = customBone.position;
                }
                //配置节点
                else
                {
                    boneTrans = GetBone(NodeType);
                    bonePos = GetBonePos(NodeType);
                    if (boneTrans == null)
                        return this;
                }
            }
            SetRotate(RotateType, Rotate);
            switch (FollowType)
            {
                case PerformFollowType.None:
                    Trans.Translate(Offset, Space.Self);
                    break;
                case PerformFollowType.Self:
                    Pos = bonePos;// + Offset;
                    Trans.Translate(Offset, boneTrans /*Space.Self*/);
                    break;
                case PerformFollowType.Follow:
                    //effectFollow = GetUnityComponet<EffectFollow>(GO);
                    //effectFollow.SetFollowObj(boneTrans, Offset);
                    FollowOwner = boneTrans;
                    break;
                case PerformFollowType.Attach:
                    Trans.parent = boneTrans;
                    LocalPos = Offset;
                    break;
            }
            SetScale();
            return this;
        }
        protected virtual void SetRotate(PerformRotateType rotateType, Vector3 rotate)
        {
            switch (rotateType)
            {
                case PerformRotateType.None:
                    break;
                case PerformRotateType.Normal:
                    Trans.Rotate(rotate);
                    break;
                case PerformRotateType.Self:
                    {
                        Rot = Quaternion.Euler(rotate + SelfMono.Rot.eulerAngles);
                        break;
                    }
                case PerformRotateType.Caster:
                    {
                        Vector3 dir = SelfMono.Pos - CastMono.Pos;
                        dir.Normalize();
                        Rot = Quaternion.Euler(rotate + Quaternion.LookRotation(dir).eulerAngles);
                        break;
                    }
                case PerformRotateType.CasterH:
                    {
                        Vector3 dir = SelfMono.Pos - CastMono.Pos;
                        dir.z = 0.0f;
                        dir.Normalize();
                        Rot = Quaternion.Euler(rotate + Quaternion.LookRotation(dir).eulerAngles);
                        break;
                    }
            }
        }
        protected virtual void SetScale()
        {
            switch (ScaleType)
            {
                case PerformScaleType.Volume:
                    float size = GetSize();
                    SetParticleScale(size);
                    SetParticleSize(size);
                    break;
                case PerformScaleType.Width:
                    SetParticleScale(SelfMono.Trans.lossyScale.x);
                    SetParticleSize(SelfMono.Trans.lossyScale.x);
                    break;
                case PerformScaleType.High:
                    SetParticleScale(SelfMono.Trans.lossyScale.y);
                    SetParticleSize(SelfMono.Trans.lossyScale.y);
                    break;
            }
            _assignSize();
        }
        public virtual void TouchGround()
        {
            //紧贴地面
            if (IsTouchGroundY())
            {
                Vector3 pos = Util.GetRaycastY(Trans, 0.1f, TouchGroundLayer);
                Pos = new Vector3(Pos.x, pos.y, Pos.z);
            }
        }
        public BasePerform SetLifeTime(float lifeTime)
        {
            LifeTime = lifeTime;
            return this;
        }
        #endregion

        #region Callback
        protected virtual void OnTriggerEnter(Collider collider)
        {
            Callback_OnTriggerIn?.Invoke(collider, this);
            if (IsDestroyWhenTriggerEnter)
            {
                DoDestroy();
            }
        }
        protected virtual void OnTriggerExit(Collider collider)
        {
            Callback_OnTriggerOut?.Invoke(collider, this);
        }
        protected virtual void OnTriggerStay(Collider collider)
        {
            Callback_OnTriggering?.Invoke(collider, this);
        }
        public void DoTriggerObjectEnter(Collider other, TriggerObj triggerObj, bool forceSense)
        {
            OnTriggerEnter(other);
        }

        public void DoTriggerObjectExit(Collider other, TriggerObj triggerObj, bool forceSense)
        {
            OnTriggerExit(other);
        }
        #endregion

        #region 纯虚函数
        protected virtual Transform GetBone(NodeType type)
        {
            if(SelfMono!=null)
                return SelfMono.Trans;
            return null;
        }
        protected virtual Vector3 GetBonePos(NodeType type)
        {
            return GetBone(type).position;
        }
        protected virtual float GetSize()
        {
            return 1;

        }
        #endregion

        #region inspector
        bool Inspector_ShowAudioClip()
        {
            return AudioClip != null;
        }
        #endregion

        #region Flick
        CoroutineHandle FlickCoroutineHandle;
        public void StopFlick()
        {
            BaseGlobal.BattleCorouter.Kill(FlickCoroutineHandle);
            foreach (var g in _meshRenders) g.enabled = (true);
        }

        public void StartFlick(float pauzeBeforeStart = 0.0f, float duration = 1.0f)
        {
            BaseGlobal.BattleCorouter.Kill(FlickCoroutineHandle);
            FlickCoroutineHandle = BaseGlobal.BattleCorouter.Run(Enum_Flicker(pauzeBeforeStart, duration));
        }
        IEnumerator<float> Enum_Flicker(float pauzeBeforeStart = 0.0f, float duration = 1.0f)
        {
            float flickerSpeedStart = 15f;
            float flickerSpeedEnd = 35f;
            yield return Timing.WaitForSeconds(pauzeBeforeStart);

            float t = 0;
            while (t < 1)
            {
                float speed = Mathf.Lerp(flickerSpeedStart, flickerSpeedEnd, MathUtil.Coserp(0, 1, t));
                float i = Mathf.Sin(Time.time * speed);
                foreach (var g in _meshRenders) g.enabled = (i > 0);
                t += Time.deltaTime / duration;
                yield return Timing.WaitForOneFrame;
            }
        }
        #endregion

    }
}
