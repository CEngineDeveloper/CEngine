//------------------------------------------------------------------------------
// UDialog.cs
// Copyright 2022 2022/5/13 
// Created by CYM on 2022/5/13
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using CYM.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
namespace CYM.UI
{
    public class UDialog : UContainer
    {
        [FoldoutGroup("FullScreen"), SerializeField]// UI界面是否为全屏
        protected bool IsFullScreen = false;
        [FoldoutGroup("FullScreen"), SerializeField, HideIf("@IsFullScreen == false")]// UI界面是否自动添加UIBlocker
        protected bool IsAddBlocker = true;
        [FoldoutGroup("FullScreen"), SerializeField, HideIf("@IsAddBlocker == false || IsFullScreen == false")]// UI界面是否自动添加UIBlocker
        protected bool IsBlockerClose = false;
        [FoldoutGroup("FullScreen"), SerializeField, HideIf("@IsAddBlocker == false || IsFullScreen == false")]// UI界面是否自动添加UIBlocker
        protected Color BlockerCol = new Color(0, 0, 0, 0.5f);

        #region Inspector
        [SerializeField]
        UButton Bnt1;
        [SerializeField]
        UButton Bnt2;
        [SerializeField]
        UButton Bnt3;
        #endregion

        #region Callback
        event Callback Callback_Bnt1;
        event Callback Callback_Bnt2;
        event Callback Callback_Bnt3;
        event Callback Callback_CustomRefresh;
        #endregion

        #region Prop
        string InputBntStr1 = "";
        string InputBntStr2 = "";
        string InputBntStr3 = "";
        #endregion

        #region life
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            Bnt1?.Init(new UButtonData() { OnClick = OnClickBnt1, Name = () => InputBntStr1, IsTrans = false });
            Bnt2?.Init(new UButtonData() { OnClick = OnClickBnt2, Name = () => InputBntStr2, IsTrans = false });
            Bnt3?.Init(new UButtonData() { OnClick = OnClickBnt3, Name = () => InputBntStr3, IsTrans = false });

            UIUtil.CreateFullscreenBG(Trans, IsFullScreen, IsAddBlocker, IsBlockerClose, BlockerCol, () => Close());
        }
        public UDialog Show(Callback onRefresh = null)
        {
            return ShowStr(null, null, null, null, null, null, onRefresh);
        }
        public UDialog ShowOK(Callback onRefresh = null, Callback BntOK = null)
        {
            return ShowStr(GetStr("Bnt_确认"), BntOK, null, null, null, null, onRefresh);
        }
        public UDialog ShowStr(string BntStr1 = "None", Callback Bnt1 = null, string BntStr2 = "None", Callback Bnt2 = null, string BntStr3 = "None", Callback Bnt3 = null, Callback onRefresh=null)
        {
            if (IsShow)
            {
                CLog.Error("ModalBox不能重复打开!!");
                return this;
            }
            Callback_CustomRefresh = onRefresh;
            InputBntStr3 = BntStr1;
            InputBntStr2 = BntStr2;
            InputBntStr1 = BntStr3;
            Callback_Bnt1 = Bnt1;
            Callback_Bnt2 = Bnt2;
            Callback_Bnt3 = Bnt3;
            Show(true);
            this.Bnt1?.Show(!BntStr1.IsInv());
            this.Bnt2?.Show(!BntStr2.IsInv());
            this.Bnt3?.Show(!BntStr3.IsInv());
            return this;
        }
        public override void Refresh()
        {
            base.Refresh();
            Callback_CustomRefresh?.Invoke();
        }
        #endregion


        #region Callback
        void OnClickBnt1(UControl control, PointerEventData data)
        {
            Callback_Bnt1?.Invoke();
            Show(false);
            PUIView?.SetDirtyAll();
        }
        void OnClickBnt2(UControl control, PointerEventData data)
        {
            Callback_Bnt2?.Invoke();
            Show(false);
            PUIView?.SetDirtyAll();
        }
        void OnClickBnt3(UControl control, PointerEventData data)
        {
            Callback_Bnt3?.Invoke();
            Show(false);
            PUIView?.SetDirtyAll();
        }
        #endregion
    }
}