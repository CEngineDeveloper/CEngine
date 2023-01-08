using CYM.UI.Particle;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//**********************************************
// Class Name	: CYMPoolManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM.UI
{
    public class BaseUIMgr : BaseGFlowMgr, IUIDirty
    {
        #region prop
        // 主界面
        public Dictionary<ViewGroup, List<UView>> GroupViews { get; private set; } = new Dictionary<ViewGroup, List<UView>>();
        public HashList<UView> MainViews { get; private set; } = new HashList<UView>();
        public HashList<UView> OpenedMainViews { get; private set; } = new HashList<UView>();
        protected int SortOrder = 0;
        // 根界面:画布
        public UView RootView { get; private set; }
        protected virtual string RootViewPrefab => "URootView";
        protected virtual string ViewName => "RootView";
        protected virtual bool IsUseUIParticleSystem => false;
        #endregion

        #region Callback Val
        public static Callback<UControl, PointerEventData> Callback_OnControlClick { get; set; }
        public event Callback Callback_OnCreatedUIViews;
        #endregion

        #region create and destroy
        // 手动调用:在适当的时机创建UI
        protected void DoCreateView()
        {
            OnCreateUIView1();
            OnCreateUIView2();
            Callback_OnCreatedUIViews?.Invoke();
        }
        // 手动调用:销毁UI
        protected void DoDestroyView()
        {
            RootView?.DoDestroy();
            foreach (var item in MainViews) 
                item.DoDestroy();
            AllMainViews.RemoveAll(x=>MainViews.Contains(x));
            MainViews.Clear();
            OpenedMainViews.Clear();
            GroupViews.Clear();
        }
        #endregion

        #region life         
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            SortOrder = NextSortOrder;
            NextSortOrder++;
        }
        protected virtual void OnCreateUIView1()
        {
            RootView = CreateRootView<UView>();
            RootView.GO.name = ViewName;

            T CreateRootView<T>() where T : UView
            {
                T tempUI = CreateUIGO<T>(RootViewPrefab);
                if (tempUI == null)
                    return null;
                tempUI.UIMgr = this;
                RootView = tempUI;
                RootView.ViewLevel = ViewLevel.Root;
                RootView.Canvas.sortingOrder = SortOrder;
                Object.DontDestroyOnLoad(RootView);
                return tempUI;
            }
        }
        protected virtual void OnCreateUIView2()
        {
            UseUIParticleSystem();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (IsDirtyShow) RefreshShow();
            if (IsDirtyData) RefreshData();
            if (IsDirtyRefresh) Refresh();
            if (IsDirtyCell) RefreshCell();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            DoDestroyView();
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            foreach (var item in MainViews)
                item.OnGameStarted1();
        }
        public override void OnCloseLoadingView()
        {
            base.OnCloseLoadingView();
            foreach (var item in MainViews)
                item.OnCloseLoadingView();
        }
        #endregion

        #region 创建
        public T CreateUIGO<T>(string path) where T : MonoBehaviour
        {
            GameObject tempGo;
            GameObject tempPrefab = null;
            //从编辑器加载
            if (path.StartsWith("Editor:"))
            {
#if UNITY_EDITOR
                var subPath = path.Split(":")[1];
                tempPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath(subPath+".prefab", typeof(GameObject)) as GameObject;
#endif
            }
            //从Resources加载
            else if (path.StartsWith("Res:"))
            {
                var subPath = path.Split(":")[0];
                tempPrefab = Resources.Load(subPath + ".prefab") as GameObject;
            }
            //从Bundle加载
            else
            {
                tempPrefab = BaseGlobal.RsUI.Get(path);
            }
            if (tempPrefab == null)
            {
                CLog.Error($"无法加载此UI：{path}");
                return null;
            }
            tempPrefab.SetActive(true);
            tempGo = Object.Instantiate(tempPrefab);

            if (tempGo == null) return null;
            T tempUI = tempGo.GetComponent<T>();
            if (tempUI == null)
            {
                CLog.Error("无法获取组建:" + typeof(T).Name + " Error path=" + path);
                return null;
            }

            return tempUI;
        }
        protected T CreateView<T>() 
            where T : UView
        {
            return CreateView<T>(typeof(T).Name, null);
        }

        /// <summary>
        /// Editor:Assets/Plugins/CEngine/_Example/UTutorialView
        /// Res:Resources目录
        /// Bundle目录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="customName"></param>
        /// <returns></returns>
        protected T CreateView<T>(string path,string customName=null) 
            where T : UView
        {
            if (RootView == null)
            {
                CLog.Error("请先创建RootView");
                return null;
            }
            T tempUI = CreateUIGO<T>(path);
            if (tempUI == null) return null;
            if(!customName.IsInv())
                tempUI.GOName = customName;
            tempUI.Attach(ViewLevel.Root, RootView);
            tempUI.Callback_OnOpen += OnOpenView;
            tempUI.Callback_OnClose += OnCloseView;
            BaseLuaMgr.RegisterInstance(path, tempUI);
            return tempUI;
        }
        #endregion

        #region set
        public void Toggle()
        {
            Show(!IsShow);
        }
        public void Show(bool b = true)
        {
            RootView.Show(b);
            if (b)
            {
                foreach (var item in MainViews)
                {
                    if (item.IsShow)
                        item.SetDirtyAll();
                }
            }
        }
        public void Close() => RootView.Show(false);
        public void BlockRaycaster(bool b) => RootView.GraphicRaycaster.enabled = !b;

        public void CloseAllGroupedView()
        {
            foreach (var item in MainViews)
            {
                if (item.Group == ViewGroup.None)
                    continue;
                item.Close();
            }
        }
        public void AddToMainViews(UView tempUI)
        {
            MainViews.Add(tempUI);
            AllMainViews.Add(tempUI);
        }
        public void AddToGroupViews(UView tempUI)
        {
            if (GroupViews.ContainsKey(tempUI.Group))
                GroupViews[tempUI.Group].Add(tempUI);
            else
                GroupViews.Add(tempUI.Group, new List<UView> { tempUI });
        }
        protected void ChangeToCameraSpace()
        {
            RootView.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            RootView.Canvas.worldCamera = UICameraObj.Obj;
        }
        protected void UseUIParticleSystem()
        {
            if (IsUseUIParticleSystem)
            {
                ChangeToCameraSpace();
                UIParticleCanvas temp = RootView.gameObject.SafeAddComponet<UIParticleCanvas>();
                temp.maskLayer = (int)SysConst.Layer_UI;
            }
        }
        #endregion

        #region get
        public UView OpenedMainView
        {
            get
            {
                if (!IsHaveOpenedMainView)
                    return null;
                return OpenedMainViews[0];
            }
        }
        public int OpenedMainViewCount => OpenedMainViews.Count;
        #endregion

        #region is
        public bool IsShow => RootView.IsShow;
        public bool IsHaveOpenedMainView => OpenedMainViews.Count > 0;
        #endregion

        #region Callback
        void OnOpenView(UView view, bool useGroup)
        {
            //UI互斥,相同UI组只能有一个UI被打开
            if (useGroup && view.Group > 0)
            {
                view.MutexPreviews.Clear();
                for (int i = 0; i < AllMainViews.Count; ++i)
                {
                    var otherView = AllMainViews[i];
                    if (!otherView.IsDragged &&
                        otherView != view &&
                        otherView.Group != ViewGroup.None &&
                        otherView.Group == view.Group &&
                        otherView.ViewLevel == view.ViewLevel &&
                        otherView.IsExclusive && view.IsExclusive &&
                        otherView.IsShow)
                    {
                        if (!view.IsMutextAll && view.RootView != otherView.RootView)
                            continue;
                        view.MutexPreviews.Add(otherView);
                        otherView.Show(false, false);
                    }
                }
            }
            if (view.Group == ViewGroup.Main)
                OpenedMainViews.Add(view);
        }
        void OnCloseView(UView view, bool useGroup)
        {
            if (useGroup && view.Group > 0)
            {
                if (view.IsReturn)
                {
                    foreach (var item in view.MutexPreviews)
                    {
                        if (!view.IsMutextAll && view.RootView != item.RootView)
                            continue;
                        item.Show(true, false);
                    }
                }
            }
            if (view.Group == ViewGroup.Main)
                OpenedMainViews.Remove(view);
        }
        #endregion

        #region dirty
        public bool IsDirtyRefresh { get; protected set; } = false;
        public bool IsDirtyCell { get; protected set; } = false;
        public bool IsDirtyData { get; protected set; } = false;
        public bool IsDirtyShow { get; protected set; } = false;
        public virtual void SetDirtyRefresh()
        {
            IsDirtyRefresh = true;
        }
        public virtual void SetDirtyCell()
        {
            IsDirtyCell = true;
        }
        public virtual void SetDirtyData()
        {
            IsDirtyData = true;
            IsDirtyRefresh = true;
        }
        public virtual void SetDirtyShow()
        {
            IsDirtyShow = true;
            IsDirtyRefresh = true;
        }
        public virtual void SetDirtyAll()
        {
            IsDirtyShow = true;
            IsDirtyData = true;
            IsDirtyRefresh = true;
        }
        public void SetDirtyAll(float delay)
        {
            Util.Invoke(()=> SetDirtyAll(),delay);
        }
        public override void Refresh()
        {
            base.Refresh();
            IsDirtyRefresh = false;
            foreach (var item in MainViews)
            {
                if (item.IsShow)
                    item.Refresh();
            }
        }
        public virtual void RefreshCell()
        {
            IsDirtyCell = false;
            foreach (var item in MainViews)
            {
                if (item.IsShow)
                    item.RefreshCell();
            }
        }
        public virtual void RefreshData()
        {
            IsDirtyData = false;
            foreach (var item in MainViews)
            {
                if (item.IsShow)
                    item.RefreshData();
            }
        }
        public virtual void RefreshShow()
        {
            IsDirtyShow = false;
            foreach (var item in MainViews)
            {
                item.RefreshShow();
            }
        }
        public virtual void RefreshAll()
        {
            IsDirtyRefresh = false;
            IsDirtyData = false;
            IsDirtyShow = false;
            foreach (var item in MainViews)
                item.RefreshAll();
        }
        #endregion

        #region static
        public static int NextSortOrder { get; private set; } = 0;
        static HashList<UView> LastOpenViews { get; set; } = new HashList<UView>();
        static HashList<UControl> LastOpenControls { get; set; } = new HashList<UControl>();
        static HashList<UView> AllMainViews { get; set; } = new HashList<UView>();
        public static event Callback<UView> Callback_OnCloseRecordView;
        public static event Callback<UControl, UView> Callback_OnCloseRecordControl;
        public static bool CloseRecordView()
        {
            if (LastOpenViews.Count == 0)
                return false;
            var temp = LastOpenViews[LastOpenViews.Count - 1];
            if (temp != null)
            {
                temp?.Close();
                Callback_OnCloseRecordView?.Invoke(temp);
                return true;
            }
            return false;
        }
        public static bool CloseRecordControl()
        {
            if (LastOpenControls.Count == 0)
                return false;
            var temp = LastOpenControls[LastOpenControls.Count - 1];
            if (temp.IsTweenCoplete)
            {
                temp.Close();
                Callback_OnCloseRecordControl?.Invoke(temp, temp.PUIView);
                return true;
            }
            return false;
        }
        public static void AddRecordView(UView view) => LastOpenViews.Add(view);
        public static void RemoveRecordView(UView view) => LastOpenViews.Remove(view);
        public static void AddRecordControl(UControl control) => LastOpenControls.Add(control);
        public static void RemoveRecordControl(UControl control) => LastOpenControls.Remove(control);
        #endregion
    }

}