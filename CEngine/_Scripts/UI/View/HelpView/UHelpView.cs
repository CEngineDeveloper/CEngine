//**********************************************
// Class Name	: HelpView
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine;
namespace CYM.UI
{
    public class BaseHelpView : UUIView
    {
        #region presenter
        [SerializeField]
        public UHelpItem Top;
        [SerializeField]
        public UHelpItem Bot;
        #endregion

        #region life
        public override void OnInit()
        {
            base.OnInit();
        }
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            Top.Init(new UHelpItemData()
            {
                Close = new UButtonData() { OnClick = (x, y) => Top.Show(false) },
            });
            Bot.Init(new UHelpItemData()
            {
                Close = new UButtonData() { OnClick = (x, y) => Bot.Show(false) },
            });
        }
        public override void Refresh()
        {
            base.Refresh();
            //刷新工作
        }
        public override void Show(bool b = true, bool useGroup = true, bool force = false)
        {
            base.Show(b, useGroup, force);
            if (!IsShow)
            {
                Top.Show(false);
                Bot.Show(false);
            }
        }
        #endregion

        #region set
        public void ShowBot(string key, float time = 60, params string[] ps)
        {
            Bot.ShowStr(GetStr(key, ps), time);
        }
        public void CloseBot()
        {
            Bot.Show(false);
        }
        #endregion

        #region Callback

        #endregion

    }

}