//------------------------------------------------------------------------------
// BaseVideo.cs
// Copyright 2020 2020/3/15 
// Created by CYM on 2020/3/15
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Video;

namespace CYM.UI
{
    public class UVideoData : UData
    {
        public Func<VideoClip> Video;
        public string VideoStr;
    }
    [AddComponentMenu("UI/Control/UVideo")]
    [HideMonoScript]
    //[RequireComponent(typeof(VideoPlayer))]
    //[RequireComponent(typeof(UnityEngine.UI.RawImage))]
    public class UVideo : UPres<UVideoData>
    {
        #region Inspector
        [SerializeField, FoldoutGroup("Inspector")]
        bool IsCloseWhenAnyKey = false;
        [SerializeField, FoldoutGroup("Inspector")]
        bool IsCloseWhenPlayEnd = false;
        [SerializeField, FoldoutGroup("Inspector"), AssetsOnly]
        VideoClip DefaultVideoClip;
        [SerializeField, FoldoutGroup("Inspector"), ChildGameObjectsOnly]
        UnityEngine.UI.RawImage IImage;
        #endregion

        VideoPlayer VideoPlayer;
        public bool IsPrepared => VideoPlayer.isPrepared;
        public bool IsPreparing { get; private set; } = false;

        public override bool IsAtom => true;
        protected override void Awake()
        {
            base.Awake();
            if (IImage == null)
            {
                IImage = GO.SafeAddComponet<UnityEngine.UI.RawImage>();
            }
            if (VideoPlayer == null)
            {
                VideoPlayer = GO.SafeAddComponet<VideoPlayer>();
                VideoPlayer.source = VideoSource.VideoClip;
                VideoPlayer.clip = DefaultVideoClip;
                VideoPlayer.playOnAwake = false;
                VideoPlayer.waitForFirstFrame = true;
                VideoPlayer.isLooping = false;
                VideoPlayer.skipOnDrop = true;
                VideoPlayer.playbackSpeed = 1;
                VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
                VideoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
                VideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            }
            VideoPlayer.prepareCompleted += Prepared;
            VideoPlayer.loopPointReached += LoopPointReached;
        }
        //public override void OnUpdate()
        //{
        //    base.OnUpdate();

        //}
        private void Update()
        {
            if (IsCloseWhenAnyKey &&
                BaseInputMgr.IsAnyKeyDown())
            {
                Close();
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            //Data.ClickClip = Const.STR_Inv;
            if (DefaultVideoClip != null)
                VideoPlayer.clip = DefaultVideoClip;
            else if (Data.Video != null)
                VideoPlayer.clip = Data.Video.Invoke();
            else if (!Data.VideoStr.IsInv())
                VideoPlayer.clip = Data.VideoStr.GetVideoClip();
        }
        public override void OnShow(bool isShow)
        {
            base.OnShow(isShow);
            if (isShow)
            {
                Play();
            }
        }
        protected override void OnClose()
        {
            base.OnClose();
            Stop();
        }

        public void Play(VideoClip clip = null)
        {
            if (VideoPlayer == null)
                return;
            if (IsPlaying()) return;
            if (clip != null) VideoPlayer.clip = clip;
            if (VideoPlayer.clip == null) return;
            if (!VideoPlayer.isPrepared)
            {
                IsPreparing = true;
                VideoPlayer.Prepare();
            }
            else VideoPlayer.Play();
            IImage.color = Color.white;
            IImage.CrossFadeAlpha(1,0,true);
        }
        public void Stop()
        {
            if (VideoPlayer == null)
                return;
            VideoPlayer.Stop();
        }

        public bool IsPlaying()
        {
            if (VideoPlayer == null)
                return false;
            return VideoPlayer.isPlaying || IsPreparing;
        }

        void Prepared(VideoPlayer vPlayer)
        {
            IsPreparing = false;
            IImage.texture = VideoPlayer.texture;
            vPlayer.Play();
        }
        private void LoopPointReached(VideoPlayer source)
        {
            if (IsCloseWhenPlayEnd)
                Close();
        }
        private void OnPlayEnd()
        {

        }
    }
}