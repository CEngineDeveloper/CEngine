//------------------------------------------------------------------------------
// UIVideoer.cs
// Copyright 2021 2021/5/20 
// Created by CYM on 2021/5/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
namespace CYM
{
    public sealed class UIVideoer : MonoBehaviour 
    {
        static VideoPlayer VideoPlayer { get; set; }
        static RawImage RawImage { get; set; }
        static GameObject VedioGO;

        private void Awake()
        {
            VedioGO = Util.CreateGlobalResourceGameObject("BaseUIVideoPlayer");
            VideoPlayer = VedioGO.GetComponentInChildren<VideoPlayer>();
            RawImage = VedioGO.GetComponentInChildren<RawImage>();
            VideoPlayer.loopPointReached += LoopPointReached;
            VideoPlayer.prepareCompleted += Prepared;
            VedioGO.SetActive(false);
            VideoPlayer?.Stop();
            RawImage?.CrossFadeAlpha(0, 0, true);
        }
        private void Update()
        {
            if (Application.isEditor &&
                BaseInputMgr.IsAnyKeyDown() &&
                VideoPlayer.isPlaying)
            {
                Stop();
            }
        }
        #region set
        public static void Play(string videoName,float fadeTime=0)
        {
            VedioGO.SetActive(true);
            if (VideoPlayer != null)
            {
                VideoPlayer.clip = BaseGlobal.RsVideo.Get(videoName);
                PlayInternal();
            }
        }
        public static void PlayResource(string videoName)
        {
            VedioGO.SetActive(true);
            if (VideoPlayer != null)
            {
                VideoPlayer.clip = BaseGlobal.GRMgr.GetResources<VideoClip>(videoName); 
                PlayInternal();
            }

        }
        private static void PlayInternal(float fadeTime=0)
        {
            if (VideoPlayer.clip == null) return;
            if (!VideoPlayer.isPrepared)
            {
                VideoPlayer.Prepare();
            }
            else VideoPlayer.Play();
            RawImage.color = Color.white;
            RawImage.CrossFadeAlpha(1, fadeTime, true);
            BaseGlobal.AudioMgr?.MuteMusic(true);
        }
        public static void Stop()
        {
            float fadeTime = 0.5f;
            RawImage.CrossFadeAlpha(0, fadeTime, true);
            Util.Invoke(()=> {
                VedioGO.SetActive(false);
                VideoPlayer?.Stop();
                BaseGlobal.AudioMgr?.MuteMusic(false);
            }, fadeTime+0.01f);
        }
        #endregion

        #region Callback
        static void OnTweenPlay()
        {

        }
        static void OnTweenStop()
        {

        }
        static void LoopPointReached(VideoPlayer source)
        {
            Stop();
            VedioGO.SetActive(false);
        }
        void Prepared(VideoPlayer vPlayer)
        {
            RawImage.texture = VideoPlayer.texture;
            vPlayer.Play();
        }
        #endregion
    }
}