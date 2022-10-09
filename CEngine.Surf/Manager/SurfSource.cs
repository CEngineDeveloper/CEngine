//**********************************************
// Class Name	: Surface_Source
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine;
namespace CYM.Surf
{
    public class SurfSource : BaseSurf
    {
        #region set
        public override string GetDefaultMatName()
        {
            return "";
        }
        protected override void CopyProp(Material preMat, Material mat)
        {
            return;
        }
        public override void Use()
        {
            base.Use();
            Revert();
        }
        #endregion
    }
}