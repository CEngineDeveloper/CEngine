//------------------------------------------------------------------------------
// UPieImage.cs
// Copyright 2021 2021/3/13 
// Created by CYM on 2021/3/13
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class UPieImageData : UData
    {
        public Func<List<float>> Value;
    }
    [HideMonoScript,AddComponentMenu("UI/Control/UPieImage")]
    public class UPieImage : UPres<UPieImageData>
    {
        [SerializeField, ChildGameObjectsOnly]
        Image[] IImages;

        [SerializeField, ChildGameObjectsOnly]
        Text[] IVals;

        public override bool IsAtom => true;
        public override void Refresh()
        {
            base.Refresh();
            if (Data.Value != null)
            {
                var vals = Data.Value.Invoke();
                Refresh(vals);
            }
        }
        public void Refresh(List<float> vals)
        {
            if (vals.Count != IImages.Length)
                return;
            if (IImages.Length < 2)
                return;
            float total = 0;
            for (int i = 0; i < vals.Count; ++i)
            {
                int index = vals.Count - 1 - i;
                total += vals[index];
                IImages[index].fillAmount = total;
            }
            if (total < 1.0f)
            {
                CLog.Error("错误！UPieImage：{0} 所有数值的总和必须为1",GOName);
            }
        }
    }
}