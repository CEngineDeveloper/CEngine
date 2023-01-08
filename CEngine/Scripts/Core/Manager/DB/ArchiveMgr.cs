using System;
using System.Collections.Generic;
using System.IO;
namespace CYM
{
    public class ArchiveMgr<T> : IArchiveMgr where T : DBBaseGame, new()
    {
        #region prop
        public ArchiveFile<T> CurArchive { get; private set; }
        #endregion

        #region private
        //默认刷新一次
        bool _isArchiveListDirty = true;
        string _basePath;
        List<ArchiveFile<T>> _allArchives = new List<ArchiveFile<T>>();
        List<ArchiveFile<T>> _lastArchives = new List<ArchiveFile<T>>();
        #endregion

        #region Callback
        Callback<bool> callback_OnSaveState;
        Callback<bool, DBBaseGame> callback_OnLoadState;
        #endregion

        #region life
        public void Init(string path)
        {
            _basePath = path;
            FileUtil.EnsureDirectory(path);
        }
        #endregion

        #region get
        // 得到存档
        public ArchiveFile<T> GetArchive(string id)
        {
            return _allArchives.Find(ar => ar.Name == id);
        }
        // 得到最后修改的时间
        public DateTime GetLastWriteTime(string name)
        {
            return new FileInfo(Path.Combine(_basePath, name)).LastWriteTime;
        }
        // 获取所有存档
        public List<ArchiveFile<T>> GetAllArchives(bool isRefresh = false)
        {
            _isArchiveListDirty = isRefresh;
            RefreshArchiveList();
            return _allArchives;
        }
        public List<IArchiveFile> GetAllBaseArchives(bool isRefresh = false)
        {
            _isArchiveListDirty = isRefresh;
            RefreshArchiveList();
            List<IArchiveFile> ret = new List<IArchiveFile>();
            foreach (var item in _allArchives)
            {
                ret.Add(item);
            }
            return ret;
        }
        // 得到存档路径
        string GetArchivePath(string name) => Path.Combine(_basePath, name + SysConst.Extention_Save);
        // 得到所有文件
        public string[] GetFiles() => FileUtil.GetFiles(_basePath, "*" + SysConst.Extention_Save, SearchOption.AllDirectories);
        #endregion

        #region set
        // 从运行中的游戏保存
        public ArchiveFile<T> Save(string id, T GameData, bool isAsyn = true,bool isDirtyList=true, bool isHide = false, Callback<bool> callback = null)
        {
            _isArchiveListDirty = isDirtyList;
            callback_OnSaveState = callback;
            CurArchive = new ArchiveFile<T>(id,default ,isHide);
            if (isAsyn)
            {
                CurArchive.SaveAsyn(GameData, GetArchivePath(id), OnSaveState);
            }
            else
            {
                CurArchive.Save(GameData, GetArchivePath(id), OnSaveState);
            }
            return CurArchive;
        }
        // 加载存档
        public ArchiveFile<T> Load(string ID, bool isReadContnet, bool isAsyn = true, Callback<bool, DBBaseGame> callback = null)
        {
            callback_OnLoadState = callback;
            string path = GetArchivePath(ID);
            CurArchive = new ArchiveFile<T>(ID, GetLastWriteTime(path));
            if (isAsyn)
            {
                CurArchive.LoadAsyn(path, isReadContnet, OnLoadState);
            }
            else
            {
                CurArchive.Load(path, isReadContnet, OnLoadState);
            }
            return CurArchive;
        }
        // 刷新存档列表
        public void RefreshArchiveList()
        {
            if (!_isArchiveListDirty)
                return;
            foreach (var item in _allArchives)
            {
                if (item.IsInHolding)
                {
                    CLog.Error("错误:有文件正在被占用,无发刷新");
                    break;
                }
            }
            _lastArchives.Clear();
            _lastArchives.AddRange(_allArchives);
            _allArchives.Clear();
            foreach (var file in GetFiles())
            {
                if (Path.GetExtension(file) == SysConst.Extention_Save)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    DateTime fileTime = GetLastWriteTime(file);
                    ArchiveFile<T> a = null;
                    // 如果以前就存在这个存档的，而且修改时间符合，则使用以前的
                    a = _lastArchives.Find(ac => ac.Name == name && ac.FileTime == fileTime);
                    if (a == null)
                    {
                        a = Load(name, false,false);
                    }
                    else
                    {
                    }
                    _allArchives.Add(a);
                }
            }

            // 按时间排序
            _allArchives.Sort((a1, a2) => -a1.SaveTime.CompareTo(a2.SaveTime));
            _isArchiveListDirty = false;
        }
        // 删除指定存档
        public void DeleteArchives(string ID)
        {
            if (!IsHaveArchive(ID))
            {
                CLog.Error("没有这个存档,错误id=" + ID);
                return;
            }
            else
            {
                File.Delete(GetArchivePath(ID));
            }
            _isArchiveListDirty = true;
        }
        #endregion

        #region is
        // 存档是否可以载入
        public bool IsArchiveValid(string id)
        {
            ArchiveFile<T> a = GetArchive(id);
            return a != null && a.IsLoadble;
        }
        // 是否存在相同的存档
        public bool IsHaveArchive(string ID) => GetArchive(ID) != null;
        public bool IsHaveArchive() => _allArchives.Count > 0;
        #endregion

        #region Callback
        private void OnLoadState(bool b, DBBaseGame data)
        {
            callback_OnLoadState?.Invoke(b, data);
            if (!b)
            {

            }
            else
            {
            }
        }
        void OnSaveState(bool b)
        {
            callback_OnSaveState?.Invoke(b);
            if (!b)
            {

            }
            else
            {

            }
        }
        #endregion
    }

}