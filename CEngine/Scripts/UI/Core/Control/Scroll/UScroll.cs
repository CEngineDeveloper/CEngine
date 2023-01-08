using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 无限滚动控件,只支持竖向和横向
/// </summary>
namespace CYM.UI
{
    public class UScrollData : UData
    {
        public Func<IList> GetCustomDatas;
        public List<Func<object, object>> Sorter = new List<Func<object, object>>();
        public Func<string> EmptyDesc;
        public string EmptyDescKey = null;

        #region Callback
        /// <summary>
        /// object1=presenter
        /// object2=custom data
        /// </summary>
        public Callback<object, object> OnRefreshItem;
        public Callback<int> OnSelectedChange;
        public Callback<UControl> OnSelectItem;
        public Callback<bool> OnScrollingChanged;
        public Callback<UControl> OnCellVisibilityChanged;
        #endregion
    }
    [AddComponentMenu("UI/Control/UScroll")]
    [HideMonoScript]
    public partial class UScroll : UPres<UScrollData>, 
        ICheckBoxContainer, 
        IDragHandler,
        IBeginDragHandler,
        IEndDragHandler
    {
        #region inspector
        [FoldoutGroup("Inspector"), SerializeField]
        public Scrollbar IScrollbar;
        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly]
        public Text IEmptyDesc;
        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly, Required]
        public RectTransform IContent;
        #endregion

        #region checkBox
        [FoldoutGroup("CheckBox"), SerializeField]
        public bool IsToggleGroup = false;
        [FoldoutGroup("CheckBox"), SerializeField,HideIf("Inspector_HideAutoSelect")]
        public bool IsAutoSelect = true;
        #endregion

        #region data
        [FoldoutGroup("Data"), SerializeField]
        bool loop;
        [FoldoutGroup("Data"), SerializeField, Tooltip("强制扩展,给按钮流出空间")]
        bool IsForceExpend = false;
        [FoldoutGroup("Data"), SerializeField]
        ScrollbarVisibilityType scrollbarVisibility = ScrollbarVisibilityType.Always;
        [FoldoutGroup("Data"), SerializeField]
        ScrollRect.MovementType MovementType = ScrollRect.MovementType.Elastic;
        #endregion

        #region snap
        [FoldoutGroup("Snap"), SerializeField]
        bool snapping;
        [FoldoutGroup("Snap"), SerializeField]
        float snapVelocityThreshold;
        [FoldoutGroup("Snap"), SerializeField]
        float snapWatchOffset;
        [FoldoutGroup("Snap"), SerializeField]
        float snapJumpToOffset;
        [FoldoutGroup("Snap"), SerializeField]
        float snapCellCenterOffset;
        [FoldoutGroup("Snap"), SerializeField]
        bool snapUseCellSpacing;
        [FoldoutGroup("Snap"), SerializeField]
        TweenType snapTweenType;
        [FoldoutGroup("Snap"), SerializeField]
        float snapTweenTime;
        #endregion

        #region private
        TextAnchor anchor = TextAnchor.UpperLeft;
        ScrollDirectionType scrollDirection;
        RectOffset padding;
        public UControl Prefab { get; private set; }
        RectTransform prefabRect;
        ScrollRect scrollRect;
        float spacing;
        public float PrefabSize { get; private set; } = 100;
        float expendSize = 1f;
        int numOfGroupCell = 1;
        bool sortReversedList = false;
        int sortBy = -1;
        #endregion

        #region prop val
        public IList CacheCustomData { get; private set; } = new List<object>();
        // 当前选择的index
        public int CurSelectIndex { get; protected set; } = -1;
        // 上一次的选择
        public int PreSelectIndex { get; protected set; } = SysConst.INT_Inv;
        #endregion

        #region life
        public override bool NeedUpdate => true;
        public override bool IsCollection => true;
        protected override void Awake()
        {
            base.Awake();
            if (GetIsAutoSelect()) CurSelectIndex = 0;
            else CurSelectIndex = -1;
        }
        protected override void FetchSubControls()
        {
            //do nothing...
            //InitializesScroller 会回收所有的Cell
        }
        public override void OnBeFetched()
        {
            FontType = nameof(CYM.FontType.None);
            base.OnBeFetched();
            InitializesScroller();
        }
        public override void OnViewShow(bool b)
        {
            base.OnViewShow(b);
            if (!b)
            {
                if (GetIsAutoSelect()) CurSelectIndex = 0;
                else CurSelectIndex = -1;
            }
            else ScrollPosition = 0;
        }
        public override void OnShow(bool isShow)
        {
            base.OnShow(isShow);
            if (!isShow)
            {
                if (GetIsAutoSelect()) CurSelectIndex = 0;
                else CurSelectIndex = -1;
            }
        }
        public override void OnUpdate() 
        {
            base.OnUpdate();
            //刷新ScrollBar,显示或者隐藏ScrollBar
            if (lastScrollbarVisibility != scrollbarVisibility)
            {
                ScrollbarVisibility = scrollbarVisibility;
                lastScrollbarVisibility = scrollbarVisibility;
            }
            //监测速度
            if (LinearVelocity != 0 && !IsScrolling)
            {
                IsScrolling = true;
                Data?.OnScrollingChanged?.Invoke(true);
            }
            else if (LinearVelocity == 0 && IsScrolling)
            {
                IsScrolling = false;
                Data?.OnScrollingChanged?.Invoke(false);
            }
            //更新滑杆速度,防止速度不稳定,增加手感
            if (scrollRect != null)
            {
                //item少的时候降低速度
                if (ScrollRectSize > cellOffsetArray.Last()) 
                    scrollRect.scrollSensitivity = 2;
                //满Item的时候回升到自然速度
                else scrollRect.scrollSensitivity = 15;

                if (BaseInputMgr.IsScrollWheel) 
                    scrollRect.velocity = Vector2.zero;
                if (UGuideView.Default) 
                    scrollRect.enabled = !UGuideView.Default.IsInMask;
            }

        }
        public override void Refresh()
        {
            base.Refresh();
            if (activeCells.data == null) 
                return;
            if (CurSelectData != null)
            {
                CurSelectIndex = GetDataIndex(CurSelectData);
                CurSelectData = null;
            }
            else
            {
                bool needReset = true;
                for (int i = 0; i < activeCells.Count; ++i)
                {
                    if (activeCells[i] == null) 
                        continue;
                    if (activeCells[i] is UScrollGroup group)
                    {
                        foreach (var item in group.Cells)
                        {
                            if (item.Index == CurSelectIndex)
                                needReset = false;
                        }
                    }
                    else
                    {
                        if (activeCells[i].Index == CurSelectIndex)
                            needReset = false;
                    }
                }
                if (needReset && GetIsAutoSelect())
                    CurSelectIndex = 0;
            }
            for (int i = 0; i < activeCells.Count; ++i)
            {
                if (activeCells[i] == null) continue;
                RefreshCell(activeCells[i], true);
            }
            Data.OnSelectedChange?.Invoke(CurSelectIndex);
        }
        public override void RefreshData()
        {
            base.RefreshData();
            if (
                Data != null && 
                Data.GetCustomDatas != null && 
                Prefab != null &&
                scrollRect != null &&
                scrollRectTransform != null&&
                IContent != null)
            {
                if (scrollDirection == ScrollDirectionType.Vertical)
                    PrefabSize = prefabRect.sizeDelta.y;
                else
                    PrefabSize = prefabRect.sizeDelta.x;
                //重新获取数据
                CacheCustomData = Data.GetCustomDatas.Invoke();
                //数据排序
                var sortCall = GetSortCall();
                if (sortCall != null && CacheCustomData is List<object> listCustomData)
                {
                    var sortBy = this.sortBy;
                    if (sortReversedList)
                        CacheCustomData = listCustomData.OrderByDescending(sortCall).ToList();
                    else
                        CacheCustomData = listCustomData.OrderBy(sortCall).ToList();
                }
                //刷新EmptyDesc
                if (IEmptyDesc != null)
                {
                    if (CacheCustomData.Count >= 0)
                    {
                        IEmptyDesc.gameObject.SetActive(true);
                        if (Data.EmptyDesc != null) IEmptyDesc.text = Data.EmptyDesc?.Invoke();
                        else IEmptyDesc.text = GetStr(Data.EmptyDescKey);
                    }
                    else IEmptyDesc.gameObject.SetActive(false);
                }
                //重载数据
                Resize(false, false);
                scrollPosition = Mathf.Clamp(NormalizedScrollPosition * ScrollSize, 0, GetScrollPositionForCellIndex(cellSizeArray.Count - 1, CellViewPositionType.Before));
                if (scrollDirection == ScrollDirectionType.Vertical)
                {
                    scrollRect.verticalNormalizedPosition = Mathf.Clamp01(1 - NormalizedScrollPosition);
                }
                else
                {
                    scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(NormalizedScrollPosition);
                }
            }
        }
        public override void RefreshCell()
        {
            base.RefreshCell();
            if (activeCells.data == null) return;
            for (int i = 0; i < activeCells.Count; ++i)
            {
                if (activeCells[i] == null) continue;
                RefreshCell(activeCells[i]);
            }
        }
        public void RefreshCell(UControl control, bool onlyRefreshSelect = false)
        {
            if (control is UScrollGroup scrollGroup)
            {
                foreach (var item in scrollGroup.Cells)
                {
                    RefreshSingle(item);
                }
            }
            else
            {
                RefreshSingle(control);
            }

            void RefreshSingle(UControl single)
            {
                single.SetActive(single.DataIndex< CacheCustomData.Count);

                if (single.GO.activeSelf)
                {
                    //刷新基类
                    if (GetIsToggleGroup())
                    {
                        if (single is UCheck checkBox && checkBox.IsToggleGroup)
                            checkBox.RefreshStateAndActiveEffectBySelect();
                    }
                    if (onlyRefreshSelect)
                        return;
                    //通过用户自定义方发刷新
                    var customData = CacheCustomData[single.DataIndex];
                    single.SetCustomData(customData);
                    Data.OnRefreshItem?.Invoke(single, customData);
                }
            }
        }
        #endregion

        #region Init
        public void Init(Func<IList> getData, Callback<object, object> onRefresh, string emptyKey = null)
        {
            var temp = new UScrollData { GetCustomDatas = getData, OnRefreshItem = onRefresh, EmptyDescKey = emptyKey };
            Init(temp);
        }
        public void Init(Func<IList> getData, Callback<object, object> onRefresh, Callback<UControl> onItemClick, List<Func<object, object>> sorter, string emptyKey = "")
        {
            var temp = new UScrollData { GetCustomDatas = getData, OnRefreshItem = onRefresh, Sorter = sorter, OnSelectItem = onItemClick, EmptyDescKey = emptyKey };
            Init(temp);
        }
        public void Init(Func<IList> getData, Callback<object, object> onRefresh,Callback<UControl> onItemClick, string emptyKey = "")
        {
            var temp = new UScrollData { GetCustomDatas = getData, OnRefreshItem = onRefresh, OnSelectItem = onItemClick, EmptyDescKey = emptyKey };
            Init(temp);
        }
        // 初始化Scroll
        public override void Init(UScrollData data)
        {
            base.Init(data);
            if (Prefab == null)
            {
                CLog.Error("Scroll:{0} 基础Prefab不能为Null", name);
                return;
            }
            if (Data.GetCustomDatas == null) CLog.Error("TableData 的 GetCustomDatas 必须设置");
            if (Data.OnRefreshItem == null) CLog.Error("TableData 的 OnRefresh 必须设置");
        }
        #endregion

        #region Set(Core)
        public void SortData(int by, bool isDirtyData = true)
        {
            if (by > Data.Sorter.Count)
                CLog.Error("ListSort,数组越界!!!");
            if (sortBy == by)
                sortReversedList = !sortReversedList;
            else if (sortBy == -1)
                sortReversedList = true;
            else
                sortReversedList = true;

            SelectIndex(-1);
            sortBy = by;
            if (isDirtyData)
                SetDirtyData();
        }
        public override void SetDirtyData()
        {
            base.SetDirtyData();
            SelectIndex(-1);
        }
        public void ToggleLoop() => Loop = !loop;
        #endregion

        #region Select
        object CurSelectData = null;
        public void SelectData(object data) => CurSelectData = data;
        public void SelectData(int index)=>CurSelectData = GetData<object>(index);
        public void SelectIndex(int index)
        {
            PreSelectIndex = CurSelectIndex;
            if (GetIsAutoSelect() && index <= -1) CurSelectIndex = 0;
            else CurSelectIndex = index;
        }
        public void SelectItem(UControl arg1)
        {
            SelectIndex(arg1.Index);
            Data?.OnSelectItem?.Invoke(arg1);
            if (arg1 is UCheck)
            {
                Refresh();
            }
        }
        #endregion

        #region get
        public Func<object, object> GetSortCall()
        {
            if (Data.Sorter == null)
                return null;
            if (Data.Sorter.Count <= sortBy)
                return null;
            if (sortBy == -1)
                return null;
            return Data.Sorter[sortBy];
        }
        // 获得控件数据
        public T GetData<T>(int dataIndex) where T : class
        {
            if (dataIndex == SysConst.INT_Inv)
                return GetData<T>();
            if (dataIndex < 0)
                return null;
            if (dataIndex >= CacheCustomData.Count)
                return null;
            return CacheCustomData[dataIndex] as T;
        }
        public T GetData<T>() where T : class
        {
            if (CacheCustomData.Count == 0)
                return default(T);
            if (CurSelectIndex.IsInv())
                return CacheCustomData[0] as T;
            var cell = GetCell<UControl>(CurSelectIndex);
            if (cell == null)
                return CacheCustomData[0] as T;
            return GetData<T>(cell.DataIndex);
        }
        public int GetDataIndex(object obj)
        {
            if (CacheCustomData == null)
                return 0;
            return CacheCustomData.IndexOf(obj);
        }
        public float GetExpendSize()
        {
            if (IsForceExpend)
                return PrefabSize;
            return expendSize;
        }
        public bool GetIsToggleGroup()=> IsToggleGroup;
        public bool GetIsAutoSelect()=> IsAutoSelect;
        //更具Index获得Cell
        public TCell GetCell<TCell>(int index) 
            where TCell :UControl
        {
            if (index < 0)
                return null;
            if (activeCells.Count == 0)
                return null;
            //获得第一个
            if (activeCells[0] is UScrollGroup fistGroup)
            {
                int cellIndex = index % fistGroup.Cells.Count;
                foreach (var item in activeCells.data)
                {
                    var group = item as UScrollGroup;
                    if (index >= group.Cells[0].Index && index <= group.Cells[group.Cells.Count - 1].Index)
                    {
                        return group.Cells[cellIndex] as TCell;
                    }
                }
            }
            else
            {
                if (activeCells.Count > index)
                {
                    return activeCells[index] as TCell;
                }
            }
            return null;
        }
        public TCell FindCell<TCell>(Func<TCell,bool> condition)
            where TCell : UControl
        {
            for (int i=0;i<activeCells.Count;++i)
            {
                var item = activeCells[i];
                if (item == null)
                    continue;
                if (!(item is TCell))
                {
                    CLog.Error($"错误item 不是{typeof(TCell).Name}类型");
                    return null;
                }
                if (condition(item as TCell))
                    return item as TCell;
            }
            return null;
        }
        public TCell FindCell<TCell,TCustomData>(Func<TCell, TCustomData, bool> condition)
            where TCell : UControl
            where TCustomData : class,new()
        {
            for (int i = 0; i < activeCells.Count; ++i)
            {
                var item = activeCells[i];
                if (item == null)
                    continue;
                if (!(item is TCell))
                {
                    CLog.Error($"错误item 不是{typeof(TCell).Name}类型");
                    return null;
                }
                if (condition(item as TCell,item.CustomData as TCustomData))
                    return item as TCell;
            }
            return null;
        }
        #endregion

        #region Inspector
        bool Inspector_HideAutoSelect()=> !IsToggleGroup;
        bool Inspector_IsPrefab()=> transform.childCount <= 0;
        [Button("统一名称")]
        public void ModifyName()
        {
            Util.UnifyChildName(IContent.gameObject);
        }
        [Button("Verticle")]
        void Inspector_Verticle()
        {
            if (IContent == null)
                return;
            LayoutGroup group = IContent.gameObject.GetComponent<LayoutGroup>();
            if (group) DestroyImmediate(group);
            IContent.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        [Button("Horizontal")]
        void Inspector_Horizontal()
        {
            if (IContent == null)
                return;
            LayoutGroup group = IContent.gameObject.GetComponent<LayoutGroup>();
            if (group) DestroyImmediate(group);
            IContent.gameObject.AddComponent<HorizontalLayoutGroup>();
        }
        [Button("AddMask")]
        void Inspector_AddMask()
        {
            var mask = gameObject.AddComponent<Mask>();
            var image = gameObject.AddComponent<Image>();
            image.sprite = UIUtil.GetUISprite();
        }
        [Button("AddPlaceholder")]
        void Inspector_AddPlaceholder()
        {
            if (IContent != null)
                return;
            IContent = new GameObject("Placeholder",typeof(RectTransform)).transform as RectTransform;
            IContent.transform.localScale = Vector3.one;
            IContent.SetParent(transform);
        }
        protected override bool Inspector_IsShowFontType()
        {
            return false;
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            if (IContent == null)
                return;
            var layout = IContent.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layout == null)
                IContent.gameObject.AddComponent<VerticalLayoutGroup>();
        }
#endif
        #endregion
    }

}