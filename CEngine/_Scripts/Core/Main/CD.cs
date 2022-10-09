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
    /// CD类,减少Count
    /// </summary>
    [Serializable, Unobfus]
    public class CD : Queue
    {
        public CD() { }
        public CD(float time, bool isFinish = false) : base(time, isFinish)
        {
        }
        public override void Update(float step = 1)
        {
            if (IsFinish) return;
            CurCount -= step;
            CurCount = Mathf.Clamp(CurCount, 0, TotalCout);
        }
        public override bool IsOver()
        {
            if (IsFinish) return true;
            return CurCount <= 0;
        }
        public override void Reset(float? count = null)
        {
            IsFinish = false;
            if (count.HasValue)
            {
                CurCount = count.Value;
                TotalCout = count.Value;
            }
            else
            {
                CurCount = ResetCount;
                TotalCout = ResetCount;
            }
        }
        public override void ForceOver()
        {
            CurCount = 0;
        }
        public override float GetRemainder()
        {
            return CurCount;
        }
    }
}
