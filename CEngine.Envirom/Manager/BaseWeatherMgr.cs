//------------------------------------------------------------------------------
// BaseWeatherMgr.cs
// Copyright 2021 2021/3/23 
// Created by CYM on 2021/3/23
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM.Envirom
{
    public class BaseWeatherMgr : BaseGFlowMgr
    {
        #region prop
        Color SkyColor;
        Color EquatorColor;
        Color GroundColor;
        Color FogColor;
        Color SunColor;
        float Intensity=0.8f;
        #endregion

        #region life
        protected virtual float LerpSpeed => 1;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            SkyColor = RenderSettings.ambientLight;
            EquatorColor = RenderSettings.ambientEquatorColor;
            GroundColor = RenderSettings.ambientGroundColor;
            FogColor = RenderSettings.fogColor;
            if (SunObj.Obj)
            {
                SunColor = SunObj.Obj.color;
                Intensity = SunObj.Obj.intensity;
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (BaseGlobal.BattleMgr!=null && 
                BaseGlobal.BattleMgr.IsGameStartOver)
            {
                var delta = Time.smoothDeltaTime * LerpSpeed;
                RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, SkyColor, delta);
                RenderSettings.ambientEquatorColor = Color.Lerp(RenderSettings.ambientEquatorColor, EquatorColor, delta);
                RenderSettings.ambientGroundColor = Color.Lerp(RenderSettings.ambientGroundColor, GroundColor, delta);
                if (RenderSettings.fog)
                {
                    RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, FogColor, delta);
                }
                if (SunObj.Obj != null)
                {
                    SunObj.Obj.color = Color.Lerp(SunObj.Obj.color, SunColor, delta);
                    SunObj.Obj.intensity = Mathf.Lerp(SunObj.Obj.intensity, Intensity, delta);
                }
            }
        }
        #endregion

        #region set
        public void SetSkyColor(Color col,bool force=false)
        {
            SkyColor = col;
            if (force)
            {
                RenderSettings.ambientLight = SkyColor;
            }
        }
        public void SetEquatorColor(Color col, bool force = false)
        {
            EquatorColor = col;
            if (force)
            {
                RenderSettings.ambientEquatorColor = EquatorColor;
            }
        }
        public void SetGroundColor(Color col, bool force = false)
        {
            GroundColor = col;
            if (force)
            {
                RenderSettings.ambientGroundColor = GroundColor;
            }
        }
        public void SetSunColor(Color col, bool force = false)
        {
            SunColor = col;
            if (force && SunObj.Obj != null)
            {
                SunObj.Obj.color = SunColor;
            }
        }
        public void SetSunIntensity(float intensity, bool force = false)
        {
            Intensity = intensity;
            if (force && SunObj.Obj != null)
            {
                SunObj.Obj.intensity = intensity;
            }
        }
        public void SetFogColor(Color col, bool force = false)
        {
            FogColor = col;
            if (force)
            {
                RenderSettings.fogColor = FogColor;
            }
        }

        #endregion
    }
}