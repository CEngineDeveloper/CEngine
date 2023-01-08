using System.Collections.Generic;
//**********************************************
// Class Name	: CYMBaseScreenController
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public class BaseScreenMgr<TUnit, TData,TDrama> : BaseGFlowMgr, IScreenMgr<TUnit>
        where TUnit : BaseUnit 
        where TData : TDBaseData
        where TDrama: TDBaseDramaData
    {
        #region Callback val
        /// <summary>
        /// 设置玩家的时候
        /// T1:oldPlayer
        /// T2:newPlayer
        /// </summary>
        public event Callback<TUnit, TUnit> Callback_OnSetPlayer;
        // 本地玩家死亡
        public event Callback Callback_OnPlayerRealDeath;
        #endregion

        #region property
        protected ITDConfig ITDConfig { get; private set; }
        protected IDBMgr<DBBaseGame> DBMgr => BaseGlobal.DBMgr;
        // 当前的玩家
        public TUnit Player { get; private set; }
        // 老玩家
        public TUnit PrePlayer { get; private set; }
        public TUnit TempPlayer { get; private set; }
        #endregion

        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
        }
        #endregion

        #region Select chara
        // 选择的ID
        public string SelectedCharaTDID { get; protected set; }
        public TData SelectedCharaData { get; private set; }
        public virtual void SelectChara(TDBaseData data)
        {
            SelectChara(data as TData);
        }
        // 选择chara
        public virtual void SelectChara(TData data)
        {
            SelectedCharaData = data;
            SelectedCharaTDID = SelectedCharaData.TDID;
        }
        // 选择chara 通过 tdid
        public void SelectChara(string tdid)
        {
            SelectedCharaTDID = tdid;
            SelectedCharaData = ITDConfig.Get<TData>(tdid) ;
        }
        // 随机选择
        public void RandSelectChara(List<string> tdids)
        {
            SelectChara(tdids.Rand());
        }
        #endregion

        #region Select drama
        public string SelectedDrama { get; protected set; }
        public TDrama SelectedDramaData { get; private set; }
        public void SelectDrama(string tdid)
        {
            SelectedDrama = tdid;
            SelectedDramaData = ITDConfig.Get<TDrama>(tdid);
        }
        #endregion

        #region set
        // 设置玩家
        // 默认会在OnBattleLoaded设置Player 
        public virtual void SetPlayer(TUnit unit, bool isSystem = false)
        {
            Player?.OnUnBeSetPlayer();
            PrePlayer = Player;
            Player = unit;
            Callback_OnSetPlayer?.Invoke(PrePlayer, Player);

            if (PrePlayer != null)
                PrePlayer.Callback_OnRealDeath -= OnPlayerRealDeath;
            if (Player != null)
                Player.Callback_OnRealDeath += OnPlayerRealDeath;

            Player?.OnBeSetPlayer();
            OnSetPlayer();
        }
        public void SetCurInputSelectPlayer()
        {
            SetPlayer(BaseInputMgr.SelectedUnit.BaseOwner as TUnit);
        }
        public void SetCurSelectPlayer()
        {
            if (BaseGlobal.PlayerSpawnMgr == null)
            {
                CLog.Error($"错误！没有设置{nameof(BaseGlobal.PlayerSpawnMgr)}");
                return;
            }
            var player = BaseGlobal.PlayerSpawnMgr.GetUnit(SelectedCharaTDID);
            if (player == null)
            {
                CLog.Error($"错误！没有设置Player:{SelectedCharaTDID}");
                return;
            }
            SetPlayer(player as TUnit,true);
        }
        #endregion

        #region get
        public virtual TUnit GetUnit(string id) => BaseGlobal.GetUnit<TUnit>(id);
        public virtual TUnit GetUnit(long id) => BaseGlobal.GetUnit<TUnit>(id);
        #endregion

        #region is
        public bool IsPlayer(TUnit target)=> Player == target;
        #endregion

        #region Callback

        protected virtual void OnPlayerRealDeath()
        {
            Callback_OnPlayerRealDeath?.Invoke();
        }
        protected override void OnAllLoadEnd1()
        {
            //创建一个临时的Player对象
            if (TempPlayer == null)
            {
                TempPlayer = Util.CreateGlobalObj<TUnit>("TempLocalPlayer");
                TempPlayer.BaseConfig.Name = "TempLocalPlayer";
                TempPlayer.IsSystem = true;
            }
            //设置默认的Player
            if (Player == null)
            {
                Player = TempPlayer;
            }
        }
        protected override void OnBattleUnLoaded()
        {
            base.OnBattleUnLoaded();
            //清空数据
            SelectedCharaData = null;
            SelectedCharaTDID = "";
            SelectedDramaData = null;
            SelectedDrama = "";
        }
        protected virtual void OnSetPlayer()
        { 
        
        }
        #endregion

        #region tempPlayer Callback
        public override void OnGameStart1()
        {
            base.OnGameStart1();
        }
        public override void OnGameStartOver()
        {
            base.OnGameStartOver();
            var player = BaseGlobal.ScreenMgr.Player;
            if (player != null)
            {
                CLog.Log($"Player:{player.TDID},{player.ID}");
            }
            else
            {
                CLog.Error($"错误！！没有设置Player，请通过ScreenMgr选择一个Player");
            }
        }
        #endregion
    }
}
