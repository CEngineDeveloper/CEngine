using System.Collections.Generic;
//**********************************************
// Class Name	: CYMTimer
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    #region other
    public class SeizeNodeData<TNode> where TNode : class
    {
        protected Dictionary<TNode, HashSet<BaseUnit>> SeizeNodes = new Dictionary<TNode, HashSet<BaseUnit>>();
        protected Dictionary<BaseUnit, TNode> SeizeNodesUnit = new Dictionary<BaseUnit, TNode>();

        public void ClearSeizeNode(TNode node, BaseUnit unit)
        {
            if (node == null) return;
            if (unit == null) return;
            //添加新的SeizeNode
            if (!SeizeNodes.ContainsKey(node))
                SeizeNodes.Add(node, new HashSet<BaseUnit>());
            SeizeNodes[node].Remove(unit);
            //设置SeizeNodesUnit
            SeizeNodesUnit.Remove(unit);
        }
        // 设置占据的Node
        public void SetSeizeNode(TNode node, BaseUnit unit)
        {
            if (node == null) return;
            if (unit == null) return;
            //添加新的SeizeNode
            if (!SeizeNodes.ContainsKey(node))
                SeizeNodes.Add(node, new HashSet<BaseUnit>());
            if (!SeizeNodes[node].Contains(unit))
                SeizeNodes[node].Add(unit);
            //设置SeizeNodesUnit
            if (SeizeNodesUnit.ContainsKey(unit)) SeizeNodesUnit[unit] = node;
            else SeizeNodesUnit.Add(unit, node);
        }

        public HashSet<BaseUnit> GetUnits(TNode node)
        {
            if (node == null) return null;
            if (SeizeNodes.ContainsKey(node))
            {
                return SeizeNodes[node];
            }
            else
            {
                return new HashSet<BaseUnit>();
            }
        }

        public TNode GetNode(BaseUnit unit)
        {
            if (unit == null)
                return null;
            if (SeizeNodesUnit.ContainsKey(unit))
            {
                TNode node = SeizeNodesUnit[unit];
                return node;
            }
            else
            {
                return null;
            }
        }

        public bool IsHaveUnit(TNode node)
        {
            if (node == null) return false;
            if (!SeizeNodes.ContainsKey(node)) return false;
            return SeizeNodes[node].Count > 0;
        }

        public void Clear()
        {
            SeizeNodes.Clear();
            SeizeNodesUnit.Clear();
        }
    }
    public class BlockNodeData<TNode> where TNode : class
    {
        HashList<TNode> AllBlockers = new HashList<TNode>();
        Dictionary<TNode, HashList<BaseUnit>> BlockNodes = new Dictionary<TNode, HashList<BaseUnit>>();
        Dictionary<BaseUnit, HashList<TNode>> BlockNodesUnit = new Dictionary<BaseUnit, HashList<TNode>>();

        // 设置阻挡得Node,输入Null表示清空
        public void SetBlockNode(BaseUnit unit, List<TNode> nodes)
        {
            if (unit == null)
                return;
            HashList<TNode> preNodes = null;
            if (BlockNodesUnit.ContainsKey(unit))
                preNodes = BlockNodesUnit[unit];

            //先清除之前残余得数据
            if (preNodes != null)
            {
                foreach (var item in preNodes)
                {
                    AllBlockers.Remove(item);
                    if (BlockNodes.ContainsKey(item))
                    {
                        BlockNodes[item].Remove(unit);
                    }
                    if (BlockNodes[item].Count == 0)
                        BlockNodes.Remove(item);
                }
                BlockNodesUnit.Remove(unit);
            }


            if (nodes != null)
            {
                HashList<TNode> newHashSetNodes = new HashList<TNode>(nodes);
                foreach (var item in nodes)
                {
                    AllBlockers.Add(item);
                    if (!BlockNodes.ContainsKey(item))
                    {
                        BlockNodes.Add(item, new HashList<BaseUnit>());
                    }
                    BlockNodes[item].Add(unit);
                }
                BlockNodesUnit.Add(unit, newHashSetNodes);
            }
        }

        public HashList<TNode> GetBlocker(BaseUnit unit)
        {
            if (unit == null)
                return null;
            if (BlockNodesUnit.ContainsKey(unit))
                return BlockNodesUnit[unit];
            return null;
        }
        public HashList<BaseUnit> GetBlockerUnits(TNode node)
        {
            if (node == null) return null;
            if (BlockNodes.ContainsKey(node))
            {
                return BlockNodes[node];
            }
            return null;
        }
        public bool IsBlocker(TNode node)
        {
            if (node == null) return false;
            if (AllBlockers.Contains(node))
            {
                return true;
            }
            return false;
        }
        public void Clear()
        {
            AllBlockers.Clear();
            BlockNodes.Clear();
            BlockNodesUnit.Clear();
        }
    }
    public class BaseSequence
    {
        protected object[] AddedObjs { get; private set; }
        protected CoroutineHandle Handle;
        protected BaseGlobal SelfBaseGlobal;
        protected BaseUnit SelfBaseUnit;
        protected BaseCoreMono SelfMono;

        public BaseSequence()
        {
            SelfBaseGlobal = BaseGlobal.Ins;
        }

        public virtual void Init(BaseCoreMono mono)
        {
            SelfMono = mono;
            SelfBaseGlobal = BaseGlobal.Ins;
            if (SelfMono is BaseUnit)
                SelfBaseUnit = SelfMono as BaseUnit;

        }

        protected TType GetAddedObjData<TType>(int index, TType defaultVal = default(TType))
        {
            if (AddedObjs == null || AddedObjs.Length <= index)
                return defaultVal;
            return (TType)(AddedObjs[index]);
        }

        public virtual void Start(params object[] ps)
        {
            AddedObjs = ps;
            BaseGlobal.BattleCorouter.Kill(Handle);
            Handle = BaseGlobal.BattleCorouter.Run(_ActionSequence());
            IsInState = true;
        }

        public void Stop()
        {
            if (Handle.IsRunning)
            {
                OnStop();
            }
            BaseGlobal.BattleCorouter.Kill(Handle);
        }

        public bool IsInState { get; protected set; } = false;


        protected virtual void OnStop()
        {
            IsInState = false;
        }
        protected virtual IEnumerator<float> _ActionSequence()
        {
            yield return Timing.WaitForOneFrame;
        }
    }
    #endregion
}
