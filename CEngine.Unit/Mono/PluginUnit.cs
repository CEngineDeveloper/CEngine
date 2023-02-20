using CYM.Unit;
namespace CYM
{
    public partial class BaseUnit : BaseCoreMono
    {
        static PluginUnit PluginUnit = new PluginUnit
        {
            OnPostAddComponet = (u, x) => {
                if (x is IHUDMgr)
                {
                    u.HUDMgr = x as IHUDMgr;
                }
                else if (x is IAttrMgr)
                {
                    u.AttrMgr = x as IAttrMgr;
                }
                else if (x is IBuffMgr)
                {
                    u.BuffMgr = x as IBuffMgr;
                }
                else if (x is IAlertMgr<TDBaseAlertData>)
                {
                    u.AlertMgr = x as IAlertMgr<TDBaseAlertData>;
                }
                else if (x is IEventMgr<TDBaseEventData>)
                {
                    u.EventMgr = x as IEventMgr<TDBaseEventData>;
                }
                else if (x is BasePerformMgr)
                {
                    u.PerformMgr = x as BasePerformMgr;
                }
                else if (x is BaseAnimMgr)
                {
                    u.AnimMgr = x as BaseAnimMgr;
                }
                else if (x is BaseImmuneMgr)
                {
                    u.ImmuneMgr = x as BaseImmuneMgr;
                }
            }
        };

        #region prop
        public IHUDMgr HUDMgr { get; protected set; }
        public IAttrMgr AttrMgr { get; protected set; }
        public IBuffMgr BuffMgr { get; protected set; }
        public IAlertMgr<TDBaseAlertData> AlertMgr { get; protected set; }
        public IEventMgr<TDBaseEventData> EventMgr { get; protected set; }
        public BasePerformMgr PerformMgr { get; protected set; }
        public BaseAnimMgr AnimMgr { get; protected set; }
        public BaseImmuneMgr ImmuneMgr { get; protected set; }
        #endregion

    }
}
