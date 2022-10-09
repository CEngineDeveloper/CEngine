//------------------------------------------------------------------------------
// BaseTable.cs
// Copyright 2018 2018/10/28 
// Created by CYM on 2018/10/28
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace CYM.UI
{
    public class UTableData : UScrollData
    {
        public Callback<int> OnTitleClick;
        public UCheckData[] TitleDatas;
    }
    [AddComponentMenu("UI/Control/UTable")]
    [HideMonoScript]
    public class UTable : UPres<UTableData>
    {
        #region inspector
        [FoldoutGroup("Inspector"), SerializeField, Required, ChildGameObjectsOnly, SceneObjectsOnly]
        UDupplicate IDP;
        [FoldoutGroup("Inspector"), SerializeField, Required, ChildGameObjectsOnly, SceneObjectsOnly]
        UScroll IScroll;
        #endregion

        #region prop
        UControl[] Titles;
        #endregion

        #region life
        #region life
        protected override void FetchSubControls()
        {
            if (IDP) AddStaticChild(IDP);
            if (IScroll) AddStaticChild(IScroll);
        }
        #endregion

        #endregion

        #region set
        public void Init(Func<IList> getData, Callback<object, object> onRefresh, List<UCheckData> titleBnts, Callback<UControl> onItemClick = null, Callback<int> onTitleClick = null)
        {
            Init(new UTableData
            {
                GetCustomDatas = getData,
                OnRefreshItem = onRefresh,
                TitleDatas = titleBnts.ToArray(),
                Sorter = GetSorters(titleBnts),
                OnSelectItem = onItemClick,
                OnTitleClick = onTitleClick,
            });
        }
        // 初始化Table menu
        public override void Init(UTableData tableData)
        {
            base.Init(tableData);

            if (IDP == null) CLog.Error("没有BaseDupplicate组件");
            if (IScroll == null) CLog.Error("没有BaseScroll组件");
            if (Data.GetCustomDatas == null) CLog.Error("TableData 的 GetCustomDatas 必须设置");
            if (Data.OnRefreshItem == null) CLog.Error("TableData 的 OnRefresh 必须设置");

            IScroll.Init(tableData);
            Titles = IDP.Init(Data.TitleDatas);
            foreach (var item in Titles)
            {
                if(item is UCheck checkBox)
                    checkBox.Data.OnClick += OnBntClick;
            }
        }
        #endregion

        #region Callback
        void OnBntClick(UControl presenter, PointerEventData data)
        {
            Data?.OnTitleClick?.Invoke(presenter.Index);
            IScroll.SortData(presenter.Index, false);
            SetDirtyData();
        }
        #endregion

        #region set
        public void SetSelectData(object data)
        {
            IScroll.SelectData(data);
        }
        public void SetSelectData(int index)
        {
            IScroll.SelectData(index);
        }
        #endregion

        #region get
        List<Func<object, object>> GetSorters(List<UCheckData> datas)
        {
            List<Func<object, object>> ret = new List<Func<object, object>>();
            if (datas == null)
                return ret;
            foreach (var item in datas)
            {
                ret.Add(item.OnSorter);
            }
            return ret;
        }
        public T GetData<T>(int dataIndex) where T : class,new()
        {
            return IScroll.GetData<T>(dataIndex);
        }
        #endregion
    }
}