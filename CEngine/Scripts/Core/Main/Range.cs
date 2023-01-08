using Sirenix.OdinInspector;
using System;
using UnityEngine;
//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2020-7-16
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
// Desc     ：此代码由陈宜明于2020年编写,版权归陈宜明所有
// Copyright (c) 2020 陈宜明 All rights reserved.
namespace CYM
{
    /// <summary>
    /// 范围值,不包含Max,val >= min, val < max;
    /// </summary>
    [Serializable, Unobfus, HideReferenceObjectPicker]
    public class Range
    {
        public float Min;
        public float Max;
        public float Sum => Min + Max;
        public float Length => Max - Min;

        public Range(float min, float max)
        {
            this.Min = min;
            this.Max = max;
        }

        public bool IsIn(float val)=> val >= Min && val < Max;
        public float Clamp(float val)=> Mathf.Clamp(val, Min, Max);
        public float Rand()=> UnityEngine.Random.Range(Min, Max);
        public float Tolerance()=> Mathf.Clamp(Max - Min, 0, float.MaxValue);
    }
}
