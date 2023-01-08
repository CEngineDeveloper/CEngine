/// <summary>
/// 旁白管理器
/// </summary>
namespace CYM.Plot
{
    public class BaseNarrationMgr<TData> : BaseGFlowMgr, INarrationMgr<TData>, IDBConverMgr<DBBaseNarration>
        where TData : TDBaseNarrationData, new()
    {
        #region Callback val
        /// <summary>
        /// 开始一段旁白
        /// </summary>
        public event Callback<TData, NarrationFragment> Callback_OnStart;
        /// <summary>
        /// 下一段旁白
        /// </summary>
        public event Callback<TData, NarrationFragment, int> Callback_OnNext;
        /// <summary>
        /// 结束一段旁白
        /// </summary>
        public event Callback<TData, NarrationFragment> Callback_OnEnd;
        public event Callback<TData> Callback_OnChange;
        Callback<TData> Callback_OnTempEnd;
        #endregion

        #region mgr
        IPlotMgr PlotMgr => BaseGlobal.PlotMgr;
        #endregion

        #region val
        public HashList<string> Showed { get; private set; } = new HashList<string>();
        /// <summary>
        /// 当前的旁白索引
        /// </summary>
        public int CurNarrationIndex { get; private set; }
        /// <summary>
        /// 有旁白?
        /// </summary>
        public bool IsStartNarration { get; private set; } = false;
        /// <summary>
        /// 暂停标记
        /// </summary>
        public bool PauseFlag { get; private set; } = false;
        public TData CurData { get; private set; }
        ITDConfig ITDConfig;
        #endregion

        #region life
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
        }
        #endregion

        #region set
        /// <summary>
        /// 开始一段旁白
        /// isPause=暂停
        /// isUnPauseOnEndTalk=对话结束后取消暂停
        /// </summary>
        /// <param name="id"></param>
        public virtual NarrationFragment Start(string id,Callback<TData> endAction = null)
        {
            Callback_OnTempEnd = endAction;
            CurData = ITDConfig.Get<TData>(id);
            if (CurData == null)
            {
                CLog.Error($"没有找到这个Plot:{id}");
                return null;
            }
            //如果剧情只显示一次,则返回
            if (CurData.IsShowOnce && Showed.Contains(id))
                return null;
            CurNarrationIndex = 0;
            if (IsHave())
            {
                Showed.Add(id);
                var ret = CurData.Fragments[CurNarrationIndex];
                Callback_OnStart?.Invoke(CurData, ret);
                Callback_OnChange?.Invoke(CurData);
                OnChange(CurData, ret);
                IsStartNarration = true;
                if (!PauseFlag)
                {
                    BattleMgr.LockBattleStartFlow(true);
                    PlotMgr?.SetPlotPause(true);
                }
                PauseFlag = true;
                return ret;
            }
            return null;
        }
        /// <summary>
        /// 下一段旁白
        /// </summary>
        public virtual NarrationFragment Next()
        {
            if (!IsStartNarration)
                return null;
            CurNarrationIndex++;
            if (IsHave())
            {
                var ret = CurNarrationFragment();
                Callback_OnNext?.Invoke(CurData, ret, CurNarrationIndex);
                Callback_OnChange?.Invoke(CurData);
                OnChange(CurData, ret);
                return ret;
            }
            else
            {
                Stop();
                return null;
            }
        }
        public virtual void Stop()
        {
            var ret = CurNarrationFragment();
            IsStartNarration = false;
            Callback_OnEnd?.Invoke(CurData, ret);
            Callback_OnChange?.Invoke(CurData);
            OnChange(CurData, ret);
            if (Callback_OnTempEnd != null)
            {
                Callback_OnTempEnd?.Invoke(CurData);
                Callback_OnTempEnd = null;
            }
            if (IsStartNarration)
                return;
            if (PauseFlag)
            {
                BattleMgr.LockBattleStartFlow(false);
                PlotMgr?.SetPlotPause(false);
            }
            //重置状态
            PauseFlag = false;
        }
        /// <summary>
        /// 是否拥有对话
        /// </summary>
        /// <returns></returns>
        public bool IsHave()
        {
            if (CurData == null) return false;
            if (CurData.Fragments == null) return false;
            if (CurNarrationIndex >= CurData.Fragments.Count)
                return false;
            return true;
        }
        #endregion

        #region get
        public NarrationFragment CurNarrationFragment()
        {
            if (!IsHave())
                return new NarrationFragment();
            return CurData.Fragments[CurNarrationIndex];
        }
        #endregion

        #region Callback
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            Showed.Clear();
        }
        protected virtual void OnChange(TData data,NarrationFragment fragment)
        { 
        
        }
        #endregion

        #region db
        public void LoadDBData(ref DBBaseNarration data)
        {
            Showed = data.Showed;
        }

        public void SaveDBData(ref DBBaseNarration data)
        {
            data.Showed = Showed;
        }
        #endregion

    }
}