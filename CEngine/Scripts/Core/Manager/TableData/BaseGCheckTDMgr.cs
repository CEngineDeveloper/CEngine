//------------------------------------------------------------------------------
// BaseGCheckTDMgr.cs
// Created by CYM on 2022/6/17
// 填写类的描述...
//------------------------------------------------------------------------------
namespace CYM
{
    public class BaseGCheckTDMgr<TData> : BaseGFlowMgr, IDBListConvertMgr<TData>
        where TData : TDBaseData, new()
    {
        public void LoadDBData<TDBData>(ref System.Collections.Generic.List<TDBData> dbData, Callback<TData, TDBData> action) where TDBData : DBBase, new()
        {
            throw new System.NotImplementedException();
        }

        public void SaveDBData<TDBData>(ref System.Collections.Generic.List<TDBData> dbData, Callback<TData, TDBData> action) where TDBData : DBBase, new()
        {
            throw new System.NotImplementedException();
        }
    }
}