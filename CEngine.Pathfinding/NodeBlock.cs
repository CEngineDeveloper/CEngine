//------------------------------------------------------------------------------
// BaseBlockerNode.cs
// Copyright 2019 2019/5/5 
// Created by CYM on 2019/5/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Pathfinding;
using UnityEngine;

namespace CYM.Pathfinding
{
    public sealed class NodeBlock : MonoBehaviour
    {
        public GraphNode Node { get; private set; }
        public BaseSceneRoot BaseSceneObject => BaseSceneRoot.Ins;
        AstarPath AstarPath => AstarPath.active;

        private void Start()
        {
            Node = AstarPath.GetNearest(transform.position, NNConstraint.None).node;
        }
    }
}