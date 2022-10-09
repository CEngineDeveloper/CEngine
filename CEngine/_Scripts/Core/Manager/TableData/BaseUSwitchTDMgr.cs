//------------------------------------------------------------------------------
// BaseUSwitchTDMgr.cs
// Created by CYM on 2022/6/14
// 切换类表格管理器
// 用于政体,宗教等唯一且带有切换功能的系统
//------------------------------------------------------------------------------
namespace CYM
{
    public class BaseUSwitchTDMgr<TData, TUnit> : BaseUFlowMgr<TUnit>, IDBSingleConverMgr<TData>
        where TUnit : BaseUnit
        where TData : TDBaseData, new()
    {
        #region prop
        ITDConfig ITDConfig;
        public TData CurData { get; set; }
        #endregion

        #region Callback
        public event Callback<TData> Callback_OnPostChanged;
        public event Callback<TData> Callback_OnPostSetted;
        public event Callback<TData> Callback_OnPostSettedOrChanged;
        #endregion

        #region life
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
        }
        #endregion

        #region set
        public void Change(string config)
        {
            var data = ITDConfig.Get<TData>(config);
            Change(data);
        }
        public void Set(string config)
        {
            var data = ITDConfig.Get<TData>(config);
            if (data == null)
            {
                CLog.Error($"{SelfUnit.GetName()}:错误没有这个配置,{typeof(TData)}:{config}");
                return;
            }
            Set(data);
        }
        public void Change(TData config)
        {
            if (config == null)
            {
                return;
            }
            OnPreChange();
            OnPreSetOrChange();
            CurData?.OnBeRemoved();
            CurData = config.Copy<TData>();
            CurData.OnBeAdded(SelfUnit);
            Callback_OnPostChanged?.Invoke(CurData);
            Callback_OnPostSettedOrChanged?.Invoke(CurData);
            OnPostChange();
            OnPostSetOrChange();
        }
        public void Set(TData config)
        {
            if (config == null)
            {
                return;
            }
            OnPreSet();
            OnPreSetOrChange();
            CurData = config.Copy<TData>();
            CurData.OnBeAdded(SelfUnit);
            Callback_OnPostSetted?.Invoke(CurData);
            Callback_OnPostSettedOrChanged?.Invoke(CurData);
            OnPostSet();
            OnPostSetOrChange();
        }
        #endregion

        #region Callback
        protected virtual void OnPreChange()
        {

        }
        protected virtual void OnPreSet()
        {

        }
        protected virtual void OnPreSetOrChange()
        {

        }

        protected virtual void OnPostChange()
        {

        }
        protected virtual void OnPostSet()
        {

        }
        protected virtual void OnPostSetOrChange()
        { 
        
        }
        #endregion

        #region DB
        public void LoadDBData<TDBData>(ref TDBData dbData,Callback<TData,TDBData> action)
            where TDBData:DBBase, new()
        {
            var temp = ITDConfig.Get<TData>(dbData.TDID);
            if (temp == null)
            {
                return;
            }
            CurData = temp.Copy<TData>();
            Util.CopyToConfig(dbData, CurData);
            action?.Invoke(CurData, dbData);
        }
        public void SaveDBData<TDBData>(ref TDBData dbData, Callback<TData, TDBData> action)
            where TDBData : DBBase, new()
        {
            if (CurData == null)
            {
                dbData.TDID = SysConst.STR_Inv;
            }
            else
            {
                Util.CopyToData(CurData, dbData);
                action?.Invoke(CurData, dbData);
            }
        }
        #endregion
    }
}