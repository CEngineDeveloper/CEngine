//------------------------------------------------------------------------------
// Objectiver.cs
// Copyright 2020 2020/6/29 
// Created by CYM on 2020/6/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace CYM.AI.Objective
{
    public abstract class Objectiver<TEnum, TUnit, TTarget>
        where TEnum : Enum
        where TUnit : BaseUnit
        where TTarget : BaseUnit
    {

        public class GlobalObjectiver : IClear
        {
            public Dictionary<int, HashSet<TTarget>> Targets { get; private set; } = new Dictionary<int, HashSet<TTarget>>();
            public GlobalObjectiver()
            {
                EnumTool<TEnum>.ForIndex(x =>
                {
                    int enumIndex = x;
                    Targets.Add(enumIndex, new HashSet<TTarget>());
                });
                BaseGlobal.AddToClearWhenBattleUnload(this);
            }
            public void Clear()
            {
                foreach (var item in Targets.Values)
                    item.Clear();
            }
            public bool IsHave(int index, TTarget target)
            {
                return Targets[index].Contains(target);
            }
            public void AddTarget(Task objAction)
            {
                if (objAction.Target == null) return;
                Targets[objAction.EnumIndex].Add(objAction.Target);
            }
            public void RemoveTarget(Task objAction)
            {
                if (objAction.Target == null) return;
                Targets[objAction.EnumIndex].Remove(objAction.Target);
            }
        }
        public class Task
        {
            #region prop
            protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
            public TTarget Target { get; private set; }
            public TEnum Type { get; private set; }
            public int EnumIndex { get; private set; }
            public float Importance { get; private set; } = 0;
            public Objectiver<TEnum, TUnit, TTarget> Objectiver { get; private set; }
            #endregion

            #region life
            public void Init(TEnum type, TTarget target, Objectiver<TEnum, TUnit, TTarget> objectiver)
            {
                Objectiver = objectiver;
                Target = target;
                Type = type;
                EnumIndex = EnumTool<TEnum>.Int(type);
            }
            #endregion

            public void CalcImportant() => Importance = Objectiver.ConfigData[EnumIndex].CalcImport.Invoke(this);
            public bool IsValid() => Objectiver.ConfigData[EnumIndex].Valid.Invoke(Target);
            public bool IsEnough() => Objectiver.ConfigData[EnumIndex].Enough.Invoke(this);
            public bool IsDefaultEnough() => Objectiver.ConfigData[EnumIndex].DefaultEnough.Invoke(this);
        }

        protected class TaskConfigData
        {
            //计算任务的重要程度
            public Func<Task, float> CalcImport = (x) => throw new NotImplementedException();
            //任务是否有效
            public Func<TTarget, bool> Valid = (x) => throw new NotImplementedException();
            //最大的任务数量
            public Func<int> MaxObjective = () => throw new NotImplementedException();
            //是否足够了执行任务的单位
            public Func<Task, bool> Enough = (x) => throw new NotImplementedException();
            //是否有足够的军团执行默认任务
            public Func<Task, bool> DefaultEnough = (x) => throw new NotImplementedException();
            //单位是否可以被用于执行任务
            public Func<TUnit, bool> CanBeAdd = (x) => throw new NotImplementedException();
            //单位自动取消任务
            public Func<TUnit, bool> CanBeRemove = (x) => throw new NotImplementedException();
            //计算优先级
            public Func<TUnit, Task, float> CalcUnitImport = (x1, x2) => throw new NotImplementedException();
            //当前任务失效后转换的其他任务
            public Callback<Task,List<TUnit>> OnInvalid = (x,u) => throw new NotImplementedException();
        }

        #region prop
        protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        protected BaseUnit SelfBaseUnit { get; private set; }
        #endregion

        #region data
        //任务配置表
        Dictionary<int, TaskConfigData> ConfigData { get; set; } = new Dictionary<int, TaskConfigData>();
        public static GlobalObjectiver GlobalObjer{ get; private set; } = new GlobalObjectiver();
        //空闲的单位
        public Dictionary<int, HashList<TUnit>> TypeIdles { get; private set; } = new Dictionary<int, HashList<TUnit>>();
        //已经被标记任务的目标
        public HashSet<TTarget> Targets { get; private set; } = new HashSet<TTarget>();
        //所有的任务
        public List<Task> AllTask { get; private set; } = new List<Task>();
        //已经被分配执行单位的任务
        public Dictionary<int, HashSet<Task>> AssignedTask { get; private set; } = new Dictionary<int, HashSet<Task>>();
        //分类任务
        public Dictionary<int, List<Task>> TypedTask { get; private set; } = new Dictionary<int, List<Task>>();
        //单位任务映射表
        public Dictionary<TUnit, Task> UnitTask { get; private set; } = new Dictionary<TUnit, Task>();
        //任务单位映射表
        public Dictionary<Task, HashSet<TUnit>> TaskUnit { get; private set; } = new Dictionary<Task, HashSet<TUnit>>();
        private Task PreTask;
        #endregion

        #region life
        public virtual void Init(BaseUnit selfUnit)
        {
            if (selfUnit == null)
            {
                CLog.Error("Objectiver.Init:selfUnit==null");
                return;
            }
            SelfBaseUnit = selfUnit;
            OnAddConfig();
            EnumTool<TEnum>.ForIndex(x =>
            {
                int enumIndex = x;
                AssignedTask.Add(enumIndex, new HashSet<Task>());
                TypedTask.Add(enumIndex, new List<Task>());
                TypeIdles.Add(enumIndex, new HashList<TUnit>());
                if (!ConfigData.ContainsKey(enumIndex))
                {
                    CLog.Error("错误!{0}:没有配置ConfigData", x.ToString());
                }
            });
            BaseUnit.Callback_OnRealDeathG += OnRealDeathG;
        }
        public void Update()
        {
            if (SelfBaseUnit.IsSystem)
                return;
            //更新用户数据
            OnUpdateCustomData();
            //更新任务
            OnUpdateTask();
            //计算任务的重要程度,消除无效的任务
            AllTask.ForSafe(item =>
            {
                if (!item.IsValid())
                {
                    var units = CancelTask(item);
                    ConfigData[item.EnumIndex].OnInvalid?.Invoke(item, units) ;
                }
                else item.CalcImportant();
            });
            //更新空闲单位
            UpdateIdle();
            //分配任务
            EnumTool<TEnum>.For(e =>
            {
                int enumIndex = EnumTool<TEnum>.Int(e);
                //排序任务
                TypedTask[enumIndex].Sort((x, y) => { return x.Importance > y.Importance ? -1 : 1; });
                //给任务分配执行者
                while (
                    IsHaveIdle(e) &&
                    IsHaveTask(e) &&
                    !IsInMaxAsdCount(e)
                    )
                {
                    var objAction = NextTask(e);
                    bool succ = AddUnitToTask(objAction);
                    if (!succ) break;
                }
            });
        }
        #endregion

        #region set
        protected void AddUnitToTask(Task objAction, List<TUnit> specificUnit)
        {
            if (specificUnit == null) return;
            foreach (var item in specificUnit)
                AddUnitToTask(objAction,item);
        }
        //添加单位到下一个任务中
        protected bool AddUnitToTask(Task objAction,TUnit specificUnit=null)
        {
            if (objAction == null) return false;
            TUnit unit = null;
            int enumIndex = objAction.EnumIndex;
            TEnum enumType = objAction.Type;
            //从空闲列表里面取出一个
            if (specificUnit == null)
            {
                if (PreTask != objAction)
                {
                    //排序Unit
                    var calcImport = ConfigData[enumIndex].CalcUnitImport;
                    foreach (var item in TypeIdles[enumIndex])
                        item.Importance = calcImport(item, objAction);
                    TypeIdles[enumIndex].Sort((x, y) => { return x.Importance > y.Importance ? -1 : 1; });
                }
                unit = TypeIdles[enumIndex].FirstOrDefault();
            }
            //填入指定的Unit
            else if(TypeIdles[enumIndex].Contains(specificUnit))
            {
                unit = specificUnit;
            }           
            if (unit == null) return false;
            PreTask = objAction;
            //添加单位任务映射
            if (!UnitTask.ContainsKey(unit))
            {
                UnitTask.Add(unit, objAction);
            }
            else
            {
                UnitTask[unit] = objAction;
            }
            //添加任务单位映射表
            if (!TaskUnit.ContainsKey(objAction))
            {
                TaskUnit.Add(objAction, new HashSet<TUnit>());
            }
            var units = TaskUnit[objAction];
            units.Add(unit);
            //标记已经被分配任务
            AssignedTask[enumIndex].Add(objAction);
            GlobalObjer?.AddTarget(objAction);
            //删除已经被分配的空闲单位
            EnumTool<TEnum>.ForIndex(e =>
            {
                TypeIdles[e].Remove(unit);
            });
            return true;
        }
        //移除指定的执行者
        void RemoveUnitFromTask(TUnit unit)
        {
            if (unit == null) return;
            if (!UnitTask.ContainsKey(unit)) return;
            var objective = UnitTask[unit];
            UnitTask.Remove(unit);
            var units = TaskUnit[objective];
            units.Remove(unit);
            if (units.Count <= 0)
            {
                TaskUnit.Remove(objective);
                AssignedTask[objective.EnumIndex].Remove(objective);
                GlobalObjer?.RemoveTarget(objective);
            }
        }
        //下一个没有充足执行者的任务
        Task NextTask(TEnum type)
        {
            int enumIndex = EnumTool<TEnum>.Int(type);
            var objActionData = TypedTask[enumIndex];
            if (objActionData.Count <= 0) return null;

            //招募没有满员的任务
            foreach (var item in objActionData)
            {
                if (!item.IsEnough())
                    return item;
            }

            //否则的话全部给到第一个            
            var first = objActionData.FirstOrDefault();
            if(!first.IsDefaultEnough())
                return first;
            return null;
        }
        //创建任务
        protected Task CreateTask(TEnum type, TTarget target)
        {
            int enumIndex = EnumTool<TEnum>.Int(type);
            if (target == null) return null;
            if (Targets.Contains(target))
            {
                foreach (var item in AllTask)
                {
                    if (item.Target == target)
                        return item;
                }
                return null;
            }
            if (!ConfigData[enumIndex].Valid.Invoke(target)) return null;
            var obj = new Task();
            AllTask.Add(obj);
            Targets.Add(target);
            TypedTask[enumIndex].Add(obj);
            obj.Init(type, target, this);
            return obj;
        }
        //转换任务
        protected Task ConvertTask(TEnum type, Task objective)
        {
            var target = objective.Target;
            if (target == null) return null;
            if (Targets.Contains(target))
            {
                foreach (var item in AllTask)
                {
                    if (item.Target == target)
                        return item;
                }
                return null;
            }
            else
            {
                return CreateTask(type, objective.Target);
            }
        }
        //取消指定的任务
        List<TUnit> CancelTask(Task objAction)
        {
            List<TUnit> tempUnits=null;
            AllTask.Remove(objAction);
            Targets.Remove(objAction.Target);
            EnumTool<TEnum>.ForIndex(x =>
            {
                TypedTask[x].Remove(objAction);
            });
            var units = GetExcuteUnits(objAction);
            if (units != null)
            {
                tempUnits = units.ToList();
                foreach (var item in tempUnits)
                {
                    RemoveUnitFromTask(item);
                }
            }
            return tempUnits;
        }
        void UpdateIdle()
        {
            EnumTool<TEnum>.ForIndex(e =>
            {
                int enumIndex = e;
                TypeIdles[enumIndex].Clear();
                foreach (var item in AllUnits)
                {
                    if (IsHaveTask(item))
                    {
                        var objAction = GetTask(item);
                        if (objAction.EnumIndex == enumIndex)
                        {
                            if (ConfigData[enumIndex].CanBeRemove.Invoke(item))
                                RemoveUnitFromTask(item);
                        }
                    }
                    if(!IsHaveTask(item))
                    {
                        if(ConfigData[enumIndex].CanBeAdd.Invoke(item))
                            TypeIdles[enumIndex].Add(item);
                    }
                }
            });
        }
        //添加任务配置
        protected void AddConfig(TEnum type, TaskConfigData config)
        {
            int enumIndex = EnumTool<TEnum>.Int(type);
            if (ConfigData.ContainsKey(enumIndex))
            {
                CLog.Error("错误!重复AddConfig:{0}", type.ToString());
                return;
            }
            ConfigData.Add(enumIndex, config);
        }
        #endregion

        #region pub get
        //得到某个单位的任务
        public Task GetTask(TUnit unit)
        {
            if (unit == null) return null;
            if (UnitTask.ContainsKey(unit))
                return UnitTask[unit];
            return null;
        }
        public List<Task> GetTasks(TEnum type)
        {
            return TypedTask[EnumTool<TEnum>.Int(type)];
        }
        //得到指定任务的所有执行者
        public HashSet<TUnit> GetExcuteUnits(Task objAction)
        {
            if (TaskUnit.ContainsKey(objAction))
                return TaskUnit[objAction];
            return null;
        }
        public int GetExcuteCount(Task objAction)
        {
            var data = GetExcuteUnits(objAction);
            if (data == null)
                return 0;
            return data.Count;
        }
        //得到指定任务类型,并且拥有执行者的任务数量
        public int GetAsdTaskCount(TEnum type)
        {
            int enumIndex = EnumTool<TEnum>.Int(type);
            return AssignedTask[enumIndex].Count;
        }
        //指定任务类型的数量
        public int GetTaskCount(TEnum type)
        {
            int enumIndex = EnumTool<TEnum>.Int(type);
            return TypedTask[enumIndex].Count;
        }
        #endregion

        #region is
        //是否有这个类型的任务
        public bool IsHaveTask(TEnum type)
        {
            int enumIndex = EnumTool<TEnum>.Int(type);
            return TypedTask[enumIndex].Count > 0;
        }
        //指定的单位是否拥有任务
        public bool IsHaveTask(TUnit unit)
        {
            return GetTask(unit) != null;
        }
        //是否有空闲的单位
        public bool IsHaveIdle(TEnum type)
        {
            return TypeIdles[EnumTool<TEnum>.Int(type)].Count > 0;
        }
        //是否达到指定类型任务的上限
        public bool IsInMaxAsdCount(TEnum type)
        {
            int enumIndex = EnumTool<TEnum>.Int(type);
            //最后一个任务,不设置上限
            if (enumIndex >= EnumTool<TEnum>.Length()) return false;
            //其他任务会有上限
            if (GetAsdTaskCount(type) >= ConfigData[enumIndex].MaxObjective.Invoke())
                return true;
            return false;
        }
        #endregion

        #region Callback
        //所有的单位
        public abstract IEnumerable<TUnit> AllUnits { get; }
        protected abstract void OnAddConfig();
        protected abstract void OnUpdateTask();
        protected virtual void OnUpdateCustomData() { }
        private void OnRealDeathG(BaseUnit arg1)
        {
            RemoveUnitFromTask(arg1 as TUnit);
        }
        #endregion
    }
}