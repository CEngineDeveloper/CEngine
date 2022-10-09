//------------------------------------------------------------------------------
// UScrollRow.cs
// Copyright 2021 2021/2/24 
// Created by CYM on 2021/2/24
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CYM
{
    [AddComponentMenu("UI/Control/UScrollGroup")]
    [HideMonoScript]
    public class UScrollGroup : UControl 
    {
        public override bool IsCollection => true;
        public override bool IsCanBeControlFetch => false;
        public override bool IsCanBeViewFetch => false;
        public override bool IsAutoInit => true;

        [SerializeField,HideInInspector]
        public List<UControl> Cells = new List<UControl>();

        public int Count
        {
            get
            {
                if (Cells == null)
                    return 0;
                return Cells.Count;
            }
        }

        protected override void FetchSubControls()
        {
            //do nothing....
        }

        public override void SetIndex(int i)
        {
            base.SetIndex(i);
            int startIndex = i * Count;
            Util.Foreach(Cells,(index,item)=> {
                item.SetIndex(startIndex+index);
                item.PDupplicate = PDupplicate;
            });
        }
        public override void SetDataIndex(int i)
        {
            base.SetDataIndex(i);
            int startIndex = i * Count;
            Util.Foreach(Cells, (index, item) => {
                item.SetDataIndex(startIndex + index);
                item.PScroll = PScroll;
            });
        }

        public void FetchChildCell()
        {
            Cells.Clear();
            for (int i = 0; i < Trans.childCount; ++i)
            {
                var control = Trans.GetChild(i).GetComponent<UControl>() ;
                if(control!=null)
                    Cells.Add(control);
            }
        }

        [Button("统一名称")]
        public void ModifyName()
        {
            Util.UnifyChildName(GO);
        }
    }
}