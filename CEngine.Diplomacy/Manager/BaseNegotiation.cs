//------------------------------------------------------------------------------
// BaseNegotiation.cs
// Copyright 2019 2019/7/15 
// Created by CYM on 2019/7/15
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CYM.Diplomacy
{
    public class BaseDipPoint<TUnit, TWar> where TUnit : BaseUnit where TWar : BaseWarfareData<TUnit>
    {
        #region prop
        protected BaseGlobal SelfBaseGlobal = BaseGlobal.Ins;
        public int Score { get; private set; } = 0;
        float scaler = 1;
        protected TUnit Self, Target;
        protected TWar WarData;
        #endregion

        #region pub
        public virtual bool IsShowWhenZero => false;
        public BaseDipPoint(float scaler) { this.scaler = scaler; }
        public BaseDipPoint() { }
        public void SetParam(TUnit self, TUnit target, TWar warData)
        {
            Self = self;
            Target = target;
            WarData = warData;
            if (Target == null) Target = Self;
        }
        public int Calc()
        {
            Score = Mathf.RoundToInt(scaler * CalcImpl());
            return Score;
        }
        public string GetDesc()
        {
            return SetStr() + ": " + UIUtil.CS(Score);
        }
        #endregion

        #region protect
        protected virtual float CalcImpl() => 0;
        protected virtual string SetStr() => "";
        protected string GetStr(string key, params object[] ps) => BaseLangMgr.Get(key, ps);
        // p 倍数
        // clamp 倍数clamp
        // p==1时，返回0
        // p==e^-clamp时，返回-100
        // p==e^clamp时返回100
        protected static float DivideToScore(float p, float clamp = 1,float mul=100)
        {
            return Mathf.Clamp(Mathf.Log(p), -clamp, clamp) * mul;
        }
        #endregion
    }
    public class BaseNegotiation<TUnit, TWarData> where TUnit : BaseUnit where TWarData : BaseWarfareData<TUnit>
    {
        public class DPArticle : BaseDipPoint<TUnit, TWarData>
        {
            public DPArticle(float v) : base(v) { }
            public override bool IsShowWhenZero => true;
            protected override float CalcImpl() => BaseGlobal.ArticleMgr.CalcArticleScore();
            protected override string SetStr() => GetStr("DipProp_合约附加");
        }

        //自身无法添加的条款
        public HashList<string> SelfExcludeArticle { get; private set; } = new HashList<string>();
        //对方无法添加的条款
        public HashList<string> TargetExcludeArticle { get; private set; } = new HashList<string>();
        public List<BaseDipPoint<TUnit, TWarData>> DipPoints { get; private set; } = new List<BaseDipPoint<TUnit, TWarData>>();
        public int PositiveScore { get; private set; }
        public int NegativeScore { get; private set; }
        public List<TDBaseArticleData> SelfArticle { get; private set; }
        public List<TDBaseArticleData> TargetArticle { get; private set; }
        public BaseNegotiation(HashList<string> selfExcludeArticle, HashList<string> targetExcludeArticle, params BaseDipPoint<TUnit, TWarData>[] ps)
        {
            SelfExcludeArticle = selfExcludeArticle;
            TargetExcludeArticle = targetExcludeArticle;
            foreach (var item in ps)
                DipPoints.Add(item);
            DipPoints.Add(new DPArticle(1.0f));
        }
        public BaseNegotiation<TUnit, TWarData> SetParam(TUnit self, TUnit target, TWarData warData, List<TDBaseArticleData> selfArticle, List<TDBaseArticleData> targetArticle)
        {
            SelfArticle = selfArticle;
            TargetArticle = targetArticle;           
            foreach (var item in DipPoints)
            {
                item.SetParam(self, target, warData);
            }
            return this;
        }
        // 获得概率
        public float CalcProb()
        {
            PositiveScore = 0;
            NegativeScore = 0;
            foreach (var point in DipPoints)
            {
                point.Calc();
                if (point.Score > 0) PositiveScore += point.Score;
                if (point.Score < 0) NegativeScore += point.Score;
            }
            foreach (var item in SelfArticle) PositiveScore += item.GetScore(item);
            foreach (var item in TargetArticle) NegativeScore += -item.GetScore(item);
            return MathUtil.Auncel(PositiveScore, NegativeScore);
        }

        //获得提示
        public string CalcHintStr()
        {
            var prop = CalcProb();
            string addtionStr = "";
            string positiveStr = GetPositiveStrJoin();
            string negativeStr = GetNegativeStrJoin();
            if (!positiveStr.IsInv()) addtionStr = positiveStr + "\n";
            if (!negativeStr.IsInv()) addtionStr += negativeStr + "\n";
            addtionStr = addtionStr.TrimEnd("\n");

            return
                Util.GetStr("Hint_DipProp", IsAgree ? Util.GetStr("Text_Dip_可能会") : Util.GetStr("Text_Dip_绝对不会"),
                UIUtil.Per(prop),
                UIUtil.CS(PositiveScore),
                UIUtil.CS(NegativeScore),
                UIUtil.CS(TotalScore),
                addtionStr);

            // 拼接字符串
            string GetStrJoin(IEnumerable<BaseDipPoint<TUnit, TWarData>> points) => string.Join("\n", points.Select(p => p.GetDesc()).ToArray());
            // 获得正面效果
            string GetPositiveStrJoin() => GetStrJoin(DipPoints.Where(p => p.Score > 0 || p.Score == 0 && p.IsShowWhenZero));
            // 获得负面效果
            string GetNegativeStrJoin() => GetStrJoin(DipPoints.Where(p => p.Score < 0));
        }
        public HashList<string> GetExcludeArticle(ArticleObjType target)
        {
            if (target == ArticleObjType.Self)
                return SelfExcludeArticle;
            return TargetExcludeArticle;
        }

        //是否同意
        bool IsAgree => PositiveScore + NegativeScore > 0;
        //总分
        int TotalScore => PositiveScore + NegativeScore;
    }
}