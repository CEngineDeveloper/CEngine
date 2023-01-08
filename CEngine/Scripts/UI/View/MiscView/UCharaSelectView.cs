//------------------------------------------------------------------------------
// UNationSelecView.cs
// Created by CYM on 2021/11/5
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace CYM.UI
{
    public class UCharaSelectView : UStaticUIView<UCharaSelectView>
    {
        #region inspector
        [SerializeField]
        UButton BntStartGame;
        [SerializeField]
        UButton BntBackToMainMenu;

        [SerializeField]
        UScroll CharaScroll;
        [SerializeField]
        UTextScroll CharaDesc;
        #endregion

        #region prop
        protected TDBaseData CurChara { get; private set; }
        protected IList AllCharas { get; private set; }
        #endregion

        #region life
        protected override string TitleKey => "选择角色";
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            BntStartGame?.Init(new UButtonData { NameKey = "开始游戏", OnClick = OnClickStartGame, HoverClip = "BntHover" });
            BntBackToMainMenu?.Init(new UButtonData { NameKey = "主菜单", OnClick = OnClickBackToMainMenu, HoverClip = "BntHover" });
            CharaScroll?.Init(GetCharaData, OnCharaRefresh, OnCharaItemClick);
            CharaDesc?.Init(new UTextData { Name = GetCharaDesc });
        }
        protected override void OnOpen(UView baseView, bool useGroup)
        {
            base.OnOpen(baseView, useGroup);
            AllCharas = GetAllCharas();
            if (AllCharas.Count > 0)
            {
                CurChara = AllCharas[0] as TDBaseData;
            }
        }
        #endregion

        #region Callback
        protected virtual void OnClickBackToMainMenu(UControl arg1, PointerEventData arg2)
        {
            Show(false);
        }
        protected virtual void OnClickStartGame(UControl arg1, PointerEventData arg2)
        {

        }
        protected virtual void OnCharaItemClick(UControl pres)
        {
            CurChara = CharaScroll.GetData<TDBaseData>(pres.DataIndex);
            SetDirtyRefresh();
        }
        protected virtual void OnCharaRefresh(object arg1, object arg2)
        {

        }
        protected virtual IList GetAllCharas() => new HashList<TDBaseData>();
        private IList GetCharaData()
        {
            return AllCharas;
        }

        string GetCharaDesc()
        {
            if (CurChara != null)
                return CurChara.GetDesc();
            return null;
        }
        #endregion
    }
}