//------------------------------------------------------------------------------
// BaseRenameView.cs
// Copyright 2019 2019/11/16 
// Created by CYM on 2019/11/16
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class URenameView : UStaticUIView<URenameView>
    {
        #region inspector
        [SerializeField]
        UInput InputField;
        [SerializeField]
        UButton BntOK;
        [SerializeField]
        UButton BntCancle;
        #endregion

        #region prop
        TDBaseData CurData;
        #endregion

        #region life
        protected override string TitleKey => "Title_重新设定名称";
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            BntOK.Init(new UButtonData { NameKey = "Bnt_确认", OnClick = OnClickOK });
            BntCancle.Init(new UButtonData { NameKey = "Bnt_取消", OnClick = (x, y) => Close() });
        }
        #endregion

        #region set
        public void Show(TDBaseData data)
        {
            CurData = data;
            Show(true);
            InputField.InputText = data.GetName();
        }
        public void Show(UUIView parent, TDBaseData data)
        {
            CurData = data;
            Show(parent);
            InputField.InputText = data.GetName();
        }
        #endregion

        #region Callback
        private void OnClickOK(UControl arg1, PointerEventData arg2)
        {
            if (InputField.IsHaveText())
            {
                CurData.SetCustomName(InputField.InputText);
                OnRenamed(CurData);
                Close();
            }
        }
        protected virtual void OnRenamed(TDBaseData data)
        {
            BaseGlobal.BattleUIMgr?.SetDirtyRefresh();
            BaseGlobal.BattleUIMgr?.SetDirtyCell();
        }
        #endregion
    }
}