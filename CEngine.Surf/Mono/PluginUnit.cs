using CYM.Surf;

namespace CYM
{
    public partial class BaseUnit : BaseCoreMono
    {
        public ISurfaceMgr<BaseModel> SurfMgr { get; protected set; }
    }
}
