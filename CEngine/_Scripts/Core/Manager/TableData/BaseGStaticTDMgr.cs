//------------------------------------------------------------------------------
// BaseGAllStaticMgr.cs
// Created by CYM on 2022/6/10
// 填写类的描述...
//------------------------------------------------------------------------------
using System.Collections.Generic;
//------------------------------------------------------------------------------
// BaseAllTDMgr.cs
// Copyright 2019 2019/11/7 
// Created by CYM on 2019/11/7
// Owner: CYM
// TableData的数据管理器,用来管理表格数据对象(产生继承于BaseConfig的对象)
// 和BaseTDMgr的区别是,此管理不是动态的,而是静态的,所有对象都会在初始的时候全部读取拷贝
// 之后只会修改保存自身本地的数据(适用于科技系统,改革系统等)
// 此管理器是全局化的,其他方面和BaseUStaticTDMgr功能一样
//------------------------------------------------------------------------------
namespace CYM
{
    public class BaseGStaticTDMgr <TData> : BaseGFlowMgr
        where TData : TDBaseData, new()
    {
        #region prop
        ITDConfig ITDConfig;
        #endregion

        #region life
        public Dictionary<string, TData> AllData { get; private set; } = new Dictionary<string, TData>();
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);

        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
            AllData.Clear();
            foreach (var item in ITDConfig.BaseDatas.Values)
            {
                var newData = item.Copy<TData>();
                newData.OnBeAdded(SelfMono);
                AllData.Add(newData.TDID, newData);
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
        protected void LoadDBData<TDBData>(List<TDBData> dbData, Callback<TData, TDBData> action)
            where TDBData : DBBase
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
        protected void SaveDBData<TDBData>(List<TDBData> dbData, Callback<TData, TDBData> action)
            where TDBData : DBBase, new()
        {
            foreach (var item in AllData.Values)
            {
                var dbTech = new TDBData();
                Util.CopyToData(item, dbTech);
                action?.Invoke(item, dbTech);
                dbData.Add(dbTech);
            }
        }
        #endregion
    }
}