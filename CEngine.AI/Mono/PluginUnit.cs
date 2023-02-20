using CYM.AI;
namespace CYM
{
    public partial class BaseUnit : BaseCoreMono
    {
        static PluginUnit PluginAI = new PluginUnit
        {
            OnPostAddComponet = (u, x) => {
                if (x is BaseAIMgr)
                {
                    u.AIMgr = x as BaseAIMgr;
                }
            }
        };
        public BaseAIMgr AIMgr { get; protected set; }
        public float Importance { get; set; }
    }
}