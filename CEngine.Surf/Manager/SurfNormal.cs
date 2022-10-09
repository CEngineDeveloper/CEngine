//**********************************************
// Class Name	: Surface_Dissolve
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine;
namespace CYM.Surf
{
    public class SurfNormal : BaseSurf
    {
        protected override void CopyProp(Material preMat, Material mat)
        {
            Texture mainTex = preMat.GetTexture("_MainTex");
            mat.SetTexture("_MainTex", mainTex);
        }
    }

}