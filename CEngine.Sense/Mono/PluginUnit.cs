using CYM.Sense;
using System.Collections.Generic;
namespace CYM
{
    public partial class BaseUnit : BaseCoreMono
    {
        static PluginUnit PluginSense = new PluginUnit
        {
            OnPostAddComponet = (u, x) => {
                if (x is ISenseMgr)
                {
                    u.SenseMgr = x as ISenseMgr;
                }
                else if (x is BaseDetectionMgr)
                {
                    u.DetectionMgr = x as BaseDetectionMgr;
                }
            }
        };
        public List<ISenseMgr> SenseMgrs { get; protected set; } = new List<ISenseMgr>();
        public ISenseMgr SenseMgr { get; protected set; }
        public BaseDetectionMgr DetectionMgr { get; protected set; }
    }
}