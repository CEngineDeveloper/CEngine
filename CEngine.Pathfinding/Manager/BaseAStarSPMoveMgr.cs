//------------------------------------------------------------------------------
// BaseAStarSPMoveMgr.cs
// Copyright 2021 2021/1/29 
// Created by CYM on 2021/1/29
// Owner: CYM
// 简单的移动管理器,没有碰撞等设置
//------------------------------------------------------------------------------

using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace CYM.Pathfinding
{
    public class BaseAStarSPMoveMgr<TState, TUnit, TTraversal, TModify> : BaseAStarMoveMgr<TState, TUnit, TTraversal, TModify>
        where TUnit : BaseUnit
        where TState : struct, Enum
        where TTraversal : BaseTraversal, new()
        where TModify : MonoModifier
    {
        #region prop
        protected sealed override bool UseFollowCoroutine => false;
        protected virtual float NextWaypointDistance => 0.2f;
        int currentWaypoint = 0;
        float distanceToWaypoint;
        bool reachedEndOfPath;
        float speedFaction;
        #endregion

        protected override void OnMoveStart()
        {
            base.OnMoveStart();
            reachedEndOfPath = false;
            currentWaypoint = 0;
        }
        protected override void OnMoveEnd()
        {
            base.OnMoveEnd();
        }
        protected sealed override IEnumerator<float> OnFollowPathCoroutine()
        {
            return base.OnFollowPathCoroutine();
        }
        protected override void OnFollowPathUpdate()
        {
            if (ABPath == null)
            {
                return;
            }
            if (!IsMovingFlag)
                return;

            if (currentWaypoint >= ABPath.vectorPath.Count)
                return ;

            reachedEndOfPath = false;
            distanceToWaypoint = 0;
            while (true)
            {
                if (currentWaypoint >= ABPath.vectorPath.Count)
                    break;

                distanceToWaypoint = MathUtil.Distance(SelfBaseUnit.Pos, ABPath.vectorPath[currentWaypoint]);
                if (distanceToWaypoint < NextWaypointDistance)
                {
                    if (currentWaypoint + 1 < ABPath.vectorPath.Count)
                        currentWaypoint++;
                    else
                    {
                        reachedEndOfPath = true;
                        break;
                    }
                }
                else break;
            }

            if (MathUtil.Approximately(SelfBaseUnit.Pos, Destination, 0.01f))
            {
                IsMovingFlag = false;
                SelfBaseUnit.Pos = Destination;
                StopPath();
                Callback_OnMoveDestination?.Invoke();
            }
            else
            {
                if (currentWaypoint >= ABPath.vectorPath.Count)
                    return;

                speedFaction = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / NextWaypointDistance) : 1f;
                Vector3 dir = (ABPath.vectorPath[currentWaypoint] - SelfBaseUnit.Pos).normalized;
                Vector3 velocity = dir * RealMoveSpeed * speedFaction;
                SelfBaseUnit.Pos += velocity * Time.deltaTime;
                SelfBaseUnit.Rot = Quaternion.Slerp(SelfBaseUnit.Rot, Quaternion.LookRotation(dir.SetY(0), Vector3.up), Time.smoothDeltaTime * RealMoveSpeed * 3.0f);
                IsMovingFlag = true;
            }
        }
    }
}