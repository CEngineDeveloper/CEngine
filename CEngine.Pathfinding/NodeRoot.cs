//------------------------------------------------------------------------------
// BlockerRoot.cs
// Copyright 2019 2019/5/8 
// Created by CYM on 2019/5/8
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CYM.Pathfinding
{
    [HideMonoScript]
    public sealed class NodeRoot : MonoBehaviour
    {
        #region prop
        AstarPath AstarPath => AstarPath.active;
        BaseAStarMgr AStarMgr => BaseGlobal.AStarMgr;
        Transform[] NodeObjs;
        public HashList<GraphNode> Nodes { get; private set; } = new HashList<GraphNode>();
        #endregion

        #region life
        private void OnEnable()
        {
            //if(AutoCalc)
            //CalcNodes();
        }
        #endregion

        #region set
        [Button("AdjToNode")]
        public void CalcNodes()
        {
            Nodes.Clear();
            NodeObjs = GetComponentsInChildren<Transform>();
            if (NodeObjs != null)
            {
                foreach (var item in NodeObjs)
                {
                    if (item == transform)
                    {
                        transform.localPosition = Vector3.zero;
                        continue;
                    }
                    GraphNode node = null;
                    if (Application.isPlaying)
                    {
                        node = AStarMgr.GetSafeNode(item.position);
                    }
                    else
                    {
                        node = AstarPath.GetNearest(item.position).node;
                    }
                    if (node == null)
                    {
                        CLog.Error("错误! 没有Node");
                        continue;
                    }
                    Nodes.Add(node);
                    item.position = (Vector3)node.position;
                    item.name = "Item";
                }
            }
        }
        #endregion
    }
}