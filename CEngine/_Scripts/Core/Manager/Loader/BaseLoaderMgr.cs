//**********************************************
// Class Name	: LoaderManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
namespace CYM
{
    public class BaseLoaderMgr : BaseGFlowMgr
    {
        #region Callback Val
        /// <summary>
        /// 当一个Loader加载完成
        /// </summary>
        public event Callback<LoadEndType, string> Callback_OnLoadEnd;
        /// <summary>
        /// 加载开始
        /// </summary>
        public event Callback Callback_OnStartLoad;
        /// <summary>
        /// 当所有的loader都加载完成
        /// </summary>
        public event Callback Callback_OnAllLoadEnd1;
        /// <summary>
        /// 当所有的loader都加载完成
        /// </summary>
        public event Callback Callback_OnAllLoadEnd2;
        #endregion

        #region member variable
        readonly List<ILoader> loderList = new List<ILoader>();
        private string LoadInfo;
        public bool IsLoadEnd { get; private set; } = false;
        public string ExtraLoadInfo { get; set; }
        Timer GUITimer = new Timer(0.2f);
        Texture2D ProgressFill => UIConfig.Ins.ProgressFill;
        Texture2D ProgressBG => UIConfig.Ins.ProgressBG;
        #endregion

        #region property
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        string[] dot = new string[] { ".", "..", "...", "....", ".....", "......" };
        int dotIndex = 0;
        public float Percent { get; set; }
        public ILoader CurLoader { get; private set; }
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedGUI = true;
        }
        public override void OnStart()
        {
            base.OnStart();
            StartLoader(
                BaseGlobal.GRMgr,
                BaseGlobal.LogoMgr,
                BaseGlobal.LangMgr,
                BaseGlobal.LuaMgr,
                BaseGlobal.ExcelMgr,
                BaseGlobal.TextAssetsMgr
                );
        }
        public override void OnGUIPaint()
        {
            if (!IsLoadEnd)
            {
            //    if (GUITimer.CheckOver())
            //    {
            //        dotIndex++;
            //        if (dotIndex >= dot.Length)
            //            dotIndex = 0;
            //    }

            //    if (SelfBaseGlobal != null &&
            //        StartLogo!=null &&
            //        BaseGlobal.LogoMgr != null && 
            //        BaseGlobal.LogoMgr.IsShowedLogo)
            //    {
            //        GUI.DrawTexture(new Rect((Screen.width / 2) - (StartLogo.width / 2), (Screen.height / 2) - (StartLogo.height / 2), StartLogo.width, StartLogo.height), StartLogo);

            //        //显示进度条
            //        if (ProgressBG != null && ProgressFill != null)
            //        {
            //            float width = UIConfig.Ins.ProgressWidth;
            //            float height = UIConfig.Ins.ProgressHeight;
            //            GUI.DrawTexture(new Rect(10, 10, width, height), ProgressBG);
            //            GUI.DrawTexture(new Rect(10, 10, width * Percent, height), ProgressFill);
            //        }
            //    }
            }
        }
        #endregion

        #region utile
        IEnumerator IEnumerator_Load()
        {
            yield return new WaitForEndOfFrame();
            Callback_OnStartLoad?.Invoke();
            for (int i = 0; i < loderList.Count; ++i)
            {
                stopwatch.Start();
                LoadInfo = loderList[i].GetLoadInfo();
                CurLoader = loderList[i];
                CLog.Info(LoadInfo);
                yield return loderList[i].Load();//SelfMono.StartCoroutine(loderList[i].Load());
                Percent = (i / (float)loderList.Count);
                stopwatch.Stop();
                CLog.Yellow($"{LoadInfo} Loading time:{ stopwatch.Elapsed.TotalSeconds}");
            }
            yield return new WaitForEndOfFrame();
            Percent = 1.0f;
            IsLoadEnd = true;
            Callback_OnLoadEnd?.Invoke(LoadEndType.Success, LoadInfo);
            Callback_OnAllLoadEnd1?.Invoke();
            Callback_OnAllLoadEnd2?.Invoke();
            CurLoader = null;
            CLog.Info("All loaded!!");
        }
        void StartLoader(params ILoader[] loaders)
        {
            if (loaders == null || loaders.Length == 0)
            {
                CLog.Error("错误,没有Loader");
                return;
            }
            foreach (var item in loaders)
            {
                loderList.Add(item);
            }
            IsLoadEnd = false;
            SelfMono.StartCoroutine(IEnumerator_Load());
        }
        #endregion
    }

}

