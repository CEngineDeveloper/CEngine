//------------------------------------------------------------------------------
// BaseCursorMgr.cs
// Copyright 2018 2018/11/1 
// Created by CYM on 2018/11/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class BaseCursorMgr : BaseGFlowMgr
    {
        #region prop
        public bool LockCursor { get; set; } = false;
        List<Texture2D> currentCursor;
        Texture2D curCursorTex;
        int curCursorIndex = 0;
        Timer AnimTimer = new Timer(0.05f);
        protected bool SetBackToNormal { get; private set; } = false;
        protected string Key { get; private set; } = "";
        #endregion

        #region mgr
        BaseAudioMgr AudioMgr => BaseGlobal.AudioMgr;
        protected CursorConfig CursorConfig => CursorConfig.Ins;
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            SetNormal();
        }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedFixedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (LockCursor)
                return;
            if (Input.GetMouseButtonDown(0) ||
                Input.GetMouseButtonDown(1))
            {
                if (currentCursor == CursorConfig.Normal ||
                    SetBackToNormal)
                {
                    SetPress();
                    SetBackToNormal = false;
                }
            }
            else if (Input.GetMouseButtonUp(0) ||
                    Input.GetMouseButtonUp(1))
            {
                if (currentCursor == CursorConfig.Press ||
                    SetBackToNormal)
                {
                    SetNormal();
                    SetBackToNormal = false;
                }
            }
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (AnimTimer.CheckOver())
            {
                if (currentCursor != null &&
                    curCursorTex != currentCursor[curCursorIndex])
                {
                    curCursorTex = currentCursor[curCursorIndex];
                    Cursor.SetCursor(currentCursor[curCursorIndex], new Vector2(3, 3), CursorMode.Auto);
                    curCursorIndex++;
                    if (currentCursor.Count <= curCursorIndex)
                        curCursorIndex = 0;
                }
            }
        }
        #endregion

        #region is
        public bool IsIn(string key)
        {
            return Key == key;
        }
        #endregion

        #region set
        public void Hide(bool b)
        {
            Cursor.lockState = b ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !b;
        }
        public void SetCursor(string key, bool isBack = false)
        {
            SetCursor(GetTexs(key), key, isBack);
        }
        protected void SetCursor(List<Texture2D> cursorList, string key, bool isBack = false)
        {
            if (cursorList == null) return;
            if (cursorList.Count == 0) return;
            if (currentCursor == cursorList) return;
            currentCursor = cursorList;
            curCursorIndex = 0;
            SetBackToNormal = isBack;
            Key = key;
            Cursor.SetCursor(currentCursor[0], new Vector2(3, 3), CursorMode.Auto);
        }
        public void SetWait()
        {
            SetCursor(CursorConfig.Wait, "Wait");
        }
        public void SetNormal()
        {
            SetCursor(CursorConfig.Normal, "Normal");
        }
        public void SetPress()
        {
            SetCursor(CursorConfig.Press, "Press");
            if (CursorConfig.PressSound != null && SelfBaseGlobal != null)
                AudioMgr?.PlayUI(CursorConfig.PressSound);
        }
        public void SetUnit()
        {
            SetCursor(CursorConfig.Unit, "Unit");
        }
        public void PlayPressAudio()
        {
            if (CursorConfig.PressSound != null)
                AudioMgr?.PlayUI(CursorConfig.PressSound);
        }
        #endregion

        #region get
        public List<Texture2D> GetTexs(string key)
        {
            return CursorConfig.GetTexs(key);
        }
        #endregion

        #region Sub battle
        protected override void OnSubBattleLoad()
        {
            base.OnSubBattleLoad();
            SetNormal();
        }
        protected override void OnSubBattleUnLoad()
        {
            base.OnSubBattleUnLoad();
            SetNormal();
        }
        #endregion

        #region Callback
        protected override void OnBattleLoad()
        {
            SetWait();
        }
        protected override void OnBattleLoaded()
        {
            SetNormal();
        }
        protected override void OnBattleUnLoad()
        {
            SetWait();
        }
        protected override void OnBattleUnLoaded()
        {
            SetNormal();
        }
        #endregion
    }
}