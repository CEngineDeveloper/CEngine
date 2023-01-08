using CYM.Audio;
using System;
using System.Collections.Generic;
namespace CYM
{
    public class BaseBGMMgr : BaseGFlowMgr
    {
        #region SoundConnection
        protected SoundConnection PreSoundConnection;
        protected SoundConnection StartSoundConnection;
        protected SoundConnection BattleSoundConnection;
        protected SoundConnection CreditsSoundConnection;
        protected SoundConnection TempSoundConnection;
        #endregion

        #region prop
        public BGMType PreBGMType { get; protected set; } = BGMType.MainMenu;
        #endregion

        #region get
        /// <summary>
        /// 得到当前歌曲的名称
        /// </summary>
        /// <returns></returns>
        public string GetCurSong()
        {
            var temp = SoundManager.GetCurrentSong();
            if (temp == null)
                return "";
            return temp.name;
        }
        #endregion

        #region is
        /// <summary>
        /// 是否暂停
        /// </summary>
        /// <returns></returns>
        public bool IsPaused()
        {
            return SoundManager.IsPaused();
        }
        public bool IsHaveMusic(string name)
        {
            return BaseGlobal.RsMusic.IsHave(name);
        }
        #endregion

        #region Set
        /// <summary>
        /// 下一首音乐
        /// </summary>
        public void Next()
        {
            SoundManager.Next();
        }
        /// <summary>
        /// 暂停
        /// </summary>
        public void PauseToggle()
        {
            SoundManager.PauseToggle();

        }
        /// <summary>
        /// 上一首
        /// </summary>
        public void Prev()
        {
            SoundManager.Prev();
        }
        public void PlayConnection(SoundConnection connection)
        {
            if (connection == null)
                return;
            SoundManager.PlayConnection(connection);
        }
        #endregion

        #region start music
        public void StartBGM(List<string> musics)
        {
            TempSoundConnection = null;
            TempSoundConnection = CreateConnection(musics);
            PlayConnection(TempSoundConnection);
        }
        public void StartBGM(string musics)
        {
            if (musics.IsInv()) return;
            TempSoundConnection = null;
            TempSoundConnection = CreateConnection(new List<string>() { musics });
            PlayConnection(TempSoundConnection);
        }
        public void StartMain()
        {
            PreBGMType = BGMType.MainMenu;
            PreSoundConnection = StartSoundConnection;
            PlayConnection(StartSoundConnection);
        }
        public void StartCredits()
        {
            PreBGMType = BGMType.Credits;
            PreSoundConnection = CreditsSoundConnection;
            PlayConnection(CreditsSoundConnection);
        }
        public void StartBattle()
        {
            PreBGMType = BGMType.Battle;
            PreSoundConnection = BattleSoundConnection;
            PlayConnection(BattleSoundConnection);
        }
        public void Revert()
        {
            if (PreBGMType == BGMType.MainMenu)
                StartMain();
            else if (PreBGMType == BGMType.Battle)
                StartBattle();
            else if (PreBGMType == BGMType.Credits)
                StartCredits();
            else
                StartMain();
        }
        #endregion

        #region get
        /// <summary>
        /// 创建音乐
        /// </summary>
        /// <returns></returns>
        protected SoundConnection CreateConnection(List<string> musics, SoundManager.PlayMethod type = SoundManager.PlayMethod.ContinuousPlayThroughWithRandomDelayInRange)
        {
            var audios = BaseGlobal.RsMusic.Get(musics);
            if (audios == null || audios.Count == 0)
                return null;
            return SoundManager.CreateSoundConnection("", type, audios.ToArray());
        }
        #endregion

        #region must Override
        protected virtual SoundConnection CreateMainBGM()
        {
            return CreateConnection(BaseGlobal.RsMusic.GetStrsByCategory(GameConfig.Ins.StartMusics));
        }
        protected virtual SoundConnection CreateCreditsBGM()
        {
            return CreateConnection(BaseGlobal.RsMusic.GetStrsByCategory(GameConfig.Ins.CreditsMusics));
        }
        protected virtual SoundConnection CreateBattleBGM()
        {
            return CreateConnection(BaseGlobal.RsMusic.GetStrsByCategory(GameConfig.Ins.BattleMusics), SoundManager.PlayMethod.ShufflePlayThroughWithRandomDelayInRange);
        }
        #endregion

        #region Callback
        protected override void OnAllLoadEnd1()
        {
            base.OnAllLoadEnd1();
            StartSoundConnection = CreateMainBGM();
            CreditsSoundConnection = CreateCreditsBGM();
            BattleSoundConnection = CreateBattleBGM();
            StartMain();
        }
        public override void OnGameStartOver()
        {
            base.OnGameStartOver();
            StartBattle();
        }
        protected override void OnBackToStart()
        {
            base.OnBackToStart();
            StartMain();
        }
        #endregion

    }

}