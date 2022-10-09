using UnityEngine;
//**********************************************
// Class Name	: BasePerform
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM.Unit
{
    public class EffectAudio : BaseMono
    {
        [SerializeField]
        float Volume = 1.0f;
        [SerializeField]
        bool IsForce = false;
        [SerializeField]
        bool IsLoop = false;
        [SerializeField]
        [Tooltip("是否缓存音效,缓存后在同一帧的时候不会播放相同的音效")]
        bool IsCache = true;
        [SerializeField]
        bool Is2D = false;
        [SerializeField]
        AudioClip[] Clips;

        AudioSource AudioSource;
        /// <summary>
        /// 如果没有检测到BasePerform 则会在OnEnable的时候播放,否则会在OnCreated的时候播放
        /// </summary>
        BasePerform Perform;

        BaseGlobal BaseGlobal => BaseGlobal.Ins;

        public override void Awake()
        {
            base.Awake();
            Perform = GetComponent<BasePerform>();
            if (Perform != null)
            {
                Perform.Callback_OnCreated += OnCreated;
            }
        }
        public override void OnDestroy()
        {
            if (Perform != null)
            {
                Perform.Callback_OnCreated -= OnCreated;
            }
            base.OnDestroy();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            if (Perform == null)
            {
                Play();
            }

        }
        public override void OnDisable()
        {
            if (AudioSource != null)
                AudioSource.Stop();
            base.OnDisable();
        }

        void OnCreated(BasePerform perform)
        {
            Play();
        }

        void Play()
        {
            if (BaseGlobal == null)
                return;
            if (Clips == null)
                CLog.Error("BaseAudioPlay没有配置音频文件:{0}", name);
            if (!Is2D)
            {
                AudioSource = BaseGlobal.AudioMgr.PlaySFX(
                    RandUtil.RandArray(Clips),
                    Pos,
                    IsLoop,
                    IsCache,
                    IsForce,
                    Volume * BaseGlobal.AudioMgr.GetRealVolumeSFX());
            }
            else
            {
                AudioSource = BaseGlobal.AudioMgr.PlayUI(RandUtil.RandArray(Clips), IsLoop);
            }
        }

    }

}