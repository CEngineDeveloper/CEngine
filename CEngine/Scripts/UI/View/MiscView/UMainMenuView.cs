//------------------------------------------------------------------------------
// BaseMainMenuView.cs
// Copyright 2021 2021/9/5 
// Created by CYM on 2021/9/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
namespace CYM.UI
{
    public class UMainMenuView : UUIView 
    {
        #region inspector
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UImage GameLogo;
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UImage Logo;
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UText VersionText;
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UText BuildTime;
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UText Tips;
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UButton BntStartGame;
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UButton BntContinueGame;
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UButton BntTutorial;
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UButton BntSettings;
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UButton BntCredits;
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UButton BntQuit;
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly, SerializeField]
        UButton BntWebsite;
        #endregion

        #region prop
        BasePlatSDKMgr PlatSDKMgr => BaseGlobal.PlatSDKMgr;
        IDBMgr<DBBaseGame> DBMgr => BaseGlobal.DBMgr;
        #endregion

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            BntStartGame?.Init(new UButtonData { NameKey = "新游戏", HoverClip = "BntHover",OnClick = OnClickNewGame, IsInteractable = IsNewGame });
            BntContinueGame?.Init(new UButtonData { NameKey = "继续游戏", HoverClip = "BntHover", OnClick = OnContinueGame, IsInteractable = IsContinueGame });
            BntTutorial?.Init(new UButtonData { NameKey = "游戏教程", HoverClip = "BntHover", OnClick = OnTutorial });
            BntSettings?.Init(new UButtonData { NameKey = "设置", HoverClip = "BntHover", OnClick = OnClickSettings });
            BntCredits?.Init(new UButtonData { NameKey = "制作人", HoverClip = "BntHover", OnClick = OnClickCredits });
            BntQuit?.Init(new UButtonData { NameKey = "退出", HoverClip = "BntHover", OnClick = OnClickExitGame });
            BntWebsite?.Init(new UButtonData { NameKey = "Websit", HoverClip = "BntHover", IsTrans = false, OnClick = OnClickWebsite });
            Logo?.Init(new UImageData { Icon = GetMainMenuLogo });
            GameLogo?.Init(new UImageData { Icon = GetGameLogo });
            VersionText?.Init(new UTextData { Name = () => Version.GameVersion, IsTrans = false });
            BuildTime?.Init(new UTextData { Name = () => Version.BuildTime, IsTrans = false });
            Tips?.Init(new UTextData { NameKey = "MainMenuTip" });
        }
        #endregion

        #region is
        private bool IsContinueGame(int arg)
        {
            if (PlatSDKMgr == null || DBMgr == null)
                return false;
            return PlatSDKMgr.IsLegimit;
        }

        private bool IsNewGame(int arg)
        {
            if (PlatSDKMgr == null)
                return false;
            return PlatSDKMgr.IsLegimit;
        }
        #endregion

        #region Callback
        protected virtual Sprite GetMainMenuLogo()
        {
            return null;
        }
        protected virtual Sprite GetGameLogo()
        {
            return null;
        }
        protected virtual void OnClickExitGame(UControl arg1, PointerEventData arg2)
        {
            BaseGlobal.Quit();
        }
        protected virtual void OnClickSettings(UControl arg1, PointerEventData arg2)
        {
            USettingsView.Default?.Toggle();
        }
        protected virtual void OnTutorial(UControl arg1, PointerEventData arg2)
        {
            //CommonUIMgr.SaveOrLoadView.Show(SaveOrLoad.Load);
        }   
        protected virtual void OnContinueGame(UControl arg1, PointerEventData arg2)
        {
            USaveOrLoadView.Default?.Show(SaveOrLoad.Load);
        }
        protected virtual void OnClickNewGame(UControl arg1, PointerEventData arg2)
        {
            //MainUIMgr.NationSelectionView.Show();
        }
        protected virtual void OnClickCredits(UControl arg1, PointerEventData arg2)
        {

        }
        protected virtual void OnClickTestGame(UControl arg1, PointerEventData arg2)
        {
            //ScreenMgr.RandSelectChara();
            //BattleMgr.StartNewGame(Const.ID_Battle_Simple);
        }
        protected virtual void OnClickWebsite(UControl arg1, PointerEventData arg2)
        {
            //BrowserView.Show(GameConfig.Ins.URL_Website);
        }
        #endregion
    }
}