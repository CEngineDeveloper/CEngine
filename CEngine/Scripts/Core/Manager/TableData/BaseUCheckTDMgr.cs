//------------------------------------------------------------------------------
// BaseUCheckTDMgr.cs
// Created by CYM on 2022/6/17
// 填写类的描述...
//------------------------------------------------------------------------------
using System.Collections.Generic;

namespace CYM
{
    public class BaseUCheckTDMgr<TData, TUnit> : BaseUFlowMgr<TUnit>, IDBDicConvertMgr<TData>
        where TUnit : BaseUnit
        where TData : TDBaseData, new()
    {
        #region prop
        ITDConfig ITDConfig;
        public SortedDictionary<int, TData> AllSlot { get; private set; } = new SortedDictionary<int, TData>();
        #endregion

        #region life
        //最大数量
        protected virtual int MaxCount => 5;
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
            AllSlot.Clear();
            for (int i = 0; i < MaxCount; ++i)
            {
                AllSlot.Add(i,new TData());
            }
        }
        #endregion

        #region set
        //放上卡片
        public void Put(int id,TData data)
        {
            if (!AllSlot.ContainsKey(id))
                return;
            var pre = AllSlot[id];
            if (!pre.IsInv())
            {
                pre.OnBeRemoved();
            }
            var newData = data.Copy<TData>();
            AllSlot[id] = newData;
            newData.OnBeAdded(SelfUnit);
        }
        //摘下卡片
        public void Pull(int id)
        {
            if (!AllSlot.ContainsKey(id))
                return;
            var pre = AllSlot[id];
            if (!pre.IsInv())
            {
                pre.OnBeRemoved();
            }
            AllSlot[id] = new TData();
        }
        #endregion

        #region is
        //判断指定槽位中是否又数据

        public bool IsHave(int id)
        {
            if (!AllSlot.ContainsKey(id))
                return false;
            var data = AllSlot[id];
            if (data.TDID.IsInv())
                return false;
            return true ;
        }
        #endregion



        #region DB
        public void LoadDBData<TDBData>(ref Dictionary<int,TDBData> dbData, Callback<int,TData, TDBData> action) 
            where TDBData : DBBase, new()
        {
            foreach (var item in dbData)
            {
                TData config;
                if (item.Value.IsInv())
                {
                    config = new TData();
                }
                else
                {
                    var temp = ITDConfig.Get<TData>(item.Value.TDID);
                    if (temp == null)
                    {
                        CLog.Error($"错误么有这个TDID:{item.Value.TDID}");
                        continue;
                    }
                    config = temp.Copy<TData>();
                }
                Util.CopyToConfig(item.Value, config);
                action?.Invoke(item.Key, config, item.Value);
                AllSlot[item.Key] = config;
            }
        }

        public void SaveDBData<TDBData>(ref Dictionary<int,TDBData> dbData, Callback<int,TData, TDBData> action) 
            where TDBData : DBBase, new()
        {
            foreach (var item in AllSlot)
            {
                var data = new TDBData();
                Util.CopyToData(item.Value, data);
                action?.Invoke(item.Key,item.Value, data);
                dbData.Add(item.Key, data);
            }
        }
        #endregion
    }
}