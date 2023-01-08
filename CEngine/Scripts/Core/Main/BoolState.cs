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
    /// 布尔值处理器
    /// </summary>
    [Serializable, Unobfus]
    public class BoolState
    {
        bool isIn = false;
        int count = 0;
        public bool Push(bool b)
        {
            if (b) Add();
            else Remove();
            return IsIn();
        }
        public void Reset()
        {
            count = 0;
        }
        public void Add()
        {
            count++;
        }
        public void Remove()
        {
            count--;
            count = Mathf.Clamp(count, 0, int.MaxValue);
        }
        public bool IsIn()
        {
            return count > 0 || isIn;
        }
        public void Set(bool b)
        {
            isIn = b;
        }
        public void Toggle()
        {
            Set(!IsIn());
        }
    }
}
