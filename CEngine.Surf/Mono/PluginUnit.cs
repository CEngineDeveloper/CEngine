using CYM.Surf;

namespace CYM
{
    public partial class BaseUnit : BaseCoreMono
    {
        static PluginUnit PluginSurf = new PluginUnit
        {
            OnPostAddComponet = (u, x) => {
                if (x is ISurfaceMgr<BaseModel>)
                {
                    u.SurfMgr = x as ISurfaceMgr<BaseModel>;
                }
            }
        };

        public ISurfaceMgr<BaseModel> SurfMgr { get; protected set; }
    }
}
