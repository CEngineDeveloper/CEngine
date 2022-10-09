using CYM.Unit;
namespace CYM
{
    public partial class BaseUnit : BaseCoreMono
    {
        public IHUDMgr HUDMgr { get; protected set; }
        public IAttrMgr AttrMgr { get; protected set; }
        public IBuffMgr BuffMgr { get; protected set; }
        public BasePerformMgr PerformMgr { get; protected set; }
        public BaseAnimMgr AnimMgr { get; protected set; }
    }
}
