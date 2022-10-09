//**********************************************
// Class Name	: SettingsView
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine;
using UnityEngine.EventSystems;
namespace CYM.UI
{
    public class UModalBoxView : UStaticUIView<UModalBoxView>
    {
        [SerializeField]
        UText Desc;
        [SerializeField]
        UButton BntOK;
        [SerializeField]
        UButton BntCancle;
        [SerializeField]
        UButton BntOther;

        event Callback Callback_Bnt1;
        event Callback Callback_Bnt2;
        event Callback Callback_Bnt3;

        string InputTitle = "";
        string InputDesc = "";
        string InputBntStr1 = "";
        string InputBntStr2 = "";
        string InputBntStr3 = "";

        UView LastNeedDirtyView { get; set; }

        #region life
        protected virtual string BntOKKey => "确认";
        protected virtual string BntCancleKey => "取消";
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            if (Title)
            {
                Title.Data.IsTrans = false;
            }
            Desc?.Init(new UTextData() { Name = () => InputDesc, IsTrans = false });
            BntOK?.Init(new UButtonData() { OnClick = OnClickBnt1, Name = () => InputBntStr1, IsTrans = false });
            BntCancle?.Init(new UButtonData() { OnClick = OnClickBnt2, Name = () => InputBntStr2, IsTrans = false });
            BntOther?.Init(new UButtonData() { OnClick = OnClickBnt3, Name = () => InputBntStr3, IsTrans = false });
        }
        #endregion

        #region set
        /// <summary>
        /// 语法糖对话框调用函数
        /// 直接传KEY
        /// </summary>
        /// <param name="key"></param>
        /// <param name="descKey"></param>
        /// <param name="BntOK"></param>
        /// <param name="paras"></param>
        public new void ShowOKTitle(string key, Callback BntOK, params object[] paras)=> ShowStr(GetStr(key), GetStr(SysConst.Prefix_Desc + key, paras), GetStr(BntOKKey), BntOK, null, null, null, null, true);
        /// <summary>
        /// 语法糖对话框调用函数
        /// </summary>
        /// <param name="descKey"></param>
        /// <param name="BntOK"></param>
        /// <param name="paras"></param>
        public new void ShowOK(string descKey, Callback BntOK, params object[] paras)=> ShowStr(null, GetStr(descKey, paras), GetStr(BntOKKey), BntOK, null, null, null, null, true);
        /// <summary>
        /// 语法糖
        /// </summary>
        /// <param name="key"></param>
        /// <param name="descKey"></param>
        /// <param name="BntOK"></param>
        /// <param name="BntCancle"></param>
        /// <param name="paras"></param>
        public new void ShowOKCancleTitle(string key,Callback BntOK, Callback BntCancle = null, params object[] paras)=> ShowStr(GetStr(key), GetStr(SysConst.Prefix_Desc + key, paras), GetStr(BntOKKey), BntOK, GetStr(BntCancleKey), BntCancle, null, null, true);
        public new void ShowOKCancle(string descKey, Callback BntOK, Callback BntCancle = null, params object[] paras)=> ShowStr(null, GetStr(descKey, paras), GetStr(BntOKKey), BntOK, GetStr(BntCancleKey), BntCancle, null, null, true);
        public void Show(string descKey, string BntKey1 = "None", Callback Bnt1 = null, string BntKey2 = "None", Callback Bnt2 = null, string BntKey3 = "None", Callback Bnt3 = null)
        {
            ShowStr(null, GetStr(descKey), GetStr(BntKey1), Bnt1, GetStr(BntKey2), Bnt2, GetStr(BntKey3), Bnt3, true);
        }
        /// <summary>
        /// 原始的模式对话框调用函数
        /// </summary>
        /// <param name="title"></param>
        /// <param name="desc"></param>
        /// <param name="BntStr1"></param>
        /// <param name="Bnt1"></param>
        /// <param name="BntStr2"></param>
        /// <param name="Bnt2"></param>
        /// <param name="BntStr3"></param>
        /// <param name="Bnt3"></param>
        /// <param name="isCanClose"></param>
        public void ShowStr(string title, string desc, string BntStr1 = "None", Callback Bnt1 = null, string BntStr2 = "None", Callback Bnt2 = null, string BntStr3 = "None", Callback Bnt3 = null, bool isCanClose = true)
        {
            if (IsShow)
            {
                CLog.Error("ModalBox不能重复打开!!");
                return;
            }
            InputTitle = title;
            InputDesc = desc;
            InputBntStr1 = BntStr1;
            InputBntStr2 = BntStr2;
            InputBntStr3 = BntStr3;
            Callback_Bnt1 = Bnt1;
            Callback_Bnt2 = Bnt2;
            Callback_Bnt3 = Bnt3;
            Show(true);
            this.BntOK?.Show(!BntStr1.IsInv());
            this.BntCancle?.Show(!BntStr2.IsInv());
            this.BntOther?.Show(!BntStr3.IsInv());
            BntClose?.Show(isCanClose);
            Title?.Show(InputTitle!=null);
            Desc?.Show(InputDesc!=null);
        }
        public void SetNeedDirtyView(UView view)
        {
            LastNeedDirtyView = view;
        }
        #endregion

        #region get
        protected override string GetTitle()
        {
            if(!InputTitle.IsInv())
                return InputTitle;
            return GetStr(TitleKey);
        }
        #endregion

        #region Utile

        #endregion

        #region Callback
        void OnClickBnt1(UControl control, PointerEventData data)
        {
            Callback_Bnt1?.Invoke();
            Show(false);
            LastNeedDirtyView?.SetDirtyAll();
            LastNeedDirtyView = null;
        }
        void OnClickBnt2(UControl control, PointerEventData data)
        {
            Callback_Bnt2?.Invoke();
            Show(false);
            LastNeedDirtyView?.SetDirtyAll();
            LastNeedDirtyView = null;
        }
        void OnClickBnt3(UControl control, PointerEventData data)
        {
            Callback_Bnt3?.Invoke();
            Show(false);
            LastNeedDirtyView?.SetDirtyAll();
            LastNeedDirtyView = null;
        }
        #endregion
    }

}