
using System;
using System.Collections.Generic;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM.Plot
{
    [Serializable]
    public class TDBasePlotData : TDBaseData
    {
        #region prop
        protected int CurPlotIndex => BaseGlobal.PlotMgr.CurPlotIndex;
        protected IPlotMgr PlotMgr => BaseGlobal.PlotMgr;
        Corouter BattleCoroutineter => BaseGlobal.BattleCorouter;
        Timer updateTimer = new Timer(1.0f);
        #endregion

        #region set
        protected void StartPlot(string tdid)
        {
            PlotMgr.Start(tdid);
        }
        protected int PushPlotIndex(int? index=null)
        {
            return PlotMgr.PushIndex(index);
        }
        protected CoroutineHandle Run(IEnumerator<float> coroutine)
        {
            return BattleCoroutineter.Run(coroutine);
        }
        #endregion

        #region life
        //自动开始剧情流程
        public virtual bool AutoStarPlot => true;
        //设置剧情暂停标志
        protected virtual bool PlotPause => true;
        //自定义剧情更新频率
        protected virtual float UpdateTime => 1.0f;
        //检查条件，如果条件满足，直接忽略次剧情，直接跳转到下一个剧情
        public virtual string CheckForNext() => SysConst.STR_Inv;
        public override void OnBeAdded(BaseCoreMono selfMono, params object[] obj)
        {
            base.OnBeAdded(selfMono, obj);
            if (PlotMgr != null && PlotPause)
            {
                PlotMgr.SetPlotPause(true);
            }
            updateTimer = new Timer(UpdateTime);
        }
        public override void OnBeRemoved()
        {
            if (PlotMgr != null && PlotPause)
            {
                PlotMgr.SetPlotPause(false);
            }
            base.OnBeRemoved();
        }
        public override void ManualUpdate()
        {
            base.ManualUpdate();
            if (updateTimer.CheckOver())
            {
                UpdateTimer();
            }
        }
        protected virtual void UpdateTimer()
        { 
        
        }
        public virtual IEnumerator<float> OnPlotStart()
        {
            yield break;
        }
        public virtual IEnumerator<float> OnPlotRun()
        {
            yield break;
        }
        public virtual void OnPlotEnd()
        {

        }
        public virtual IEnumerator<float> CustomStartBattleFlow()
        {
            yield break;
        }
        protected void RunMain()
        {
            PlotMgr?.RunMain();
        }
        #endregion
    }
}