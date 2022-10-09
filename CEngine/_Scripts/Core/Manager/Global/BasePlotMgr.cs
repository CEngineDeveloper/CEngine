using CYM.UI;
using System;
using System.Collections.Generic;
/// <summary>
/// 剧情管理器,比较复杂
/// </summary>
namespace CYM
{
    public class BasePlotMgr<TData> : BaseGFlowMgr, IPlotMgr 
        where TData : TDBasePlotData, new()
    {
        #region Callback value
        public event Callback<bool, bool, int> Callback_OnPlotMode;
        public event Callback<int> Callback_OnChangePlotIndex;
        #endregion

        #region ghost unit
        /// <summary>
        /// 幽灵单位，专门为剧情特殊处理的幽灵单位
        /// 比如：在剧情模式下其他角色都暂停，但是幽灵单位可以自由移动
        /// 比如：在剧情模式下其他角色都暂停动画，但是幽灵动画可以播放动画
        /// </summary>
        public HashList<BaseUnit> GhostUnits { get; private set; } = new HashList<BaseUnit>();
        //单位是否可以被选择
        public HashList<BaseUnit> GhostSelUnits { get; private set; } = new HashList<BaseUnit>();
        //单位是否可以移动
        public HashList<BaseUnit> GhostMoveUnits { get; private set; } = new HashList<BaseUnit>();
        //单位是否可以执行AI逻辑
        public HashList<BaseUnit> GhostAIUnits { get; private set; } = new HashList<BaseUnit>();
        //单位是否可以播放动画
        public HashList<BaseUnit> GhostAnimUnits { get; private set; } = new HashList<BaseUnit>();
        #endregion

        #region Block UI
        HashSet<string> IgnoreBlockClick { get; set; } = new HashSet<string>();
        HashSet<string> IgnoreBlockClickOnce { get; set; } = new HashSet<string>();
        HashSet<UView> IgnoreBlockClickView { get; set; } = new HashSet<UView>();
        public bool IsBlockClick { get; private set; }
        public bool IsBlockSelectUnit { get; private set; }
        #endregion

        #region prop
        /// <summary>
        /// 剧情标记
        /// </summary>
        public HashList<string> TempPlotFlag { get; protected set; } = new HashList<string>();
        public List<CoroutineHandle> TempPlotCoroutines { get; private set; } = new List<CoroutineHandle>();
        public CoroutineHandle MainPlotCoroutine { get; private set; }
        public CoroutineHandle StartPlotCoroutine { get; private set; }
        /// <summary>
        /// 是否禁用AI
        /// </summary>
        public bool IsEnableAI { get; protected set; } = true;
        /// <summary>
        /// 当前的剧情节点
        /// </summary>
        public int CurPlotIndex { get; protected set; } = 0;
        protected Corouter BattleCoroutine => BaseGlobal.BattleCorouter;
        /// <summary>
        /// 是否开启了剧情模式
        /// </summary>
        public BoolState PlotPauseState { get; private set; } = new BoolState();
        public TData CurData { get; private set; }
        ITDConfig ITDConfig;
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            CurData?.ManualUpdate();
        }
        #endregion

        #region is
        public bool IsInPlot(params string[] tdid)
        {
            if (CurData == null)
                return false;
            foreach (var item in tdid)
            {
                if (item == CurData.TDID)
                    return true;
            }
            return false;
        }
        public bool IsHavePlot() => BaseGlobal.DiffMgr.IsHavePlot() && !SysConsole.Ins.IsNoPlot;
        public bool IsInPlotPause() => PlotPauseState.IsIn();
        public bool IsInPlot()=> IsInPlotPause() || CurData!=null || MainPlotCoroutine.IsRunning;
        public bool IsGhostSel(BaseUnit unit) => GhostSelUnits.Contains(unit);
        public bool IsGhostMove(BaseUnit unit) => GhostMoveUnits.Contains(unit);
        public bool IsGhostAI(BaseUnit unit) => GhostAIUnits.Contains(unit);
        public bool IsGhostAnim(BaseUnit unit) => GhostAnimUnits.Contains(unit);
        public bool IsInIgnoreBlockClick(UControl control)
        {
            if (control == null)
                return false;
            return IgnoreBlockClick.Contains(control.GOName) ||
                IgnoreBlockClickOnce.Contains(control.GOName);
        }
        public bool IsInIgnoreBlockClickView(UView view)
        {
            if (view == null)
                return false;
            return IgnoreBlockClickView.Contains(view);
        }
        #endregion

