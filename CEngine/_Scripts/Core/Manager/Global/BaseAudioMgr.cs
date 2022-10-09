using CYM.Audio;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public class BaseAudioMgr : BaseMgr
    {
        public const float DefaultMaxDistance = 18.0f;
        protected HashSet<AudioClip> CacheAudioClip = new HashSet<AudioClip>();
        protected HashSet<string> CacheAudioClipKey = new HashSet<string>();
        protected ISettingsMgr<DBBaseSettings> SettingsMgr => BaseGlobal.SettingsMgr;
        public AudioListener AudioListener { get; private set; }
        public SoundManager Ins { get; private set; }
        #region prop
        public bool IsEnableSFX { get; protected set; } = true;
        #endregion

        #region life
        public sealed override MgrType MgrType => MgrType.Global;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
            NeedUpdate = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            if (Ins == null)
            {
                Ins = Util.CreateGlobalObj<SoundManager>("SoundManager");
                AudioListener = Ins.gameObject.AddComponent<AudioListener>();
            }
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (CacheAudioClip.Count > 0)
                CacheAudioClip.Clear();
            if (CacheAudioClipKey.Count > 0)
                CacheAudioClipKey.Clear();

        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (AudioListener != null)
            {
                if (BaseGlobal.MainCamera != null)
                    AudioListener.transform.position = BaseGlobal.MainCamera.transform.position;
                else
                    AudioListener.transform.position = BaseGlobal.Ins.Pos;
            }
        }
        #endregion

        #region set
        // 关闭音效
        // 和MuteSFX 的区别在于这个用于临时处理,不会保存到数据库
        public void EnableSFX(bool b)
        {
            IsEnableSFX = b;
        }
        public AudioSource PlayUI(AudioClip clip, bool isLoop = false, bool isCache = false)
        {
            if (isCache)
            {
                if (!CacheAudioClip.Contains(clip))
                    CacheAudioClip.Add(clip);
                else
                    return null;
            }
            var temp = SoundManager.PlaySFX(clip, isLoop);
            if (temp != null)
                temp.spatialBlend = 0.0f;
            return temp;
        }
        public AudioSource PlaySFX(AudioClip clip, Vector3? pos = null, bool isLoop = false, bool isCache = false, bool isForce = false, float volume = float.MaxValue)
        {
            if (clip == null)
                return null;
            if (!isForce && !IsEnableSFX)
                return null;
            if (isCache)
            {
                if (!CacheAudioClip.Contains(clip))
                    CacheAudioClip.Add(clip);
                else
                    return null;
            }
            var ret = SoundManager.PlaySFX(clip, isLoop, 0.0f, volume, float.MaxValue, pos.HasValue ? pos.Value : default);
            if (ret != null)
            {
                ret.rolloffMode = AudioRolloffMode.Linear;
                ret.spatialBlend = 1.0f;
                ret.maxDistance = DefaultMaxDistance;
            }
            return ret;
        }
        public void PlayPlUI(string clipName, BaseUnit player, bool isLoop = false)
        {
            AudioSource temp = PlayPlSFX(clipName, player, AudioListener.transform.position, isLoop);
            if (temp != null)
                temp.spatialBlend = 0.0f;
        }
        public AudioSource PlayPlSFX(string clipName, BaseUnit player, Vector3? pos = null, bool isLoop = false, float maxDis = 60.0f)
        {
            if (clipName == null) return null;
            if (player == null) return null;
            if (!player.IsPlayer()) return null;
            return PlaySFX(clipName, pos, isLoop, maxDis);
        }
        public void PlayMusic(AudioClip clip, bool isLoop = false)
        {
            SoundManager.Play(clip, isLoop);
        }
        public void Stop()
        {
            SoundManager.Stop();
        }
        public void Pause()
        {
            SoundManager.Pause();
        }
        public void Resume()
        {
            SoundManager.UnPause();
        }
        public virtual void Mute(bool toggle = true)
        {
            SoundManager.Mute(toggle);
            if(SettingsMgr!=null)
                SettingsMgr.Settings.Mute = toggle;
        }
        public virtual void MuteMusic(bool toggle = true)
        {
            SoundManager.MuteMusic(toggle);
            if (SettingsMgr != null)
                SettingsMgr.Settings.MuteMusic = toggle;
        }
        public virtual void MuteSFX(bool toggle = true)
        {
            SoundManager.MuteSFX(toggle);
            if (SettingsMgr != null)
                SettingsMgr.Settings.MuteSFX = toggle;
        }
        public virtual void SetVolumeMusic(float volume)
        {
            SoundManager.SetVolumeMusic(volume);
            if (SettingsMgr != null)
                SettingsMgr.Settings.VolumeMusic = volume;
        }
        public virtual void SetVolumeSFX(float volume)
        {
            SoundManager.SetVolumeSFX(volume);
            if (SettingsMgr != null)
                SettingsMgr.Settings.VolumeSFX = volume;
        }
        public virtual void SetVolume(float volume)
        {
            SoundManager.SetVolume(volume);
            if (SettingsMgr != null)
                SettingsMgr.Settings.Volume = volume;
        }
        public virtual void SetVolumeVoice(float volume)
        {
            if (SettingsMgr != null)
                SettingsMgr.Settings.VolumeVoice = volume;
        }
        public AudioSource PlayUI(string clipName, bool isLoop = false, float volume = float.MaxValue)
        {
            if (clipName.IsInv())
                return null;
            AudioSource temp = PlaySFX(clipName, AudioListener.transform.position, isLoop, 18, false, true, volume);
            if (temp != null)
                temp.spatialBlend = 0.0f;
            return temp;
        }
        public AudioSource PlaySFX2D(string clipName, bool isLoop = false,bool isCache=false)
        {
            if (!IsEnableSFX)
                return null;
            if (clipName.IsInv())
                return null;
            AudioSource temp = PlaySFX(clipName, AudioListener.transform.position, isLoop, 18, isCache, true);
            if (temp != null)
                temp.spatialBlend = 0.0f;
            return temp;
        }
        public AudioSource PlaySFX(string clipName, Vector3? pos = null, bool isLoop = false, float maxDis = DefaultMaxDistance, bool isCache = false, bool isForce = false, float volume = float.MaxValue)
        {
            if (!isForce && !IsEnableSFX) return null;
            if (clipName == null) return null;
            if (clipName.IsInv()) return null;
            AudioSource temp = null;
            var clip = BaseGlobal.RsAudio.Get(clipName);
            if (isCache)
            {
                if (!CacheAudioClip.Contains(clip))
                    CacheAudioClip.Add(clip);
                else
                    return null;
            }

            if (pos.HasValue)
                temp = SoundManager.PlaySFX(clip, isLoop, 0.0f, volume, float.MaxValue, pos.Value);
            else
                temp = SoundManager.PlaySFX(clip, isLoop, 0.0f, volume, float.MaxValue);
            if (temp != null)
            {
                temp.rolloffMode = AudioRolloffMode.Linear;
                temp.spatialBlend = 1.0f;
                temp.maxDistance = maxDis;
            }
            return temp;
        }
        public void PlaySFX(string[] clipName, Vector3? pos = null, bool isLoop = false, float maxDis = 60.0f)
        {
            if (clipName == null)
                return;
            foreach (var item in clipName)
                PlaySFX(item, pos, isLoop, maxDis);
        }


        public void AddToCache(AudioClip clip)
        {
            CacheAudioClip.Add(clip);
        }
        public bool IsInCache(AudioClip clip)
        {
            if (CacheAudioClip.Contains(clip))
                return true;
            return false;
        }
        public void AddToCache(string clip)
        {
            CacheAudioClipKey.Add(clip);
        }
        public bool IsInCache(string clip)
        {
            if (CacheAudioClipKey.Contains(clip))
                return true;
            return false;
        }
        #endregion

        #region get
        public virtual bool IsMute()
        {
            if (SettingsMgr == null)
                return false;
            return SettingsMgr.Settings.Mute;
        }
        public virtual bool IsMuteMusic()
        {
            if (SettingsMgr == null)
                return false;
            return SettingsMgr.Settings.MuteMusic;
        }
        public virtual bool IsMuteSFX()
        {
            if (SettingsMgr == null)
                return false;
            return SettingsMgr.Settings.MuteSFX;
        }
        public virtual float GetVolumeMusic()
        {
            if (SettingsMgr == null)
                return 1;
            return SettingsMgr.Settings.VolumeMusic;
        }
        public virtual float GetVolumeSFX()
        {
            if (SettingsMgr == null)
                return 1;
            return SettingsMgr.Settings.VolumeSFX;
        }
        public virtual float GetVolume()
        {
            if (SettingsMgr == null)
                return 1;
            return SettingsMgr.Settings.Volume;
        }
        public virtual float GetVolumeVoice()
        {
            if (SettingsMgr == null)
                return 1;
            return SettingsMgr.Settings.VolumeVoice;
        }
        #endregion

        #region real
        public virtual float GetRealVolumeVoice()
        {
            if (SettingsMgr == null)
                return 1;
            return SettingsMgr.Settings.VolumeVoice * SettingsMgr.Settings.Volume;
        }

        public virtual float GetRealVolumeSFX()
        {
            return SoundManager.GetVolumeSFX() * SoundManager.GetVolume();
        }
        #endregion

    }
}
