//------------------------------------------------------------------------------
// BaseScrollCore.cs
// Copyright 2019 2019/5/27 
// Created by CYM on 2019/5/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public partial class UScroll
    {
        #region const
        const string NameRecycledCells = "RecycledCells";
        const string NameFirstPadder = "FirstPadder";
        const string NameLastPadder = "LastPadder";
        #endregion

        #region life
        void InitializesScroller()
        {
            if (initialized)
                return;
            if (IContent == null)
            {
                CLog.Error("IContent 为 null");
                return;
            }
            ClearChilds();
            GameObject contentGO = IContent.gameObject;
            scrollRect = GO.SafeAddComponet<ScrollRect>();
            scrollRect.content = IContent;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.2f;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.movementType = MovementType;
            scrollRect.scrollSensitivity = 20;
            scrollRectTransform = scrollRect?.GetComponent<RectTransform>();
            //scrollBarSize = IScrollbar?.size??0;
            // set up the last values for updates
            lastScrollbarVisibility = scrollbarVisibility;
            // 设置背景阻挡,这个很重要,因为ScrollView需要相应Drag事件
            Image bg = GetComponent<Image>();
            if (bg != null) bg.raycastTarget = true;
            // 删除任何Layout
            LayoutGroup tempLayoutGroup = contentGO.GetComponent<LayoutGroup>();
            if (tempLayoutGroup != null)
            {
                if (tempLayoutGroup is HorizontalOrVerticalLayoutGroup hvLayout)
                {
                    spacing = hvLayout.spacing;
                    padding = hvLayout.padding;
                    anchor = hvLayout.childAlignment;

                    if (tempLayoutGroup is VerticalLayoutGroup)
                        scrollDirection = ScrollDirectionType.Vertical;
                    else if (tempLayoutGroup is HorizontalLayoutGroup)
                        scrollDirection = ScrollDirectionType.Horizontal;
                }
                DestroyImmediate(tempLayoutGroup);
            }
            // 添加Layout
            if (scrollDirection == ScrollDirectionType.Vertical) 
                layoutGroup = contentGO.SafeAddComponet<VerticalLayoutGroup>();
            else layoutGroup = contentGO.SafeAddComponet<HorizontalLayoutGroup>();
            // 设置 Layout 参数
            layoutGroup.spacing = spacing;
            layoutGroup.padding = padding;
            layoutGroup.childAlignment = anchor;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            if (scrollDirection == ScrollDirectionType.Vertical)
            {
                scrollRect.verticalScrollbar = IScrollbar;
                scrollRect.vertical = true;
                scrollRect.horizontal = false;
            }
            else
            {
                scrollRect.horizontalScrollbar = IScrollbar;
                scrollRect.vertical = false;
                scrollRect.horizontal = true;
            }
            // create the recycled cell view container
            contentGO = new GameObject(NameRecycledCells, typeof(RectTransform));
            contentGO.transform.SetParent(scrollRect.transform, false);
            recycledCellContainer = contentGO.GetComponent<RectTransform>();
            recycledCellContainer.gameObject.SetActive(false);
            // 获得所有的子节点
            List<Transform> childTrans = new List<Transform>();
            for (int i = 0; i < IContent.childCount; ++i)
            {
                childTrans.Add(IContent.GetChild(i));
            }
            // 回收所有有效的节点
            foreach(var item in childTrans)
            {
                var control = item.GetComponent<UControl>();
                if (control == null) continue;
                if (Prefab == null)
                {
                    Prefab = control;
                    UpdatePrefabChildCell();
                }
                AddCellChild(control);
                RecycleCell(control);
            }
            // 判断Prefab是否为空
            if (Prefab == null)
            {
                CLog.Error("Scroll的BasePrefab不能为空,Error gameobject:{0}", gameObject.name);
                return;
            }
            else UpdatePrefabChildCell();
            // create the padder objects
            var tempGO = new GameObject(NameFirstPadder, typeof(RectTransform), typeof(LayoutElement));
            tempGO.transform.SetParent(IContent, false);
            firstPadder = tempGO.GetComponent<LayoutElement>();
            tempGO = new GameObject(NameLastPadder, typeof(RectTransform), typeof(LayoutElement));
            tempGO.transform.SetParent(IContent, false);
            lastPadder = tempGO.GetComponent<LayoutElement>();
            // 设置BasePrefabSize
            prefabRect = Prefab.transform as RectTransform;
            if (scrollDirection == ScrollDirectionType.Vertical) 
                PrefabSize = prefabRect.sizeDelta.y;
            else 
                PrefabSize = prefabRect.sizeDelta.x;
            // 创建Item
            float needCount = (ScrollRectSize / PrefabSize) - recycledCells.Count; 
            for (int i = 0; i <= needCount; ++i)
            {
                var cell = CreateCell(Prefab);
                RecycleCell(cell);
            }
            initialized = true;
            void UpdatePrefabChildCell()
            {
                if (Prefab is UScrollGroup scrollGroup)
                {
                    scrollGroup.FetchChildCell();
                    numOfGroupCell = Mathf.Clamp(scrollGroup.Count, 1, int.MaxValue);
                }
            }
        }

        protected override void OnEnable()
        {
            scrollRect?.onValueChanged.AddListener(OnScrollRectValueChanged);
        }

        protected override void OnDisable()
        {
            scrollRect?.onValueChanged.RemoveListener(OnScrollRectValueChanged);
        }
        #endregion

        #region Move Position
        public void Move(float val = 0)
        {
            float target = ScrollPosition + val;
            if (_tweenMove != null) DOTween.Kill(_tweenMove);
            _tweenMove = DOTween.To(() => ScrollPosition, x => ScrollPosition = x, target, 0.5f);
        }
        public void MoveNegative()=> Move(-1 * PrefabSize);
        public void MovePositive()=> Move(PrefabSize);
        public void JumpToData(object obj,
            float scrollerOffset = 0,
            float cellOffset = 0,
            bool useSpacing = true,
            TweenType tweenType = TweenType.immediate,
            float tweenTime = 0f,
            Action jumpComplete = null,
            LoopJumpDirectionType loopJumpDirection = LoopJumpDirectionType.Closest)
        {
            var index = GetDataIndex(obj);
            JumpToDataIndex(index, scrollerOffset, cellOffset, useSpacing, tweenType, tweenTime, jumpComplete, loopJumpDirection);  
        }
        /// <summary>
        /// Jump to a position in the scroller based on a dataIndex. This overload allows you
        /// to specify a specific offset within a cell as well.
        /// </summary>
        /// <param name="dataIndex">he data index to jump to</param>
        /// <param name="scrollerOffset">The offset from the start (top / left) of the scroller in the range 0..1.
        /// Outside this range will jump to the location before or after the scroller's viewable area</param>
        /// <param name="cellOffset">The offset from the start (top / left) of the cell in the range 0..1</param>
        /// <param name="useSpacing">Whether to calculate in the spacing of the scroller in the jump</param>
        /// <param name="tweenType">What easing to use for the jump</param>
        /// <param name="tweenTime">How long to interpolate to the jump point</param>
        /// <param name="jumpComplete">This delegate is fired when the jump completes</param>
        public void JumpToDataIndex(int dataIndex,
            float scrollerOffset = 0,
            float cellOffset = 0,
            bool useSpacing = true,
            TweenType tweenType = TweenType.immediate,
            float tweenTime = 0f,
            Action jumpComplete = null,
            LoopJumpDirectionType loopJumpDirection = LoopJumpDirectionType.Closest
            )
        {
            var cellOffsetPosition = 0f;
            if (cellOffset != 0)
            {
                // calculate the cell offset position
                // get the cell's size
                var cellSize = GetCellSize(dataIndex);
                if (useSpacing)
                {
                    // if using spacing add spacing from one side
                    cellSize += spacing;
                    // if this is not a bounday cell, then add spacing from the other side
                    if (dataIndex > 0 && dataIndex < (NumberOfCells - 1)) cellSize += spacing;
                }
                // calculate the position based on the size of the cell and the offset within that cell
                cellOffsetPosition = cellSize * cellOffset;
            }
            if (scrollerOffset == 1f)
            {
                cellOffsetPosition += padding.bottom;
            }

            // cache the offset for quicker calculation
            var offset = -(scrollerOffset * ScrollRectSize) + cellOffsetPosition;
            var newScrollPosition = 0f;
            if (loop)
            {
                // if looping, then we need to determine the closest jump position.
                // we do that by checking all three sets of data locations, and returning the closest one
                // get the scroll positions for each data set.
                // Note: we are calculating the position based on the cell view index, not the data index here
                var set1Position = GetScrollPositionForCellIndex(dataIndex, CellViewPositionType.Before) + offset;
                var set2Position = GetScrollPositionForCellIndex(dataIndex + NumberOfCells, CellViewPositionType.Before) + offset;
                var set3Position = GetScrollPositionForCellIndex(dataIndex + (NumberOfCells * 2), CellViewPositionType.Before) + offset;
                // get the offsets of each scroll position from the current scroll position
                var set1Diff = (Mathf.Abs(scrollPosition - set1Position));
                var set2Diff = (Mathf.Abs(scrollPosition - set2Position));
                var set3Diff = (Mathf.Abs(scrollPosition - set3Position));
                switch (loopJumpDirection)
                {
                    case LoopJumpDirectionType.Closest:

                        // choose the smallest offset from the current position (the closest position)
                        if (set1Diff < set2Diff)
                        {
                            if (set1Diff < set3Diff) newScrollPosition = set1Position;
                            else newScrollPosition = set3Position;
                        }
                        else
                        {
                            if (set2Diff < set3Diff) newScrollPosition = set2Position;
                            else newScrollPosition = set3Position;
                        }
                        break;
                    case LoopJumpDirectionType.Up:
                        newScrollPosition = set1Position;
                        break;
                    case LoopJumpDirectionType.Down:
                        newScrollPosition = set3Position;
                        break;
                }
            }
            else
            {
                // not looping, so just get the scroll position from the dataIndex
                newScrollPosition = GetScrollPositionForCellIndex(loop ? NumberOfCells + dataIndex : dataIndex, CellViewPositionType.Before) +  offset; //GetScrollPositionForDataIndex(dataIndex, CellViewPositionType.Before)
            }
            // clamp the scroll position to a valid location
            newScrollPosition = Mathf.Clamp(newScrollPosition, 0, GetScrollPositionForCellIndex(cellSizeArray.Count - 1, CellViewPositionType.Before));
            // if spacing is used, adjust the final position
            if (useSpacing)
            {
                // move back by the spacing if necessary
                newScrollPosition = Mathf.Clamp(newScrollPosition - spacing, 0, GetScrollPositionForCellIndex(cellSizeArray.Count - 1, CellViewPositionType.Before));
            }
            // ignore the jump if the scroll position hasn't changed
            if (newScrollPosition == scrollPosition)
            {
                jumpComplete?.Invoke();
                return;
            }
            // start tweening
            StartCoroutine(_tweenPosition(tweenType, tweenTime, ScrollPosition, newScrollPosition, jumpComplete));
        }
        #endregion

        #region get
        float GetCellSize(int index) => PrefabSize;
        #endregion

        #region reload data & resize
        /// <summary>
        /// This function will create an internal list of sizes and offsets to be used in all calculations.
        /// It also sets up the loop triggers and positions and initializes the cell views.
        /// </summary>
        /// <param name="keepPosition">If true, then the scroller will try to go back to the position it was at before the resize</param>
        void Resize(bool keepPosition, bool isRefreshCell)
        {
            // cache the original position
            var originalScrollPosition = scrollPosition;
            // clear out the list of cell view sizes and create a new list
            cellSizeArray.Clear();
            var offset = 0f;
            // add a size for each row in our data based on how many the delegate tells us to create
            for (var i = 0; i < NumberOfCells; i++)
            {
                // add the size of this cell based on what the delegate tells us to use. Also add spacing if this cell isn't the first one
                cellSizeArray.Add(GetCellSize(i) + (i == 0 ? 0 : layoutGroup.spacing));
                offset += cellSizeArray[cellSizeArray.Count - 1];
            }
            // if looping, we need to create three sets of size data
            if (loop)
            {
                // if the cells don't entirely fill up the scroll area, 
                // make some more size entries to fill it up
                if (offset < ScrollRectSize)
                {
                    int additionalRounds = Mathf.CeilToInt(ScrollRectSize / offset);
                    DuplicateCellSizes(additionalRounds, cellSizeArray.Count);
                }
                // set up the loop indices
                loopFirstCellIndex = cellSizeArray.Count;
                loopLastCellIndex = loopFirstCellIndex + cellSizeArray.Count - 1;
                // create two more copies of the cell sizes
                DuplicateCellSizes(2, cellSizeArray.Count);
            }
            // calculate the offsets of each cell view
            cellOffsetArray.Clear();
            var offset2 = 0f;
            for (var i = 0; i < cellSizeArray.Count; i++)
            {
                offset2 += cellSizeArray[i];
                cellOffsetArray.Add(offset2);
            }
            // set the size of the active cell view container based on the number of cell views there are and each of their sizes
            if (scrollDirection == ScrollDirectionType.Vertical)
            {
                if (ScrollRectSize > cellOffsetArray.Last())//没有充满
                    IContent.sizeDelta = new Vector2(IContent.sizeDelta.x, ScrollRectSize + padding.top + padding.bottom + GetExpendSize());
                else //充满
                    IContent.sizeDelta = new Vector2(IContent.sizeDelta.x, cellOffsetArray.Last() + GetExpendSize() + padding.top + padding.bottom);
            }
            else
            {
                if (ScrollRectSize > cellOffsetArray.Last())//没有充满
                    IContent.sizeDelta = new Vector2(ScrollRectSize + padding.left + padding.right + GetExpendSize(), IContent.sizeDelta.y);
                else //充满
                    IContent.sizeDelta = new Vector2(cellOffsetArray.Last() + GetExpendSize() + padding.left + padding.right, IContent.sizeDelta.y);
            }
            // if looping, set up the loop positions and triggers
            if (loop)
            {
                loopFirstScrollPosition = GetScrollPositionForCellIndex(loopFirstCellIndex, CellViewPositionType.Before) + (spacing * 0.5f);
                loopLastScrollPosition = GetScrollPositionForCellIndex(loopLastCellIndex, CellViewPositionType.After) - ScrollRectSize + (spacing * 0.5f);

                loopFirstJumpTrigger = loopFirstScrollPosition - ScrollRectSize;
                loopLastJumpTrigger = loopLastScrollPosition + ScrollRectSize;
            }
            // create the visibile cells
            ResetVisibleCells(isRefreshCell);
            // if we need to maintain our original position
            if (keepPosition)
            {
                ScrollPosition = originalScrollPosition;
            }
            else
            {
                if (loop) ScrollPosition = loopFirstScrollPosition;
                else ScrollPosition = 0;
            }
            // set up the visibility of the scrollbar
            ScrollbarVisibility = scrollbarVisibility;

            /// <summary>
            /// Create a copy of the cell view sizes. This is only used in looping
            /// </summary>
            /// <param name="numberOfTimes">How many times the copy should be made</param>
            /// <param name="cellCount">How many cells to copy</param>
            void DuplicateCellSizes(int numberOfTimes, int cellCount)
            {
                for (var i = 0; i < numberOfTimes; i++)
                {
                    for (var j = 0; j < cellCount; j++)
                    {
                        cellSizeArray.Add(cellSizeArray[j] + (j == 0 ? layoutGroup.spacing : 0));
                    }
                }
            }
        }
        /// <summary>
        /// This sets up the visible cells, adding and recycling as necessary
        /// </summary>
        void ResetVisibleCells(bool isRefreshCell)
        {
            // calculate the range of the visible cells
            var startIndex = GetCellViewIndexAtPosition(scrollPosition);
            var endIndex = GetCellViewIndexAtPosition(scrollPosition + (scrollDirection == ScrollDirectionType.Vertical ? scrollRectTransform.rect.height : scrollRectTransform.rect.width));
            // go through each previous active cell and recycle it if it no longer falls in the range
            var i = 0;
            remainingCellIndices.Clear();
            while (i < activeCells.Count)
            {
                if (activeCells[i].Index < startIndex || activeCells[i].Index > endIndex)
                {
                    RecycleCell(activeCells[i]);
                }
                else
                {
                    // this cell index falls in the new range, so we add its
                    // index to the reusable list
                    remainingCellIndices.Add(activeCells[i].Index);
                    i++;
                }
            }
            if (remainingCellIndices.Count == 0)
            {
                // there were no previous active cells remaining, 
                // this list is either brand new, or we jumped to 
                // an entirely different part of the list.
                // just add all the new cell views
                for (i = startIndex; i <= endIndex; i++)
                {
                    AddCell(i, ListPositionType.Last, isRefreshCell);
                }
            }
            else
            {
                // we are able to reuse some of the previous
                // cell view
                // first add the views that come before the 
                // previous list, going backward so that the
                // new views get added to the front
                for (i = endIndex; i >= startIndex; i--)
                {
                    if (i < remainingCellIndices.First())
                    {
                        AddCell(i, ListPositionType.First, isRefreshCell);
                    }
                }
                // next add teh views that come after the
                // previous list, going forward and adding
                // at the end of the list
                for (i = startIndex; i <= endIndex; i++)
                {
                    if (i > remainingCellIndices.Last())
                    {
                        AddCell(i, ListPositionType.Last, isRefreshCell);
                    }
                }
            }
            // update the start and end indices
            StartCellIndex = startIndex;
            EndCellIndex = endIndex;
            // adjust the padding elements to offset the cell views correctly
            if (NumberOfCells > 0)
            {
                // calculate the size of each padder
                var firstSize = cellOffsetArray[StartCellIndex] - cellSizeArray[StartCellIndex];
                var lastSize = cellOffsetArray.Last() - cellOffsetArray[EndCellIndex];
                if (scrollDirection == ScrollDirectionType.Vertical)
                {
                    // set the first padder and toggle its visibility
                    firstPadder.minHeight = firstSize;
                    firstPadder.gameObject.SetActive(firstPadder.minHeight > 0);
                    // set the last padder and toggle its visibility
                    lastPadder.minHeight = lastSize;
                    lastPadder.gameObject.SetActive(lastPadder.minHeight > 0);
                }
                else
                {
                    // set the first padder and toggle its visibility
                    firstPadder.minWidth = firstSize;
                    firstPadder.gameObject.SetActive(firstPadder.minWidth > 0);
                    // set the last padder and toggle its visibility
                    lastPadder.minWidth = lastSize;
                    lastPadder.gameObject.SetActive(lastPadder.minWidth > 0);
                }
            }
        }
        #endregion

        #region private value
        /// <summary>
        /// The size of the visible portion of the scroller
        /// </summary>
        float ScrollRectSize
        {
            get
            {
                if (scrollDirection == ScrollDirectionType.Vertical)
                    return scrollRectTransform.rect.height;
                else
                    return scrollRectTransform.rect.width;
            }
        }
        /// <summary>
        /// The absolute position in pixels from the start of the scroller
        /// </summary>
        float ScrollPosition
        {
            get => scrollPosition;
            set
            {
                // make sure the position is in the bounds of the current set of views
                value = Mathf.Clamp(value, 0, GetScrollPositionForCellIndex(cellSizeArray.Count - 1, CellViewPositionType.Before));
                // only if the value has changed
                if (scrollPosition != value)
                {
                    scrollPosition = value;
                    if (scrollDirection == ScrollDirectionType.Vertical)
                        scrollRect.verticalNormalizedPosition = 1 - (scrollPosition / ScrollSize);
                    else
                        scrollRect.horizontalNormalizedPosition = (scrollPosition / ScrollSize);
                }
            }
        }
        /// <summary>
        /// The size of the active cell view container minus the visibile portion
        /// of the scroller
        /// </summary>
        float ScrollSize
        {
            get
            {
                if (scrollDirection == ScrollDirectionType.Vertical)
                    return Mathf.Max(IContent.rect.height - scrollRectTransform.rect.height, 0);
                else
                    return Mathf.Max(IContent.rect.width - scrollRectTransform.rect.width, 0);
            }
        }
        /// <summary>
        /// The normalized position of the scroller between 0 and 1
        /// </summary>
        float NormalizedScrollPosition
        {
            get
            {
                var scrollPosition = ScrollPosition;
                return (scrollPosition <= 0 ? 0 : this.scrollPosition / ScrollSize);
            }
        }
        /// <summary>
        /// Whether the scroller should loop the resulting cell views.
        /// Looping creates three sets of internal size data, attempting
        /// to keep the scroller in the middle set. If the scroller goes
        /// outside of this set, it will jump back into the middle set,
        /// giving the illusion of an infinite set of data.
        /// </summary>
        bool Loop
        {
            get => loop;
            set
            {
                // only if the value has changed
                if (loop != value)
                {
                    // get the original position so that when we turn looping on
                    // we can jump back to this position
                    var originalScrollPosition = scrollPosition;
                    loop = value;
                    // call resize to generate more internal elements if loop is on,
                    // remove the elements if loop is off
                    Resize(false, true);
                    if (loop) ScrollPosition = loopFirstScrollPosition + originalScrollPosition;
                    else ScrollPosition = originalScrollPosition - loopFirstScrollPosition;
                    // update the scrollbars
                    ScrollbarVisibility = scrollbarVisibility;
                }
            }
        }
        /// <summary>
        /// Sets how the visibility of the scrollbars should be handled
        /// </summary>
        ScrollbarVisibilityType ScrollbarVisibility
        {
            get => scrollbarVisibility;
            set
            {
                scrollbarVisibility = value;
                // make sure we actually have some cell views
                if (IScrollbar != null && cellOffsetArray != null && cellOffsetArray.Count > 0)
                {
                    if (cellOffsetArray.Last() < ScrollRectSize || loop)
                        IScrollbar.gameObject.SetActive(scrollbarVisibility == ScrollbarVisibilityType.Always);
                    else
                        IScrollbar.gameObject.SetActive(scrollbarVisibility != ScrollbarVisibilityType.Never);
                }
            }
        }
        /// <summary>
        /// This is the velocity of the scroller.
        /// </summary>
        Vector2 Velocity
        {
            get => scrollRect.velocity;
            set => scrollRect.velocity = value;
        }
        /// <summary>
        /// The linear velocity is the velocity on one axis.
        /// The scroller should only be moving one one axix.
        /// </summary>
        float LinearVelocity
        {
            get
            {
                if (scrollRect == null)
                    return 0;
                // return the velocity component depending on which direction this is scrolling
                return (scrollDirection == ScrollDirectionType.Vertical ? scrollRect.velocity.y : scrollRect.velocity.x);
            }
            set
            {
                if (scrollRect == null)
                    return;
                // set the appropriate component of the velocity
                if (scrollDirection == ScrollDirectionType.Vertical)
                    scrollRect.velocity = new Vector2(0, value);
                else scrollRect.velocity = new Vector2(value, 0);
            }
        }
        /// <summary>
        /// Whether the scroller is scrolling or not
        /// </summary>
        bool IsScrolling { get; set; }
        /// <summary>
        /// Whether the scroller is tweening or not
        /// </summary>
        bool IsTweening { get; set; }
        /// <summary>
        /// This is the first cell view index showing in the scroller's visible area
        /// </summary>
        int StartCellIndex { get; set; }
        /// <summary>
        /// This is the last cell view index showing in the scroller's visible area
        /// </summary>
        int EndCellIndex { get; set; }
        /// <summary>
        /// This is the first data index showing in the scroller's visible area
        /// </summary>
        int StartDataIndex => StartCellIndex % NumberOfCells;
        /// <summary>
        /// This is the last data index showing in the scroller's visible area
        /// </summary>
        int EndDataIndex => EndCellIndex % NumberOfCells;
        int NumberOfCells
        {
            get
            {
                if (CacheCustomData == null)
                    return 0;
                float temp = CacheCustomData.Count / (float)numOfGroupCell;
                return Mathf.CeilToInt(temp);
            }
        }
        /// <summary>
        /// Set after the scroller is first created. This allwos
        /// us to ignore OnValidate changes at the start
        /// </summary>
        bool initialized = false;
        /// <summary>
        /// Cached reference to the scrollRect's transform
        /// </summary>
        RectTransform scrollRectTransform;
        /// <summary>
        /// Cached reference to the layout group that handles view positioning
        /// </summary>
        HorizontalOrVerticalLayoutGroup layoutGroup;
        /// <summary>
        /// List of views that have been recycled
        /// </summary>
        SmallList<UControl> recycledCells = new SmallList<UControl>();
        SmallList<int> remainingCellIndices = new SmallList<int>();
        /// <summary>
        /// Cached reference to the element used to offset the first visible cell view
        /// </summary>
        LayoutElement firstPadder;
        /// <summary>
        /// Cached reference to the element used to keep the cell views at the correct size
        /// </summary>
        LayoutElement lastPadder;
        /// <summary>
        /// Cached reference to the container that holds the recycled cell views
        /// </summary>
        RectTransform recycledCellContainer;
        /// <summary>
        /// Internal list of cell view sizes. This is created when the data is reloaded 
        /// to speed up processing.
        /// </summary>
        SmallList<float> cellSizeArray = new SmallList<float>();
        /// <summary>
        /// Internal list of cell view offsets. Each cell view offset is an accumulation 
        /// of the offsets previous to it.
        /// This is created when the data is reloaded to speed up processing.
        /// </summary>
        SmallList<float> cellOffsetArray = new SmallList<float>();
        /// <summary>
        /// The list of cell views that are currently being displayed
        /// </summary>
        SmallList<UControl> activeCells = new SmallList<UControl>();
        /// <summary>
        /// The scrollers position
        /// </summary>
        float scrollPosition;
        /// <summary>
        /// The index of the first element of the middle section of cell view sizes.
        /// Used only when looping
        /// </summary>
        int loopFirstCellIndex;
        /// <summary>
        /// The index of the last element of the middle seciton of cell view sizes.
        /// used only when looping
        /// </summary>
        int loopLastCellIndex;
        /// <summary>
        /// The scroll position of the first element of the middle seciotn of cell views.
        /// Used only when looping
        /// </summary>
        float loopFirstScrollPosition;
        /// <summary>
        /// The scroll position of the last element of the middle section of cell views.
        /// Used only when looping
        /// </summary>
        float loopLastScrollPosition;
        /// <summary>
        /// The position that triggers the scroller to jump to the end of the middle section
        /// of cell views. This keeps the scroller in the middle section as much as possible.
        /// </summary>
        float loopFirstJumpTrigger;
        /// <summary>
        /// The position that triggers the scroller to jump to the start of the middle section
        /// of cell views. This keeps the scroller in the middle section as much as possible.
        /// </summary>
        float loopLastJumpTrigger;
        /// <summary>
        /// The cell view index we are snapping to
        /// </summary>
        int snapCellViewIndex;
        /// <summary>
        /// The data index we are snapping to
        /// </summary>
        int snapDataIndex;
        /// <summary>
        /// Whether we are currently jumping due to a snap
        /// </summary>
        bool snapJumping;
        /// <summary>
        /// What the previous inertia setting was before the snap jump.
        /// We cache it here because we need to turn off inertia while
        /// manually tweeing.
        /// </summary>
        bool snapInertia;
        /// <summary>
        /// The cached value of the last scrollbar visibility setting. This is checked every
        /// frame to see if the scrollbar visibility needs to be changed.
        /// </summary>
        ScrollbarVisibilityType lastScrollbarVisibility;
        #endregion

        #region private method
        //创建Item
        UControl CreateCell(UControl cellPrefab)
        {
            // no recyleable cell found, so we create a new view
            // and attach it to our container
            var go = Instantiate(cellPrefab.gameObject);
            go.name = cellPrefab.name;
            UControl cell = go.GetComponent<UControl>();
            cell.transform.SetParent(IContent);
            cell.transform.localPosition = Vector3.zero;
            cell.transform.localRotation = Quaternion.identity;
            AddCellChild(cell);
            return cell;
        }
        // 创建或者添加Item
        void AddCell(int cellIndex, ListPositionType listPosition, bool isRefreshCell)
        {
            if (NumberOfCells == 0) return;
            // get the dataIndex. Modulus is used in case of looping so that the first set of cells are ignored
            var dataIndex = cellIndex % NumberOfCells;
            // request a cell view from the delegate
            if (Prefab == null)
                throw new NotImplementedException("没有BasePrefab");
            // see if there is a view to recycle
            UControl cell = null;
            if (recycledCells.Count > 0)
                cell = recycledCells.RemoveAt(0);
            if (cell == null)
                cell = CreateCell(Prefab);
            // set the cell's properties
            cell.SetIndex(cellIndex);
            cell.SetDataIndex(dataIndex);
            //设置BaseCheckBox
            if (isRefreshCell) RefreshCell(cell);
            // add the cell view to the active container
            cell.transform.SetParent(IContent, false);
            cell.transform.localScale = Vector3.one;
            // add a layout element to the cellView
            if (cell.LayoutElement == null)
                cell.EnsureLayoutElement();
            LayoutElement layoutElement = cell.LayoutElement;
            // set the size of the layout element
            if (scrollDirection == ScrollDirectionType.Vertical)
            {
                layoutElement.minHeight = cellSizeArray[cellIndex] - (cellIndex > 0 ? layoutGroup.spacing : 0);
                layoutElement.minWidth = prefabRect.sizeDelta.x;
            }
            else
            {
                layoutElement.minWidth = cellSizeArray[cellIndex] - (cellIndex > 0 ? layoutGroup.spacing : 0);
                layoutElement.minHeight = prefabRect.sizeDelta.y;
            }
            // add the cell to the active list
            if (listPosition == ListPositionType.First)
                activeCells.AddStart(cell);
            else
                activeCells.Add(cell);
            // set the hierarchy position of the cell view in the container
            if (listPosition == ListPositionType.Last)
                cell.transform.SetSiblingIndex(IContent.childCount - 2);
            else if (listPosition == ListPositionType.First)
                cell.transform.SetSiblingIndex(1);
            // call the visibility change delegate if available
            Data?.OnCellVisibilityChanged?.Invoke(cell);
        }
        //回收Item
        void RecycleCell(UControl cell)
        {
            // remove the cell view from the active list
            activeCells.Remove(cell);
            // add the cell view to the recycled list
            recycledCells.Add(cell);
            // move the GameObject to the recycled container
            cell.transform.SetParent(recycledCellContainer);
            // reset the cellView's properties
            cell.SetDataIndex(0);
            cell.SetIndex(0);
        }
        /// <summary>
        /// Gets the index of a cell at a given position based on a subset range.
        /// This function uses a recursive binary sort to find the index faster.
        /// </summary>
        /// <param name="position">The pixel offset from the start of the scroller</param>
        /// <param name="startIndex">The first index of the range</param>
        /// <param name="endIndex">The last index of the rnage</param>
        /// <returns></returns>
        int GetCellIndexAtPosition(float position, int startIndex, int endIndex)
        {
            // if the range is invalid, then we found our index, return the start index
            if (startIndex >= endIndex) return startIndex;
            // determine the middle point of our binary search
            var middleIndex = (startIndex + endIndex) / 2;
            // if the middle index is greater than the position, then search the last
            // half of the binary tree, else search the first half
            if ((cellOffsetArray[middleIndex] + (scrollDirection == ScrollDirectionType.Vertical ? padding.top : padding.left)) >= position)
                return GetCellIndexAtPosition(position, startIndex, middleIndex);
            else
                return GetCellIndexAtPosition(position, middleIndex + 1, endIndex);
        }
        /// <summary>
        /// Handler for when the scroller changes value
        /// </summary>
        /// <param name="val">The scroll rect's value</param>
        void OnScrollRectValueChanged(Vector2 val)
        {
            // set the internal scroll position
            if (scrollDirection == ScrollDirectionType.Vertical)
                scrollPosition = (1f - val.y) * ScrollSize;
            else scrollPosition = val.x * ScrollSize;
            scrollPosition = Mathf.Clamp(scrollPosition, 0, GetScrollPositionForCellIndex(cellSizeArray.Count - 1, CellViewPositionType.Before));
            // if the snapping is turned on, handle it
            if (snapping && !snapJumping)
            {
                // if the speed has dropped below the threshhold velocity
                if (Mathf.Abs(LinearVelocity) <= snapVelocityThreshold && LinearVelocity != 0)
                {
                    // Make sure the scroller is not on the boundary if not looping
                    var normalized = NormalizedScrollPosition;
                    if (loop || (!loop && normalized > 0 && normalized < 1.0f))
                    {
                        // Call the snap function
                        if (NumberOfCells != 0)
                        {
                            // set snap jumping to true so other events won't process while tweening
                            snapJumping = true;
                            // stop the scroller
                            LinearVelocity = 0;
                            // cache the current inertia state and turn off inertia
                            snapInertia = scrollRect.inertia;
                            scrollRect.inertia = false;
                            // calculate the snap position
                            var snapPosition = ScrollPosition + (ScrollRectSize * Mathf.Clamp01(snapWatchOffset));
                            // get the cell view index of cell at the watch location
                            snapCellViewIndex = GetCellViewIndexAtPosition(snapPosition);
                            // get the data index of the cell at the watch location
                            snapDataIndex = snapCellViewIndex % NumberOfCells;
                            // jump the snapped cell to the jump offset location and center it on the cell offset
                            JumpToDataIndex(snapDataIndex, snapJumpToOffset, snapCellCenterOffset, snapUseCellSpacing, snapTweenType, snapTweenTime, null);
                        }
                    }
                }
            }
            // if looping, check to see if we scrolled past a trigger
            if (loop)
            {
                var velocity = Vector2.zero;
                if (scrollPosition < loopFirstJumpTrigger)
                {
                    velocity = scrollRect.velocity;
                    ScrollPosition = loopLastScrollPosition - (loopFirstJumpTrigger - scrollPosition) + spacing;
                    scrollRect.velocity = velocity;
                }
                else if (scrollPosition > loopLastJumpTrigger)
                {
                    velocity = scrollRect.velocity;
                    ScrollPosition = loopFirstScrollPosition + (scrollPosition - loopLastJumpTrigger) - spacing;
                    scrollRect.velocity = velocity;
                }
            }
            ResetVisibleCells(true);
        }
        /// <summary>
        /// Gets the scroll position in pixels from the start of the scroller based on the cellViewIndex
        /// </summary>
        /// <param name="cellViewIndex">The cell index to look for. This is used instead of dataIndex in case of looping</param>
        /// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
        /// <returns></returns>
        float GetScrollPositionForCellIndex(int cellViewIndex, CellViewPositionType insertPosition)
        {
            if (NumberOfCells == 0) return 0;
            if (cellViewIndex < 0) cellViewIndex = 0;

            if (cellViewIndex == 0 && insertPosition == CellViewPositionType.Before)
            {
                return 0;
            }
            else
            {
                if (cellViewIndex < cellOffsetArray.Count)
                {
                    // the index is in the range of cell view offsets
                    if (insertPosition == CellViewPositionType.Before)
                    {
                        // return the previous cell view's offset + the spacing between cell views
                        return cellOffsetArray[cellViewIndex - 1] + spacing + (scrollDirection == ScrollDirectionType.Vertical ? padding.top : padding.left);
                    }
                    else
                    {
                        // return the offset of the cell view (offset is after the cell)
                        return cellOffsetArray[cellViewIndex] + (scrollDirection == ScrollDirectionType.Vertical ? padding.top : padding.left);
                    }
                }
                else
                {
                    // get the start position of the last cell (the offset of the second to last cell)
                    return cellOffsetArray[cellOffsetArray.Count - 2];
                }
            }
        }
        /// <summary>
        /// Gets the index of a cell view at a given position
        /// </summary>
        /// <param name="position">The pixel offset from the start of the scroller</param>
        /// call the overrloaded method on the entire range of the list
        /// <returns></returns>
        int GetCellViewIndexAtPosition(float position)=>GetCellIndexAtPosition(position, 0, cellOffsetArray.Count - 1);
        #endregion

        #region Tweening
        float _tweenTimeLeft;
        object _tweenMove;
        /// <summary>
        /// Moves the scroll position over time between two points given an easing function. When the
        /// tween is complete it will fire the jumpComplete delegate.
        /// </summary>
        /// <param name="tweenType">The type of easing to use</param>
        /// <param name="time">The amount of time to interpolate</param>
        /// <param name="start">The starting scroll position</param>
        /// <param name="end">The ending scroll position</param>
        /// <param name="jumpComplete">The action to fire when the tween is complete</param>
        /// <returns></returns>
        IEnumerator _tweenPosition(TweenType tweenType, float time, float start, float end, Action tweenComplete)
        {
            if (tweenType == TweenType.immediate || time == 0)
            {
                // if the easing is immediate or the time is zero, just jump to the end position
                ScrollPosition = end;
            }
            else
            {
                // zero out the velocity
                scrollRect.velocity = Vector2.zero;
                // fire the delegate for the tween start
                IsTweening = true;
                _tweenTimeLeft = 0;
                var newPosition = 0f;
                // while the tween has time left, use an easing function
                while (_tweenTimeLeft < time)
                {
                    newPosition = MathUtil.Tween(tweenType, start, end, (_tweenTimeLeft / time));
                    if (loop)
                    {
                        // if we are looping, we need to make sure the new position isn't past the jump trigger.
                        // if it is we need to reset back to the jump position on the other side of the area.
                        if (end > start && newPosition > loopLastJumpTrigger)
                        {
                            //Debug.Log("name: " + name + " went past the last jump trigger, looping back around");
                            newPosition = loopFirstScrollPosition + (newPosition - loopLastJumpTrigger);
                        }
                        else if (start > end && newPosition < loopFirstJumpTrigger)
                        {
                            //Debug.Log("name: " + name + " went past the first jump trigger, looping back around");
                            newPosition = loopLastScrollPosition - (loopFirstJumpTrigger - newPosition);
                        }
                    }

                    // set the scroll position to the tweened position
                    ScrollPosition = newPosition;
                    // increase the time elapsed
                    _tweenTimeLeft += Time.unscaledDeltaTime;
                    yield return null;
                }
                // the time has expired, so we make sure the final scroll position
                // is the actual end position.
                ScrollPosition = end;
            }
            // the tween jump is complete, so we fire the delegate
            tweenComplete?.Invoke();
            // fire the delegate for the tween ending
            IsTweening = false;
        }
        #endregion

        #region enum
        public enum LoopJumpDirectionType
        {
            Closest,
            Up,
            Down
        }
        /// <summary>
        /// The direction this scroller is handling
        /// </summary>
        public enum ScrollDirectionType
        {
            Vertical,
            Horizontal
        }
        /// <summary>
        /// Which side of a cell to reference.
        /// For vertical scrollers, before means above, after means below.
        /// For horizontal scrollers, before means to left of, after means to the right of.
        /// </summary>
        public enum CellViewPositionType
        {
            Before,
            After
        }
        /// <summary>
        /// This will set how the scroll bar should be shown based on the data. If no scrollbar
        /// is attached, then this is ignored. OnlyIfNeeded will hide the scrollbar based on whether
        /// the scroller is looping or there aren't enough items to scroll.
        /// </summary>
        public enum ScrollbarVisibilityType
        {
            OnlyIfNeeded,
            Always,
            Never
        }
        /// <summary>
        /// Where in the list we are
        /// </summary>
        private enum ListPositionType
        {
            First,
            Last
        }
        #endregion
    }
}