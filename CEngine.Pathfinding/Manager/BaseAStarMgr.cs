//------------------------------------------------------------------------------
// BaseAStarMgr.cs
// Copyright 2019 2019/4/17 
// Created by CYM on 2019/4/17
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Pathfinding
{
    public class TraversalConstraint:NNConstraint
    {
        public static TraversalConstraint Traversal { get; private set; } = new TraversalConstraint();
        BaseTraversal selfTraversal;
        public void SetTraversal(BaseTraversal traversal)
        {
            selfTraversal = traversal;
        }
        public override bool Suitable(GraphNode node)
        {
            if (!selfTraversal.CanTraverse(null, node))
                return false;
            return base.Suitable(node);
        }
    }
    public class GraphNodeRange
    {
        public GraphNodeRange Parent;
        public GraphNode Node;
        public int Depth = 0;
        public float Distance = 0;

        public GraphNodeRange(GraphNodeRange parent, GraphNode node)
        {
            Parent = parent;
            Node = node;
            if (Parent != null)
            {
                Depth = Parent.Depth + 1;
                Distance = Parent.Distance + Vector3.Distance((Vector3)Node.position, (Vector3)Parent.Node.position);
            }
        }
    }
    public class BaseAStarMgr : BaseGFlowMgr
    {
        #region Callback val
        public event Callback<BaseUnit> Callback_OnMoveStart;
        public event Callback<BaseUnit, Vector3, Vector3, Vector3, Vector3> Callback_OnMovingAlone;
        public event Callback<BaseUnit> Callback_OnMoveEnd;
        public event Callback<BaseUnit, GraphNode> Callback_OnSeizeNode;
        #endregion

        #region virtual
        protected virtual ITraversalProvider CommonTraversalProvider => null;
        #endregion

        #region prop
        //占位的Node
        SeizeNodeData<GraphNode> SeizeNodeData = new SeizeNodeData<GraphNode>();
        //阻断的Node
        BlockNodeData<GraphNode> BlockNodeData = new BlockNodeData<GraphNode>();
        public AstarPath Ins => AstarPath.active;
        public float NodeSize => Ins.data.gridGraph.nodeSize;
        Corouter BattleCoroutine => BaseGlobal.BattleCorouter;
        CoroutineHandle UnitMoveCoroutineHandle;
        BaseUnit PreMoveUnit;
        #endregion

        #region life
        public override void OnGameStart1()
        {
            base.OnGameStart1();
            if (Ins)
            {
                Ins.maxNearestNodeDistance = float.MaxValue;
            }
            GlobalMoveState.Reset();
        }
        #endregion

        #region move
        public ABPath Move(BaseUnit unit, Vector3 pos, float speed = 1.0f)
        {
            if (pos.IsInv())
            {
                return null;
            }
            if (UnitMoveCoroutineHandle.IsRunning)
            {
                BattleCoroutine.Kill(UnitMoveCoroutineHandle);
                Callback_OnMoveEnd?.Invoke(PreMoveUnit);
            }
            var path = StartABPath(unit.Pos, pos, null);
            UnitMoveCoroutineHandle = BattleCoroutine.Run(MoveAlongPath(unit, path, speed));
            return path;
        }
        #endregion

        #region set
        public void SetWalkable(Vector3 pos,bool b)
        {
            var node = Ins.GetNearest(pos).node;
            node.Walkable = b;
        }
        //打开关闭AStarPath
        public void EnableAStarPath(bool b)
        {
            if (Ins == null) return;
            Ins.enabled = b;
        }
        //计算常量路径
        public ConstantPath StartConstantPath(Vector3 start, int movePoint, OnPathDelegate callback, ITraversalProvider traversalProvider = null)
        {
            var path = ConstantPath.Construct(start, Mathf.Clamp(movePoint, 1, 1000) * 1000, callback);
            path.traversalProvider = CommonTraversalProvider;
            if (traversalProvider != null)
                path.traversalProvider = traversalProvider;

            AstarPath.StartPath(path);
            if (callback != null) { }
            else path.BlockUntilCalculated();
            return path;
        }
        //计算AB路径
        public ABPath StartABPath(Vector3 start, Vector3 end, OnPathDelegate callback, ITraversalProvider traversalProvider = null)
        {
            var path = ABPath.Construct(start, end, callback);
            path.traversalProvider = CommonTraversalProvider;
            if (traversalProvider != null)
                path.traversalProvider = traversalProvider;

            AstarPath.StartPath(path);
            if (callback != null) { }
            else path.BlockUntilCalculated();
            return path;
        }
        // 清理Node
        public void ClearSeizeNode(GraphNode node, BaseUnit unit)
        {
            SeizeNodeData.ClearSeizeNode(node, unit);
        }
        // 设置占据的Node
        public void SetSeizeNode(GraphNode node, BaseUnit unit)
        {
            SeizeNodeData.SetSeizeNode(node, unit);
            Callback_OnSeizeNode?.Invoke(unit, node);
        }
        // 设置阻挡得Node,输入Null表示清空
        public void SetBlockNode(BaseUnit unit, List<GraphNode> nodes)
        {
            BlockNodeData.SetBlockNode(unit, nodes);
        }
        // 移动一个单位到另一个单位的边上(随机位置)
        public bool RandArroundUnit(BaseUnit targetUnit, BaseUnit moveUnit, int range = 8)
        {
            var nodes = GetBFS(targetUnit.Pos, range, moveUnit.AStarMoveMgr.IsCanTraversal, true);
            HashList<GraphNode> RandNodes = new HashList<GraphNode>();
            //节点数量太少,无法摆放军团
            if (nodes.Count <= 1) return false;
            //去掉中心Node
            nodes.RemoveAt(0);
            //获得备用Node
            GraphNode SpareNode = null;
            for (int i = nodes.Count - 1; i > 0; --i)
            {
                var item = nodes[i];
                if (!IsHaveUnit(item) && !IsBlocker(item))
                {
                    SpareNode = item;
                    RandNodes.Add(item);
                }
            }
            //获得随机Node
            GraphNode RandNode;
            RandNode = RandNodes.Rand();
            //获得Final Node
            GraphNode FinalNode = IsHaveUnit(RandNode) || IsBlocker(RandNode) ? SpareNode : RandNode;
            if (FinalNode != null)
            {
                moveUnit.AStarMoveMgr.SetToNode(FinalNode);
                moveUnit.MoveMgr.RandRotationY();
                return true;
            }
            //进入递归
            else
            {
                return RandArroundUnit(targetUnit, moveUnit, range + 1);
            }
        }
        // 移动一个单位到另一个单位的边上
        public bool SetArroundUnit(BaseUnit targetUnit, BaseUnit moveUnit)
        {
            var seed = GetDistNode(moveUnit, targetUnit.Pos, false, true, true, true,true);
            if (seed != null)
            {
                moveUnit.AStarMoveMgr.SetToNode(seed);
                moveUnit.MoveMgr.RandRotationY();
                return true;
            }
            return false;
        }
        // 移动单位到一个指定的位置点上面
        public bool SetPosition(Vector3 pos, BaseUnit moveUnit,bool yRot=true)
        {
            var seed = GetDistNode(moveUnit, pos, false, true, true, true,true);
            if (seed != null)
            {
                moveUnit.AStarMoveMgr.SetToNode(seed);
                if(yRot)
                    moveUnit.MoveMgr.RandRotationY();
                return true;
            }
            return false;
        }
        //将角色位置修正到附近可以摆放的点
        public bool FixedToNode(BaseUnit moveUnit)
        {
            var seed = GetDistNode(moveUnit, moveUnit.Pos, false, true, true, true, true);
            if (seed != null)
            {
                moveUnit.AStarMoveMgr.SetToNode(seed);
                moveUnit.MoveMgr.RandRotationY();
                return true;
            }
            return false;
        }
        #endregion

        #region get
        //获得与单位链接的一层Node
        public HashList<GraphNode> GetConnections(BaseUnit unit)
        {
            HashList<GraphNode> ret = new HashList<GraphNode>();
            var unitNode = GetNode(unit);
            if (unitNode == null) return new HashList<GraphNode>();
            unitNode.GetConnections(ret.Add);
            return ret;
        }
        //获得Blocker外围的一层链接Node
        public HashList<GraphNode> GetConnectionsBlocker(BaseUnit unit)
        {
            var blocker = GetBlocker(unit);
            if (blocker.Count == 0) blocker.Add(GetNode(unit));
            HashList<GraphNode> ret = new HashList<GraphNode>();
            foreach (var item in blocker)
            {
                item.GetConnections((x) =>
                {
                    if (!blocker.Contains(x))
                        ret.Add(x);
                });
            }
            return ret;
        }
        //获得与此节点链接的相关节点
        public HashList<GraphNode> GetConnections(GraphNode node)
        {
            HashList<GraphNode> ret = new HashList<GraphNode>();
            node.GetConnections(ret.Add);
            return ret;
        }
        //获得所有与这些节点链接的节点
        public List<HashList<GraphNode>> GetAllConnections(HashList<GraphNode> nodes)
        {
            List<HashList<GraphNode>> ret = new List<HashList<GraphNode>>();
            if (nodes == null)
                return ret;
            foreach (var item in nodes)
            {
                ret.Add(GetConnections(item));
            }
            return ret;
        }
        public Vector3 GetMousePos()
        {
            var temp = GetMouseNode();
            if (temp == null)
                return SysConst.VEC_FarawayPos;
            if (!temp.Walkable)
                return SysConst.VEC_FarawayPos;
            return (Vector3)temp.position;
        }
        public HashSet<BaseUnit> GetUnits(GraphNode node)
        {
            if (node == null) return null;
            return SeizeNodeData.GetUnits(node);
        }
        public HashList<GraphNode> GetBlocker(BaseUnit unit)
        {
            if (unit == null) return null;
            return BlockNodeData.GetBlocker(unit);
        }
        public HashList<BaseUnit> GetBlockerUnits(GraphNode node)
        {
            if (node == null) return null;
            return BlockNodeData.GetBlockerUnits(node);
        }
        public HashList<BaseUnit> GetBlockerUnits(BaseUnit unit, bool isContainSelf = true)
        {
            if (unit == null) return null;
            var selfNode = unit.AStarMoveMgr.CurNode;
            var allBlockers = GetBlocker(unit);
            HashList<BaseUnit> ret = new HashList<BaseUnit>();
            foreach (var item in allBlockers)
            {
                var units = GetBlockerUnits(item);
                if (units != null)
                {
                    foreach (var tempUnit in units)
                    {
                        if (!isContainSelf)
                        {
                            if (tempUnit != unit && tempUnit.AStarMoveMgr.CurNode != selfNode)
                                ret.Add(tempUnit);
                        }
                        else
                            ret.Add(tempUnit);

                    }
                }
            }
            return ret;
        }
        public uint GetTag(Vector3 pos)
        {
            var node = GetNode(pos);
            if (node == null)
                return 0;
            return node.Tag;
        }
        #endregion

        #region get range node
        static Queue<GraphNode> BFSQueue = new Queue<GraphNode>();
        static Dictionary<GraphNode, int> BFSMap = new Dictionary<GraphNode, int>();
        static HashList<GraphNode> BFSResult = new HashList<GraphNode>();
        static Dictionary<GraphNode, GraphNodeRange> BFSRange = new Dictionary<GraphNode, GraphNodeRange>();
        public HashList<GraphNode> GetDistanceRange(Vector3 pos, float distance, Func<GraphNode, bool> filter = null)
        {
            BFSQueue.Clear();
            BFSResult.Clear();
            BFSRange.Clear();
            var depth = distance / NodeSize;
            GraphNode seed = GetNode(pos);
            Action<GraphNodeRange, GraphNode> callback = (parent, node) =>
            {
                if (node.Walkable && !BFSResult.Contains(node))
                {
                    if (filter != null && !filter(node)) return;
                    var nRangeItem = new GraphNodeRange(parent, node);
                    if (nRangeItem.Distance > distance) return;
                    if (nRangeItem.Depth > depth) return;

                    BFSResult.Add(node);
                    BFSQueue.Enqueue(node);
                    BFSRange.Add(node, nRangeItem);
                }
            };

            callback(null, seed);

            while (BFSQueue.Count > 0)
            {
                GraphNode n = BFSQueue.Dequeue();
                GraphNodeRange nRangeItem = BFSRange[n];
                if (nRangeItem.Distance > distance) break;
                if (nRangeItem.Depth > depth) break;
                n.GetConnections((x) =>
                {
                    callback(nRangeItem, x);
                });
            }
            return BFSResult;
        }
        //获得周围符合条件的点
        //isJump:是否会跳过阻隔,继续遍历符合条件的点
        public HashList<GraphNode> GetBFS(Vector3 pos, int depth, Func<GraphNode, bool> filter = null, bool isJump = false)
        {
            BFSQueue.Clear();
            BFSMap.Clear();
            BFSResult.Clear();
            GraphNode seed = GetNode(pos);
            var currentDist = -1;
            Action<GraphNode> callback = node =>
            {
                if (node.Walkable && !BFSResult.Contains(node))
                {
                    var curTempDist = currentDist;
                    if (isJump)
                    {

                        if (filter != null && !filter(node)) { }
                        else curTempDist = currentDist + 1;
                    }
                    else
                    {
                        if (filter != null && !filter(node)) return;
                        else curTempDist = currentDist + 1;
                    }
                    BFSMap.Add(node, curTempDist);
                    BFSResult.Add(node);
                    BFSQueue.Enqueue(node);
                }
            };

            callback(seed);
            while (BFSQueue.Count > 0)
            {
                GraphNode n = BFSQueue.Dequeue();
                currentDist = BFSMap[n];
                if (currentDist >= depth) break;
                n.GetConnections(callback);
            }
            return BFSResult;
        }
        public int GetBFSDepth(GraphNode node)
        {
            if (BFSMap.ContainsKey(node))
                return BFSMap[node];
            return 0;
        }
        #endregion

        #region get node
        // 获得节点.有可能不可行走
        public GraphNode GetNode(Vector3 pos)
        {
            if (Ins == null)
            {
                return null;
            }
            var node = Ins.GetNearest(pos, NNConstraint.None);
            return node.node;
        }
        // 获得可行走的节点
        public GraphNode GetSafeNode(Vector3 pos)
        {
            if (Ins == null)
            {
                return null;
            }
            var node = Ins.GetNearest(pos, NNConstraint.Default);
            return node.node;
        }
        public GraphNode GetSafeTraversalNode(BaseUnit unit,Vector3? pos=null)
        {
            if (Ins == null)
            {
                return null;
            }
            if (pos == null)
                pos = unit.Pos;
            TraversalConstraint.Traversal.SetTraversal(unit.AStarMoveMgr.Traversal);
            var node = Ins.GetNearest(pos.Value, TraversalConstraint.Traversal);
            return node.node;
        }
        public GraphNode GetRandCircleNode(Vector3 pos, float radio)
        {
            var node = GetSafeNode(RandUtil.RandCirclePoint(pos, RandUtil.Range(radio, radio * 1.5f)));
            return node;
        }
        // 获得最远距离的节点,符合以下条件:此节点没有单位,此节点可以穿越,此节点是可行走的
        public GraphNode GetDistNode(
            BaseUnit selfUnit, 
            Vector3 pos, 
            bool isMaxDistance = true, 
            bool isCanTraversal = true, 
            bool isNoUnit = true, 
            bool isNoBlocker = false, 
            bool isDistanceWithEnd = true)
        {
            HashSet<GraphNode> exclude = new HashSet<GraphNode>();
            return GetCustomNode(pos);

            GraphNode GetCustomNode(Vector3 tempPos)
            {
                var target = GetSafeNode(tempPos);
                //如果种子节点符合条件就直接返回
                if (IsCondition(target))
                    return target;
                GraphNode defaultNode = null;
                GraphNode inConditionNode = null;
                GraphNode unConditionNode = null;
                HashList<GraphNode> links = new HashList<GraphNode>();
                target.GetConnections(x => { links.Add(x); });
                exclude.Add(target);

                float inConditionDistance = -1;
                float unConditionDistance = -1;
                if (isMaxDistance)
                {
                    inConditionDistance = -1;
                    unConditionDistance = -1;
                }
                else
                {
                    inConditionDistance = float.MaxValue;
                    unConditionDistance = float.MaxValue;
                }
                foreach (var item in links)
                {
                    defaultNode = item;
                    if (exclude.Contains(item)) continue;
                    exclude.Add(item);
                    var curDistance = 0.0f;
                    if (isDistanceWithEnd) curDistance = MathUtil.AutoSqrDistance((Vector3)target.position, (Vector3)item.position);
                    else curDistance = MathUtil.AutoSqrDistance(selfUnit.Pos, (Vector3)item.position);
                    if (IsCondition(item))
                    {
                        if (isMaxDistance)
                        {
                            if (curDistance > inConditionDistance)
                            {
                                inConditionDistance = curDistance;
                                inConditionNode = item;
                            }
                        }
                        else
                        {
                            if (curDistance < inConditionDistance)
                            {
                                inConditionDistance = curDistance;
                                inConditionNode = item;
                            }
                        }
                    }
                    else
                    {
                        if (isMaxDistance)
                        {
                            if (curDistance > unConditionDistance)
                            {
                                unConditionDistance = curDistance;
                                unConditionNode = item;
                            }
                        }
                        else
                        {
                            if (curDistance < unConditionDistance)
                            {
                                unConditionDistance = curDistance;
                                unConditionNode = item;
                            }
                        }
                    }
                }
                if (inConditionNode == null)
                {
                    if (unConditionNode == null)
                        return defaultNode;
                    return GetCustomNode((Vector3)unConditionNode.position);
                }
                else
                {
                    return inConditionNode;
                }
            }
            bool IsCondition(GraphNode graphNode)
            {
                bool ret_isCanTraversal = true;
                bool ret_isNoUnit = true;
                bool ret_isNoBlocker = true;
                if (isCanTraversal) ret_isCanTraversal = selfUnit.AStarMoveMgr.IsCanTraversal(graphNode);
                if (isNoUnit) ret_isNoUnit = !IsHaveUnit(graphNode);
                if (isNoBlocker) ret_isNoBlocker = !IsBlocker(graphNode);
                return ret_isCanTraversal && ret_isNoUnit && ret_isNoBlocker;
            }
        }
        //获得最近的Node,必须是链接
        public GraphNode GetClosedNode(BaseUnit selfUnit,int depath=int.MaxValue, Vector3? target = null)
        {
            if (target == null)
                target = selfUnit.Pos;
            HashSet<GraphNode> exclude = new HashSet<GraphNode>();
            return GetCloset(0);

            GraphNode GetCloset(int range)
            {
                List<GraphNode> closedTargetNode = new List<GraphNode>();
                GraphNode closetNode = null;
                GraphNode targetNode = GetNode(target.Value);
                if (targetNode == null) return null;
                var nodes = GetBFS(target.Value, range, selfUnit.AStarMoveMgr.IsCanTraversal, true);
                float maxDis_self = float.MaxValue;
                foreach (var item in nodes)
                {
                    if (exclude.Contains(item)) continue;
                    //过滤掉自身节点
                    if (targetNode == item) continue;
                    if (!item.Walkable) continue;
                    if (!selfUnit.AStarMoveMgr.IsCanTraversal(item)) continue;
                    //得到距离最近的一个节点
                    var dis_self = MathUtil.AutoSqrDistance(selfUnit.Pos, (Vector3)item.position);
                    if (dis_self < maxDis_self)
                    {
                        closetNode = item;
                        maxDis_self = dis_self;
                    }
                    exclude.Add(item);
                }
                if (closetNode != null)
                    return closetNode;
                var curDep = range + 1;
                if (curDep >= depath)
                    return null; 
                return GetCloset(curDep);
            }
        }
        // 通过指定一个目标点,获得距离自己最近的一个点
        // isJump:是否会跳过阻隔,继续遍历
        public GraphNode GetClosedNode(BaseUnit selfUnit, Vector3? target=null)
        {
            return GetClosedNode(selfUnit,int.MaxValue,target);
        }

        // 获得鼠标位置节点
        public GraphNode GetMouseNode()
        {
            if (BaseGlobal.MainCamera == null) return null;
            if (BaseGlobal.MainCamera.orthographic) return null;
            if (BaseInputMgr.IsStayInUI) return null;
            Ray ray = BaseGlobal.MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, float.MaxValue, (LayerMask)SysConst.Layer_Terrain))
            {
                if (hit.collider == null)
                    return null;
                var temp = GetNode(hit.point);
                return temp;
            }
            return null;
        }
        // 获得单位位置节点
        public GraphNode GetNode(BaseUnit unit)
        {
            return GetNode(unit.Pos);
        }
        public float DistanceWithBlocker(BaseUnit withBloderUnit, GraphNode node)
        {
            var blockers = GetBlocker(withBloderUnit);
            float maxDistance = float.MaxValue;
            foreach (var item in blockers)
            {
                float curDistance = MathUtil.AutoSqrDistance((Vector3)node.position, (Vector3)item.position);
                if (curDistance < maxDistance)
                {
                    maxDistance = curDistance;
                }
            }
            return maxDistance;
        }
        #endregion

        #region is
        public bool IsWalkable(Vector3 pos)
        {
            var node = GetNode(pos);
            if (node == null) return false;
            return node.Walkable;
        }
        public bool IsHaveUnit(GraphNode node)
        {
            if (node == null) return false;
            return GetUnits(node).Count > 0;
        }
        public bool IsHaveUnit(Vector3 pos)
        {
            return IsHaveUnit(GetNode(pos));
        }
        public bool IsConnection(HashList<GraphNode> nodes, GraphNode target)
        {
            if (nodes == null || target == null) return false;
            List<HashList<GraphNode>> connections = GetAllConnections(nodes);
            foreach (var list in connections)
            {
                if (list.Contains(target))
                    return true;
            }
            return false;
        }
        public bool IsConnection(GraphNode node, GraphNode target)
        {
            if (node == null || target == null) return false;
            List<GraphNode> links = new List<GraphNode>();
            node.GetConnections(links.Add);
            return links.Contains(target);
        }
        public bool IsConnectionBlocker(GraphNode node, BaseUnit target)
        {
            if (node == null || target == null) return false;
            var blocker = GetConnectionsBlocker(target);
            return blocker.Contains(node);
        }
        public bool IsBlocker(GraphNode node)
        {
            return BlockNodeData.IsBlocker(node);
        }
        public bool IsTag(GraphNode node,uint tag)
        {
            return node.Tag == tag;
        }
        #endregion

        #region IEnumerator
        IEnumerator<float> MoveAlongPath(BaseUnit unit, ABPath path, float speed)
        {
            if (path.error || path.vectorPath.Count == 0)
                throw new ArgumentException("Cannot follow an empty path");

            PreMoveUnit = unit;
            Callback_OnMoveStart?.Invoke(unit);

            // Very simple movement, just interpolate using a catmull rom spline
            float distanceAlongSegment = 0;
            for (int i = 0; i < path.vectorPath.Count - 1; i++)
            {
                var p0 = path.vectorPath[Mathf.Max(i - 1, 0)];
                // Start of current segment
                var p1 = path.vectorPath[i];
                // End of current segment
                var p2 = path.vectorPath[i + 1];
                var p3 = path.vectorPath[Mathf.Min(i + 2, path.vectorPath.Count - 1)];

                var segmentLength = Vector3.Distance(p1, p2);

                while (distanceAlongSegment < segmentLength)
                {
                    var interpolatedPoint = MathUtil.CatmullRom(p0, p1, p2, p3, distanceAlongSegment / segmentLength);

                    var targetRot = Quaternion.LookRotation((p2 - p1).SetY(0), Vector3.up);
                    unit.Rot = Quaternion.Slerp(unit.Rot, targetRot, Time.deltaTime * 10);

                    unit.transform.position = interpolatedPoint;

                    Callback_OnMovingAlone?.Invoke(unit, p0, p1, p2, p3);
                    yield return Timing.WaitForOneFrame;
                    distanceAlongSegment += Time.deltaTime * speed;
                }

                distanceAlongSegment -= segmentLength;
            }

            Vector3 target = path.vectorPath[path.vectorPath.Count - 1];
            unit.Pos = target;
            Callback_OnMoveEnd?.Invoke(unit);
        }
        #endregion

        #region Callback
        protected override void OnBattleUnLoad()
        {
            BlockNodeData.Clear();
            SeizeNodeData.Clear();
            base.OnBattleUnLoad();
        }
        #endregion

        #region Global Move
        public static BoolState GlobalMoveState { get; private set; } = new BoolState();
        public static float MultipleSpeed { get; private set; } = 1.0f;
        public void SetSpeedRate(float mul)
        {
            MultipleSpeed = mul;
        }
        #endregion
    }
}