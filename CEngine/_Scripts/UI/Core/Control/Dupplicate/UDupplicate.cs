using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UDupplicateData:UData
    {
        /// <summary>
        /// obj1=presenter
        /// obj2=用户自定义数据
        /// </summary>
        public Callback<object, object> OnRefreshItem = null;
        /// <summary>
        /// obj1=presenter
        /// obj2=用户自定义数据
        /// </summary>
        public Callback<object, object> OnFixedRefreshItem = null;
        /// <summary>
        /// 获取用户自定义数据 方法
        /// </summary>
        public Func<IList> GetCustomDatas = null;
        /// <summary>
        /// int1 =当前的index
        /// int2 =上次的index
        /// </summary>
        public Callback<int, int> OnSelectChange = null;
        /// <summary>
        /// 当前选得index
        /// </summary>
        public Callback<UControl> OnClickSelected = null;
        /// <summary>
        /// 固定数量
        /// </summary>
        public int FixedCount = 0;
        /// <summary>
        /// 是否自动关闭多余的控件
        /// </summary>
        public bool IsAutoClose = true;
        public int GetInitCount()
        {
            if (FixedCount >= 0)
                return FixedCount;
            if (GetCustomDatas != null)
                return GetCustomDatas.Invoke().Count;
            return 0;
        }
    }
    [AddComponentMenu("UI/Control/UDupplicate")]
    [HideMonoScript]
    public partial class UDupplicate : UPres<UDupplicateData>, ICheckBoxContainer
    {
        #region Inspector
        [FoldoutGroup("CheckBox"), SerializeField, Tooltip("是否会自动选择一个,自动Toggle")]
        bool IsToggleGroup = false;
        [FoldoutGroup("CheckBox"), SerializeField, Tooltip("是否默认选择一个"), HideIf("Inspector_HideAutoSelect")]
        bool IsAutoSelect = true;
        [FoldoutGroup("CheckBox"), SerializeField, Tooltip("是否刷新自生界面")]
        bool IsLinkSelfView = false;

        [FoldoutGroup("Data"), SerializeField]
        bool IsInitOnStart = false;
        [FoldoutGroup("Data"), ShowIf("Inspector_IsInitCount"), SerializeField]
        int Count = 0;
        #endregion

        #region public
        public List<GameObject> GOs { get; private set; } = new List<GameObject>();
        public List<UControl> Controls { get; private set; } = new List<UControl>();

        //子对象数量
        public int GOCount => GOs.Count;
        //当前选择的index
        public int CurSelectIndex { get; protected set; } = SysConst.INT_Inv;
        //上一次的选择
        public int PreSelectIndex { get; protected set; } = SysConst.INT_Inv;
        #endregion

        #region prop
        // 用户自定义数据缓存
        IList CustomDatas = new List<object>();
        // 刷新Layout
        Timer RefreshLayoutTimer = new Timer(0.02f);
        List<UCheck> ToggleGroupCheckBoxs { get; set; } = new List<UCheck>();
        bool IsInitedCount = false;
        GameObject Prefab;
        #endregion

        #region life
        public override bool NeedFixedUpdate => true;
        public override bool IsAutoInit => true;
        public override bool IsCollection => true;
        public UControl this[int index]
        {
            get
            {
                return Controls[index];
            }
        }
        protected override void Awake()
        {
            base.Awake();
            if (IsInitOnStart) _InitCount(Count);
            if (GetIsAutoSelect()) CurSelectIndex = 0;
        }
        public override void OnBeFetched()
        {
            FontType =nameof(CYM.FontType.None);
            base.OnBeFetched();
        }
        protected override void FetchSubControls()
        {
            for (int i = 0; i < GO.transform.childCount; ++i)
            {
                var item = GO.transform.GetChild(i).GetComponent<UControl>();
                if (item == this) continue;
                if (item == null) continue;
                var element = item.GetComponent<LayoutElement>();
                var ignore = item.GetComponent<UIgnore>();
                if (element != null && element.ignoreLayout) continue;
                if (ignore != null) continue;
                item.IsManualFetch = true;
            }
        }
        protected override void Start()
        {
            base.Start();
            if (IsInitOnStart) 
                InitData();
        }
        public override void OnViewShow(bool b)
        {
            base.OnViewShow(b);
            CurSelectIndex = 0;
        }
        public override void OnShow(bool isShow)
        {
            base.OnShow(isShow);
            CurSelectIndex = 0;
        }
        // 刷新
        public override void Refresh()
        {
            if (Data.OnRefreshItem != null)
            {
                IsDirtyRefresh = false;
                CustomDatas = Data.GetCustomDatas?.Invoke();
                //自动选择
                if (GetIsAutoSelect() &&
                    CustomDatas != null)
                {
                    if (CurSelectIndex > CustomDatas.Count)
                        CurSelectIndex = 0;
                    else if (CurSelectIndex < 0 && CustomDatas.Count > 0)
                        CurSelectIndex = 0;
                }

                foreach (var item in Controls)
                {
                    if (item == null)
                        continue;
                    //如果CustomData数量少于presenter数量,则关闭Slot
                    if (
                        Data.IsAutoClose &&
                        CustomDatas != null &&
                        item.Index >= CustomDatas.Count
                        )
                    {
                        item.Close();
                        continue;
                    }
                    item.Show();
                    Data.OnRefreshItem.Invoke(item, GetCustomData(CustomDatas, item.Index));
                    if (GetIsToggleGroup() && item is UCheck checkBox && checkBox.IsToggleGroup)
                        checkBox.RefreshStateAndActiveEffectBySelect();
                }
                RefreshLayoutTimer.Restart();
            }
            else
            {
                base.Refresh();
            }
            Data.OnSelectChange?.Invoke(CurSelectIndex, PreSelectIndex);
        }
        public override void OnFixedUpdate()
        {
            if (!IsShow)
                return;
            if (Data.OnFixedRefreshItem != null)
            {
                foreach (var item in Controls)
                {
                    if (item == null)
                        continue;
                    //如果CustomData数量少于presenter数量,则关闭Slot
                    if (Data.IsAutoClose &&
                        CustomDatas != null &&
                        item.Index >= CustomDatas.Count)
                    {
                        item.Close();
                        continue;
                    }
                    item.Show();
                    Data.OnFixedRefreshItem.Invoke(item, GetCustomData(CustomDatas, item.Index));
                }
            }
            else
            {
                base.OnFixedUpdate();
            }

            if (RefreshLayoutTimer.CheckOverOnce())
            {
                //刷新布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(RectTrans);
            }
        }
        #endregion

        #region init
        public UControl[] Init(Func<IList> getCustomDatas, Callback<object, object> onRefresh, Callback<UControl> onClickSelected = null)
        {
            Data.GetCustomDatas = getCustomDatas;
            Data.OnRefreshItem = onRefresh;
            Data.OnFixedRefreshItem = null;
            Data.OnClickSelected = onClickSelected;
            return InitCountAndData(Data.GetInitCount());
        }
        public UControl[] Init<TData>(TData data, Func<IList> getCustomDatas, Callback<object, object> onRefresh, Callback<object, object> onFixedRefresh=null, Callback<UControl> onClickSelected=null)
            where TData : UData, new()
        {
            Data.GetCustomDatas = getCustomDatas;
            Data.OnRefreshItem = onRefresh;
            Data.OnFixedRefreshItem = onFixedRefresh;
            Data.OnClickSelected = onClickSelected;
            return InitCountAndData(Data.GetInitCount(), data);
        }
        public UControl[] Init(UDupplicateData pdata, Func<IList> getCustomDatas, Callback<object, object> onRefresh, Callback<object, object> onFixedRefresh = null, Callback<UControl> onClickSelected = null)
        {
            ReInit(pdata);
            Data.GetCustomDatas = getCustomDatas;
            Data.OnRefreshItem = onRefresh;
            Data.OnFixedRefreshItem = onFixedRefresh;
            Data.OnClickSelected = onClickSelected;
            return InitCountAndData(Data.GetInitCount());
        }
        public UControl[] Init<TData>(int fixedCount, TData pData = null, Callback<object, object> onRefresh = null, Callback<object, object> onFixedRefresh = null)
            where TData : UData, new()
        {
            Data.FixedCount = fixedCount;
            Data.OnRefreshItem = onRefresh;
            Data.OnFixedRefreshItem = onFixedRefresh;

            return InitCountAndData(Data.GetInitCount(), pData); ;
        }
        public UControl[] Init<TData>(params TData[] data)
            where TData : UData, new()
        {
            if (data == null)
            {
                Show(false);
                return null;
            }
            Data.OnRefreshItem = null;
            Data.OnFixedRefreshItem = null;
            return InitCountAndData(data.Length, data);
        }
        public UControl[] Init<TData>(Callback<object, object> onRefresh /*obj1=presenter obj2=用户自定义数据*/, Callback<object, object> onFixedRefresh, params TData[] data)
            where TData : UData, new()
        {
            if (data == null) return null;
            Data.OnRefreshItem = onRefresh;
            Data.OnFixedRefreshItem = onFixedRefresh;
            return InitCountAndData(data.Length, data);
        }
        public UControl[] Init<TData>(UDupplicateData pdata, TData data=null)
            where TData : UData, new()
        {
            ReInit(pdata);
            if (data == null)
                data = new TData { IsShow = (x)=>true };
            return InitCountAndData(Data.GetInitCount(), data);
        }
        //创建固定的数量，一般用于Lua调用
        public UControl[] Create(int count)
        {
            return InitCountAndData(count);
        }
        #endregion

        #region get custom
        public T GetData<T>(UControl item) where T : class
        {
            if (CustomDatas.Count <= 0) return null;
            if (CustomDatas == null) return null;
            if (item.Index >= CustomDatas.Count) return CustomDatas[0] as T; 
            if (item.Index < 0) return null;
            if (item.Index >= CustomDatas.Count) return CustomDatas[0] as T;
            return CustomDatas[item.Index] as T;
        }
        public T GetData<T>(int curSelect) where T : class
        {
            if (CustomDatas.Count <= 0) return null;
            if (Controls.Count <= curSelect) return CustomDatas[0] as T;
            var item = Controls[curSelect];
            if (item.Index < 0) return CustomDatas[0] as T;
            if (item.Index >= CustomDatas.Count) return CustomDatas[0] as T;
            return CustomDatas[item.Index] as T;
        }
        public T GetData<T>() where T : class
        {
            if (CustomDatas.Count <= 0) return null;
            if (Controls.Count <= CurSelectIndex) return CustomDatas[0] as T;
            var item = Controls[CurSelectIndex];
            if (item.Index < 0) return null;
            if (item.Index >= CustomDatas.Count) return CustomDatas[0] as T;
            return CustomDatas[item.Index] as T;
        }
        object GetCustomData(IList customData, int index)
        {
            if (customData == null) return null;
            if (index >= customData.Count) return null;
            if (index < 0) return null;
            return customData[index];
        }
        #endregion

        #region get control
        public UControl GetControl(int index)
        {
            if (Controls.Count <= index) return null;
            var item = Controls[index];
            if (item.Index < 0) return null;
            return item;
        }
        public TP GetControl<TP>(int index) where TP:UControl
        {
            return GetControl(index) as TP;
        }
        #endregion

        #region get

        public bool GetIsToggleGroup()
        {
            return IsToggleGroup;
        }

        public bool GetIsAutoSelect()
        {
            return IsAutoSelect;
        }
        #endregion

        #region set
        public void ForControls(Callback<UControl> callback)
        {
            foreach (var item in Controls)
            {
                if (item.IsShow)
                {
                    callback(item);
                }
            }
        }
        #endregion

        #region select
        public void SelectItem(UControl arg1)
        {
            if (arg1 == null)
                return;
            PreSelectIndex = CurSelectIndex;
            CurSelectIndex = arg1.Index;
            Data.OnClickSelected?.Invoke(arg1);

            //刷星自生界面
            if (IsLinkSelfView)
            {
                PUIView?.SetDirtyAll();
            }

            //刷新自生
            if (arg1 is UCheck)
            {
                Refresh();
            }
        }
        public void SelectItem(int index)
        {
            var data = GetControl(index);
            SelectItem(data);
        }
        public void SelectIndex(int index)
        {
            PreSelectIndex = CurSelectIndex;
            if (GetIsAutoSelect() && index <= -1) CurSelectIndex = 0;
            else CurSelectIndex = index;
        }
        #endregion

        #region utile
        UControl[] InitCountAndData<TData>(int count, TData data = null)
            where TData : UData, new()
        {
            _InitCount(count);
            return _InitDataSingle(data);
        }
        UControl[] InitCountAndData<TData>(int count, TData[] data)
            where TData : UData, new()
        {
            _InitCount(count);
            return _InitDataMulti(data);
        }
        UControl[] InitCountAndData(int count)
        {
            _InitCount(count);
            return _InitDataRaw();
        }

        // 通过数量初始化
        private void _InitCount(int count)
        {
            if (IsInitedCount)
            {
                CLog.Error("InitCount 无法初始化2次!!!! " + Path);
                return;
            }
            IsInitedCount = true;
            GOs.Clear();
            for (int i = 0; i < Trans.childCount; ++i)
            {
                Transform temp = Trans.GetChild(i);
                var ele = temp.GetComponent<LayoutElement>();
                if (ele != null && ele.ignoreLayout)
                {
                    continue;
                }
                var ignore = temp.GetComponent<UIgnore>();
                if (ignore != null)
                {
                    continue;
                }
                GOs.Add(temp.gameObject);
            }
            if (Prefab == null && GOs.Count > 0)
            {
                Prefab = GOs[0];
            }
            if (Prefab == null)
            {
                CLog.Error("{0}: Prefab == null", Path);
                return;
            }
            if (Prefab.name.StartsWith(SysConst.STR_Base))
                CLog.Error($"不能使用基础UI Prefab 初始化:{Prefab.name}");

            //差值
            int subCount = count - GOs.Count;

            if (subCount > 0)
            {
                //生成剩余的游戏对象
                for (int i = 0; i < subCount; ++i)
                {
                    GameObject temp = GameObject.Instantiate(Prefab, this.RectTrans.position, this.RectTrans.rotation);
                    (temp.transform as RectTransform).SetParent(this.RectTrans);
                    (temp.transform as RectTransform).localScale = Vector3.one;
                    GOs.Add(temp);
                }
            }

            Controls.Clear();
            for (int i = 0; i < GOs.Count; ++i)
            {
                var tempPresenter = GOs[i].GetComponent<UControl>();
                if (tempPresenter == null)
                {
                    CLog.Error("Item:没有添加组件");
                    return;
                }
                Controls.Add(tempPresenter);

                if (i < count)
                    tempPresenter.Show(true);
                else
                    tempPresenter.Show(false);

                if (tempPresenter is UCheck checkBox)
                {
                    ToggleGroupCheckBoxs.Add(checkBox);
                }
            }
        }
        UControl[] _InitDataRaw()
        {
            ClearChilds();
            for (int i = 0; i < GOs.Count; i++)
            {
                if (GOs[i] == null)
                {
                    CLog.Error("有的GO为null");
                }
            }
            UControl[] ts = GOs.Where(go => go != null).Select(go => go.GetComponent<UControl>()).ToArray();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                {
                    CLog.Error(string.Format("取出组件为null, type={0}", typeof(UControl)));
                    break;
                }
                else
                {
                    if (AddStaticChild(ts[i]))
                    {
                        ts[i].SetIndex(i);
                        ts[i].SetDataIndex(i);
                        ts[i].PDupplicate = this;
                    }
                    ts[i].Init();
                }
            }
            return ts;
        }
        UControl[] _InitDataSingle<TData>(TData data)
            where TData : UData, new()
        {
            ClearChilds();
            if (data == null)
            {
                data = new TData();
            }
            for (int i = 0; i < GOs.Count; i++)
            {
                if (GOs[i] == null)
                {
                    CLog.Error("有的GO为null");
                }
            }
            UPres<TData>[] ts = GOs.Where(go => go != null).Select(go => go.GetComponent<UPres<TData>>()).ToArray();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                {
                    CLog.Error(string.Format("取出组件为null, type={0}", typeof(UPres<TData>)));
                    break;
                }
                else
                {
                    if (AddStaticChild(ts[i]))
                    {
                        ts[i].SetIndex(i);
                        ts[i].SetDataIndex(i);
                        ts[i].PDupplicate = this;
                    }
                    ts[i].Init(data);
                }
            }
            return ts;
        }
        UControl[] _InitDataMulti<TData>(TData[] data)
            where TData : UData, new()
        {
            ClearChilds();
            for (int i = 0; i < GOs.Count; i++)
            {
                if (GOs[i] == null)
                {
                    CLog.Error("有的GO为null");
                }
            }
            UPres<TData>[] ts = GOs.Where(go => go != null).Select(go => go.GetComponent<UPres<TData>>()).ToArray();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                {
                    CLog.Error(string.Format("取出组件为null, type={0},如果想要忽略,请添加IgnoreElement组件", typeof(UPres<TData>)));
                    break;
                }
                else
                {
                    if (AddStaticChild(ts[i]))
                    {
                        ts[i].SetIndex(i);
                        ts[i].SetDataIndex(i);
                        ts[i].PDupplicate = this;
                    }
                    if (data != null)
                    {
                        if (i < data.Length)
                            ts[i].Init(data[i]);
                    }
                    else
                    {
                        ts[i].Init(new TData());
                    }
                }
            }
            return ts;
        }
        UControl[] InitData()
        {
            ClearChilds();
            for (int i = 0; i < GOs.Count; i++)
            {
                if (GOs[i] == null)
                {
                    CLog.Error("有的GO为null");
                }
            }
            UControl[] ts = GOs.Where(go => go != null).Select(go => go.GetComponent<UControl>()).ToArray();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                {
                    CLog.Error(string.Format("取出组件为null, type={0},如果想要忽略,请添加IgnoreElement组件", typeof(UControl)));
                    break;
                }
                else
                {
                    if (AddStaticChild(ts[i]))
                    {
                        ts[i].SetIndex(i);
                        ts[i].SetDataIndex(i);
                        ts[i].PDupplicate = this;
                    }
                }
            }
            return ts;
        }
        #endregion

        #region callback
        // 鼠标进入
        public override void OnPointerEnter(PointerEventData eventData) { }
        // 鼠标退出
        public override void OnPointerExit(PointerEventData eventData) { }
        // 鼠标点击
        public override void OnPointerClick(PointerEventData eventData) { }
        // 鼠标按下
        public override void OnPointerDown(PointerEventData eventData) { }
        // 鼠标按下
        public override void OnPointerUp(PointerEventData eventData) { }
        // 点击状态变化
        public override void OnInteractable(bool b, bool effect = true) { }
        #endregion

        #region Inspector
        bool Inspector_HideAutoSelect()
        {
            return !IsToggleGroup;
        }
        protected override bool Inspector_IsShowFontType()
        {
            return false;
        }
        [Button("统一名称")]
        public void ModifyName()
        {
            Util.UnifyChildName(GO);
        }
        [Button("Verticle")]
        void Inspector_Verticle()
        {
            LayoutGroup group = gameObject.GetComponent<LayoutGroup>();
            if (group) DestroyImmediate(group);
            gameObject.AddComponent<VerticalLayoutGroup>();
        }
        [Button("Horizontal")]
        void Inspector_Horizontal()
        {
            LayoutGroup group = gameObject.GetComponent<LayoutGroup>();
            if (group) DestroyImmediate(group);
            gameObject.AddComponent<HorizontalLayoutGroup>();
        }
        [Button("Grid")]
        void Inspector_Grid()
        {
            LayoutGroup group = gameObject.GetComponent<LayoutGroup>();
            if (group) DestroyImmediate(group);
            gameObject.AddComponent<GridLayoutGroup>();
        }
        bool Inspector_IsInitCount()
        {
            return IsInitOnStart;
        }
        bool Inspector_IsPrefab()
        {
            return transform.childCount <= 0;
        }
        bool IsShowCheckbox()
        {
            if (Prefab == null)
                return false;
            return Prefab.GetComponent<UCheck>() != null;
        }
        #endregion
    }

}