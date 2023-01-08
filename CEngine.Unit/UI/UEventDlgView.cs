//------------------------------------------------------------------------------
// UEventDlgView.cs
// Created by CYM on 2021/11/7
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using CYM.Unit;

namespace CYM.UI
{
    public class UEventDlgView : UStaticUIView<UEventDlgView>
    {
        #region Inspector
        [SerializeField]
        UImage Illstration;
        [SerializeField]
        UText Desc;
        [SerializeField]
        UText Count;
        [SerializeField]
        UDupplicate DPOption;
        #endregion

        #region prop
        TDBaseEventData CurEventData;
        IEventMgr<TDBaseEventData> NEventMgr => BaseGlobal.ScreenMgr?.Player?.EventMgr;
        #endregion

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            Title.CancleInit();
            DPOption.Init(
                GetOption,
                (p, d) =>
                {
                    UButton item = p as UButton;
                    EventOption option = d as EventOption;
                    item.NameText = CurEventData?.GetOpName(option);
                    item.Data.OnClick = (x, y) => NEventMgr.SelOption(CurEventData, option);
                    item.Data.OnEnter = (x, y) => ShowTipStr(CurEventData?.GetOpHintStr(option));
                },null);
        }
        protected override void OnOpenDelay(UView baseView, bool useGroup)
        {
            base.OnOpenDelay(baseView, useGroup);
            SetDirtyLayout(Desc.RectTrans);
            SetDirtyLayout(DPOption.LayoutGroup);
        }
        #endregion

        #region set
        public void ShowIfHave()
        {
            if (!NEventMgr.IsHave())
            {
                this.Show(false);
            }
            else
            {
                Show(NEventMgr.First());
            }
            void Show(TDBaseEventData eventData)
            {
                CurEventData = eventData;
                this.Show(true, true, true);
                PlayClip("Event");
                if (Illstration) Illstration.IconSprite = eventData.GetIllstration();
                if (Desc) Desc.NameText = eventData.GetDesc();
                if (Title) Title.NameText = eventData.GetName();
                if (Count) Count.NameText = NEventMgr.Count().ToString();
            }
        }

        #endregion

        #region get
        IList GetOption()
        {
            return CurEventData.Options;
        }
        #endregion

        #region Callback
        protected override void OnSetPlayer(BaseUnit oldPlayer, BaseUnit newPlayer)
        {
            base.OnSetPlayer(oldPlayer,newPlayer);
            if (oldPlayer.EventMgr != null)
            {
                if (oldPlayer != null)
                {
                    oldPlayer.EventMgr.Callback_OnEventAdded -= OnEventAdded;
                    oldPlayer.EventMgr.Callback_OnEventRemoved -= OnEventRemoved;
                }
                if (newPlayer != null)
                {
                    newPlayer.EventMgr.Callback_OnEventAdded += OnEventAdded;
                    newPlayer.EventMgr.Callback_OnEventRemoved += OnEventRemoved;
                }
            }
        }

        private void OnEventRemoved(TDBaseEventData arg1) => ShowIfHave();
        private void OnEventAdded(TDBaseEventData arg1) => ShowIfHave();
        #endregion
    }
}