//**********************************************
// Class Name	: BaseBTAIMgr
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM.AI
{
    public class BaseBTAIMgr<TData> : BaseTableDataAIMgr<TData> 
        where TData : TDBaseBTData, new()
    {
        //#region prop
        //ITDLuaMgr TDLuaMgr;
        //#endregion

        //#region life
        //public override void OnAffterAwake()
        //{
        //    base.OnAffterAwake();
        //    TDLuaMgr = BaseLuaMgr.GetTDLuaMgr(typeof(TData));
        //}
        //public override void OnRealDeath()
        //{
        //    Remove();
        //    base.OnRealDeath();
        //}
        //public override void OnFixedUpdate()
        //{
        //    base.OnFixedUpdate();
        //}
        //public override void ManualUpdate()
        //{
        //    base.ManualUpdate();
        //    if (!IsActiveAI) return;
        //    if (CurData != null)
        //    {
        //        CurData.OnUpdate();
        //    }
        //}
        //#endregion

        //#region set
        ///// <summary>
        ///// 改变行为树
        ///// </summary>
        //public virtual void Change(string btKey)
        //{
        //    if (btKey.IsInv())
        //        return;
        //    Remove();
        //    TData tempData = TDLuaMgr.Get<TData>(btKey);
        //    if (tempData != null)
        //    {
        //        CurData = tempData.Copy<TData>();
        //        CurData.OnBeAdded(SelfBaseUnit);
        //    }
        //    else
        //    {
        //        CLog.Error("错误,没有这个类型的BT:{0}", btKey);
        //    }
        //}
        ///// <summary>
        ///// 移除行为树
        ///// </summary>
        //protected virtual void Remove()
        //{
        //    if (CurData != null)
        //        CurData.OnBeRemoved();
        //    CurData = null;
        //}
        ///// <summary>
        ///// 重置行为树
        ///// </summary>
        //public void SetTreeDirty()
        //{
        //    CurData.SetTreeDirty();
        //}

        ///// <summary>
        ///// 是否拥有行树
        ///// </summary>
        ///// <returns></returns>
        //public virtual bool IsHave()
        //{
        //    return CurData != null;
        //}
        //#endregion

        //#region must override
        //public TData CurData { get; private set; }
        //#endregion

    }
}