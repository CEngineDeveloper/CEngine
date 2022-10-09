using System;
using System.Runtime.Serialization;
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
    /// 队列基类,增加Count
    /// </summary>
    [Serializable, Unobfus]
    public class Queue: IDeserializationCallback, ISerializable
    {
        #region val
        public float ResetCount { get; protected set; } = 0;
        public float CurCount { get; protected set; }
        public float TotalCout { get; protected set; }
        public bool IsFinish { get; protected set; } = false;
        #endregion

        public Queue()
        {
        }
        public Queue(float time, bool isFinish = false)
        {
            ResetCount = time;
            Reset(time);
            IsFinish = isFinish;
        }
        public virtual void Update(float step)
        {
            if (IsFinish) return;
            CurCount += step;
        }
        public virtual bool IsOver()
        {
            if (IsFinish) return true;
            return CurCount >= TotalCout;
        }
        public float Progress => CurCount / TotalCout;
        public virtual void Reset(float? count = null)
        {
            CurCount = 0;
            IsFinish = false;
            if (count.HasValue) TotalCout = count.Value;
            else TotalCout = ResetCount;
        }
        public void SetTotalCount(float count)
        {
            TotalCout = count;
        }
        public void SetCurCount(float count)
        {
            CurCount = count;
        }
        public void SetResetCount(float count)
        {
            ResetCount = count;
        }
        public void SetIsFinish(bool b)
        {
            IsFinish = b;
        }
        public virtual void ForceOver()
        {
            CurCount = TotalCout;
        }
        public bool CheckOver()
        {
            if (IsOver())
            {
                Reset();
                return true;
            }
            return false;
        }
        public virtual float GetRemainder()
        {
            return TotalCout - CurCount;
        }
        //结束
        public void Finish()
        {
            if (!IsFinish)
            {
                IsFinish = true;
            }
        }
        //检查是否结束,结束后立马Finish,调用Reset之前,不会再次进入
        public bool CheckOverOnce()
        {
            if (IsFinish) return false;
            if (IsOver())
            {
                Finish();
                return true;
            }
            return false;
        }

        #region data
        public Queue(SerializationInfo info, StreamingContext context)
        {
            ResetCount = (int)info.GetValue("ResetCount", typeof(int));
            CurCount = (int)info.GetValue("CurCount", typeof(int));
            TotalCout = (int)info.GetValue("TotalCout", typeof(int));
            IsFinish = (bool)info.GetValue("IsFinish", typeof(bool));
        }
        void IDeserializationCallback.OnDeserialization(object sender)
        {
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ResetCount", ResetCount);
            info.AddValue("CurCount", CurCount);
            info.AddValue("TotalCout", TotalCout);
            info.AddValue("IsFinish", IsFinish);
        }
        #endregion
    }
}
