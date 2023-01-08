using CYM.Pool;
using System.Collections.Generic;
namespace CYM
{
    public static class ACPool
    {
        static Dictionary<string, IObjPool> Pools = new Dictionary<string, IObjPool>();
        public static void RecycleAll()
        {
            foreach (var item in Pools)
                item.Value.RecycleAll();
        }

        public static T Get<T>(string key = null) where T : BaseCondition
        {
            string typeName = typeof(T).Name;
            if (key != null) typeName = key;
            if (!Pools.ContainsKey(typeName))
            {
                Pools.Add(typeName, new ObjPool<T>());
            }
            var ret = Pools[typeName].GetObj() as T;
            ret.Reset();
            return ret;
        }
    }

    // 基类条件管理器
    public class BaseConditionMgr : BaseGFlowMgr
    {
        #region prop
        private List<ExCondition> exConditions = new List<ExCondition>();
        private List<SimpleCondition> andSimpleConditions = new List<SimpleCondition>();
        private List<SimpleCondition> orSimpleConditions = new List<SimpleCondition>();
        private List<BaseCondition> andCacheConditions = new List<BaseCondition>();
        private List<BaseCondition> orCacheConditions = new List<BaseCondition>();
        #endregion

        #region param
        public BaseUnit ParamSelfBaseUnit { get; private set; }
        public bool IsReseted { get; private set; } = false;
        #endregion

        #region normal
        public void Add(ACCType accType, params BaseCondition[] conditions)
        {
            if (!IsReseted)
            {
                CLog.Error("ACM 组件没有Reset,无法添加条件");
                return;
            }
            if (conditions == null || conditions.Length <= 0) return;
            if (accType == ACCType.And) andCacheConditions.AddRange(conditions);
            else if (accType == ACCType.Or) orCacheConditions.AddRange(conditions);
        }
        public void Add(ACCType accType, params SimpleCondition[] conditions)
        {
            if (!IsReseted)
            {
                CLog.Error("ACM 组件没有Reset,无法添加条件");
                return;
            }
            if (conditions == null || conditions.Length <= 0) return;
            if (accType == ACCType.And) andSimpleConditions.AddRange(conditions);
            else if (accType == ACCType.Or) orSimpleConditions.AddRange(conditions);
        }
        public void Add(params ExCondition[] conditions)
        {
            if (!IsReseted)
            {
                CLog.Error("ACM 组件没有Reset,无法添加条件");
                return;
            }
            if (conditions == null || conditions.Length <= 0) return;
            exConditions.AddRange(conditions);
        }
        public void Add(ExCondition condition)
        {
            if (!IsReseted)
            {
                CLog.Error("ACM 组件没有Reset,无法添加条件");
                return;
            }
            if (condition == null) return;
            exConditions.Add(condition);
        }
        public void Add(List<BaseTarget> targets)
        {
            if (!IsReseted)
            {
                CLog.Error("ACM 组件没有Reset,无法添加条件");
                return;
            }
            if (targets != null)
            {
                foreach (var item in targets)
                    item.DoCondition(ParamSelfBaseUnit);
            }
        }
        public void Add(BaseTarget target)
        {
            if (!IsReseted)
            {
                CLog.Error("ACM 组件没有Reset,无法添加条件");
                return;
            }
            if (target != null)
            {
                target.DoCondition(ParamSelfBaseUnit);
            }
        }
        public void Reset(BaseUnit self)
        {
            IsReseted = true;
            ParamSelfBaseUnit = self;
            exConditions.Clear();
            andSimpleConditions.Clear();
            orSimpleConditions.Clear();
            andCacheConditions.Clear();
            orCacheConditions.Clear();
            ACPool.RecycleAll();
        }
        #endregion

        #region get
        // 获取条件描述
        public string GetDesc()
        {
            if (IgnoreCondition) return "";
            IsIgnoreReset = true;
            IsTrue(false);

            //条件字符串
            string mustCondition = "";
            string orCondition = "";

            string andDesc = "";
            for (int i = 0; i < andCacheConditions.Count; ++i)
            {
                if (andCacheConditions[i].IsCost)
                    continue;

                if (!andCacheConditions[i].IsIgnore)
                    andDesc += andCacheConditions[i].GetDesc() + "\n";
            }
            for (int i = 0; i < andSimpleConditions.Count; ++i)
                andDesc += andSimpleConditions[i].GetDesc() + "\n";

            if (andDesc != "")
                mustCondition = BaseLangMgr.Get("AC_需要满足条件", andDesc);

            //条件之一
            string orDesc = "";
            for (int i = 0; i < orCacheConditions.Count; ++i)
            {
                if (!orCacheConditions[i].IsIgnore)
                    orDesc += orCacheConditions[i].GetDesc() + "\n";
            }

            if (orSimpleConditions != null)
            {
                for (int i = 0; i < orSimpleConditions.Count; ++i)
                    orDesc += orSimpleConditions[i].GetDesc() + "\n";
            }

            if (orDesc != "")
                orCondition = BaseLangMgr.Get("AC_需要满足以下条件之一", orDesc);

            string finalStr = (mustCondition + orCondition).TrimEnd("\n");
            return finalStr;
        }

