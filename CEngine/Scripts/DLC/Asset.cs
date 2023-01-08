using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CYM.DLC
{
    public enum LoadStateType
    {
        None,
        Loading,
        Succ,
        Fail,
    }
    public class Asset : IEnumerator
    {
        #region IEnumerator implementation
        public void Reset() { }
        public bool MoveNext()=> !IsDone;
        public object Current=> null;
        #endregion

        #region prop
        public Bundle Bundle { get; protected set; }
        public int References { get; private set; }
        public string AssetName { get; protected set; }
        public string BundleName { get; protected set; }
        public string AssetFullPath { get; private set; }
        public string RealBundleName { get; private set; }
        public string DlcName { get; private set; }
        public string Mapkey { get; private set; }
        public System.Type AssetType { get; protected set; }
        public virtual bool IsDone { get { return true; } }
        public virtual float Progress { get { return 1; } }
        public Object Object { get; protected set; }
        #endregion

        #region life
        internal Asset(string bundleName, string assetName, System.Type type)
        {
            this.AssetName = assetName;
            this.BundleName = bundleName;
            this.AssetFullPath = DLCManager.GetAssetPath(bundleName, assetName);
            this.Mapkey = bundleName + assetName;
            this.RealBundleName = DLCManager.GetRealBundleName(AssetFullPath);
            this.DlcName = DLCManager.GetDLCName(bundleName, assetName);
            AssetType = type;
        }

        internal void Load()
        {
            OnLoad();
        }

        internal void Unload(bool all)
        {
            if (Object != null)
            {
                if (Object.GetType() != typeof(GameObject))
                {
                    Resources.UnloadAsset(Object);
                }
                //Object = null;
            }
            OnUnload(all);
        }
        // 不要手动调用此函数
		internal void Retain()
        {
            References++;
        }
        // 不要手动调用此函数
        internal void Release()
        {
            if (--References < 0)
            {
                CLog.Error("refCount < 0");
            }
        }
        #endregion

        #region is
        public bool IsDispose()
        {
            return References <= 0;
        }
        #endregion

        #region Callback
        protected virtual void OnLoad() { }
        public virtual void OnUpdate() { }
        protected virtual void OnUnload(bool all) { }
        #endregion
    }

    public class BundleAsset : Asset
    {
        internal BundleAsset(string bundleName, string assetName, System.Type type) : base(bundleName, assetName, type)
        {
        }

        protected override void OnLoad()
        {
#if UNITY_EDITOR
            if (BuildConfig.Ins.IsEditorMode)
            {
                Object = UnityEditor.AssetDatabase.LoadAssetAtPath(AssetFullPath, AssetType);
                return;
            }
#endif
            {
                Bundle = DLCManager.LoadBundle(RealBundleName, DlcName);
                if (Bundle == null)
                {
                    if (DLCManager.IsNextLogError)
                        CLog.Error("没有这个Bundle,错误的realBundleName:{0} dlcName:{1}", RealBundleName, DlcName);
                    return;
                }
                Object = Bundle.LoadAsset(AssetName, AssetType);
            }

        }


        protected override void OnUnload(bool all)
        {
            if (Bundle != null)
                DLCManager.UnloadBundle(Bundle,all);
        }
    }

    public class BundleAssetAsync : BundleAsset
    {
        protected AssetBundleRequest abRequest;
        protected int loadCount = 0;

        internal BundleAssetAsync(string bundleName, string assetName, System.Type type) : base(bundleName, assetName, type)
        {

        }

        protected override void OnLoad()
        {
#if UNITY_EDITOR
            if (BuildConfig.Ins.IsEditorMode)
            {
                Object = UnityEditor.AssetDatabase.LoadAssetAtPath(AssetFullPath, AssetType);
                return;
            }
#endif
            {
                Bundle = DLCManager.LoadBundle(RealBundleName, DlcName, true);
                loadState = LoadStateType.Loading;
                loadCount = 0;
            }
        }

        protected override void OnUnload(bool all)
        {
            base.OnUnload(all);
            abRequest = null;
        }

        public override void OnUpdate()
        {
            if (IsDone)
                return;

#if UNITY_EDITOR
            if (BuildConfig.Ins.IsEditorMode)
            {

                if (Object == null)
                    loadState = LoadStateType.Fail;
                else
                    loadState = LoadStateType.Succ;
                return;
            }
#endif
            {

                if (Bundle == null || Bundle.Error != null)
                {
                    loadState = LoadStateType.Fail;
                    return;
                }

                if (Bundle.IsDone && loadCount == 0)
                {
                    abRequest = Bundle.LoadAssetAsync(Path.GetFileName(AssetFullPath), AssetType);
                    loadCount++;
                    if (abRequest == null)
                        loadState = LoadStateType.Fail;
                }
                else if (abRequest != null && abRequest.isDone && loadCount == 1)
                {
                    loadCount++;
                    Object = abRequest.asset;
                    if (Object == null)
                        loadState = LoadStateType.Fail;
                    else
                        loadState = LoadStateType.Succ;
                }
                else
                {
                    loadState = LoadStateType.Loading;
                }

            }
        }

        protected LoadStateType loadState = LoadStateType.None;

        public override bool IsDone
        {
            get
            {
                return loadState == LoadStateType.Fail ||
                    loadState == LoadStateType.Succ;

            }
        }

        public override float Progress
        {
            get
            {

#if UNITY_EDITOR
                if (BuildConfig.Ins.IsEditorMode)
                {

                    if (Object == null)
                        return 0.0f;
                    else
                        return 1.0f;
                }
#endif
                {
                    if (Bundle == null || Bundle.Error != null)
                        return 0.0f;

                    return (abRequest == null ? 0.0f : abRequest.progress + Bundle.Progress) * 0.5f;
                }
            }
        }
    }

    public class BundleAssetScene : BundleAssetAsync
    {
        AsyncOperation asyncOperation;
        internal BundleAssetScene(string bundleName, string assetName, System.Type type) : base(bundleName, assetName, type)
        {

        }


        protected override void OnLoad()
        {
#if UNITY_EDITOR
            if (BuildConfig.Ins.IsEditorMode)
            {
                asyncOperation = EditorSceneManager.LoadSceneAsyncInPlayMode(AssetFullPath, new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None));
                loadState = LoadStateType.Loading;
                loadCount = 0;
                return;
            }
#endif
            {
                Bundle = DLCManager.LoadBundle(RealBundleName, DlcName, true);
                if (Bundle == null)
                {
                    CLog.Error($"无法加载场景Bundle！！{RealBundleName}:{DlcName}");
                }
                else
                { 
                
                }
                loadState = LoadStateType.Loading;
                loadCount = 0;
            }
        }

        protected override void OnUnload(bool all)
        {
            base.OnUnload(all);
            asyncOperation = null;
        }

        public override void OnUpdate()
        {
            if (IsDone)
                return;

#if UNITY_EDITOR
            if (BuildConfig.Ins.IsEditorMode)
            {

                if (asyncOperation == null)
                    loadState = LoadStateType.Fail;
                else if (asyncOperation.isDone)
                    loadState = LoadStateType.Succ;
                else
                    loadState = LoadStateType.Loading;
                return;
            }
#endif
            {

                if (Bundle.Error != null)
                {
                    CLog.Error($"无法加载场景Bundle！！{RealBundleName}:{DlcName}:{Bundle.Error}");
                }
                if (Bundle == null || Bundle.Error != null)
                {                    
                    loadState = LoadStateType.Fail;
                    return;
                }

                if (Bundle.IsDone && loadCount == 0)
                {
                    asyncOperation = SceneManager.LoadSceneAsync(AssetName, LoadSceneMode.Additive);//Path.GetFileNameWithoutExtension(AssetFullPath)
                    loadCount++;
                    if (asyncOperation == null)
                        loadState = LoadStateType.Fail;
                }
                else if (asyncOperation != null && asyncOperation.isDone && loadCount == 1)
                {
                    loadCount++;
                    loadState = LoadStateType.Succ;
                }
                else
                {
                    loadState = LoadStateType.Loading;
                }

            }
        }

        public override float Progress
        {
            get
            {
#if UNITY_EDITOR
                if (BuildConfig.Ins.IsEditorMode)
                {
                    if (asyncOperation == null)
                        return 0.0f;
                    return asyncOperation.progress;
                }
#endif
                {
                    if (Bundle == null)
                        return 0.0f;
                    return (asyncOperation == null ? 0.0f : asyncOperation.progress + Bundle.Progress) * 0.5f;
                }

            }
        }
    }
}
