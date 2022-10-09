//------------------------------------------------------------------------------
// BaseLogoMgr.cs
// Copyright 2018 2018/11/12 
// Created by CYM on 2018/11/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYM
{
    public class BaseLogoMgr : BaseGFlowMgr, ILoader
    {
        #region prop
        Tweener tweener;
        UIConfig LogoConfig => UIConfig.Ins;
        List<LogoData> Logos => UIConfig.Ins.Logos;
        Image LogoImage => LogoPlayer.Image;
        Image LogoBG => LogoPlayer.BG;
        UVideo LogoVideo => LogoPlayer?.Video;
        Logo LogoPlayer;
        CanvasGroup CanvasGroup;
        #endregion

        #region life
        protected override string ResourcePrefabKey => "BaseLogoPlayer";
        protected override void OnStartLoad()
        {
            base.OnStartLoad();
            LogoPlayer = ResourceObj.GetComponent<Logo>();
            CanvasGroup = LogoPlayer.GetComponent<CanvasGroup>();
        }
        protected override void OnAllLoadEnd2()
        {
            base.OnAllLoadEnd2();
            DOTween.To(() => CanvasGroup.alpha, x => CanvasGroup.alpha = x, 0.0f, 1.0f).OnComplete(OnTweenEnd);
        }

        private void OnTweenEnd()
        {
            GameObject.Destroy(LogoPlayer.gameObject);
        }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (LogoVideo != null && LogoVideo.IsPlaying())
            {
                if (Input.anyKeyDown)
                {
                    LogoVideo.Stop();
                }
            }
        }
        #endregion

        #region loader
        public string GetLoadInfo()
        {
            return "Show Logo";
        }
        public IEnumerator Load()
        {

            bool IsNoLogo = Logos == null || Logos.Count == 0;
            if (LogoConfig.IsEditorMode() && !IsNoLogo)
            {
                while (LogoPlayer == null) 
                    yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(0.01f);
                for (int i = 0; i < Logos.Count; ++i)
                {
                    if (tweener != null) tweener.Kill();
                    if (Logos[i].IsImage())
                    {
                        LogoBG.color = Logos[i].BGColor;
                        LogoImage.color = new Color(1, 1, 1, 0);
                        tweener = DOTween.ToAlpha(() => LogoImage.color, x => LogoImage.color = x, 1.0f, Logos[i].InTime);
                        LogoImage.sprite = Logos[i].Logo;
                        LogoImage.SetNativeSize();
                        yield return new WaitForSeconds(Logos[i].WaitTime);
                        if (tweener != null) tweener.Kill();
                        tweener = DOTween.ToAlpha(() => LogoImage.color, x => LogoImage.color = x, 0.0f, Logos[i].OutTime);
                    }
                    else if (Logos[i].IsVideo())
                    {
                        LogoVideo.Play(Logos[i].Video);
                        while (LogoVideo.IsPreparing)
                            yield return new WaitForEndOfFrame();
                        LogoVideo.Show();
                        while (LogoVideo.IsPlaying())
                            yield return new WaitForEndOfFrame();
                        if (!IsNextVideo(i))
                            LogoVideo.Close();
                    }

                    if (i < Logos.Count - 1)
                        yield return new WaitForSeconds(Logos[i].OutTime);
                }
                LogoBG.color = Color.black;
                if (Logos.Count>0)
                    yield return new WaitForSeconds(0.5f);
            }
            IsShowedLogo = true;
            StarterUI.DoDestroy();

            bool IsNextVideo(int i)
            {
                var index = i + 1;
                if (index < Logos.Count - 1)
                {
                    return Logos[index].IsVideo();
                }
                return false;
            }
        }
        #endregion

        #region is
        public bool IsShowedLogo { get; private set; } = false;
        #endregion
    }
}