        // 获得消耗字符窜
        public string GetCost()
        {
            if (IgnoreCondition) return "";

            string mustCondition = "";
            //必须条件
            string cost = "";
            if (andCacheConditions != null)
            {
                for (int i = 0; i < andCacheConditions.Count; ++i)
                {
                    if (andCacheConditions[i].IsCost)
                        cost += andCacheConditions[i].GetCost();
                }
            }
            if (cost != "")
                mustCondition = BaseLangMgr.Get("AC_消耗", cost);
            return mustCondition;
        }
        //获得独占条件描述
        public string GetExDesc()
        {
            if (IgnoreCondition) return "";
            if (exConditions != null)
            {
                for (int i = 0; i < exConditions.Count; ++i)
                {
                    if (!exConditions[i].DoAction())
                        return exConditions[i].GetDesc();
                }
            }
            return "";
        }
        public string GetAll()
        {
            //优先返回独占条件
            string exDesc = GetExDesc();
            if (exDesc != "")
                return exDesc;

            string cost = GetCost();
            string desc = GetDesc();
            if (desc != "" && cost != "") return string.Format("{0}\n{1}", cost, desc);
            else if (cost != "") return cost;
            else if (desc != "") return desc;
            else return UIUtil.Green(BaseLangMgr.Get("AC_您可以执行此操作"));
        }
        #endregion

        #region is
        // 是否忽略
        public bool IgnoreCondition
        {
            get
            {
                if (!BaseGlobal.BattleMgr.IsLoadBattleEnd)
                    return true;
                if (SysConsole.Ins.IsIgnoreCondition)
                    return true;
                return !BaseGlobal.IsUnReadData;
            }
        }
        bool IsIgnoreReset { get; set; } = false;
        // 根据所有的条件判断
        public bool IsTrue(bool isFast = true)
        {
            if (!IsReseted && !IsIgnoreReset)
            {
                CLog.Error("ACM 组件没有Reset,无法添加条件");
                return false;
            }
            IsIgnoreReset = false;
            IsReseted = false;
            if (IgnoreCondition) return true;
            bool isEx = true;
            bool isAnd = true;
            bool isAndSimple = true;
            bool isOr = false;
            bool isOrSimple = false;

            //判断独占条件
            for (int i = 0; i < exConditions.Count; ++i)
            {
                if (exConditions[i].IsOnlyPlayer && !ParamSelfBaseUnit.IsPlayer()) continue;
                if (!exConditions[i].DoAction())
                {
                    isEx = false;
                    if (isFast) return false;
                }
            }

            //判断必须的条件
            for (int i = 0; i < andCacheConditions.Count; ++i)
            {
                BaseCondition con = andCacheConditions[i];
                if (con.IsOnlyPlayer && !ParamSelfBaseUnit.IsPlayer()) continue;
                bool ret = con.DoAction();
                if (!ret)
                {
                    isAnd = false;
                    if (isFast) return false;
                }
            }

            for (int i = 0; i < andSimpleConditions.Count; ++i)
            {
                if (andSimpleConditions[i].IsOnlyPlayer && !ParamSelfBaseUnit.IsPlayer()) continue;
                if (!andSimpleConditions[i].DoAction())
                {
                    isAndSimple = false;
                    if (isFast) return false;
                }
            }

            //判断多选一的条件           
            for (int i = 0; i < orCacheConditions.Count; ++i)
            {
                BaseCondition con = orCacheConditions[i];
                if (con.IsOnlyPlayer && !ParamSelfBaseUnit.IsPlayer()) continue;
                bool r = con.DoAction();
                if (r)
                {
                    isOr = true;
                    if (isFast) return true;
                }
            }

            for (int i = 0; i < orSimpleConditions.Count; ++i)
            {
                if (orSimpleConditions[i].IsOnlyPlayer && !ParamSelfBaseUnit.IsPlayer()) continue;
                if (orSimpleConditions[i].DoAction())
                {
                    isOrSimple = true;
                    if (isFast) return true;
                }
            }

            if (!isEx)
                return false;
            return isAnd && isAndSimple && (orCacheConditions.Count == 0 || isOr) && (orSimpleConditions.Count == 0 || isOrSimple);
        }
        #endregion

    }

}