        #region ghost
        public void AddToGhostSelUnits(params BaseUnit[] unit)
        {
            if (unit == null)
                return;
            foreach (var item in unit)
            {
                GhostUnits.Add(item);
                GhostSelUnits.Add(item);
            }
        }
        public void AddToGhostMoveUnits(params BaseUnit[] unit)
        {
            if (unit == null)
                return;
            foreach (var item in unit)
            {
                GhostUnits.Add(item);
                GhostMoveUnits.Add(item);
            }
        }
        public void RemoveFromGhostMoveUnits(params BaseUnit[] unit)
        {
            if (unit == null)
                return;
            foreach (var item in unit)
            {
                GhostUnits.Remove(item);
                GhostMoveUnits.Remove(item);
            }
        }
        public void AddToGhostAIUnits(params BaseUnit[] unit)
        {
            if (unit == null)
                return;
            foreach (var item in unit)
            {
                GhostUnits.Add(item);
                GhostAIUnits.Add(item);
            }
        }
        public void RemoveFromGhostSelUnits(params BaseUnit[] unit)
        {
            if (unit == null)
                return;
            foreach (var item in unit)
            {
                GhostUnits.Remove(item);
                GhostSelUnits.Remove(item);
            }
        }
        public void RemoveFromGhostAIUnits(params BaseUnit[] unit)
        {
            if (unit == null)
                return;
            foreach (var item in unit)
            {
                GhostUnits.Remove(item);
                GhostAIUnits.Remove(item);
            }
        }
        public void AddToGhostAnimUnits(params BaseUnit[] unit)
        {
            if (unit == null)
                return;
            foreach (var item in unit)
            {
                GhostUnits.Add(item);
                GhostAnimUnits.Add(item);
            }
        }
        public void RemoveFromGhostAnimUnits(params BaseUnit[] unit)
        {
            if (unit == null)
                return;
            foreach (var item in unit)
            {
                GhostUnits.Remove(item);
                GhostAnimUnits.Remove(item);
            }
        }
        #endregion

        #region blocker unit
        public void BlockSelectUnit(bool b)
        {
            BaseInputMgr.UnSelectUnit();
            IsBlockSelectUnit = b;
        }
        public bool IsBlockerUnit(BaseUnit unit)
        {
            if (IsBlockSelectUnit)
            {
                if (!IsGhostSel(unit))
                    return true;
            }
            return false;
        }
        #endregion

        #region blocker ui
        public void BlockClick(bool b)
        {
            IsBlockClick = b;
        }
        public void AddIgnoreBlockClickOnce(UControl control)
        {
            if (IgnoreBlockClickOnce.Contains(control.GOName))
                return;
            IgnoreBlockClickOnce.Add(control.GOName);
        }
        public void RemIgnoreBlockClickOnce(UControl control)
        {
            IgnoreBlockClickOnce.Remove(control.GOName);
        }
        public bool IsIgnoreBlockClickOnce(UControl control)
        {
            return IgnoreBlockClickOnce.Contains(control.GOName);
        }
        public void AddIgnoreBlockClick(UControl control)
        {
            if (IgnoreBlockClick.Contains(control.GOName))
                return;
            IgnoreBlockClick.Add(control.GOName);
        }
        public void RemIgnoreBlockClick(UControl control)
        {
            IgnoreBlockClick.Remove(control.GOName);
        }
        #endregion

        #region set
        /// <summary>
        /// 禁用ai
        /// </summary>
        /// <param name="b"></param>
        public virtual void EnableAI(bool b)
        {
            IsEnableAI = b;
        }
        /// <summary>
        /// 启动剧情,自定义
        /// </summary>
        public void SetPlotPause(bool b, int type = 0)
        {
            PlotPauseState.Push(b);
            OnPlotPause(b, PlotPauseState.IsIn(), type);
        }

