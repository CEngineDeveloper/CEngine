using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CYM.DLC
{
    public class Bundle
    {
        protected bool _isDone = false;
        public int References { get; private set; }
        public virtual string Error { get; protected set; }
        public virtual float Progress { get { return 1; } }
        public virtual bool IsDone => _isDone;
        public virtual AssetBundle AssetBundle => assetBundle;
        public readonly HashList<Bundle> Dependencies = new HashList<Bundle>();
        //防止Asset bundle产生循环依赖的巧妙设计
        public Bundle Parent { get; set; }
        public string Path { get; protected set; }
        public string Name { get; internal set; }
        AssetBundle assetBundle;
        public virtual bool IsValid()
        {
            if (assetBundle == null)
                return false;
            return true;
        }

        internal Bundle(string url)
        {
            _isDone = false;
            Path = url;
        }

        internal void Load()
        {
            OnLoad();
        }

        internal void Unload(bool all)
        {
            OnUnload(all);
        }

        public void AddDependencies(Bundle subBundle)
        {
            subBundle.Parent = this;
            Dependencies.Add(subBundle);
        }

        public T LoadAsset<T>(string assetName) where T : Object
        {
            if (Error != null)
            {
                return null;
            }
            return AssetBundle.LoadAsset(assetName, typeof(T)) as T;
        }

        public Object LoadAsset(string assetName, System.Type assetType)
        {
            if (Error != null)
            {
                return null;
            }
            return AssetBundle.LoadAsset(assetName, assetType);
        }

        public T[] LoadAllAssets<T>() where T : Object
        {
            if (Error != null)
            {
                return null;
            }
            return AssetBundle.LoadAllAssets<T>();
        }

        public AssetBundleRequest LoadAssetAsync(string assetName, System.Type assetType)
        {
            if (Error != null)
            {
                return null;
            }
            return AssetBundle.LoadAssetAsync(assetName, assetType);
        }

        /// <summary>
        /// 不要手动调用此函数
        /// </summary>
        internal void Retain()
        {
            References++;
        }
        /// <summary>
        /// 不要手动调用此函数
        /// </summary>
        internal void Release()
        {
            if (--References < 0)
            {
                CLog.Error("refCount < 0");
            }
        }

        protected virtual void OnLoad()
        {
            string realPath = Path + SysConst.Extention_AssetBundle;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (File.Exists(realPath))
#endif
            assetBundle = AssetBundle.LoadFromFile(realPath);
            if (assetBundle == null)
            {
                Error = realPath + ",加载AssetBundle失败 => LoadFromFile failed.";
                //CLog.Red(Error);
            }
            _isDone = true;
        }

        protected virtual void OnUnload(bool all)
        {
            if (assetBundle != null)
            {
                assetBundle.Unload(all);
                assetBundle = null;
            }
            _isDone = false;
        }
    }

    public class BundleAsync : Bundle, IEnumerator
    {
        #region IEnumerator implementation

        public bool MoveNext()
        {
            return !IsDone;
        }

        public void Reset()
        {
        }

        public object Current
        {
            get
            {
                return AssetBundle;
            }
        }
        public override bool IsValid()
        {
            if (request == null)
                return false;
            return true;
        }
        #endregion

        public override AssetBundle AssetBundle
        {
            get
            {
                if (Dependencies.Count == 0)
                {
                    return request.assetBundle;
                }

                foreach (var item in Dependencies)
                {
                    if (item.Dependencies.Contains(this))
                        continue;

                    if (item.AssetBundle == null)
                    {
                        return null;
                    }
                }

                return request.assetBundle;
            }
        }

        public override float Progress
        {
            get
            {
                if (Dependencies.Count == 0)
                {
                    return request.progress;
                }

                float value = request.progress;
                foreach (var item in Dependencies)
                {
                    if (item.Dependencies.Contains(this))
                        continue;
                    value += item.Progress;
                }
                return value / (Dependencies.Count + 1);
            }
        }

        public override bool IsDone
        {
            get
            {
                if (request == null)
                    return true;
                if (Dependencies.Count != 0)
                {
                    foreach (var item in Dependencies)
                    {
                        if (item.Error != null)
                        {
                            Error = "Falied to load Dependencies " + item;
                            return true;
                        }
                        if (item.Dependencies.Contains(this))
                            continue;

                        if (!item.IsDone)
                        {
                            return false;
                        }
                    }
                }
                return request.progress>=1 && request.isDone && _isDone;
            }
        }

        AssetBundleCreateRequest request;

        protected override void OnLoad()
        {
            string realPath = Path + SysConst.Extention_AssetBundle;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (File.Exists(realPath))
#endif
            request = AssetBundle.LoadFromFileAsync(realPath);
            if (request == null)
            {
                Error = realPath + ",加载AssetBundle失败 => LoadFromFile Async failed.";
                //CLog.Red(Error);
            }
            _isDone = true;
        }

        protected override void OnUnload(bool all)
        {
            if (request != null)
            {
                if (request.assetBundle != null)
                {
                    request.assetBundle.Unload(all);
                }
                request = null;
            }
            _isDone = false;
        }

        internal BundleAsync(string url) : base(url)
        {

        }
    }
}
