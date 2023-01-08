//------------------------------------------------------------------------------
// UScenarioDlgView.cs
// Created by CYM on 2021/11/9
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.Unit;

namespace CYM.UI
{
    public class UScenarioDlgView : UStaticUIView<UScenarioDlgView>
    {
        #region Inspector
        [SerializeField]
        UImage Illstration;
        [SerializeField]
        UText Desc;
        #endregion

        #region prop
        TDBaseAlertData CurAlertData;
        #endregion

        #region life
        protected override string GetTitle()
        {
            if (CurAlertData == null) return "";
            return GetStr(CurAlertData.TDID);
        }
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            Title.CancleInit();
        }
        #endregion

        #region set
        public void Show(TDBaseAlertData alert)
        {
            CurAlertData = alert;
            Show(true);
            Title.NameText = alert.TitleStr;
            Illstration.IconSprite = alert.GetIllustration();
            Desc.NameText = alert.DetailStr;
            BaseGlobal.AudioMgr?.PlaySFX2D(alert.StartSFX);
        }
        public void Show(BaseUnit player,string key,string illu,string sfx, params object[] ps)
        {
            if (!player.IsPlayer())
                return;
            Show(true);
            Title.NameText = GetStr("Text_Info");
            Illstration.IconSprite = illu.GetIllustration();
            Desc.NameText = GetStr(key, ps);
            BaseGlobal.AudioMgr?.PlaySFX2D(sfx);
        }
        #endregion
    }
}