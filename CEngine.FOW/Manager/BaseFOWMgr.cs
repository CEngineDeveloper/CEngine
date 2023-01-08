//------------------------------------------------------------------------------
// BaseFOWMgr.cs
// Copyright 2018 2018/11/11 
// Created by CYM on 2018/11/11
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FoW;
using System.Linq;
using UnityEngine;

namespace CYM.FOW
{
    public class BaseFOWMgr : BaseGFlowMgr
    {
        #region prop
        FogOfWarPPSv2 FOWPPS { get; set; }
        private FogOfWarTeam FOW { get; set; }
        TweenerCore<float, float, FloatOptions> fogAlphaTween;
        public bool IsFogShow { get; private set; } = true;
        public HashList<BaseFOWRevealerMgr> FOWRevealerList { get; private set; } = new HashList<BaseFOWRevealerMgr>();
        public AnimationCurve CurveFOW { get; set; } = new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(1.0f, 0f));
        #endregion

        #region life
        public virtual BaseCameraMgr CameraMgr => BaseGlobal.CameraMgr;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedFixedUpdate = true;
        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            if (CameraMgr != null &&
                CameraMgr.MainCamera != null)
            {
                FOWPPS = Util.GetSetting<FogOfWarPPSv2>(CameraMgr.MainCameraTrans.gameObject);
                FOW = FogOfWarTeam.instances.FirstOrDefault();
                EnableFOW(true);
                FOW?.SetAll(255);
            }
        }
        public override void OnGameStart1()
        {
            base.OnGameStart1();

        }
        public override void OnGameStartOver()
        {
            base.OnGameStartOver();
            SetDirty();
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            EnableFOW(false);
            FOWRevealerList.Clear();
        }
        public override void Refresh()
        {
            base.Refresh();
            foreach (var item in FOWRevealerList)
            {
                item.Refresh();
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (FOW && FOWPPS)
            {
                float val = CameraMgr.GetCustomScrollVal(0.3f);
                FOWPPS.fogColor.value.a = CurveFOW.Evaluate(val);
            }
        }
        #endregion

        #region set
        public void Show(bool b)
        {
            if (IsFogShow == b)
                return;
            IsFogShow = b;
            if (fogAlphaTween != null)
                fogAlphaTween.Kill();
            //tween地图颜色
            fogAlphaTween = DOTween.To(() => FOWPPS.fogColor.value.a, x => FOWPPS.fogColor.value.a = x, b ? 0.3f : 0.0f, 0.3f);
        }
        /// <summary>
        /// 设置所有
        /// </summary>
        /// <param name="val"></param>
        public void SetAll(byte val)
        {
            FOW?.SetAll(val);
        }
        public void EnableFOW(bool b)
        {
            if (FOW)
            {
                FOW.enabled = b;
            }
        }
        #endregion

        #region is
        /// <summary>
        /// 是否在迷雾内
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="minFog"></param>
        /// <returns></returns>
        public bool IsInFog(Vector3 pos, byte minFog = 70)
        {
            if (FOW)
                return FOW.GetFogValue(pos) > minFog;
            return false;
        }
        public bool IsInFog(Vector3 pos)
        {
            if (FOW)
                return FOW.GetFogValue(pos) > 0;
            return false;
        }
        #endregion
    }
}