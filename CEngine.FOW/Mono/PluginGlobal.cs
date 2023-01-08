using CYM.FOW;
namespace CYM
{
    public partial class BaseGlobal : BaseCoreMono
    {
        static PluginGlobal PluginFOW = new PluginGlobal
        {
            OnInstall = (g) => {
                FOWMgr = g.AddComponent<BaseFOWMgr>();
            }
        };
        public static BaseFOWMgr FOWMgr { get; private set; }
    }
}