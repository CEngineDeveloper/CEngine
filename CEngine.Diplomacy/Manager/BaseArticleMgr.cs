//------------------------------------------------------------------------------
// BaseArticleMgr.cs
// Copyright 2019 2019/7/28 
// Created by CYM on 2019/7/28
// Owner: CYM
// 外交条约模拟器
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
namespace CYM.Diplomacy
{
    public interface IAlertArticleData: ITDBaseData
    {
        //Self条款
        List<TDBaseArticleData> SelfArticle { get; set; }
        //Target条款
        List<TDBaseArticleData> TargetArticle { get; set; }
        //战争
        public IWarfareData WarfareData { get; set; }
    }
    public interface IArticleMgr<out TOut>
    {
        //我方的协议
        List<TDBaseArticleData> GetTempBaseSelfArticlies();
        //对方的协议
        List<TDBaseArticleData> GetTempBaseTargetArticlies();
        int CalcArticleScore();
        void RemoveArticle(long id);
        void PushNagotiationToAlert(IAlertArticleData data);
        bool IsStarNegotiation { get; }
        TOut Get(long id);
    }
    public class BaseArticleMgr<TUnit, TWarData, TArticleData> : BaseGFlowMgr, IArticleMgr<TArticleData>, IDBListConverMgr<DBBaseArticle>
        where TUnit : BaseUnit
        where TWarData : BaseWarfareData<TUnit>
        where TArticleData : TDBaseArticleData, new()
    {
        #region event
        public event Callback<TArticleData> Callback_OnAddArticle;
        public event Callback<TArticleData> Callback_OnRemoveArticle;
        #endregion

        #region prop
        //谈判模板
        readonly Dictionary<string, BaseNegotiation<TUnit, TWarData>> NegotiationTemps = new Dictionary<string, BaseNegotiation<TUnit, TWarData>>();
        //我方的协议
        public List<TDBaseArticleData> BaseTempSelfArticlies { get; protected set; } = new List<TDBaseArticleData>();
        public List<TArticleData> TempSelfArticlies { get; protected set; } = new List<TArticleData>();
        //对方的协议
        public List<TDBaseArticleData> BaseTempTargetArticlies { get; protected set; } = new List<TDBaseArticleData>();
        public List<TArticleData> TempTargetArticlies { get; protected set; } = new List<TArticleData>();
        //最终确认后的协议
        public IDDicList<TArticleData> Datas { get; protected set; } = new IDDicList<TArticleData>();
        //当前的谈判类型
        public string CurNegotiationType { get; protected set; } = SysConst.STR_Inv;
        //当前的谈判模板
        public BaseNegotiation<TUnit, TWarData> CurNegotiation { get; protected set; }
        //是否调用了StarNegotiation,任何谈判都必须以StarNegotiation开始
        public bool IsStarNegotiation { get; private set; } = false;
        //我方谈判对象
        public TUnit SelfArticleUnit { get; private set; } = null;
        //对方谈判对象
        public TUnit TargetArticleUnit { get; private set; } = null;
        //当前的战争对象
        public TWarData CurWarData { get; private set; } = null;
        ITDConfig ITDConfig { get; set; }
        #endregion

        #region life
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TArticleData));
        }
        #endregion

        #region set
        //开始新的谈判
        public BaseArticleMgr<TUnit, TWarData, TArticleData> StarNegotiation(string negotiationID, TUnit self, TUnit target, TWarData warData)
        {
            clearNagotiation();
            IsStarNegotiation = true;
            CurNegotiationType = negotiationID;
            CurNegotiation = GetNegotiation(negotiationID);
            SelfArticleUnit = self;
            TargetArticleUnit = target;
            CurWarData = warData;
            return this;
        }
        public BaseArticleMgr<TUnit, TWarData, TArticleData> SetWar(TWarData warData)
        {
            CurWarData = warData;
            return this;
        }
        //创建谈判模板
        public void CreateNegotiationTemp(string id, HashList<string> selfExcludeArticle, HashList<string> targetExcludeArticle, params BaseDipPoint<TUnit, TWarData>[] ps)
        {
            if (NegotiationTemps.ContainsKey(id)) return;
            NegotiationTemps.Add(id, new BaseNegotiation<TUnit, TWarData>(selfExcludeArticle, targetExcludeArticle, ps));
        }
        //推送谈判内容到Alert,用于给对方发送信息
        public void PushNagotiationToAlert(IAlertArticleData data)
        {
            if (!IsStarNegotiation)
            {
                CLog.Error("AddArticle 没有调用StarNegotiation");
                return;
            }
            data.SelfArticle.Clear();
            data.TargetArticle.Clear();
            foreach (var item in TempSelfArticlies)
            {
                item.ID = IDUtil.Gen();
                item.BaseTarget = TargetArticleUnit;
                item.OnBeAdded(SelfArticleUnit);
                Datas.Add(item);
                data.SelfArticle.Add(item);
            }
            foreach (var item in TempTargetArticlies)
            {
                item.ID = IDUtil.Gen();
                item.BaseTarget = SelfArticleUnit;
                item.OnBeAdded(TargetArticleUnit);
                Datas.Add(item);
                data.TargetArticle.Add(item);
            }
            data.WarfareData = CurWarData;
            clearNagotiation();
        }
        public void SetArticle(string articleID, ArticleObjType target, Callback<TArticleData> action)
        {
            if (IsContainArticle(target, articleID))
            {
                RemoveArticle(target, articleID);
            }
            AddArticle(articleID, target, action);
        }
        public void AddArticle(string articleID, ArticleObjType targetType, Callback<TArticleData> action)
        {
            if (!IsStarNegotiation)
            {
                CLog.Error("AddArticle 没有调用StarNegotiation");
                return;
            }
            TArticleData data = ITDConfig.Get<TArticleData>(articleID);
            if (data == null) return;
            if (!IsCanAddArticle(targetType, data)) return;
            var newData = data.Copy<TArticleData>();
            action?.Invoke(newData);
            if (IsRepeate(targetType, newData)) return;

            if (targetType == ArticleObjType.Self)
            {
                newData.ArticleObjType = targetType;
                newData.SetSelfBaseUnit(SelfArticleUnit);
                newData.BaseTarget = TargetArticleUnit;
                TempSelfArticlies.Add(newData);
                BaseTempSelfArticlies.Add(newData);
                if (newData.IsMutex) RemoveArticle(ArticleObjType.Target, TempTargetArticlies.FindLast(x => x.TDID == articleID));
                TempSelfArticlies = TempSelfArticlies.OrderBy((x) => x.TDID).ToList();
                BaseTempSelfArticlies = BaseTempSelfArticlies.OrderBy((x) => x.TDID).ToList();
            }
            else if (targetType == ArticleObjType.Target)
            {
                newData.ArticleObjType = targetType;
                newData.SetSelfBaseUnit(TargetArticleUnit);
                newData.BaseTarget = SelfArticleUnit;
                TempTargetArticlies.Add(newData);
                BaseTempTargetArticlies.Add(newData);
                if (newData.IsMutex) RemoveArticle(ArticleObjType.Self, TempSelfArticlies.FindLast(x => x.TDID == articleID));
                TempTargetArticlies = TempTargetArticlies.OrderBy((x) => x.TDID).ToList();
                BaseTempTargetArticlies = BaseTempTargetArticlies.OrderBy((x) => x.TDID).ToList();
            }
            if (BaseGlobal.IsUnReadData)
                Callback_OnAddArticle?.Invoke(newData);
            return;
        }
        public void RemoveArticle(ArticleObjType target, string articleID)
        {
            RemoveArticle(target, GetTempArticle(target).FindLast(x => x.TDID == articleID));
        }
        public void RemoveArticle(ArticleObjType target, TArticleData data)
        {
            if (!IsStarNegotiation)
            {
                CLog.Error("AddArticle 没有调用StarNegotiation");
                return;
            }
            if (data == null) return;
            if (target == ArticleObjType.Self)
            {
                TempSelfArticlies.Remove(data);
                BaseTempSelfArticlies.Remove(data);
                TempSelfArticlies = TempSelfArticlies.OrderBy((x) => x.TDID).ToList();
                BaseTempSelfArticlies = BaseTempSelfArticlies.OrderBy((x) => x.TDID).ToList();
            }
            if (target == ArticleObjType.Target)
            {
                TempTargetArticlies.Remove(data);
                BaseTempTargetArticlies.Remove(data);
                TempTargetArticlies = TempTargetArticlies.OrderBy((x) => x.TDID).ToList();
                BaseTempTargetArticlies = BaseTempTargetArticlies.OrderBy((x) => x.TDID).ToList();
            }
            if (BaseGlobal.IsUnReadData)
                Callback_OnRemoveArticle?.Invoke(data);
        }
        public void RemoveArticle(long id)
        {
            Datas.Remove(id);
        }
        public void RemoveArticle(TArticleData data)
        {
            Datas.Remove(data.ID);
        }
        void clearNagotiation()
        {
            CurNegotiationType = null;
            CurNegotiation = null;
            IsStarNegotiation = false;
            BaseTempSelfArticlies.Clear();
            BaseTempTargetArticlies.Clear();
            TempSelfArticlies.Clear();
            TempTargetArticlies.Clear();
        }
        #endregion

        #region get
        public int CalcArticleScore()
        {
            var total = 0;
            foreach (var item in TempSelfArticlies) total += item.GetScore(item);
            foreach (var item in TempTargetArticlies) total += -item.GetScore(item);
            return total;
        }
        //计算对方的接受度
        public float CalcProp(IAlertArticleData data)
        {
            var negotiation = GetNegotiation(data.TDID);
            if (negotiation == null) return 0;
            return negotiation.SetParam(data.CastBaseUnit as TUnit, data.SelfBaseUnit as TUnit, data.WarfareData as TWarData, data.SelfArticle, data.TargetArticle)
                .CalcProb();
        }
        //计算对方接受的概率
        public float CalcProp()
        {
            if (CurNegotiation == null) return 0;
            return CurNegotiation.SetParam(SelfArticleUnit, TargetArticleUnit, CurWarData, BaseTempSelfArticlies, BaseTempTargetArticlies).
                CalcProb();
        }
        //计算详细提示字符
        public string CalcHint()
        {
            if (CurNegotiation == null) return "";
            return CurNegotiation.SetParam(SelfArticleUnit, TargetArticleUnit, CurWarData, BaseTempSelfArticlies, BaseTempTargetArticlies).
                CalcHintStr();
        }
        public List<TDBaseArticleData> GetTempBaseSelfArticlies() => BaseTempSelfArticlies;
        public List<TDBaseArticleData> GetTempBaseTargetArticlies() => BaseTempTargetArticlies;
        public TArticleData Get(long id)
        {
            if (!Datas.ContainsID(id))
                return null;
            return Datas.Get(id);
        }
        public T Get<T>(long id) where T : TDBaseArticleData
        {
            if (!Datas.ContainsID(id))
                return default;
            return Datas.Get(id) as T;
        }
        public BaseNegotiation<TUnit, TWarData> GetNegotiation(string tdid)
        {
            if (NegotiationTemps.ContainsKey(tdid))
                return NegotiationTemps[tdid];
            else
            {
                CLog.Error("错误!没有这个谈判模板:{0}", tdid);
                return null;
            }
        }
        public List<TArticleData> GetTempArticle(ArticleObjType target)
        {
            if (target == ArticleObjType.Self)
                return TempSelfArticlies;
            return TempTargetArticlies;
        }
        public TUnit GetArticleUnit(ArticleObjType type, bool isIverse = false)
        {
            if (!isIverse)
            {
                if (type == ArticleObjType.Self)
                    return SelfArticleUnit;
                return TargetArticleUnit;
            }
            else
            {
                if (type == ArticleObjType.Self)
                    return TargetArticleUnit;
                return SelfArticleUnit;
            }
        }
        #endregion

        #region Is show
        public bool IsRepeate(ArticleObjType target, TArticleData data)
        {
            if (target == ArticleObjType.Self)
            {
                foreach (var item in TempSelfArticlies)
                {
                    if (item.IsSame(data))
                        return true;
                }
            }
            else if (target == ArticleObjType.Target)
            {
                foreach (var item in TempTargetArticlies)
                {
                    if (item.IsSame(data))
                        return true;
                }
            }
            return false;
        }
        public bool IsCanAddArticle(ArticleObjType target, TArticleData data)
        {
            if (data == null)
                return false;

            //多重条约
            if (!data.IsCanMultiple)
            {
                if (IsContainArticle(target, data.TDID))
                    return false;
            }

            //特定排除条约判断
            if (CurNegotiation != null)
            {
                var articles = CurNegotiation.GetExcludeArticle(target);
                if (articles.Contains(data.TDID))
                    return false;
            }

            return true;
        }
        public bool IsCanAddArticle(ArticleObjType target, string articleTDID)
        {
            var data = ITDConfig.Get<TArticleData>(articleTDID);
            if (data == null) return false;
            return IsCanAddArticle(target, data);
        }
        public bool IsContainArticle(ArticleObjType target, string articleTDID)
        {
            var articles = GetTempArticle(target);
            foreach (var item in articles)
            {
                if (item.TDID.Equals(articleTDID))
                    return true;
            }
            return false;
        }
        //指定外交动作是否为一个谈判模板
        public bool IsNagotiation(string tdid)
        {
            if (tdid == null) return false;
            return NegotiationTemps.ContainsKey(tdid);
        }
        public bool IsSelfHaveArticle => TempSelfArticlies.Count > 0;
        public bool IsTargetHaveArticle => TempTargetArticlies.Count > 0;
        #endregion

        #region is
        public bool IsContainInTemp(string tdid)
        {
            foreach (var item in TempSelfArticlies)
            {
                if (tdid == item.Str1 ||
                    tdid == item.Str2 ||
                    tdid == item.Str3)
                    return true;
            }
            foreach (var item in TempTargetArticlies)
            {
                if (tdid == item.Str1 ||
                    tdid == item.Str2 ||
                    tdid == item.Str3)
                    return true;
            }
            return false;
        }
        public bool IsContainInTemp(long id)
        {
            foreach (var item in TempSelfArticlies)
            {
                if (id == item.Long1 ||
                    id == item.Long2 ||
                    id == item.Long3)
                    return true;
            }
            foreach (var item in TempTargetArticlies)
            {
                if (id == item.Long1 ||
                    id == item.Long2 ||
                    id == item.Long3)
                    return true;
            }
            return false;
        }
        #endregion

        #region DB
        public void LoadDBData(ref List<DBBaseArticle> data)
        {
            data.ForEach((x) =>
            {
                TArticleData article = ITDConfig.Get<TArticleData>(x.TDID).Copy<TArticleData>();
                Util.CopyToConfig(x, article);
                article.OnBeAdded(GetEntity<TUnit>(x.Self));
                article.BaseTarget = GetEntity<TUnit>(x.Target);
                article.Float1 = x.Float1;
                article.Float2 = x.Float2;
                article.Float3 = x.Float3;
                article.Int1 = x.Int1;
                article.Int2 = x.Int2;
                article.Int3 = x.Int3;
                article.Str1 = x.Str1;
                article.Str2 = x.Str2;
                article.Str3 = x.Str3;
                article.Bool1 = x.Bool1;
                article.Bool2 = x.Bool2;
                article.Bool3 = x.Bool3;
                article.Long1 = x.Long1;
                article.Long2 = x.Long2;
                article.Long3 = x.Long3;
                article.ArticleObjType = x.ArticleObjType;
                Datas.Add(article);
            });
        }

        public void SaveDBData(ref List<DBBaseArticle> ret)
        {
            ret = new List<DBBaseArticle>();
            var temp = ret;
            Datas.ForEach((x) =>
            {
                if (x.SelfBaseUnit == null || x.BaseTarget == null)
                    return;
                DBBaseArticle dbItem = new DBBaseArticle();
                Util.CopyToData(x, dbItem);
                dbItem.Self = x.SelfBaseUnit.ID;
                dbItem.Target = x.BaseTarget.ID;
                dbItem.Float1 = x.Float1;
                dbItem.Float2 = x.Float2;
                dbItem.Float3 = x.Float3;
                dbItem.Int1 = x.Int1;
                dbItem.Int2 = x.Int2;
                dbItem.Int3 = x.Int3;
                dbItem.Str1 = x.Str1;
                dbItem.Str2 = x.Str2;
                dbItem.Str3 = x.Str3;
                dbItem.Bool1 = x.Bool1;
                dbItem.Bool2 = x.Bool2;
                dbItem.Bool3 = x.Bool3;
                dbItem.Long1 = x.Long1;
                dbItem.Long2 = x.Long2;
                dbItem.Long3 = x.Long3;
                dbItem.ArticleObjType = x.ArticleObjType;
                temp.Add(dbItem);
            });
        }
        #endregion

        #region Callback
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            Datas.Clear();
            clearNagotiation();
        }
        #endregion
    }
}