//**********************************************
// Class Name	: Surface_Dissolve
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using UnityEngine;
namespace CYM.Surf
{
    public class SurfDissolve : BaseSurf
    {
        #region prop
        private float delay = 0.0f;
        private float speed = 1.0f;
        private Timer delayTimer = new Timer();
        private float curAmount = 0.0f;
        #endregion

        #region life
        Tuple<string,string> PropMainTex = new Tuple<string, string>("_MainTex","_MainTex");
        Tuple<string, string> PropNormalMap = new Tuple<string, string>("_BumpMap", "_BumpMap");
        string PropAmount = "_Amount";
        public SurfDissolve SetPropName(Tuple<string, string> mainTex, Tuple<string, string> bumbMap,string amount)
        {
            PropMainTex = mainTex;
            PropNormalMap = bumbMap;
            PropAmount = amount;
            return this;
        }
        public SurfDissolve SetDelay(float p)
        {
            delay = p;
            return this;
        }
        public SurfDissolve SetSpeed(float p)
        {
            speed = p;
            return this;
        }
        #endregion

        #region set
        public override void Use()
        {
            base.Use();
            delayTimer.Restart();
        }
        protected override void CopyProp(Material preMat, Material mat)
        {
            Texture mainTex = preMat.GetTexture(PropMainTex.Item1);
            Texture mbumbMap = preMat.GetTexture(PropNormalMap.Item1);
            if(mainTex)
                mat.SetTexture(PropMainTex.Item2, mainTex);
            if (mbumbMap)
                mat.SetTexture(PropNormalMap.Item2, mbumbMap);
            curAmount = 0.0f;
            mat.SetFloat(PropAmount, curAmount);
        }
        #endregion

        #region get
        public override string GetDefaultMatName()
        {
            return "DissolveFire";
        }
        #endregion

        #region update
        public override void Update()
        {
            base.Update();
            UpdateDissolve();
        }
        void UpdateDissolve()
        {
            if (delayTimer.Elapsed() < delay) return;
            if (GetRenderers() != null)
            {
                curAmount += Time.deltaTime * speed;
                curAmount = Mathf.Clamp01(curAmount);
                ForeachMaterial((x) => x.SetFloat(PropAmount, curAmount));
            }
        }
        #endregion

    }
}