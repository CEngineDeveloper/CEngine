using CYM.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public partial class BaseUnit : BaseCoreMono
    {
        public IMoveMgr MoveMgr { get; protected set; }
        public IAStarMoveMgr AStarMoveMgr => MoveMgr as IAStarMoveMgr;
    }
}