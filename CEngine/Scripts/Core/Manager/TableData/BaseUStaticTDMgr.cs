//------------------------------------------------------------------------------
// BaseAllTDMgr.cs
// Copyright 2019 2019/11/7 
// Created by CYM on 2019/11/7
// Owner: CYM
// TableData的数据管理器,用来管理表格数据对象(产生继承于BaseConfig的对象)
// 和BaseTDMgr的区别是,此管理不是动态的,而是静态的,所有对象都会在初始的时候全部读取拷贝
// 之后只会修改保存自身本地的数据(适用于科技系统,改革系统等)
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CYM
{
    public class BaseUStaticTDMgr<TData, TUnit> : BaseUFlowMgr<TUnit>, IDBListConvertMgr<TData>
        where TUnit : BaseUnit
        where TData : TDBaseData, new()
    {
        #region prop
        ITDConfig ITDConfig;
        #endregion

        #region life
        public Dictionary<string, TData> AllData { get; private set; } = new Dictionary<string, TData>();
        public List<TData> ListData { get; private set; } = new List<TData>();
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
            AllData.Clear();
            ListData.Clear();
            foreach (var item in ITDConfig.BaseDatas.Values)
            {
                var newData = item.Copy<TData>();
                newData.OnBeAdded(SelfUnit);
                AllData.Add(newData.TDID, newData);
                ListData.Add(newData);
            }
        }
        #endregion

        #region get
        public TData GetData(string tdid)
        {
            if (!AllData.ContainsKey(tdid)) return null;
            return AllData[tdid];
        }
        #endregion

        #region is
        public bool IsContains(string tdid)
        {
            return AllData.ContainsKey(tdid);
        }
        #endregion

        #region DB
        public void LoadDBData<TDBData>(ref List<TDBData> dbData, Callback<TData, TDBData> action)
            where TDBData : DBBase, new()
        {
            foreach (var item in dbData)
            {
                if (!AllData.ContainsKey(item.TDID)) 
                    continue;
                var techData = AllData[item.TDID];
                Util.CopyToConfig(item, techData);
                action?.Invoke(techData, item);
            }
        }
        public void SaveDBData<TDBData>(ref List<TDBData> dbData, Callback<TData, TDBData> action)
            where TDBData : DBBase,new()
        {
            foreach (var item in AllData.Values)
            {
                var dbTech = new TDBData();
                Util.CopyToData(item, dbTech);
                action?.Invoke(item,dbTech);
                dbData.Add(dbTech);
            }
        }
        #endregion
    }
}