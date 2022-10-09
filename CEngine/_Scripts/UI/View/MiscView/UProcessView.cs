//------------------------------------------------------------------------------
// BaseProcessView.cs
// Copyright 2018 2018/3/22 
// Created by CYM on 2018/3/22
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM.UI
{
    public class UProcessView : UUIView
    {
        [SerializeField]
        UProgress ProgressBar;
        float curAmount = 0.0f;

        #region life
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void Awake()
        {
            base.Awake();
            if (BaseGlobal.DBMgr != null)
            {
                BaseGlobal.DBMgr.Callback_OnSaveState += OnSaveState;
                BaseGlobal.DBMgr.Callback_OnLoadState += OnLoadState;
            }
        }
        public override void OnDestroy()
        {
            if (BaseGlobal.DBMgr != null)
            {
                BaseGlobal.DBMgr.Callback_OnSaveState -= OnSaveState;
                BaseGlobal.DBMgr.Callback_OnLoadState -= OnLoadState;
            }
            base.OnDestroy();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            curAmount += Time.deltaTime;
            ProgressBar.IFill.fillAmount = curAmount;
            if (curAmount >= 1.0f)
            {
                curAmount = 0.0f;
            }
        }
        #endregion

        #region set
        public void Show(string key = null)
        {
            curAmount = 0.0f;
            if (key == null)
            {
                ProgressBar.IValue.text = "Please Wait..";
            }
            else
            {
                ProgressBar.IValue.text = BaseLangMgr.Get(key);
            }
            base.Show(true);
        }
        public void ShowStr(string str)
        {
            curAmount = 0.0f;
            ProgressBar.ValueText = str;
            base.Show(true);
        }
        #endregion

        #region Callback
        private void OnSaveState(bool arg1)
        {
            if (!arg1)
            {
                Show("Process_Saving");
            }
            else
            {
                Close();
            }
        }
        private void OnLoadState(bool arg1, DBBaseGame data)
        {
            if (!arg1)
            {
                Show("Process_Loading");
            }
            else
            {
                Close();
            }
        }
        #endregion
    }
}