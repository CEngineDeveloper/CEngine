//------------------------------------------------------------------------------
// TDBaseArticle.cs
// Created by CYM on 2023/1/20
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using System;

namespace CYM.Diplomacy
{
    public enum ArticleDescType
    {
        Normal,
        Dialog,
        Hint,
    }
    public enum ArticleObjType
    {
        Self,
        Target,
    }
    public enum ArticleType
    {
        Give,
        Obtain,
    }
    [Serializable]
    public class TDBaseArticleData : TDBaseData
    {
        #region config
        public ArticleType Type { get; set; } = ArticleType.Give;
        public float Float1 { get; set; }
        public float Float2 { get; set; }
        public float Float3 { get; set; }
        public int Int1 { get; set; }
        public int Int2 { get; set; }
        public int Int3 { get; set; }
        public long Long1 { get; set; }
        public long Long2 { get; set; }
        public long Long3 { get; set; }
        public string Str1 { get; set; }
        public string Str2 { get; set; }
        public string Str3 { get; set; }
        public bool Bool1 { get; set; }
        public bool Bool2 { get; set; }
        public bool Bool3 { get; set; }
        //是否可以出现多个:比如割地条款可以多个出现,但是赔款,承认失败则只有一个
        public bool IsCanMultiple { get; set; } = false;
        //是否为双方互斥,比如赔款,承认失败都是互斥
        public bool IsMutex { get; set; } = false;
        public CYM.Range ValueRange { get; set; } = new CYM.Range(100, 1500);
        public ArticleObjType ArticleObjType { get; set; } = ArticleObjType.Self;
        #endregion

        #region get
        public Func<TDBaseArticleData, int> GetScore = (x) => 10;
        public Func<BaseUnit, BaseUnit, bool> IsShow = (self, target) => true;
        #endregion

        #region set
        public void ChangeValue(float val)
        {
            if (GetValue == null || SetValue == null)
            {
                CLog.Error("GetValue和SetValue 为null");
                return;
            }
            var ret = Mathf.Clamp(GetValue(this) + val, ValueRange.Min, ValueRange.Max);
            SetValue(this, ret);
        }
        #endregion

        #region get
        public int WarpScore(int val)
        {
            if (ArticleObjType == ArticleObjType.Self) return val;
            else if (ArticleObjType == ArticleObjType.Target) return -val;
            throw new Exception("无效的类型！！！");
        }
        #endregion

        #region is
        public bool IsSame(TDBaseArticleData other)
        {
            return
                TDID == other.TDID &&
                Type == other.Type &&

                Float1 == other.Float1 &&
                Float2 == other.Float2 &&
                Float3 == other.Float3 &&

                Int1 == other.Int1 &&
                Int2 == other.Int2 &&
                Int3 == other.Int3 &&

                Str1 == other.Str1 &&
                Str2 == other.Str2 &&
                Str3 == other.Str3 &&

                Bool1 == other.Bool1 &&
                Bool2 == other.Bool2 &&
                Bool3 == other.Bool3 &&

                Long1 == other.Long1 &&
                Long2 == other.Long2 &&
                Long3 == other.Long3 &&

                ArticleObjType == other.ArticleObjType;
        }
        #endregion

        #region Callback
        public Func<TDBaseArticleData, ArticleDescType, string> GetTitleDesc;
        public Func<TDBaseArticleData, float> GetValue;
        public Func<TDBaseArticleData, float> GetValueStep;
        public Callback<TDBaseArticleData> OnExecute;
        public Callback<TDBaseArticleData, float> SetValue;
        #endregion

        #region prop
        //来源
        public BaseUnit BaseTarget { get; set; }
        public bool IsShowValCtrl => SetValue != null;
        #endregion
    }
}