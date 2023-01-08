//------------------------------------------------------------------------------
// USettingsView.cs
// Created by CYM on 2021/12/2
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using System.Collections.Generic;
namespace CYM.UI
{
    public class USettingsView : UStaticUIView<USettingsView>
    {
        #region Inspector
        [FoldoutGroup("Title"), SerializeField]
        UText GameplayTitle;
        [FoldoutGroup("Title"), SerializeField]
        UText VideoTitle;
        [FoldoutGroup("Title"), SerializeField]
        UText AudioTitle;

        [FoldoutGroup("Gameplay"), SerializeField]
        UDupplicate GameplaySlider;
        [FoldoutGroup("Gameplay"), SerializeField]
        UDupplicate GameplayCheckbox;
        [FoldoutGroup("Gameplay"), SerializeField]
        UDupplicate GameplayDroplist;

        [FoldoutGroup("Graphics"), SerializeField]
        UDupplicate GraphicsSlider;
        [FoldoutGroup("Graphics"), SerializeField]
        UDupplicate GraphicsCheckbox;
        [FoldoutGroup("Graphics"), SerializeField]
        UDupplicate GraphicsDroplist;

        [FoldoutGroup("Audio"), SerializeField]
        UDupplicate AudioSlider;
        [FoldoutGroup("Audio"), SerializeField]
        UDupplicate AudioCheckbox;
        [FoldoutGroup("Audio"), SerializeField]
        UDupplicate AudioDroplist;

        [FoldoutGroup("Button"), SerializeField]
        UButton BntRevert;
        [FoldoutGroup("Button"), SerializeField]
        UButton BntSave;
        #endregion

        #region Life
        protected override string TitleKey => "游戏设置";
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            GameplayTitle?.Init(new UTextData { NameKey = "游戏设置" });
            VideoTitle?.Init(new UTextData { NameKey = "视频设置" });
            AudioTitle?.Init(new UTextData { NameKey = "音频设置" });

            //gameplay slider
            GameplaySlider?.Init(GameplayGetSliderData());

            //gameplay droplist
            List<UDropdownData> gameplayDroplist = new List<UDropdownData>{
                new UDropdownData { NameKey = "多语言", Opts = () => BaseLangMgr.GetAllLanguages(), Value = () => (int)BaseGlobal.LangMgr.CurLangType, OnValueChanged = OnLanguageChanged },
            };
            gameplayDroplist.AddRange(GameplayGetDroplistData());
            GameplayDroplist?.Init(gameplayDroplist.ToArray());

            //gameplay checkbox
            List<UCheckData> gameplayChecklist = new List<UCheckData> {
                new UCheckData { NameKey = "自动存储", IsOn = () => BaseGlobal.Settings.IsAutoSave, OnValueChanged = x => BaseGlobal.Settings.IsAutoSave=x }
            };
            gameplayChecklist.AddRange(GameplayGetCheckboxData());
            GameplayCheckbox?.Init(gameplayChecklist.ToArray());

            //graphics slider
            GraphicsSlider?.Init(GraphicsGetSliderData());

            //graphics droplist
            List<UDropdownData> graphicsDroplist = new List<UDropdownData>{
                new UDropdownData { NameKey = "画面质量", Enum = typeof(GamePropType), Value = () => (int)BaseGlobal.Settings.Quality, OnValueChanged = (x)=>BaseGlobal.SettingsMgr.SetQuality(x)},
                new UDropdownData { NameKey = "窗口样式", Enum = typeof(WindowType), Value = () => (int)BaseGlobal.Settings.WindowType, OnValueChanged = (x)=>BaseGlobal.SettingsMgr.SetWindowType((WindowType)x) },
                new UDropdownData { NameKey = "分辨率", Opts = () => BaseGlobal.SettingsMgr.GetResolutionStrs(), Value = () => BaseGlobal.Settings.Resolution, OnValueChanged = (x)=>BaseGlobal.SettingsMgr.SetResolution(x) }
            };
            graphicsDroplist.AddRange(GraphicsGetDroplistData());
            GraphicsDroplist?.Init(graphicsDroplist.ToArray());

            //graphics checkbox
            GraphicsCheckbox?.Init(GraphicsGetCheckboxData());

            //audio slider
            List<USliderData> audioSliderList = new List<USliderData>
            {
                new USliderData { NameKey = "主音量", Value = () =>BaseGlobal.AudioMgr.GetVolume(), OnValueChanged = (x)=>BaseGlobal.AudioMgr.SetVolume(x) },
                new USliderData { NameKey = "背景音乐", Value = () =>BaseGlobal.AudioMgr.GetVolumeMusic(), OnValueChanged = (x)=>BaseGlobal.AudioMgr.SetVolumeMusic(x) }
            };
            audioSliderList.AddRange(AudioGetSliderData());
            AudioSlider?.Init(audioSliderList.ToArray());

            //audio droplist
            AudioDroplist?.Init(AudioGetDroplistData());

            //audio checklist
            List<UCheckData> audioCheckList = new List<UCheckData> {
                new UCheckData { NameKey = "开启音效", IsOn = () => !BaseGlobal.AudioMgr.IsMuteSFX(), OnValueChanged = (x)=>BaseGlobal.AudioMgr.MuteSFX(!x)},
                new UCheckData { NameKey = "开启音乐", IsOn = () => !BaseGlobal.AudioMgr.IsMuteMusic(), OnValueChanged = (x)=>BaseGlobal.AudioMgr.MuteMusic(!x) }
            };
            audioCheckList.AddRange(AudioGetCheckboxData());
            AudioCheckbox?.Init(audioCheckList.ToArray());

            BntRevert?.Init(new UButtonData { NameKey = "还原", OnClick = OnBntRevert });
            BntSave?.Init(new UButtonData { NameKey = "保存",OnClick = OnBntSave });
        }
        #endregion

        #region Gameplay Get data
        protected virtual UCheckData[] GameplayGetCheckboxData()=> new UCheckData[] { };
        protected virtual UDropdownData[] GameplayGetDroplistData()=> new UDropdownData[] { };
        protected virtual USliderData[] GameplayGetSliderData()=> new USliderData[] { };
        #endregion

        #region Gameplay Get data
        protected virtual UCheckData[] GraphicsGetCheckboxData()=> new UCheckData[] { };
        protected virtual UDropdownData[] GraphicsGetDroplistData()=> new UDropdownData[] { };
        protected virtual USliderData[] GraphicsGetSliderData()=> new USliderData[] { };
        #endregion

        #region Audio Get data
        protected virtual UCheckData[] AudioGetCheckboxData()=> new UCheckData[] { };
        protected virtual UDropdownData[] AudioGetDroplistData()=> new UDropdownData[] { };
        protected virtual USliderData[] AudioGetSliderData()=> new USliderData[] { };
        #endregion

        #region Callback
        protected virtual void OnBntRevert(UControl arg1, PointerEventData arg2)
        {
            ShowOKCancle("Msg_还原设置",
                () =>
                {
                    BaseGlobal.SettingsMgr.Revert();
                },
                null);
        }
        private void OnBntSave(UControl arg1, PointerEventData arg2)
        {
            BaseGlobal.SettingsMgr?.Save();
            Close();
        }
        protected override void OnClickClose(UControl control, PointerEventData data)
        {
            base.OnClickClose(control, data);
            BaseGlobal.SettingsMgr?.Save();
        }
        private void OnLanguageChanged(int arg1)
        {
            LanguageType type = (LanguageType)arg1;
            BaseGlobal.LangMgr.Switch(type);
            Prefers.SetCustomLanguage();
        }
        #endregion
    }
}