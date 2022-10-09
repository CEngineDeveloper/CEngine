//**********************************************
// Class Name	: UnitSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using Sirenix.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace CYM
{
    [Serializable]
    // 存档文件头，用于存储少量信息
    public class ArchiveHeader
    {
        // 存档TDID
        public string PlayerID;
        // 用来测试兼容性
        public int Version;
        // 游戏时间
        public int PlayTime;
        public int ContentLength;
        // 游戏日期，ticks
        public long SaveTimeTicks;
        // hash,用来检测文件是否在传输过程中出错或者被意外修改
        public string ContentHash;
        // 内容是否为压缩，格式为GZ
        public bool IsCompressed;
        //是否为隐藏
        public bool IsHide = false;

        public DateTime SaveTime => new DateTime(SaveTimeTicks);

        public ArchiveHeader()
        {
            Version = -1;
            PlayTime = 0;
        }
    }

    // 游戏存档
    public class ArchiveFile<T> : IArchiveFile where T : DBBaseGame
    {
        #region public
        public string Name { get; private set; }
        public DateTime SaveTime => Header.SaveTime;
        public bool IsBroken { get; private set; }
        public ArchiveHeader Header { get; private set; }
        public DateTime FileTime { get; private set; }        // FileTime用于快速发现文件是否没变
        public bool IsLoadble => !IsBroken && IsCompatible;
        public bool IsCompatible => Header.Version == Version.Config.Data;
        public TimeSpan PlayTime => new TimeSpan(0, 0, Header.PlayTime);
        public bool IsInHolding { get; private set; } = false;
        // 当存档载入仅读取文件头时，GameDatas为空
        public T GameDatas { get; private set; }
        public DBBaseGame BaseGameDatas => GameDatas;
        #endregion

        #region prop
        byte[] Content;
        bool isHide = false;
        #endregion

        #region set
        public ArchiveFile(string name,DateTime fileTime = new DateTime(), bool isHide = false)
        {
            this.isHide = isHide;
            Name = name;
            FileTime = fileTime;
            Header = new ArchiveHeader();
        }
        #endregion

        #region save and load
        // 载入存档
        public void Load(string path, bool isReadContent, Callback<bool, DBBaseGame> callback)
        {
            callback?.Invoke(false, null);
            internalLoad(path, isReadContent);
            callback?.Invoke(true, GameDatas);
        }
        public async void LoadAsyn(string path, bool isReadContent, Callback<bool, DBBaseGame> callback)
        {
            callback?.Invoke(false, null);
            await Task.Run(() =>
            {
                internalLoad(path, isReadContent);
            });
            callback?.Invoke(true, GameDatas);
        }

        // 保存存档
        public void Save(T datas, string path, Callback<bool> callback)
        {
            callback?.Invoke(false);
            internalSave(datas, path);
            callback?.Invoke(true);
        }
        public async void SaveAsyn(T datas, string path, Callback<bool> callback)
        {
            callback?.Invoke(false);
            await Task.Run(() =>
            {
                internalSave(datas, path);
            });
            callback?.Invoke(true);
        }
        #endregion

        #region internal
        void internalSave(T datas, string path)
        {
            bool IsHash = GameConfig.Ins.DBCompressed;
            bool IsCompressed = GameConfig.Ins.DBHash;
            if (IsInHolding) return;
            using (Stream stream = new FileStream(path, FileMode.Create))
            {
                IsInHolding = true;
                GameDatas = datas ?? throw new ArgumentNullException("datas");
                //保存文件头
                Header.PlayTime = GameDatas.PlayTime;
                Header.PlayerID = GameDatas.PlayerID;
                Header.Version = Version.Config.Data;
                Header.IsHide = isHide;
                Header.SaveTimeTicks = DateTime.Now.Ticks;
                Header.IsCompressed = IsCompressed;
                if (IsHash) Header.ContentHash = FileUtil.Hash(Content);
                else Header.ContentHash = null;
                //保存内容
                Content = SerializationUtility.SerializeValue(GameDatas, DataFormat.Binary);
                if (IsCompressed) Content = FileUtil.GZCompressToBytes(Content);
                //写入长度
                Header.ContentLength = Content.Length;
                string headerStr = JsonUtility.ToJson(Header);
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(headerStr);
                    writer.Write(Content);
                    writer.Close();
                }
                stream.Close();
                IsInHolding = false;
            }
        }
        void internalLoad(string path, bool isReadContent)
        {
            if (IsInHolding) return;
            using (Stream stream = File.OpenRead(path))
            {
                IsInHolding = true;
                try
                {
                    BinaryReader reader = new BinaryReader(stream);
                    string headerStr = null;
                    //使用try防止无效的存档
                    headerStr = reader.ReadString();
                    if (string.IsNullOrEmpty(headerStr))
                    {
                        IsBroken = true;
                    }
                    else
                    {
                        Header = JsonUtility.FromJson<ArchiveHeader>(headerStr);
                        int contentSize = Header.ContentLength;
                        if (contentSize <= 0)
                        {
                            IsBroken = true;
                        }
                        else
                        {
                            Content = reader.ReadBytes(contentSize);
                            if (!string.IsNullOrEmpty(Header.ContentHash))
                            {
                                // 内容损坏
                                if (Header.ContentHash != FileUtil.Hash(Content))
                                {
                                    IsBroken = true;
                                    return;
                                }
                            }
                            if (isReadContent && IsCompatible && contentSize > 0)
                            {
                                byte[] toBeDeserialized = null;
                                if (Header.IsCompressed)
                                {
                                    toBeDeserialized = FileUtil.GZDecompressToBytes(Content);
                                }
                                else
                                {
                                    toBeDeserialized = Content;
                                }
                                GameDatas = SerializationUtility.DeserializeValue<T>(toBeDeserialized, DataFormat.Binary);
                            }
                        }
                    }
                    reader.Close();
                }
                catch (Exception e)
                {
                    IsBroken = true;
                    IsInHolding = false;
                    CLog.Error("读取存档{0}时出现异常:{1}, 因此认为是损坏的存档。", Name, e.Message);
                }
                IsInHolding = false;
            }
            return;
        }
        #endregion
    }

}