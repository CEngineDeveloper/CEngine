//**********************************************
// Class Name	: Unit
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public struct DeathParam
    {
        public static DeathParam Default { get; private set; } = new DeathParam { IsDelayDespawn = true, Caster = null };
        public bool IsDelayDespawn ;
        public BaseUnit Caster;
    }
    /// <summary>
    /// 1.BaseUnit 的 NeedUpdate 和 NeedFixedUpdate 默认必须关掉,
    /// 2.因为其他的物件可能会继承BaseUnit,而很多物件不需要Update和FixedUpdate
    /// 3.OnEnable 时候会自动归位缩放
    /// </summary>
    public partial class BaseUnit : BaseCoreMono
    {
        #region mgr list
        public IUnitMgr UnitMgr { get; set; }
        public IUnitSpawnMgr<BaseUnit> SpawnMgr { get; set; }
        public Dictionary<Type, IUnitMgr> UnitMgrs { get; private set; } = new Dictionary<Type, IUnitMgr>();
        #endregion

        #region inspector
        [FoldoutGroup("Base"), SerializeField, TextArea, Tooltip("用户自定义描述")]
        protected string Desc = "";
        [FoldoutGroup("Base"), SerializeField, Tooltip("单位的TDID")]
        protected new string TDID = "";
        [FoldoutGroup("Base"), MinValue(0), SerializeField, Tooltip("单位的队伍")]
        public int Team = 0;
        #endregion

        #region base
        public TDBaseData BaseConfig { get; protected set; } = new TDBaseData();
        public DBBaseUnit DBBaseData { get; protected set; } = new DBBaseUnit();
        //如果没有Owner，则Owner会设置为自身
        public BaseUnit BaseOwner { get; protected set; }
        public BaseUnit BasePreOwner { get; protected set; }
        public DeathParam DeathParam { get; private set; }
        #endregion

        #region prop
        protected bool IsCanGamePlayInput => BaseInputMgr.IsCanGamePlayInput();
        #endregion

        #region timer
        Timer DeathRealTimer = new Timer();
        Timer DeathEffStartTimer = new Timer();
        #endregion

        #region Callback
        public event Callback<bool> Callback_OnTurnStart;
        public event Callback Callback_OnTurnEnd;
        public event Callback Callback_OnCantEndTurn;
        public event Callback Callback_OnPreEndTurn;
        public event Callback Callback_OnTurnOperating;
        public event Callback Callback_OnUnBeSetPlayer;
        public event Callback Callback_OnBeSetPlayer;
        public event Callback Callback_OnMouseDown;
        public event Callback Callback_OnMouseUp;
        public Callback Callback_OnMouseEnter { get; set; }
        public Callback Callback_OnMouseExit { get; set; }
        public event Callback<bool> Callback_OnBeSelected;
        public event Callback Callback_OnUnBeSelected;
        public event Callback<BaseUnit> Callback_OnSetOwner;
        public static Callback<BaseUnit> Callback_OnRealDeathG { get; internal set; }
        public static Callback<BaseUnit> Callback_OnDeathG { get; internal set; }
        #endregion

        #region time
        public virtual float DeathDespawnTime => DeathRealTime + 0.1f; // 彻底消除的时间
        public virtual float DeathRealTime => 3.0f; // 从Death到RealDeath的时间
        public virtual float DeathEffStartTime => 1.0f;// 死亡效果开始的时间
        #endregion

        #region life
        public override MonoType MonoType => MonoType.Unit;
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnEnable()
        {
            Trans.localScale = Vector3.one;
            base.OnEnable();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            UpdateRendered();
            if (!IsLive && !IsRealDeath)
            {
                if (DeathRealTimer.CheckOverOnce())
                    OnRealDeath();
                else if (DeathEffStartTimer.CheckOverOnce())
                    OnDissolve();
            }
        }
        public void OnTurnStart(bool isForce)
        {
            Callback_OnTurnStart?.Invoke(isForce);
        }
        public void OnTurnEnd()
        {
            Callback_OnTurnEnd?.Invoke();
        }
        public void OnCantEndTurn()
        {
            Callback_OnCantEndTurn?.Invoke();
        }
        public void OnPreEndTurn()
        {
            Callback_OnPreEndTurn?.Invoke();
        }
        public void OnTurnOperating()
        {
            Callback_OnTurnOperating?.Invoke();
        }
        #endregion

        #region unit life
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            RegetNode();
        }
        public override void OnInit()
        {
            IsLive = false;
            base.OnInit();
            if (DeathEffStartTime > DeathRealTime)
                throw new Exception("溶解的时间不能大于死亡时间");
        }
        public override void OnDeath()
        {
            if (!IsLive) return;
            IsLive = false;
            Callback_OnDeathG?.Invoke(this);
            DeathRealTimer.Restart(DeathRealTime);
            DeathEffStartTimer.Restart(DeathEffStartTime);
            UnitMgr?.Despawn(this);
            base.OnDeath();
        }
        public override void OnBirth()
        {
            if (IsLive) return;
            IsLive = true;
            IsRealDeath = false;
            base.OnBirth();
        }
        public override void OnRealDeath()
        {
            Callback_OnRealDeathG?.Invoke(this);
            IsRealDeath = true;
            base.OnRealDeath();
        }
        protected virtual void UpdateRendered()
        {
            if (BaseGlobal.CameraMgr == null)
                return;
            if (!BaseGlobal.CameraMgr.IsEnable)
                return;
            if (BaseGlobal.CameraMgr.MainCamera == null)
                return;
            Vector3 pos = BaseGlobal.CameraMgr.MainCamera.WorldToViewportPoint(Trans.position);
            IsRendered = (pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f);
            if (IsRendered != IsLastRendered)
            {
                if (IsRendered) OnBeRender();
                else OnBeUnRender();

                IsLastRendered = IsRendered;
            }
        }
        protected virtual void OnBecameInvisible() => IsVisible = false;
        protected virtual void OnBecameVisible() => IsVisible = true;
        #endregion

        #region life set
        public override T AddComponent<T>()
        {
            var ret = base.AddComponent<T>();
            //加入组件列表
            if (ret is IUnitMgr entityMgr) UnitMgrs.Add(entityMgr.UnitType, entityMgr);
            return ret;
        }
        #endregion

        #region set
        // 设置小队
        public void SetTeam(int? team)
        {
            if (team.HasValue)
                Team = team.Value;
        }
        // 设置TDID
        public void SetTDID(string tdid)
        {
            if (tdid.IsInv())
            {
                base.TDID = TDID = gameObject.name;
            }
            else
            {
                base.TDID = TDID = tdid;
                if (int.TryParse(tdid,out int intID))
                {
                    INID = intID;
                }
            }
        }
        public virtual void SetRTID(long rtid) => ID = rtid;
        public virtual void SetConfig(TDBaseData config)
        {
            if (config == null) 
                config = new TDBaseData();
            BaseConfig = config;
            BaseConfig.OnBeAdded(this);
        }
        public void SetOwner(BaseUnit owner)
        {
            BasePreOwner = BaseOwner;
            BaseOwner = owner;
            //只有非读取数据阶段才能触发回调
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnSetOwner?.Invoke(owner);
            }
        }
        //设置DB数据
        public virtual void SetDBData(DBBaseUnit dbData)
        {
            DBBaseData = dbData;
        }
        //设置死亡参数
        public void SetDeathParam(DeathParam param)
        {
            DeathParam = param;
        }
        //执行删除动作，死亡，解散等
        public virtual void DoDeath()
        {
            OnDeath();
            Clear();
            SpawnMgr?.Despawn(this, DeathParam.IsDelayDespawn ? DeathDespawnTime : 0);
        }
        public override void OnDisable()
        {
            base.OnDisable();
            Clear();
        }
        #endregion

        #region get
        public virtual string GetTDID() => TDID;
        //获得综合评分
        public float Score =>0;
        public override string ToString()
        {
            if (BaseConfig == null) return base.ToString();
            return BaseConfig.GetName();
        }
        public string GetName()
        {
            if (BaseConfig == null) return base.ToString();
            return BaseConfig.GetName();
        }
        public string GetDesc()
        {
            if (BaseConfig == null) return base.ToString();
            return BaseConfig.GetDesc(); 
        }
        public Sprite GetIcon()
        {
            if (BaseConfig == null) return null;
            return BaseConfig.GetIcon();
        }
        public IUnitMgr GetUnitMgr(Type unitType)
        {
            if (UnitMgrs.ContainsKey(unitType))
            {
                return UnitMgrs[unitType];
            }
            return null;
        }
        #endregion

        #region is
        // 是否为系统类型
        public bool IsSystem { get; set; }
        // 是否为荒野类型
        public virtual bool IsWild => BaseConfig.IsWild;
        // 是否死亡
        public bool IsLive { get; protected set; } = false;
        // 是否真的死亡
        public bool IsRealDeath { get; protected set; } = false;
        // 是否被渲染(计算位置是否在摄像机中)
        public bool IsRendered { get; private set; } = false;
        // 是否被摄像机渲染
        public bool IsVisible { get; private set; } = false;
        // 上一帧被渲染
        public bool IsLastRendered { get; private set; } = false;
        // 是否为本地玩家
        public virtual bool IsPlayer() => BaseGlobal.ScreenMgr.Player == this;
        // 是否为其他玩家
        public virtual bool IsPlayerCtrl() => IsPlayer();
        public virtual bool IsAI() => !IsPlayerCtrl();
        // 是否是敌人
        public virtual bool IsEnemy(BaseUnit other)
        {
            if (other == null)
                return false;
            return other.Team != Team;
        }
        // 是否是友军
        public virtual bool IsFriend(BaseUnit other)
        {
            if (other == null)
                return false;
            return other.Team == Team;
        }
        // Self or Friend
        public virtual bool IsSOF(BaseUnit other)
        {
            if (other == null)
                return false;
            return IsFriend(other) || IsSelf(other);
        }
        // 是否为本地玩家的对立面
        public virtual bool IsOpposite() => false;
        // 是否为自己
        public virtual bool IsSelf(BaseUnit other)
        {
            if (other == null)
                return false;
            return this == other;
        }
        // 是否为中立怪
        public virtual bool IsNeutral() => Team == 2;
        // 非中立怪 敌人
        public virtual bool IsUnNeutralEnemy(BaseUnit other)
        {
            if (other == null)
                return false;
            if (other.IsNeutral())
                return false;
            return IsEnemy(other);
        }
        #endregion

        #region callback
        protected virtual void OnBeRender() { }
        protected virtual void OnBeUnRender() { }
        protected virtual void OnMouseDown()
        {
            if (Application.isMobilePlatform) return;
            if (!IsCanGamePlayInput) return;
            if (!BaseGlobal.BattleMgr.IsLoadBattleEnd) return;
            Callback_OnMouseDown?.Invoke();
        }
        protected virtual void OnMouseUp()
        {
            if (Application.isMobilePlatform) return;
            if (!IsCanGamePlayInput) return;
            if (!BaseGlobal.BattleMgr.IsLoadBattleEnd) return;
            Callback_OnMouseUp?.Invoke();
        }
        protected virtual void OnMouseEnter()
        {
            if (Application.isMobilePlatform) 
                return;
            if (!BaseGlobal.BattleMgr.IsLoadBattleEnd)
                return;
            if (BaseInputMgr.IsStayInUI)
                return;
            BaseGlobal.InputMgr.DoEnterUnit(this);
        }

        protected virtual void OnMouseExit()
        {
            if (Application.isMobilePlatform) 
                return;
            if (!BaseGlobal.BattleMgr.IsLoadBattleEnd) 
                return;
            if (BaseInputMgr.IsStayInUI)
                return;
            BaseGlobal.InputMgr.DoExitUnit(this);
        }

        public virtual void OnBeSelected(bool isRepeat)
        {
            if (!IsCanGamePlayInput) return;
            Callback_OnBeSelected?.Invoke(isRepeat);
        }
        public virtual void OnUnBeSelected()
        {
            Callback_OnUnBeSelected?.Invoke();
        }
        public virtual void OnUnBeSetPlayer()
        {
            Callback_OnUnBeSetPlayer?.Invoke();
        }
        public virtual void OnBeSetPlayer()
        {
            Callback_OnBeSetPlayer?.Invoke();
        }

        #endregion

        #region inspector
        [Button("CopyName")]
        void CopyName()
        {
            Util.CopyTextToClipboard(GOName);
        }
        public virtual void AdjHeight()
        { 
        
        }
        #endregion

        #region node
        Dictionary<int, Transform> Bones = new Dictionary<int, Transform>();
        Dictionary<string, Transform> ExtendBones = new Dictionary<string, Transform>();
        protected Transform Model { get; private set; }
        public void RegetNode()
        {
            Model = Trans;
            Bones.Clear();
            ExtendBones.Clear();
            Transform[] trans = GetComponentsInChildren<Transform>();
            for (int i = 0; i < trans.Length; ++i)
            {
                if (trans[i].name == SysConst.STR_Model)
                    Model = trans[i];
            }
            if (Model == null)
                return;

            Bone[] bonescom = GetComponentsInChildren<Bone>();
            if (bonescom != null)
            {
                foreach (var item in bonescom)
                {
                    int index = (int)item.Type;
                    if (index != -1)
                    {
                        if (Bones.ContainsKey(index))
                        {
                            Bones[index] = item.Trans;
                        }
                        else
                        {
                            Bones.Add(index, item.Trans);
                        }
                    }
                    else
                    {
                        string name = item.ExtendName;
                        if (ExtendBones.ContainsKey(name))
                        {
                            CLog.Error("ExtenBone 名称重复:{0}", name);
                        }
                        else
                        {
                            ExtendBones.Add(name, item.Trans);
                        }
                    }
                }

            }
        }
        public Transform GetExtendBone(string name)
        {
            if (name.IsInv())
                return null;
            if (ExtendBones.ContainsKey(name))
            {
                return ExtendBones[name];
            }
            else
            {
                CLog.Error("没有这个拓展骨骼:{0}", name);
                return null;
            }
        }
        public Transform GetNode(NodeType nodeType)
        {
            if (nodeType == NodeType.None)
                return null;
            if (Bones == null)
                return null;
            int boneIndex = (int)nodeType;
            if (!Bones.ContainsKey(boneIndex))
            {
                if (nodeType == NodeType.Center ||
                    nodeType == NodeType.Top ||
                    nodeType == NodeType.Pivot ||
                    nodeType == NodeType.Muzzle)
                {
                    if (Model == null)
                        CLog.Error("单位没有Model error id=" + ID);
                    GameObject temp = new GameObject("pos-" + nodeType);
                    temp.transform.parent = Model;
                    temp.transform.localScale = Vector3.one;
                    temp.transform.localRotation = Quaternion.identity;
                    temp.transform.position = GetVirtualPos(nodeType);
                    BaseGlobal.CommonCorouter.Run(DelayPos(temp, nodeType));
                    Bones.Add((int)nodeType, temp.transform);
                }
                else
                {
                    CLog.Error("no this bone in the unit" + name + " ,error bone name=" + nodeType);
                    return null;
                }
            }
            return Bones[boneIndex];

            IEnumerator<float> DelayPos(GameObject go, NodeType type)
            {
                yield return Timing.WaitForOneFrame;
                if (go != null)
                    go.transform.position = GetVirtualPos(type);
            }
        }
        public Vector3 GetPos(NodeType nodeType)
        {
            if (nodeType == NodeType.None)
                return SysConst.VEC_FarawayPos;
            Transform boneTrans = GetNode(nodeType);
            if (boneTrans == null)
            {
                CLog.Error("no this bone in the unit" + name + " ,error bone name=" + nodeType);
                return Vector3.one;
            }
            return boneTrans.position;

        }
        private Vector3 GetVirtualPos(NodeType nodeType)
        {
            if (nodeType == NodeType.Top)
                return GetTop();
            if (nodeType == NodeType.Center)
                return GetCenter();
            if (nodeType == NodeType.Pivot)
                return Pos;
            return GetCenter();
        }
        public Vector3 GetCenter()
        {
            return Pos + GetHight() * 0.5f * Vector3.up;//
        }
        public Vector3 GetTop()
        {
            return Pos + GetHight() * Vector3.up;// + Mono.Pos;
        }
        public virtual float GetHight()
        {
            return 1.0f;
        }
        #endregion
    }
}