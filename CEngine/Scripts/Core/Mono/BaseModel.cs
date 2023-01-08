//**********************************************
// Class Name	: Unit
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
// 负责单位的 IK监听 动画事件触发 以及其他mono的工作 单位属性显示等等
//**********************************************


using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public class BaseModel : BaseMono
    {
        [SerializeField]
        bool IsFootIK = true;
        protected BaseUnit SelfBaseUnit;
        protected Animator animator { get; set; }
        protected BaseGlobal SelfBaseGlobal { get; set; }

        public override void Awake()
        {
            base.Awake();
            SelfBaseGlobal = BaseGlobal.Ins;
        }
        public override void OnBeSetup()
        {
            base.OnBeSetup();
            SelfBaseUnit = GetComponentInChildren<BaseUnit>();
            animator = GetComponentInChildren<Animator>();
        }

        #region Anim Event
        protected virtual void OnAnimatorIK(int layerIndex)
        {
            if (IsFootIK)
            {
                if (animator != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1.0f);
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1.0f);
                }
            }
        }
        protected virtual void OnTrigger(int param1)
        {
            if (SelfBaseUnit == null)
                return;
            SelfBaseUnit.OnAnimTrigger(param1);
        }
        #endregion

        //#region AI
        //public virtual AI.BT.Tree GetTree()
        //{
        //    return null;
        //}
        //#endregion

        //#region Attr
        //public virtual Dictionary<string, float> GetAttr()
        //{
        //    return null;
        //}
        //#endregion
    }

}