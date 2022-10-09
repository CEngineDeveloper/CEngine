//------------------------------------------------------------------------------
// BaseLoadingView.cs
// Copyright 2020 2020/1/16 
// Created by CYM on 2020/1/16
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class ULoadingView : UStaticUIView<ULoadingView>
    {
        #region presenter
        [SerializeField]
        List<Sprite> LoadingSprities;
        [SerializeField]
        UImage RawBG;
        [SerializeField]
        UImage BG;
        [SerializeField]
        UProgress Loading;
        [SerializeField]
        UText LoadEndTip;
        [SerializeField]
        UText Tip;
        [SerializeField]
        UText EADesc;
        [SerializeField]
        UImage Logo;
        #endregion

        #region mgr
        protected IBattleMgr<TDBaseBattleData> BattleMgr => BaseGlobal.BattleMgr;
        protected ILevelMgr<TDBaseLevelData> LevelMgr => BaseGlobal.LevelMgr;
        protected BaseInputMgr InputMgr => BaseGlobal.InputMgr;
        #endregion

        #region prop
        UITweenColor UITweenColor;
        #endregion

        #region life
        protected Sprite GetLogo() => null;
        protected override void OnCreatedView()
        {
            base.OnCreatedView();

            if (BattleMgr != null)
            {
                BattleMgr.Callback_OnLoadingProgress += OnLoadingProgress;
                BattleMgr.Callback_OnRandTip += OnRandTip;
                BattleMgr.Callback_OnUnLoad += OnBattleUnLoad;
                BattleMgr.Callback_OnUnLoaded += OnBattleUnLoaded;
                BattleMgr.Callback_OnLoadStart += OnBattleLoadStart;
                BattleMgr.Callback_OnInPauseLoadingView += OnInPauseLoadingView;
            }
            if (LevelMgr != null)
            {
                LevelMgr.Callback_OnLoadingProgress += OnLoadingProgress;
                LevelMgr.Callback_OnRandTip += OnRandTip;
                LevelMgr.Callback_OnUnLoad += OnLevelUnLoad;
                LevelMgr.Callback_OnUnLoaded += OnLevelUnLoaded;
                LevelMgr.Callback_OnGameStart += OnLevelGameStart;
                LevelMgr.Callback_OnLoadStart += OnLevelLoadStart;
            }
            if (InputMgr != null)
            {
                InputMgr.Callback_OnAnyKeyDown += OnAnyKeyDown;
            }
            BG?.Init(new UImageData { OnClick = OnClickBG });
            if(LoadEndTip!=null)
                UITweenColor = LoadEndTip.GetComponent<UITweenColor>();

        }

        public override void DoDestroy()
        {
            if (BattleMgr != null)
            {
                BattleMgr.Callback_OnLoadingProgress -= OnLoadingProgress;
                BattleMgr.Callback_OnRandTip -= OnRandTip;
                BattleMgr.Callback_OnUnLoad -= OnBattleUnLoad;
                BattleMgr.Callback_OnUnLoaded -= OnBattleUnLoaded;
                BattleMgr.Callback_OnLoadStart -= OnBattleLoadStart;
                BattleMgr.Callback_OnInPauseLoadingView -= OnInPauseLoadingView;
            }
            if (LevelMgr != null)
            {
                LevelMgr.Callback_OnLoadingProgress -= OnLoadingProgress;
                LevelMgr.Callback_OnRandTip -= OnRandTip;
                LevelMgr.Callback_OnUnLoad -= OnLevelUnLoad;
                LevelMgr.Callback_OnUnLoaded -= OnLevelUnLoaded;
                LevelMgr.Callback_OnGameStart -= OnLevelGameStart;
                LevelMgr.Callback_OnLoadStart -= OnLevelLoadStart;
            }
            if (InputMgr != null)
            {
                InputMgr.Callback_OnAnyKeyDown -= OnAnyKeyDown;
            }
            base.DoDestroy();
        }

        public override void Show(bool b = true, bool useGroup = true, bool force = false)
        {
            base.Show(b, useGroup, force);
            LoadEndTip?.Show(false);
            if (b)
            {
                //LoadEndTip?.Show(false);
                if (Loading)
                {
                    Loading.Show(true);
                    Loading.IValue.supportRichText = true;
                    Loading.Refresh(0.0f, Util.GetStr("开始加载"));
                }
                if (Logo)
                {
                    Logo.Refresh(GetLogo());
                }
                OnRandTip();
                OnRandBG();
            }
        }
        #endregion

        #region Callback
        void OnLoadingProgress(string str, float val)
        {
            Loading?.Refresh(val, string.Format("{0} {1}", str, UIUtil.PerC(val)));
        }
        private void OnRandTip()
        {
            Tip?.Refresh(BaseLangMgr.RandLoadTip());
        }
        private void OnRandBG()
        {
            if (LoadingSprities != null && LoadingSprities.Count > 0)
                RawBG.Refresh(LoadingSprities.Rand());
        }
        private void OnInPauseLoadingView()
        {
            Loading?.Show(false);
            LoadEndTip?.Show(true);
            LoadEndTip?.Refresh(GetStr("游戏加载完成"));
            UITweenColor?.DoTween();
        }
        public override void OnCloseLoadingView()
        {
            base.OnCloseLoadingView();
            Show(false);
        }
        private void OnClickBG(UControl arg1, PointerEventData arg2)
        {
            ManualCloseLopTip();
        }
        private void OnAnyKeyDown()
        {
            ManualCloseLopTip();
        }
        void ManualCloseLopTip()
        {
            if (!BattleMgr.IsInBattle) return;
            if (!BattleMgr.IsLoadBattleEnd) return;
            if (!BattleMgr.IsInPauseLoadingView) return;
            BattleMgr.UnPauseLoadingView();
            Show(false);
        }
        #endregion

        #region Battle load
        void OnBattleLoadStart()
        {
            Show(true);
        }
        void OnBattleUnLoad()
        {
            Show(true);
        }
        void OnBattleUnLoaded()
        {
            Show(false);
        }
        #endregion

        #region Level load
        void OnLevelGameStart()
        {
            Show(false);
        }
        void OnLevelLoadStart()
        {
            Show(true);
        }
        void OnLevelUnLoad()
        {
            Show(true);
        }
        void OnLevelUnLoaded()
        {
            Show(false);
        }
        #endregion

        #region is
        public static bool IsInLoading
        {
            get
            {
                if (Default == null)
                    return false;
                return Default.IsShow;
            }
        }
        #endregion
    }
}