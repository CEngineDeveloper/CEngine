//------------------------------------------------------------------------------
// BaseAStarRVOMgr.cs
// Created by CYM on 2022/3/17
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using Pathfinding;
using System;
using System.Collections.Generic;
using Pathfinding.RVO;

namespace CYM.Pathfinding
{
    public class BaseAStarRVOMgr<TState, TUnit, TTraversal, TModify> : BaseAStarMoveMgr<TState, TUnit, TTraversal, TModify>
        where TUnit : BaseUnit
        where TState : struct, Enum
        where TTraversal : BaseTraversal, new()
        where TModify : MonoModifier
    {
		#region prop
		protected virtual float MinDesiredSpeed => 0.5f;
		//减速距离
		protected virtual float SlowdownDistance => 1;
		//抵达目标点的精度
		protected virtual float ArrivalAccuracy => 0.1f;
		//抵达目标点的Timeout,原地踏步超过这个时间将会强制停止移动
		protected virtual float ArrivalTimeout => 0.5f;
		protected sealed override bool UseBlockNode => false;
		protected sealed override bool UseSeizeNode => false;
        protected sealed override bool UseFollowCoroutine => false;
		protected virtual float MaxSpeed => 10;
		protected virtual float MinSpeed => 0.5f;
		protected RVOController RVOController { get; private set; }
		#endregion

		#region prop
		List<Vector3> vectorPath;
		float curArrivalTime = 0;
		bool isCanpath = false;
		float repathRate = 1;
		float nextRepath = 0;
	    bool canSearchAgain = true;
		Path path = null;

		int wp;
	    float moveNextDist = 1;
        #endregion

        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
			RVOController = SelfMono.SetupMonoBehaviour<RVOController>();
		}
        protected override void OnMoveStart()
        {
            base.OnMoveStart();
			curArrivalTime = 0;
		}
        protected override void OnMoveEnd()
        {
            base.OnMoveEnd();
			curArrivalTime = 0;
		}
		public void ToggleRepeatPath(bool b)
		{
			isCanpath = b;
		}
        public override bool StartPath(Vector3 pos, float speed)
		{
			canSearchAgain = false;
			return base.StartPath(pos, speed);
        }
        protected sealed override IEnumerator<float> OnFollowPathCoroutine()
        {
            return base.OnFollowPathCoroutine();
        }
		void RecalculatePath()
		{
			if (!isCanpath)
				return;
			nextRepath = Time.time + repathRate * (UnityEngine.Random.value + 0.5f);
			StartPath(SearchedPos,BaseMoveSpeed);
		}
		protected override bool OnPathFindCompleted(Path pin, float speed)
		{
			bool ret = base.OnPathFindCompleted(pin, speed);
			if (ret)
			{
				ABPath p = pin as ABPath;

				canSearchAgain = true;

				if (path != null) path.Release(this);
				path = p;
				p.Claim(this);

				if (p.error)
				{
					wp = 0;
					vectorPath = null;
					return false;
				}


				Vector3 p1 = p.originalStartPoint;
				Vector3 p2 = SelfUnit.Trans.position;
				p1.y = p2.y;
				float d = (p2 - p1).magnitude;
				wp = 0;

				vectorPath = p.vectorPath;
				Vector3 waypoint;

				if (moveNextDist > 0)
				{
					for (float t = 0; t <= d; t += moveNextDist * 0.6f)
					{
						wp--;
						Vector3 pos = p1 + (p2 - p1) * t;

						do
						{
							wp++;
							waypoint = vectorPath[wp];
						} while (RVOController.To2D(pos - waypoint).sqrMagnitude < moveNextDist * moveNextDist && wp != vectorPath.Count - 1);
					}
				}
			}
			return ret;
        }
		protected override void OnFollowPathUpdate()
        {
			//动态重新寻路
			if (IsMovingFlag && Time.time >= nextRepath && canSearchAgain)
			{
				RecalculatePath();
			}

            Vector3 pos = SelfUnit.Pos;
			float desiredSpeed = 0.5f;

			if (vectorPath != null && vectorPath.Count != 0)
			{
				while ((RVOController.To2D(pos - vectorPath[wp]).sqrMagnitude < moveNextDist * moveNextDist && wp != vectorPath.Count - 1) || wp == 0)
				{
					wp++;
					Callback_OnMovingStep?.Invoke();
				}

				// Current path segment goes from vectorPath[wp-1] to vectorPath[wp]
				// We want to find the point on that segment that is 'moveNextDist' from our current position.
				// This can be visualized as finding the intersection of a circle with radius 'moveNextDist'
				// centered at our current position with that segment.
				var p1 = vectorPath[wp - 1];
				var p2 = vectorPath[wp];

				// Calculate the intersection with the circle. This involves some math.
				var t = VectorMath.LineCircleIntersectionFactor(RVOController.To2D(SelfUnit.Trans.position), RVOController.To2D(p1), RVOController.To2D(p2), moveNextDist);
				// Clamp to a point on the segment
				t = Mathf.Clamp01(t);
				Vector3 waypoint = Vector3.Lerp(p1, p2, t);

				// Calculate distance to the end of the path
				float remainingDistance = RVOController.To2D(waypoint - pos).magnitude + RVOController.To2D(waypoint - p2).magnitude;
				for (int i = wp; i < vectorPath.Count - 1; i++) remainingDistance += RVOController.To2D(vectorPath[i + 1] - vectorPath[i]).magnitude;

				// Set the target to a point in the direction of the current waypoint at a distance
				// equal to the remaining distance along the path. Since the rvo agent assumes that
				// it should stop when it reaches the target point, this will produce good avoidance
				// behavior near the end of the path. When not close to the end point it will act just
				// as being commanded to move in a particular direction, not toward a particular point
				var rvoTarget = (waypoint - pos).normalized * remainingDistance + pos;
				// When within [slowdownDistance] units from the target, use a progressively lower speed
				desiredSpeed = Mathf.Clamp(remainingDistance / SlowdownDistance, MinDesiredSpeed, 1) * RealMoveSpeed;
				RVOController.SetTarget(rvoTarget,Mathf.Clamp(desiredSpeed, MinSpeed,MaxSpeed), MaxSpeed);
			}
			else
			{
				// Stand still
				RVOController.SetTarget(pos, Mathf.Clamp(desiredSpeed, MinSpeed, MaxSpeed), MaxSpeed);
			}

			// Get a processed movement delta from the rvo controller and move the character.
			// This is based on information from earlier frames.
			var movementDelta = RVOController.CalculateMovementDelta(Time.deltaTime);
			pos += movementDelta;

			// Rotate the character if the velocity is not extremely small
			if (IsMovingFlag && Time.deltaTime > 0 && movementDelta.magnitude / Time.deltaTime > 0.01f)
			{
				var rot = SelfUnit.Trans.rotation;
				var targetRot = Quaternion.LookRotation(movementDelta, RVOController.To3D(Vector2.zero, 1));
				const float RotationSpeed = 5;
				if (RVOController.movementPlane == MovementPlane.XY)
				{
					targetRot = targetRot * Quaternion.Euler(-90, 180, 0);
				}
				SelfUnit.Trans.rotation = Quaternion.Slerp(rot, targetRot, Time.deltaTime * RotationSpeed);
			}

			//采样地图高度
			if (IsPositionChange && TerrainObj.Ins != null)
			{
				Vector3 bakePos = BaseSceneRoot.Ins.GetBakedColliderPos(SelfBaseUnit);
				float terrainY = TerrainObj.Ins.SampleHeight(pos);
				if (bakePos.y == SysConst.VEC_Inv.y)
				{
					pos.y = terrainY;
				}
				else
				{
					if (bakePos.y > terrainY)
						pos.y = bakePos.y;
					else
						pos.y = terrainY;
				}
			}

			//原地踏步Timeout
			if (IsMovingFlag )
			{
				if (!IsPositionChange)
				{
					curArrivalTime += Time.deltaTime*BaseAStarMgr.MultipleSpeed;
					if (curArrivalTime > ArrivalTimeout)
					{
						ForceStop();
					}
				}
				else
				{
					curArrivalTime = 0;
					Callback_OnMovingAlone?.Invoke();
				}
			}

			//判断单位是否抵达目标点
			if (IsMovingFlag && MathUtil.ApproximatelyXZ(SelfBaseUnit.Pos, Destination, ArrivalAccuracy))
			{
				ForceStop();
			}
			SelfUnit.Trans.position = pos;
			void ForceStop()
			{
				IsMovingFlag = false;
				Destination = SelfBaseUnit.Pos;
				RVOController.SetTarget(Destination, Mathf.Clamp(desiredSpeed, MinSpeed, MaxSpeed), MaxSpeed);
				vectorPath.Clear();
				StopPath();
				Callback_OnMoveDestination?.Invoke();
			}
		}
    }
}