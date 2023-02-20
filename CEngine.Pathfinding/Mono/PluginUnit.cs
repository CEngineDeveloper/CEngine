using CYM.Pathfinding;
namespace CYM
{
    public partial class BaseUnit : BaseCoreMono
    {
        static PluginUnit PluginMove = new PluginUnit
        {
            OnPostAddComponet = (u, x) => {
                if (x is IMoveMgr)
                {
                    u.MoveMgr = x as IMoveMgr;
                }
            }
        };

        public IMoveMgr MoveMgr { get; protected set; }
        public IAStarMoveMgr AStarMoveMgr => MoveMgr as IAStarMoveMgr;
    }
}