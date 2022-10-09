using CYM.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public partial class BaseGlobal : BaseCoreMono
    {
        Plugin Pathfinding = new Plugin  { 
            OnInstall = (g)=> {
                AStarMgr = g.AddComponent<BaseAStarMgr>();
            } 
        };

        #region Componet
        public static BaseAStarMgr AStarMgr { get; protected set; }
        #endregion
    }
}
