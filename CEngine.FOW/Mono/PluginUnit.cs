using CYM.FOW;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public partial class BaseUnit : BaseCoreMono
    {
        static PluginUnit PluginFOW = new PluginUnit
        {
            OnPostAddComponet = (u, x) => {
                if (x is BaseFOWRevealerMgr)
                {
                    u.FOWMgr = x as BaseFOWRevealerMgr;
                }
            }
        };
        public BaseFOWRevealerMgr FOWMgr { get; protected set; }
    }
}