//------------------------------------------------------------------------------
// BaseVoiceMgr.cs
// Copyright 2023 2023/1/21 
// Created by CYM on 2023/1/21
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using System.Collections.Generic;

namespace CYM.Unit
{
    public class BaseVoiceMgr : BaseMgr
    {
        protected HashSet<string> CacheAudioClipKey = new HashSet<string>();
        public virtual float DefaultMaxDistance => 8.0f;
        protected AudioSource VoiceAudioSource { get; set; }

        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            VoiceAudioSource = SelfMono.EnsureComponet<AudioSource>();
            VoiceAudioSource.playOnAwake = false;
            VoiceAudioSource.rolloffMode = AudioRolloffMode.Linear;
            VoiceAudioSource.spatialBlend = 1.0f;
            VoiceAudioSource.minDistance = 1.0f;
            VoiceAudioSource.maxDistance = DefaultMaxDistance;
        }
        #endregion

        #region set
        public void PlayPlayerSFX(string sfx)
        {
            if (!SelfBaseUnit.IsPlayerCtrl())
                return;
            BaseGlobal.AudioMgr.PlaySFX(sfx, SelfBaseUnit.Pos, false, 0.1f);
        }
        public void PlaySFX(string sfx, bool isCache = false, float volume = 1.0f)
        {
            BaseGlobal.AudioMgr.PlaySFX(sfx, SelfBaseUnit.Pos, false, 0.1f, isCache, false, BaseGlobal.AudioMgr.GetRealVolumeSFX() * volume);
        }
        public void PlayVoice(string sfx, float volume = 1.0f, bool interrupt = true)
        {
            if (sfx.IsInv()) return;
            if (!BaseGlobal.AudioMgr.IsEnableSFX) return;
            if (BaseGlobal.AudioMgr.IsMuteSFX()) return;
            VoiceAudioSource.volume = volume * BaseGlobal.AudioMgr.GetRealVolumeVoice();
            var clip = BaseGlobal.RsAudio.Get(sfx);
            if (!BaseGlobal.AudioMgr.IsInCache(clip))
            {
                if (VoiceAudioSource.isPlaying && !interrupt)
                    return;
                BaseGlobal.AudioMgr.AddToCache(clip);
                VoiceAudioSource.Stop();
                VoiceAudioSource.PlayOneShot(clip);
            }
        }
        public void PlayGroupVoice(string sfx, int index, float volume = 1.0f, bool interrupt = true)
        {
            if (BaseGlobal.AudioMgr.IsInCache(sfx))
                return;
            PlayVoice(sfx + index, volume, interrupt);
            BaseGlobal.AudioMgr.AddToCache(sfx);
        }
        public void PlaySFX2D(string id)
        {
            BaseGlobal.AudioMgr.PlaySFX2D(id, false);
        }
        public void PlayGroupSFX2D(string id, int index)
        {
            if (BaseGlobal.AudioMgr.IsInCache(id))
                return;
            PlaySFX2D(id + index);
            BaseGlobal.AudioMgr.AddToCache(id);
        }
        public void PlayUI(string id)
        {
            BaseGlobal.AudioMgr.PlayUI(id);
        }
        #endregion
    }
}