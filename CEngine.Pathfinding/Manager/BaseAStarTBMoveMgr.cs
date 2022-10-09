//------------------------------------------------------------------------------
// BaseMoveMgr.cs
// Copyright 2019 2019/4/17 
// Created by CYM on 2019/4/17
// Owner: CYM
// 回合制游戏专用的移动组件,需要和BaseLogicTurn配合使用
//------------------------------------------------------------------------------

using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Pathfinding
{
    [Serializable]
    public class DBBaseTBMove:DBBaseMove
    {
        public float CurMovePoint = 0;
    }
    public class BaseAStarTBMoveMgr<TState, TUnit, TTraversal,TModify> : BaseAStarMoveMgr<TState, TUnit, TTraversal, TModify> , IAStarTBMoveMgr
        where TUnit : BaseUnit 
        where TState : struct, Enum
        where TTraversal : BaseTraversal, new()
        where TModify:MonoModifier
    {
        #region Callback
        public Callback Callback_OnResetMovePoint { get; set; }
        #endregion

        #region Constant
        protected sealed override bool UseBlockNode => true;
        protected sealed override bool UseSeizeNode => true;
        public virtual float PerMovePoint => MaxMovePoint == 0 ? 0 : CurMovePoint / MaxMovePoint;
        public virtual float CurMovePoint { get; set; } = 0;
        public virtual float MaxMovePoint { get; set; }
        protected override bool UseFollowCoroutine => true;
        public override bool IsCanMove => CurMovePoint > 0.0f;
        //路径展示的节点
        HashList<GraphNode> ConstantNodesDraw = new HashList<GraphNode>();
        //所有可移动的节点
        HashList<GraphNode> ConstantNodesMove = new HashList<GraphNode>();
        public bool IsForceBreak { get;private set; } = false;
        public bool IsFinalPosMove { get; private set; } = false;
        protected virtual float InitBaseMoveSpeed => 10;
        protected bool IsCanUnitOverlap { get; set; } = false;
        #endregion

        #region life
        public override void OnBirth()
        {
            base.OnBirth();
            SetMoveSpeed(InitBaseMoveSpeed);
        }
        public override void OnDeath()
        {
            base.OnDeath();
            CancleMoveTarget();
        }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedTurnbase = true;
        }
        protected override void OnMoveEnd()
        {
            base.OnMoveEnd();

            ChangeToEndState();
            if (MoveTarget_IsInTarget())
                CancleMoveTarget();
        }
        #endregion

        #region set
        public HashList<GraphNode> CalcConstant()
        {
            float range = CurMovePoint;
            ConstantNodesDraw.Clear();
            ConstantNodesMove.Clear();
            ConstantNodesMove = AStarMgr.GetDistanceRange(SelfBaseUnit.Pos, range, x => Traversal.CanTraverse(null, x));

            foreach (var item in ConstantNodesMove)
            {
                //过滤掉占有单位得节点,并且这个节点不是自身,防止重复绕路
                if (AStarMgr.IsHaveUnit(item)) continue;
                ConstantNodesDraw.Add(item);
            }
            return ConstantNodesDraw;
        }
        public void SetForceBreak(bool b)
        {
            IsForceBreak = b;
        }
        public virtual void ResetMovePoint(float? movePoint = null)
        {
            if (movePoint == null)
                CurMovePoint = MaxMovePoint;
            else CurMovePoint = movePoint.Value;
            OnResetMovePoint();
            Callback_OnResetMovePoint?.Invoke();
        }
        public void ShowPath(bool b)
        {
            if (!b)
            {
                return;
            }
            if (!SelfBaseUnit.IsPlayer()) return;
            if (!IsHaveMoveTarget()) return;
            var state = StateMachine.GetState(MoveTarget_State);
            var curState = StateMachine.CurState;
            if (!IsMoveTargetState(curState))
            {
                Vector3 realPoint = Vector3.zero;
                if (MoveTarget_PosPreview == Vector3.zero) return;
                if (MoveTarget_PosPreview.IsInv()) return;
                realPoint = (Vector3)AStarMgr.GetDistNode(SelfBaseUnit, MoveTarget_PosPreview, false, true, false, false, false).position;
                PreviewPath(realPoint);
            }
        }
        #endregion

        #region move
        public bool MoveToPos(Vector3 pos, float speed)
        {
            IsCanUnitOverlap = false;
            return StartPath(pos, speed);
        }
        // 移动到指定节点
        public bool MoveIntoNode(GraphNode node, float speed)
        {
            if (node == null) return false;
            IsCanUnitOverlap = false;
            return StartPath((Vector3)node.position, speed);
        }
        // 移动到指定单位
        public bool MoveIntoUnit(BaseUnit unit, float speed)
        {
            if (unit == null) return false;
            if (unit.Pos.IsInv()) return false;
            if (unit.MoveMgr == null) return false;
            var node = unit.AStarMoveMgr.CurNode;
            if (node == null) return false;
            IsCanUnitOverlap = true;
            return StartPath((Vector3)node.position, speed);
        }
        // 移动到指定单位边上
        public bool MoveToUnit(BaseUnit unit, float speed)
        {
            if (unit == null) return false;
            if (unit == SelfBaseUnit) return false;
            if (unit.Pos.IsInv()) return false;
            GraphNode closetNode = AStarMgr.GetClosedNode(SelfBaseUnit, unit.Pos);
            if (closetNode == null) return false;
            IsCanUnitOverlap = false;
            return StartPath((Vector3)closetNode.position, speed);
        }
        #endregion

        #region is
        //单位是否处于移动状态种
        public override bool IsMoving => FollowPathCoroutine.IsRunning || base.IsMoving;
        //移动范围是否可以链接到目标
        public bool IsCanConstantConnection(BaseUnit unit)
        {
            if (ConstantNodesMove == null || ConstantNodesMove.Count == 0) return false;
            HashList<GraphNode> links = AStarMgr.GetConnectionsBlocker(unit);
            foreach (var item in links)
            {
                if (ConstantNodesMove.Contains(item))
                    return true;
            }

            return false;
        }
        public bool IsHaveMoveTarget() => MoveTarget_IsValid;
        public bool IsMoveTargetState(TState state) => MoveTargetStateDatas.ContainsKey(state);
        public bool IsCanAutoExcuteMoveTarget() => IsCanMove && IsHaveMoveTarget();
        #endregion

        #region Callback
        public override void OnTurnbase(bool day, bool month, bool year)
        {
            base.OnTurnbase(day,month,year);
            ResetMovePoint();
        }
        public override void OnBeNewSpawned()
        {
            base.OnBeNewSpawned();
            ResetMovePoint();
        }
        protected virtual void OnResetMovePoint()
        {

        }
        protected override Vector3 OnModifyFinalPos(Vector3 pos)
        {
            return GetFinalPos(pos);
        }
        protected virtual void OnMoveStep(float movedDistance, float toalDistance, float nodeSize, float segmentLength) { }
        protected virtual bool OnPreLerpMoveAlone(Vector3 nextPos, float moveStep, float movedDistance, float toalDistance, float nodeSize, float segmentLength, bool isFinalCorrect = false)
        {
            return true;
        }
        protected virtual void OnLerpMoveAlone(float moveStep, float movedDistance, float toalDistance, float nodeSize, float segmentLength, bool isFinalCorrect = false)
        {
            CurMovePoint -= moveStep;
        }
        #endregion

        #region move target prop
        private HashSet<TState> MoveTargetStateToNode = new HashSet<TState>();
        private HashSet<TState> MoveTargetStateToUnit = new HashSet<TState>();
        //移动状态
        private Dictionary<TState, Tuple<TState, Func<bool>, Func<bool>>> MoveTargetStateDatas = new Dictionary<TState, Tuple<TState, Func<bool>, Func<bool>>>();
        //是否到达目标
        private bool MoveTarget_IsInTarget() => MathUtil.Approximately(SelfBaseUnit.Pos, MoveTarget_PosReal) || MoveTarget_Node == CurNode;
        //目标是否有效
        protected bool MoveTarget_IsValid { get; set; } = false;
        //期待的状态
        public TState MoveTarget_State { get; protected set; }
        //预览的目标位置
        public Vector3 MoveTarget_PosPreview { get; protected set; } = Vector3.zero;
        //期待的目标点
        public Vector3 MoveTarget_PosReal { get; protected set; } = Vector3.zero;
        public BaseUnit MoveTarget_Unit { get; protected set; }
        //目标节点
        public GraphNode MoveTarget_Node { get; protected set; }
        #endregion

        #region pub move target
        //执行MovaTarget,返回true:表示正在移动,返回false:表示移动完毕
        public bool ExcuteMoveTarget(bool isManual)
        {
            if (IsMoveTargetState(MoveTarget_State))
            {
                //自动执行的时候判断是否为默认状态(只有默认状态才能执行移动)
                if (!isManual && EnumTool<TState>.Int(StateMachine.CurState) != 0)
                {
                    CancleMoveTarget();
                    return false;
                }
                ChangeState(MoveTarget_State, true, true);
                //执行命令
                if (IsMoving)
                {
                    return true;
                }
                //执行移动后的Action
                else
                {
                    ChangeToEndState();
                    CancleMoveTarget();
                    return false;
                }
            }
            return false;
        }
        //设置MoveTarget
        public void SetMoveTarget(TState state, GraphNode node)
        {
            if (node == null) return;
            SetMoveTarget(state, null, node);
        }
        //设置MoveTarget
        public void SetMoveTarget(TState state, BaseUnit unit)
        {
            if (unit == null) return;
            SetMoveTarget(state, unit, null);
        }
        //取消移动目标
        public void CancleMoveTarget()
        {
            MoveTarget_IsValid = false;
            MoveTarget_State = EnumTool<TState>.Invert(0);
            MoveTarget_PosPreview = Vector3.zero;
            MoveTarget_PosReal = Vector3.zero;
            MoveTarget_Unit = null;
            MoveTarget_Node = null;
        }
        #endregion

        #region private move target
        //添加移动状态
        protected override void AddState(TState state, BaseMoveState stateData, TState? endState = null, Func<bool> isRange = null, Func<bool> isAction = null)
        {
            base.AddState(state, stateData, endState, isRange, isAction);
            if (endState != null)
            {
                MoveTargetStateDatas.Add(state, new Tuple<TState, Func<bool>, Func<bool>>(endState.Value, isRange, isAction));
                if (isRange == null && isAction == null) MoveTargetStateToNode.Add(state);
                else MoveTargetStateToUnit.Add(state);
            }
        }
        //设置移动状态
        private void SetMoveTarget(TState state, BaseUnit unit, GraphNode node)
        {
            //正确性判断
            if (unit != null && !MoveTargetStateToUnit.Contains(state))
            {
                CLog.Error("错误!SetMoveTarget,unit != null,但state确是:" + state.ToString());
                return;
            }
            else if (node != null && !MoveTargetStateToNode.Contains(state))
            {
                CLog.Error("错误!SetMoveTarget,node != null,但state确是:" + state.ToString());
                return;
            }
            //执行移动
            if (IsMoveTargetState(state))
            {
                MoveTarget_IsValid = true;
                MoveTarget_State = state;
                MoveTarget_Node = node;
                MoveTarget_Unit = unit;
                var endState = MoveTargetStateDatas[state].Item1;
                var isRange = MoveTargetStateDatas[state].Item2;
                var isAction = MoveTargetStateDatas[state].Item3;
                bool isInRange = isRange == null ? false : isRange.Invoke();
                bool isInAction = isAction == null ? false : isAction.Invoke();
                if (isInRange && isInAction)
                {
                    ChangeState(endState, true, true);
                }
                else if (IsCanMove && !isInRange)
                {
                    ExcuteMoveTarget(true);
                    SetMoveTargetPosReal(Destination);
                    SetMoveTargetPosPreview(null, null, Destination);
                }
                else
                {
                    SetMoveTargetPosPreview(unit, node, null);
                    ShowPath(true);
                }
            }
        }
        //改变移动状态到End状态
        private void ChangeToEndState()
        {
            if (IsHaveMoveTarget())
            {
                var endState = MoveTargetStateDatas[MoveTarget_State].Item1;
                var isRange = MoveTargetStateDatas[MoveTarget_State].Item2;
                var isAction = MoveTargetStateDatas[MoveTarget_State].Item3;
                bool isInRange = isRange == null ? false : isRange.Invoke();
                bool isInAction = isAction == null ? false : isAction.Invoke();
                if (isInRange && isInAction)
                {
                    ChangeState(endState, true, true);
                }
                else
                {
                    ChangeState((TState)(object)0, true, true);
                }
            }
        }
        //设置MoveTarget预览
        protected void SetMoveTargetPosPreview(BaseUnit unit, GraphNode node, Vector3? pos)
        {
            if (unit != null) MoveTarget_PosPreview = (unit.Pos);
            if (node != null) MoveTarget_PosPreview = ((Vector3)node.position);
            if (pos != null) MoveTarget_PosPreview = pos.Value;
        }
        //设置MoveTarget位置
        protected void SetMoveTargetPosReal(Vector3 pos)
        {
            MoveTarget_PosReal = pos;
            MoveTarget_Node = AStarMgr.GetSafeNode(pos);
        }
        #endregion

        #region IEnumerator

        protected override IEnumerator<float> OnFollowPathCoroutine()
        {
            IsMovingFlag = false;
            //是否为第一段移动
            bool isFirstMoved = false;
            //当前已经走过的路段
            float distanceAlongSegment = 0;
            //当前路段长度
            var segmentLength = 0.0f;
            //节点的大小
            float nodeSize = AStarMgr.Ins.data.gridGraph.nodeSize;
            //最大可以移动的距离
            float maxMoveDistance = nodeSize * MaxMovePoint;
            //已经移动的距离
            float movedDistance = 0;
            var moveStep = Time.smoothDeltaTime * RealMoveSpeed;
            IsForceBreak = false;
            IsFinalPosMove = false;
            for (int i = 0; i < ABPath.vectorPath.Count - 1; i++)
            {
                CurIndex = i;
                var p1 = ABPath.vectorPath[i];
                var p2 = ABPath.vectorPath[i + 1];
                segmentLength = Vector3.Distance(p1, p2);
                if (CurMovePoint <= segmentLength)
                {
                    p2 = (Vector3)AStarMgr.GetSafeNode(p2).position; ;
                }
                while (IsHaveMoveSegment() && !IsForceBreak)
                {
                    if (CurMovePoint < nodeSize &&
                        MathUtil.Approximately(SelfBaseUnit.Pos, p2))
                    {
                        IsForceBreak = true;
                    }
                    else LerpMove(p1, p2,1,true,false);

                    yield return Timing.WaitForOneFrame;
                    if (!isFirstMoved)
                    {
                        isFirstMoved = true;
                        Callback_OnFirstMovingAlone?.Invoke();
                    }
                }

                distanceAlongSegment -= segmentLength;
                OnMoveStep(movedDistance, maxMoveDistance, nodeSize, segmentLength);
                CurIndex++;
                if (!IsCanMove || IsForceBreak) 
                    break;
            }

            //计算最后的安全落点
            yield return Timing.WaitUntilDone(FinalPosMove());
            if (CurMovePoint < 1) CurMovePoint = 0;
            IsMovingFlag = true;
            Callback_OnMoveDestination?.Invoke();
            StopPath();

            //最后位置点的矫正
            IEnumerator<float> FinalPosMove()
            {
                IsFinalPosMove = true;
                Vector3 startPos = SelfBaseUnit.Pos;
                var finalPos = GetFinalPos(startPos);
                if (MathUtil.Approximately(startPos, finalPos)) yield break;
                //将单位移动到最终的目标节点
                distanceAlongSegment = 0;
                segmentLength = Vector3.Distance(startPos, finalPos);
                while (IsHaveMoveSegment())
                {
                    LerpMove(startPos, finalPos, 1, segmentLength > nodeSize,true);
                    yield return Timing.WaitForOneFrame;
                }
                SelfBaseUnit.Pos = finalPos;

                //如果目标无效,进入递归
                if (!MathUtil.Approximately(finalPos, GetFinalPos(SelfBaseUnit.Pos)))
                {
                    yield return Timing.WaitUntilDone(FinalPosMove());
                }
                IsForceBreak = false;
                IsFinalPosMove = false;
            }

            bool LerpMove(Vector3 p1, Vector3 p2, float speedMul = 1.0f, bool isRot = true,bool isFinalCorrect=false)
            {
                float aiSpeedMul = 1;
                if (
                    SelfBaseUnit != null &&
                    SelfBaseUnit.IsAI()
                    )
                {
                    aiSpeedMul = 10;
                }
                if (!isFinalCorrect && IsForceBreak) return false;
                var tempMoveStep = moveStep * speedMul * aiSpeedMul;
                var interpolatedPoint = Vector3.Lerp(p1, p2, distanceAlongSegment / segmentLength);
                var targetRot = Quaternion.identity;
                var newDir = CorrectRotYByUpward(p2 - p1);
                if (newDir != Vector3.zero)
                    targetRot = Quaternion.LookRotation(newDir, RotationUpward);
                if (isRot) SelfBaseUnit.Rot = Quaternion.Slerp(SelfBaseUnit.Rot, targetRot, tempMoveStep);
                bool isValid = OnPreLerpMoveAlone(interpolatedPoint, tempMoveStep, movedDistance, maxMoveDistance, nodeSize, segmentLength, isFinalCorrect);
                if (!isValid) return false;
                SelfBaseUnit.Pos = interpolatedPoint;
                movedDistance += tempMoveStep;
                distanceAlongSegment += tempMoveStep;
                OnLerpMoveAlone(tempMoveStep, movedDistance, maxMoveDistance, nodeSize, segmentLength, isFinalCorrect);
                Callback_OnMovingAlone?.Invoke();
                return true;
            }
            //是否有移动的路段
            bool IsHaveMoveSegment()
            {
                return distanceAlongSegment < segmentLength;
            }

        }
        Vector3 GetFinalPos(Vector3 pos)
        {
            Vector3 finalPos = (Vector3)AStarMgr.GetSafeNode(pos).position;
            //单位可以和目标点重叠(finalPos必须是目标位置点才能重叠,所以需要Approximately判断,Approximately判断为了防止移动中途路径上的重叠)
            if (IsCanUnitOverlap && MathUtil.Approximately(Destination, finalPos)) { }
            else finalPos = (Vector3)AStarMgr.GetDistNode(SelfBaseUnit, finalPos,false, true, true, false,false).position;
            return finalPos;
        }
        #endregion

        #region DB
        public DBBaseTBMove GetDBData()
        {
            DBBaseTBMove dbData = new DBBaseTBMove();
            dbData.CurMovePoint = CurMovePoint;
            dbData.MoveTarget = !MoveTarget_Unit.IsInv() ? MoveTarget_Unit.ID : SysConst.INT_Inv;
            dbData.FaceTarget = !FaceTarget.IsInv() ? FaceTarget.ID : SysConst.INT_Inv;
            dbData.IsValidMoveTarget = MoveTarget_IsValid;
            dbData.MoveTargetState = EnumTool<TState>.Int(MoveTarget_State);
            dbData.MoveTargetPosPreview = MoveTarget_PosPreview.ToVec3();
            dbData.MoveTargetPosReal = MoveTarget_PosReal.ToVec3();
            dbData.CurMoveState = EnumTool<TState>.Int(StateMachine.CurState);
            return dbData;
        }
        public void Load(DBBaseTBMove data)
        {
            CurMovePoint = data.CurMovePoint;
            MoveTarget_Unit = GetEntity(data.MoveTarget);
            FaceTarget = GetEntity(data.FaceTarget);
            MoveTarget_IsValid = data.IsValidMoveTarget;
            MoveTarget_State = (TState)(object)data.MoveTargetState;
            StateMachine.SetCurState((TState)(object)data.CurMoveState, false);
            SetMoveTargetPosPreview(null, null, data.MoveTargetPosPreview.V3);
            SetMoveTargetPosReal(data.MoveTargetPosReal.V3);
        }
        #endregion
    }
}