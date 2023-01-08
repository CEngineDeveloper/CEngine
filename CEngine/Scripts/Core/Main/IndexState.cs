using System;
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
    /// 索引值处理器
    /// </summary>
    [Serializable, Unobfus]
    public class IndexState
    {
        public IndexState(int max)
        {
            maxCount = max;
        }

        int preCount = 0;
        int count = 0;
        int maxCount = 3;
        public int Reset(int index = 0)
        {
            preCount = count;
            count = index;
            return count;
        }
        public int Add()
        {
            int ret = count;
            preCount = ret;
            count++;
            if (count > maxCount)
                count = 0;
            return ret;
        }
        public int Added()
        {
            preCount = count;
            count++;
            if (count > maxCount)
                count = 0;
            return count;
        }
        public int Remove()
        {
            int ret = count;
            preCount = ret;
            count--;
            if (count < 0)
                count = maxCount;
            return ret;
        }
        public int Cur()
        {
            return count;
        }
        public int Pre()
        {
            return preCount;
        }
        public bool IsMaxCount()
        {
            return count >= maxCount;
        }
        public bool IsPreMaxCount()
        {
            return preCount >= maxCount;
        }
    }
}
