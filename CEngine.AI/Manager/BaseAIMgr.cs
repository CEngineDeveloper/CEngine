using System.Collections.Generic;
namespace CYM.AI
{
    public class BaseAIMgr : BaseMgr
    {
        #region prop
        /// <summary>
        /// 自定义,禁止AI标志
        /// </summary>
        public bool IsEnableAI { get; protected set; } = true;
        public BaseUnit CurTarget { get; protected set; }
        #endregion

        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        public override void OnBirth()
        {
            base.OnBirth();
            CurTarget = null;
            EnableAI(true);
        }
        public override void OnDeath()
        {
            base.OnDeath();
            EnableAI(false);
        }
        #endregion

        #region is
        // AI 总控开关
        public virtual bool IsActiveAI
        {
            get
            {
                if (BaseGlobal.PlotMgr != null && 
                    !BaseGlobal.PlotMgr.IsEnableAI)
                    return false;
                if (SelfBaseUnit.IsPlayerCtrl())
                    return false;
                return IsEnableAI;
            }
        }
        //目标是否有效
        public virtual bool IsValidTarget()
        {
            return CurTarget != null && CurTarget.IsLive;
        }
        public bool IsHaveTarget()
        {
            if (CurTarget == null) return false;
            if (CurTarget.IsLive == false) return false;
            return CurTarget != null;
        }
        #endregion

        #region set
        public TUnit FindClosetForce<TUnit>(List<TUnit> data) where TUnit : BaseUnit
        {
            TUnit ret = null;
            float maxDis = float.MaxValue;
            foreach (var item in data)
            {
                if (!item.IsLive) continue;
                float curDis = MathUtil.SqrDistance(SelfBaseUnit.Pos, item.Pos);
                if (curDis <= maxDis)
                {
                    ret = item;
                    maxDis = curDis;
                }
            }
            return ret;
        }

        //寻找一个目标
        public BaseUnit FindTarget()
        {
            CurTarget = CalcTarget();
            return CurTarget;
        }
        public void SetTarget(BaseUnit target)
        {
            CurTarget = target;
        }
        // 开启AI
        public void EnableAI(bool b)
        {
            IsEnableAI = b;
        }
        #endregion

        #region override
        protected virtual BaseUnit CalcTarget() => throw new System.NotImplementedException();
        #endregion

    }

}