using CYM.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public partial class BaseGlobal : BaseCoreMono
    {
        static PluginGlobal PluginPathfinding = new PluginGlobal  
        { 
            OnInstall = (g)=> {
                AStarMgr = g.AddComponent<BaseAStarMgr>();
            } 
        };

        #region Componet
        public static BaseAStarMgr AStarMgr { get; protected set; }
        #endregion
    }
}
