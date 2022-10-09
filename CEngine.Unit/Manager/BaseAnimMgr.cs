//**********************************************
// Class Name	: BaseAnimMgr
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
namespace CYM.Unit
{
    public class BaseAnimMgr : BaseMgr
    {
        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        #endregion

        #region methon
        public virtual void Play(string animName, bool fixedTime = true) => throw new NotImplementedException();
        public virtual void CrossFade(string animName, float transDuration = 0.1f, bool fixedTime = true) => throw new NotImplementedException();
        public virtual void EnableAnim(bool b) { }
        #endregion

        #region is
        public virtual bool IsInTranslation(int layer = 0) => throw new NotImplementedException();
        public virtual bool IsName(string animName, int layer = 0)
        {
            if (animName == null)
                throw new ArgumentNullException("animName");
            return false;
        }
        public virtual bool IsTransitionName(string animName, int layer = 0)
        {
            if (animName == null)
                throw new ArgumentNullException("animName");
            return false;
        }
        #endregion
    }
}