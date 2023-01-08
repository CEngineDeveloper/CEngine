//------------------------------------------------------------------------------
// UBattleMenuView.cs
// Created by CYM on 2021/9/13
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.EventSystems;
namespace CYM.UI
{
    public class UBattleMenuView : UStaticUIView<UBattleMenuView>
    {
        #region inspector
        [SerializeField]
        UDupplicate DPMenuBnt;
        [SerializeField]
        UButton BntBack;
        #endregion

        #region life
        protected override string TitleKey => "选项菜单";
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            DPMenuBnt?.Init(
                new UButtonData { NameKey = "保存游戏", OnClick = OnClickBntSaveGame },
                new UButtonData { NameKey = "加载游戏", OnClick = OnClickBntLoadGame },
                new UButtonData { NameKey = "游戏设置", OnClick = OnClickBntSettings },
                new UButtonData { NameKey = "返回主菜单", OnClick = OnClickBntMainMenu },
                new UButtonData { NameKey = "退出到桌面", OnClick = OnClickExitToDesktop }
                );
            BntBack?.Init(new UButtonData { NameKey = "返回游戏", OnClick = (x, y) => Show(false) });
        }
        #endregion

        #region Callback
        protected virtual void OnClickBntLoadGame(UControl arg1, PointerEventData arg2)
        {
            USaveOrLoadView.Default?.Show(SaveOrLoad.Load);
        }
        protected virtual void OnClickBntSaveGame(UControl arg1, PointerEventData arg2)
        {
            USaveOrLoadView.Default?.Show(SaveOrLoad.Save);
        }
        protected virtual void OnClickBntMainMenu(UControl arg1, PointerEventData arg2)
        {
            BaseGlobal.BattleMgr?.GoToStart();
        }
        protected virtual void OnClickBntSettings(UControl arg1, PointerEventData arg2)
        {
            USettingsView.Default?.Show();
        }
        protected virtual void OnClickExitToDesktop(UControl arg1, PointerEventData arg2)
        {
            BaseGlobal.Quit();
        }
        #endregion
    }
}