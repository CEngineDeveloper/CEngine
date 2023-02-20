//**********************************************
// Class Name	: UnitSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CYM
{
    public class BaseDBMgr<TGameData> : BaseGFlowMgr, IDBMgr<TGameData>
        where TGameData : DBBaseGame, new()
    {
        #region 存档
        public ArchiveMgr<TGameData> ArchiveMgr { get; private set; } = new ArchiveMgr<TGameData>();
        #endregion

        #region Callback
        // true：存储完毕，false：开始存储
        public event Callback<bool> Callback_OnSaveState;
        // true：加载完毕，false：开始加载
        public event Callback<bool, DBBaseGame> Callback_OnLoadState;
        // 开始自动存档
        public event Callback Callback_OnStartAutoSave;
        // 结束自动存档
        public event Callback Callback_OnEndAutoSave;
        // 游戏存储
        public event Callback<ArchiveFile<TGameData>> Callback_OnSaveGame;
        public event Callback<DBBaseGame> Callback_OnGenerateNewGameData;
        public event Callback<DBBaseGame> Callback_OnModifyGameData;
        public event Callback<DBBaseGame> Callback_OnReadGameData;
        public event Callback<DBBaseGame> Callback_OnReadGameDataStart;
        public event Callback<DBBaseGame> Callback_OnReadGameDataEnd;
        public event Callback<DBBaseGame> Callback_OnWriteGameData;
        public event Callback<DBBaseGame> Callback_OnRead1;
        public event Callback<DBBaseGame> Callback_OnRead2;
        public event Callback<DBBaseGame> Callback_OnRead3;
        public event Callback<DBBaseGame> Callback_OnReadEnd;
        #endregion

        #region prop
        // 当前游戏存储的数据
        public TGameData CurGameData { get; protected set; } = new TGameData();
        // 是有拥有snapshot
        public bool HasSnapshot { get; protected set; } = false;
        // 玩的时间
        public int PlayTime { get; protected set; } = 0;
        #endregion

        #region flag
        bool _isAutoSaveFlag = false;
        #endregion

        #region 生命周期
        public override void OnEnable()
        {
            base.OnEnable();
            try
            {
                ArchiveMgr.Init(GetCloudArchivePath());
            }
            catch (Exception e)
            {
                if (e != null)
                    CLog.Error(e.ToString());
            }
        }
        public override void OnStart()
        {
            base.OnStart();
            ArchiveMgr.RefreshArchiveList();
        }
        #endregion

        #region is
        //本机上是否有存档
        public bool IsHaveArchives()
        {
            if (ArchiveMgr.IsHaveArchive())
                return true;
            return false;
        }
        // 是否存在当前的存档
        public bool IsHaveSameArchives(string ID)=> ArchiveMgr.IsHaveArchive(ID);
        // 是否有游戏数据
        public bool IsHaveGameData()=> CurGameData != null;
        // 是否可以继续游戏
        public virtual bool IsCanContinueGame()
        {
            string id = Prefers.GetLastAchiveID();
            return ArchiveMgr.IsHaveArchive(id) && ArchiveMgr.IsArchiveValid(id);
        }
        public bool IsHolding
        {
            get
            {
                if (ArchiveMgr == null)
                    return false;
                if (ArchiveMgr.CurArchive == null)
                    return false;
                return ArchiveMgr.CurArchive.IsInHolding;
            }
        }
        #endregion

        #region Get
        // 获得默认的存档名称
        public string GetDefaultSaveName() => string.Format($"{BuildConfig.Ins.Name}{DateTime.Now.Minute}{DateTime.Now.Second}");
        // 获得默认的自动存档名称
        public string GetDefaultAutoSaveName() => SysConst.Prefix_AutoSave + GetDefaultSaveName();
        // 获取所有的存档
        public List<ArchiveFile<TGameData>> GetAllArchives(bool isRefresh = false) => ArchiveMgr.GetAllArchives(isRefresh);
        // 云存档的路径
        public virtual string GetCloudArchivePath() => SysConst.Path_CloudDB;
        // 本地存档路劲
        public virtual string GetLocalArchivePath() => SysConst.Path_LocalDB;
        public IArchiveMgr GetAchieveMgr() => ArchiveMgr;
        public string GetTempSavePath()=> Path.Combine(SysConst.Path_LocalDB, SysConst.STR_DBTempSaveName + SysConst.Extention_Save);
        #endregion

        #region Set
        // 开始新游戏
        public TGameData StartNewGame()
        {
            CurGameData = OnGenerateNewGameData();
            return CurGameData;
        }
        // 删除存档
        public void DeleteArchives(string ID) => ArchiveMgr.DeleteArchives(ID);
        // 快照
        // isSnapshot=true 设置快照标记,否则表示临时快照表示内部使用
        public TGameData Snapshot(bool isSnapshot = true)
        {
            CurGameData = new TGameData();
            WriteGameDBData();
            HasSnapshot = isSnapshot;
            return CurGameData;
        }
        #endregion

        #region Save and Load
        // 另存当前游戏
        // isSetDirty=true 刷新存储文件(会卡) ,否则不刷新,比如自动存储的时候不需要刷新
        // isSnapshot=true 通过最近的一次快照存储游戏
        public void SaveAs(string id, bool useSnapshot, bool isAsyn, bool isDirtyList, bool isHide)
        {
            //保存
            if (id != SysConst.STR_DBTempSaveName)
            {
                Prefers.SetLastAchiveID(id);
            }
            if (useSnapshot)
            {
                //使用最近的一次快照
                if (!HasSnapshot)
                {
                    throw new NotImplementedException("最近一次没有快照,请手动调用Sanpshot");
                }
            }
            else
            {
                //临时快照
                Snapshot(false);
            }
            ArchiveFile<TGameData>  archiveFile = ArchiveMgr.Save(id, CurGameData, isAsyn, isDirtyList, isHide, OnSaveState);
            Callback_OnSaveGame?.Invoke(archiveFile);
        }
        // 自动存储
        public void AutoSave(bool useSnapshot = false, bool isForce = false)
        {
            bool saveType = BaseGlobal.SettingsMgr.Settings.IsAutoSave;
            if (saveType == false && !isForce) return;
            _isAutoSaveFlag = true;
            SaveAs(SysConst.Prefix_AutoSave + Util.GetStr(BaseGlobal.Player.TDID), useSnapshot, true, false, false);
        }
        public void SaveTemp(bool useSnapshot = false, bool isAsyn=false)
        {
            SaveAs(SysConst.STR_DBTempSaveName, useSnapshot, isAsyn, false, true);
        }
        // 加载游戏
        public void Load(string ID, bool isAsyn, Callback<bool, DBBaseGame> callback)
        {
            ArchiveMgr.Load(ID, true, isAsyn, (x, data) =>
            {
                OnLoadState(x, data);
                callback?.Invoke(x, data);
            });
        }
        #endregion

        #region DB 读取和写入函数
        // 统一读取:手动调用
        public void ReadGameDBData()
        {
            var data = CurGameData;
            //读取数据开始
            Callback_OnReadGameDataStart?.Invoke(data);
            OnReadGameDataStart(data);
            //读取数据开始
            Callback_OnReadGameData?.Invoke(data);
            OnReadGameData(data);
            //读取数据1
            Callback_OnRead1?.Invoke(data);
            SelfBaseGlobal.OnRead1(data);
            //读取数据2
            Callback_OnRead2?.Invoke(data);
            SelfBaseGlobal.OnRead2(data);
            //读取数据3
            Callback_OnRead3?.Invoke(data);
            SelfBaseGlobal.OnRead3(data);
            //读取数据End
            Callback_OnReadEnd?.Invoke(data);
            SelfBaseGlobal.OnReadEnd(data);
            //读取数据结束
            Callback_OnReadGameDataEnd?.Invoke(data);
            OnReadGameDataEnd(data);
        }
        // 统一写入:手动调用
        public void WriteGameDBData()
        {
            CurGameData = new TGameData();
            var data = CurGameData;
            OnWriteGameData(ref data);
            SelfBaseGlobal.OnWrite(data);
            Callback_OnWriteGameData?.Invoke(data);
        }
        #endregion

        #region DB 主流程
        // 创建自定义存档
        protected virtual TGameData OnGenerateNewGameData()
        {
            var data = new TGameData();
            data.CurDateTime = new DateTime(1,1,1);
            data.PlayerRTID = SysConst.LONG_Inv;
            data.PlayerTDID = BaseGlobal.ScreenMgr.SelectedCharaTDID;
            data.BattleTDID = BattleMgr.BattleID;
            Callback_OnGenerateNewGameData?.Invoke(data);
            return data;
        }
        protected virtual TGameData OnModifyGameData(TGameData data)
        {
            Callback_OnModifyGameData?.Invoke(data);
            return data;
        }
        // 读取存档
        protected virtual void OnReadGameData(TGameData data)
        {
            PlayTime = data.PlayTime;
            BaseGlobal.ScreenMgr.SelectChara(data.PlayerTDID);
        }
        protected virtual void OnReadGameDataStart(TGameData data) 
        {

        }
        protected virtual void OnReadGameDataEnd(TGameData data) 
        {

        }
        // 写入存档
        protected virtual void OnWriteGameData(ref TGameData data)
        {
            data.PlayTime = PlayTime + (int)Time.realtimeSinceStartup;
            data.PlayerTDID = BaseGlobal.Player.TDID;
            data.PlayerRTID = BaseGlobal.Player.ID;
        }
        private void OnLoadState(bool arg1, DBBaseGame data)
        {
            Callback_OnLoadState?.Invoke(arg1, data);
            if (arg1 == false)
            {

            }
            else
            {
                CurGameData = ArchiveMgr.CurArchive.GameDatas;
                CurGameData = OnModifyGameData(CurGameData);
            }
        }
        private void OnSaveState(bool arg1)
        {
            Callback_OnSaveState?.Invoke(arg1);
            if (arg1 == false)
            {
                if (_isAutoSaveFlag)
                {
                    Callback_OnStartAutoSave?.Invoke();
                }
            }
            else
            {
                if (_isAutoSaveFlag)
                {
                    Callback_OnEndAutoSave?.Invoke();
                    _isAutoSaveFlag = false;
                }
            }
        }
        #endregion
    }

}