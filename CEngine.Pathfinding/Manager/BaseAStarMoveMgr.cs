//------------------------------------------------------------------------------
// BaseMoveMgr.cs
// Copyright 2019 2019/4/17 
// Created by CYM on 2019/4/17
// Owner: CYM
// 单位身上的移动组件基类
//------------------------------------------------------------------------------
using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Pathfinding
{
    [Serializable]
    public class DBBaseMove
    {
        public int CurMoveState = 0;
        public int MoveTargetState = 0;
        public long MoveTarget = SysConst.INT_Inv;
        public long FaceTarget = SysConst.INT_Inv;
        public Vec3 MoveTargetPosPreview;
        public Vec3 MoveTargetPosReal;
        public bool IsValidMoveTarget;
    }
    public class BaseTraversal : ITraversalProvider
    {
        protected BaseAStarMgr AStarPathMgr => BaseGlobal.AStarMgr;
        protected BaseUnit SelfUnit { get; private set; }

        public BaseTraversal()
        {

        }
        public void Init(BaseUnit legion)
        {
            SelfUnit = legion;
        }

        public bool CanTraverse(Path path, GraphNode node)
        {
            return Filter(node);
        }

        public uint GetTraversalCost(Path path, GraphNode node)
        {
            return DefaultITraversalProvider.GetTraversalCost(path, node);
        }

        public virtual bool Filter(GraphNode node)
        {
            bool defaulRet = node.Walkable;
            int customRet = 0;
            HashList<BaseUnit> blockerUnits = AStarPathMgr.GetBlockerUnits(node);
            if (blockerUnits != null)
            {
                foreach (var item in blockerUnits)
                {
                    if (item != null)
                    {
                        if (!OnFilter(item))
                            customRet++;
                    }
                }
            }
            return customRet==0 && defaulRet;
        }
        protected virtual bool OnFilter(BaseUnit item)
        {
            return item.IsSOF(SelfUnit) || item == SelfUnit;
        }
    }

    public class NoModifier : MonoModifier
    {
        public override int Order => 0;

        public override void Apply(Path path)
        {
            
        }
    }
    public abstract class BaseAStarMoveMgr<TState, TUnit, TTraversal, TModify> : BaseMgr, IMoveMgr, IAStarMoveMgr
        where TState : struct, Enum
        where TUnit : BaseUnit
        where TTraversal : BaseTraversal, new()
        where TModify : MonoModifier
    {
        #region Callback val
        public Callback Callback_OnMoveStart { get; set; }
        public Callback Callback_OnMoveEnd { get; set; }
        public Callback Callback_OnMovingAlone { get; set; }
        public Callback Callback_OnFirstMovingAlone { get; set; }
        public Callback Callback_OnMovingStep { get; set; }
        public Callback Callback_OnMoveDestination { get; set; }
        #endregion

        #region val
        public BaseUnit GetSelfBaseUnit() => SelfBaseUnit;
        public Quaternion NewQuateration { get; private set; } = Quaternion.identity;
        public BaseUnit FaceTarget { get; protected set; }
        public float SearchedSpeed { get; protected set; }
        public Vector3 SearchedPos { get; protected set; } = SysConst.VEC_FarawayPos;
        public Vector3 Destination { get; protected set; } = SysConst.VEC_FarawayPos;
        public float BaseMoveSpeed { get; protected set; } = 10.0f;
        public float BaseRotSpeed { get; protected set; } = 0.5f;
        public virtual float RealMoveSpeed => BaseMoveSpeed * BaseAStarMgr.MultipleSpeed;
        public virtual float RealRotSpeed=>BaseRotSpeed * BaseAStarMgr.MultipleSpeed;
        public Vector3 RotationUpward
        {
            get
            {
                if (BaseGlobal.Is2D)
                    return Vector3.forward;
                return Vector3.up;
            }
        }
        public int CurIndex { get; protected set; }
        Vector3 LastPos;
        Quaternion LastRot;
        #endregion

        #region Astar
        public GraphNode PreNode { get; protected set; }
        public GraphNode CurNode { get; protected set; }
        protected NodeRoot RootBlockerNode;
        #endregion

        #region mgr
        protected Corouter BattleCoroutine => BaseGlobal.BattleCorouter;
        protected BaseAStarMgr AStarMgr => BaseGlobal.AStarMgr;
        protected CharaStateMachine<TState, TUnit, BaseMoveState> StateMachine { get; set; } = new CharaStateMachine<TState, TUnit, BaseMoveState>();
        #endregion

        #region prop
        protected TUnit SelfUnit => SelfBaseUnit as TUnit;
        protected CoroutineHandle FollowPathCoroutine;
        public BaseTraversal Traversal { get;private set; }
        protected TModify PathModify { get; private set; }
        #endregion

        #region life
        public ABPath ABPath { get; private set; }
        public Seeker Seeker { get; private set; }
        protected virtual bool UseSeizeNode => throw new NotImplementedException();
        protected virtual bool UseBlockNode => throw new NotImplementedException();
        protected virtual bool UseFollowCoroutine => throw new NotImplementedException();
        public sealed override MgrType MgrType => MgrType.Unit;
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            Traversal = new TTraversal();
            Traversal.Init(SelfBaseUnit);
            StateMachine.Init(SelfBaseUnit);
            Seeker = SelfMono.SetupMonoBehaviour<Seeker>();
            PathModify = SelfMono.SetupMonoBehaviour<TModify>();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            AStarMgr.Callback_OnSeizeNode += OnSeizeNode;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            AStarMgr.Callback_OnSeizeNode -= OnSeizeNode;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            RootBlockerNode = SelfMono.GetComponentInChildren<NodeRoot>();
        }
        public override void OnStart()
        {
            base.OnStart();
            GrabNewQuateration();
        }
        public override void OnBirth()
        {
            base.OnBirth();
            IsLockRotationFlag = false;
        }
        public override void OnBirth3()
        {
            base.OnBirth3();
            CalcCurNode();
            CalcCurBlock();
        }
        public override void OnDeath()
        {
            base.OnDeath();
            ClearNode();
            ClearBlock();
        }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (SelfBaseUnit != null)
            {
                if (!SelfUnit.IsLive)
                    return;
                //位置变化记录
                LastPos = SelfUnit.Pos;
                LastRot = SelfUnit.Rot;
                 
                //执行旋转
                if (!IsLockRotation)
                {
                    if (!IsMoving)
                        SelfBaseUnit.Rot = Quaternion.Slerp(SelfBaseUnit.Rot, NewQuateration, Time.smoothDeltaTime * RealRotSpeed);
                    else
                        NewQuateration = SelfBaseUnit.Rot;
                }
                //使用携程
                if (UseFollowCoroutine) { }
                else
                {
                    OnFollowPathUpdate();
                }
                StateMachine?.OnUpdate();

                //位置变化记录
                IsPositionChange = !MathUtil.Approximately(LastPos, SelfUnit.Pos);
                IsRotationChange = LastRot != SelfUnit.Rot;
            }
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (SelfBaseUnit != null)
            {
                StateMachine?.OnFixedUpdate();
            }
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            SetState(StateMachine.CurState, false);
        }
        #endregion

        #region protector set
        protected virtual void AddState(TState state, BaseMoveState stateData, TState? endState = null, Func<bool> isRange = null, Func<bool> isAction = null)
        {
            StateMachine.AddState(state, stateData);
        }
        #endregion

        #region rotation
        protected Vector3 CorrectRotYByUpward(Vector3 dir)
        {
            if (BaseGlobal.Is2D)
            {
                dir.x = 0;
                dir.y = 0;
            }
            else
            {
                dir.y = 0;
            }
            return dir;
        }
        public void LockRotation(bool b)
        {
            IsLockRotationFlag = b;
            GrabNewQuateration();
        }
        public void LookDir(Vector3 dir)
        {
            IsLockRotationFlag = false;
            dir = CorrectRotYByUpward(dir);
            var normalDir = dir.normalized;
            if (normalDir == Vector3.zero)
                return;
            NewQuateration = Quaternion.LookRotation(normalDir, RotationUpward);
        }
        public void Look(Vector3 pos)
        {
            if (pos == SelfBaseUnit.Pos)
                return;

            Vector3 dir = (pos - SelfBaseUnit.Pos);
            LookDir(dir);
        }
        public void Look(BaseUnit unit)
        {
            if (unit == null) return;
            Look(unit.Pos);
            FaceTarget = unit;
        }
        public void SetRotationY(float rot)
        {
            NewQuateration = Quaternion.AngleAxis(rot, RotationUpward);
        }
        public void GrabNewQuateration(Quaternion? qua = null)
        {
            if (qua != null)
                SelfBaseUnit.Rot = qua.Value;
            NewQuateration = SelfBaseUnit.Rot;
        }
        public void RandRotationY()
        {
            RandUtil.RandForwardY(SelfBaseUnit, RotationUpward);
            GrabNewQuateration();
        }
        #endregion

        #region path
        public virtual bool StartPath(Vector3 pos, float speed)
        {
            if (!IsCanMove) return false;
            if (pos.IsInv()) return false;
            SearchedPos = pos;
            SearchedSpeed = speed;
            if (MathUtil.Approximately(pos, SelfBaseUnit.Pos))
                return false;
            Destination = OnModifyFinalPos(pos);
            if (MathUtil.Approximately(Destination, SelfBaseUnit.Pos))
                return false;
            ABPath = ABPath.Construct(SelfBaseUnit.Pos, Destination);
            ABPath.traversalProvider = Traversal;
            Seeker.StartPath(ABPath, (path) => OnPathFindCompleted(path, speed));
            return true;
        }
        public virtual void StopPath()
        {
            OnMoveEnd();
        }
        public void PreviewPath(Vector3 pos)
        {
            ABPath ABPath;
            if (pos.IsInv()) return;
            if (pos == SelfBaseUnit.Pos) return;
            pos = OnModifyFinalPos(pos);
            ABPath = ABPath.Construct(SelfBaseUnit.Pos, pos);
            ABPath.traversalProvider = Traversal;
            Seeker.StartPath(ABPath);
            AstarPath.BlockUntilCalculated(ABPath);
            if (ABPath.error)
            {
                CLog.Error(ABPath.errorLog);
            }
        }
        protected virtual bool OnPathFindCompleted(Path p, float speed)
        {
            ABPath = p as ABPath;
            //无路可走,直接返回,不要报错
            if (ABPath.vectorPath.Count == 0)
            {
                StopPath();
                OnPathNoWay();
                return false;
            }
            if (ABPath.error)
            {
                CLog.Error($"Name:{SelfBaseUnit.GetName()},Pos:{SelfBaseUnit.Pos},SearchedPos:{SearchedPos},\n{ABPath.errorLog}");
                StopPath();
                OnPathError();
                return false;
            }
            SetMoveSpeed(speed);
            OnMoveStart();
            return true;
        }
        #endregion

        #region set
        public virtual bool AdjToNode()
        {
            ClearNode();
            GraphNode node = AStarMgr.GetNode(SelfMono.Pos);
            if (node == null) return false;
            SelfMono.Pos = (Vector3)node.position;
            CalcCurNode();
            return true;
        }
        public void SetRotateSpeed(float speed)
        {
            BaseRotSpeed = speed;
        }
        public void SetMoveSpeed(float speed)
        {
            BaseMoveSpeed = speed;
        }
        public virtual void SetToNode(GraphNode node)
        {
            ClearBlock();
            ClearNode();
            SelfBaseUnit.Pos = (Vector3)node.position;
            CalcCurNode();
            CalcCurBlock();
        }
        public void ChangeState(TState state, bool isForce = false, bool isManual = true) => StateMachine.ChangeState(state, isForce, isManual);
        public void SetState(TState state, bool isManual = true) => StateMachine.SetCurState(state, isManual);
        #endregion

        #region get
        public List<Vector3> GetDetailPathPoints(ABPath path = null)
        {
            var realPath = path == null ? ABPath : path;
            List<Vector3> ret = new List<Vector3>();
            if (realPath == null) return ret;
            for (int i = 0; i < realPath.vectorPath.Count - 1; i++)
            {
                var p0 = realPath.vectorPath[i];
                var p1 = realPath.vectorPath[Mathf.Min(i + 1, realPath.vectorPath.Count - 1)];

                float step = 1f;
                int maxCount = 6;
                while (step < maxCount)
                {
                    var interpolatedPoint = Vector3.Lerp(p0, p1, step / maxCount);
                    ret.Add(interpolatedPoint);
                    step++;
                }
            }
            return ret;
        }
        protected virtual List<GraphNode> GetBlockerNode()
        {
            List<GraphNode> ret = new List<GraphNode>();
            if (CurNode != null) ret.Add(CurNode);
            if (RootBlockerNode != null)
            {
                RootBlockerNode.CalcNodes();
                ret.AddRange(RootBlockerNode.Nodes);
            }
            return ret;
        }
        #endregion

        #region is
        protected bool IsMovingFlag { get; set; } = false;
        public virtual bool IsLockRotation => IsLockRotationFlag;
        //是否广义的移动
        public virtual bool IsMoving => IsMovingFlag && IsPositionChange;
        //是否主动移动
        public bool IsActiveMoving => IsMovingFlag;
        //是否被动移动
        public bool IsPassiveMoving => !IsMovingFlag && IsPositionChange;
        //是否广泛的移动
        public bool IsGeneralMoving => IsPositionChange || IsMovingFlag;
        public virtual bool IsCanMove => true;
        public bool IsInState(TState state) => EnumTool<TState>.Int(StateMachine.CurState) == EnumTool<TState>.Int(state);
        public bool IsCanTraversal(GraphNode node)
        {
            if (Traversal == null) return true;
            return Traversal.CanTraverse(null, node);
        }
        public bool IsLockRotationFlag { get; private set; } = false;
        public bool IsPositionChange { get; private set; } = false;
        public bool IsRotationChange { get; private set; } = false;
        //当前Unit的node是否可以连接到目标Unit的Blocker范围内,一般可以用来做攻击检测
        public bool IsInBlockerRange(BaseUnit unit)
        {
            HashList<GraphNode> connection = new HashList<GraphNode>();
            CurNode.GetConnections(connection.Add);
            HashList<GraphNode> targetUnitBlocker = AStarMgr.GetBlocker(unit);
            if (targetUnitBlocker != null)
            {
                foreach (var item in targetUnitBlocker)
                {
                    if (connection.Contains(item))
                        return true;
                }
            }
            else
            {
                CLog.Error("{0}:目标单位没有Blocker", unit.BaseConfig.GetName());
            }
            return false;
        }
        public bool IsInConnection(BaseUnit target)
        {
            //如果没有链接,就返回
            HashList<GraphNode> blockers = AStarMgr.GetBlocker(SelfBaseUnit);
            if (!AStarMgr.IsConnection(blockers, target.AStarMoveMgr.CurNode))
                return false;
            return true;
        }
        public bool IsInDestination()=> MathUtil.Approximately(Destination, SelfUnit.Pos);
        public bool IsNoInDestination() => !IsInDestination();
        public bool IsInPos(Vector3 target,float k)=> MathUtil.Approximately(SelfUnit.Pos, target, k);
        #endregion

        #region Calc Node
        private void CalcCurNode()
        {
            if (!UseSeizeNode)
                return;
            PreNode = CurNode;
            CurNode = AStarMgr.GetSafeNode(SelfBaseUnit.Pos);
            if (CurNode == null) CLog.Error("没有获取到寻路节点!!!{0}", SelfBaseUnit.GOName);
            else SelfBaseUnit.Pos = (Vector3)CurNode.position;
            AStarMgr.SetSeizeNode(CurNode, SelfBaseUnit);
        }
        private void ClearNode()
        {
            if (!UseSeizeNode)
                return;
            PreNode = CurNode;
            CurNode = AStarMgr.GetSafeNode(SelfBaseUnit.Pos);
            AStarMgr.ClearSeizeNode(CurNode, SelfBaseUnit);
        }
        private void CalcCurBlock()
        {
            if (!UseBlockNode)
                return;
            AStarMgr.SetBlockNode(SelfBaseUnit, GetBlockerNode());
        }
        private void ClearBlock()
        {
            if (!UseBlockNode)
                return;
            AStarMgr.SetBlockNode(SelfBaseUnit, null);
        }
        #endregion

        #region Callback
        protected virtual void OnPathNoWay()
        { 
        
        }
        protected virtual void OnPathError()
        {

        }
        protected virtual Vector3 OnModifyFinalPos(Vector3 pos)
        {
            return pos;
        }

        protected virtual void OnMoveStart()
        {
            IsMovingFlag = true;
            BaseAStarMgr.GlobalMoveState.Add();
            StateMachine.CurStateData?.OnMoveStart();

            ClearNode();
            ClearBlock();

            GrabNewQuateration();
            Callback_OnMoveStart?.Invoke();
            if (UseFollowCoroutine)
            {
                BattleCoroutine.Kill(FollowPathCoroutine);
                FollowPathCoroutine = BattleCoroutine.Run(OnFollowPathCoroutine());
            }
        }
        protected virtual void OnMoveEnd()
        {
            IsMovingFlag = false;
            BaseAStarMgr.GlobalMoveState.Remove();
            BaseGlobal.AStarMgr?.SetSpeedRate(1.0f);
            StateMachine.CurStateData?.OnMoveEnd();

            CalcCurNode();
            CalcCurBlock();

            GrabNewQuateration();

            Callback_OnMoveEnd?.Invoke();
            if (UseFollowCoroutine)
                BattleCoroutine.Kill(FollowPathCoroutine);
        }
        protected virtual void OnSeizeNode(BaseUnit unit, GraphNode node)
        {

        }
        #endregion

        #region IEnumerator
        protected virtual IEnumerator<float> OnFollowPathCoroutine()
        {
            yield break;
        }

        protected virtual void OnFollowPathUpdate()
        {
            return;
        }
        #endregion

        #region state
        public class BaseMoveState : CharaState<TState, TUnit>
        {
            #region mgr
            protected IMoveMgr MoveMgr => SelfUnit.MoveMgr;
            #endregion

            #region life
            public virtual Color Color => Color.black;
            protected virtual bool MustPlayerDrawPath => true;
            public override void OnBeAdded()
            {
                base.OnBeAdded();
            }
            public override void Enter()
            {
                base.Enter();
            }
            public override void Exit()
            {
                base.Exit();
            }
            #endregion

            #region set
            protected void DrawPath(Color col)
            {

            }
            protected void ClearPath()
            {

            }
            #endregion

            #region Callback
            public virtual void OnMoveStart() { DrawPath(Color); }
            public virtual void OnMoveEnd() { ClearPath(); }
            #endregion
        }
        #endregion
    }
}