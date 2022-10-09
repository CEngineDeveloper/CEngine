using CYM.FOW;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public partial class BaseGlobal : BaseCoreMono
    {
        Plugin FOW = new Plugin
        {
            OnInstall = (g) => {
                FOWMgr = g.AddComponent<BaseFOWMgr>();
            }
        };
        public static BaseFOWMgr FOWMgr { get; private set; }
    }
}