        /// <summary>
        /// 开始一段剧情
        /// </summary>
        public virtual bool Start(string id,int? index=null)
        {
            if (!IsHavePlot())
                return false;
            if (id.IsInv())
                return false;
            var temp = ITDConfig.Get<TData>(id);
            if (temp == null)
            {
                CLog.Error("无法找到剧情:{0}", id);
                return false;
            }
            else
            {
                CurData?.OnBeRemoved();
                CurData = temp.Copy<TData>();
            }
            CurPlotIndex = 0;
            if (index != null)
                CurPlotIndex = index.Value;
            var next = CurData.CheckForNext();
            if (next.IsInv())
            {
                ClearAllPlotData();
                StopAllTempPlot();
                CurData.OnBeAdded(SelfBaseGlobal);
                RunStart();
                if (CurData.AutoStarPlot)
                {
                    RunMain();
                }
            }
            else
            {
                Start(next);
            }
            return true;
        }
        void RunStart()
        {
            BattleCoroutine.Kill(StartPlotCoroutine);
            StartPlotCoroutine = BattleCoroutine.Run(CurData.OnPlotStart());
        }
        public void RunMain()
        {
            BattleCoroutine.Kill(MainPlotCoroutine);
            MainPlotCoroutine = BattleCoroutine.Run(EnumMain());
        }
        //开始一段附加剧情
        public void RunTemp(IEnumerator<float> enumerator,string flag=null)
        {
            if (flag != null)
            {
                if (TempPlotFlag.Contains(flag))
                    return;
                TempPlotFlag.Add(flag);
            }
            var temp = BattleCoroutine.Run(enumerator);
            TempPlotCoroutines.Add(temp);
        }
        // 推进剧情
        public virtual int PushIndex(int? index=null)
        {
            if (index == null)
                CurPlotIndex++;
            else
            {
                if (index.Value != (CurPlotIndex + 1))
                {
                    CLog.Error("错误！！剧情没有线性推进，PushIndex必须线性推进，否则可以使用SetIndex");
                }
                CurPlotIndex = index.Value;
            }
            Callback_OnChangePlotIndex?.Invoke(CurPlotIndex);
            return CurPlotIndex;
        }
        //直接设置剧情Index
        public void SetIndex(int index)
        {
            CurPlotIndex = index;
            Callback_OnChangePlotIndex?.Invoke(CurPlotIndex);
        }
        //自定义战场片头剧情，适合闯关，和新游戏开始
        public CoroutineHandle CustomStartBattleCoroutine()
        {
            if (CurData == null)
                return BattleCoroutine.Run(CustomStartBattleFlow());
            return BattleCoroutine.Run(CurData.CustomStartBattleFlow());
        }
        protected virtual IEnumerator<float> CustomStartBattleFlow()
        {
            yield break;

        }
        #endregion

        #region stop
        // 停止一段剧情
        public virtual void Stop()
        {
            StopAllTempPlot();
            BattleCoroutine.Kill(StartPlotCoroutine);
            BattleCoroutine.Kill(MainPlotCoroutine);
            if (CurData == null)
                return;
            CurData.OnBeRemoved();
            CurData = null;
        }
        //停止所有的临时剧情
        void StopAllTempPlot()
        {
            TempPlotFlag.Clear();
            foreach (var item in TempPlotCoroutines)
            {
                BattleCoroutine.Kill(item);
            }
            TempPlotCoroutines.Clear();
        }
        //清空剧情相关的数据
        void ClearAllPlotData()
        {
            PlotPauseState.Reset(); ;
            GhostUnits.Clear();
            GhostSelUnits.Clear();
            GhostMoveUnits.Clear();
            GhostAIUnits.Clear();
            GhostAnimUnits.Clear();
            //clear blocker
            IgnoreBlockClick.Clear();
            IgnoreBlockClickOnce.Clear();
            IgnoreBlockClickView.Clear();
            //other flag
            IsBlockSelectUnit = false;
            IsBlockClick = false;
        }
        #endregion

        #region Callback
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            ClearAllPlotData();
            Stop();
        }
        protected override void OnAllLoadEnd1()
        {
        }
        protected virtual void OnPlotPause(bool b, bool curState, int type)
        {
            Callback_OnPlotMode?.Invoke(b, curState, type);
        }
       
        IEnumerator<float> EnumMain()
        {
            if (CurData == null)
                yield break;
            yield return Timing.WaitUntilDone(CurData.OnPlotRun());
            CurData?.OnPlotEnd();
        }
        #endregion
    }

}