namespace CYM.AI.BT
{
    public struct Status
    {
        public static Status Bool(bool b)
        {
            return b ? Status.Succ : Status.Fail;
        }

        Status(int status)
        {
            _status = status;
        }

        public static readonly Status Reset = new Status();
        public static readonly Status Fail = new Status(2);
        public static readonly Status Succ = new Status(1);
        public static readonly Status Run = new Status(3);

        public bool IsSuccess { get { return this == Succ; } }

        public bool IsFail { get { return this == Fail; } }

        public bool IsDone { get { return IsSuccess || IsFail; } }

        public bool IsRunning { get { return this == Status.Run; } }

        public static bool operator ==(Status a, Status b)
        {
            return a._status == b._status;
        }

        public static bool operator !=(Status a, Status b)
        {
            return a._status != b._status;
        }

        // 0为reset状态
        int _status;

        public string Name
        {
            get
            {
                if (IsSuccess)
                {
                    return "Success";
                }
                else if (IsFail)
                {
                    return "Fail";
                }
                else if (this == Run)
                {
                    return "Running";
                }
                else
                {
                    return "Reset";
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// 节点基类
    /// </summary>
    public abstract class Node
    {

        #region inspector
        public virtual void InspectorExcude()
        {
            InspectorAppend = "";
            if (CustomTypeName.IsInv())
                InspectorTypeName = string.Format($"<b>{TypeName}</b>");
            else
                InspectorTypeName = string.Format($"<b>{CustomTypeName}</b>");
        }
        public string InspectorTypeName = "";
        public string InspectorAppend = "";
        public string TypeName
        {
            get
            {
                return GetType().Name;
            }
        }
        public string CustomTypeName { get; set; } = SysConst.STR_None;
        public bool IsDrawChildren { get { return CustomTypeName == SysConst.STR_None; } }
        public bool IsNewLine { get; set; } = true;
        public bool IsHide { get; set; } = false;
        public bool IsDrawTypeName { get; set; } = true;
        public void AppendInpsectorStr(string str)
        {
            InspectorAppend += str;
        }
        public string FinalInspectorStr
        {
            get
            {
                if (IsDrawTypeName)
                    return InspectorTypeName + ":" + InspectorAppend;
                else
                    return InspectorAppend;
            }
        }
        #endregion

        public Tree Tree;
        public Status Status { get; private set; }

        protected Node()
        {
        }

        public virtual void SetTree(Tree tree)
        {
            Tree = tree;
        }

        // 通常用于被打断的节点，保证OnStop被调用
        public void Stop()
        {
            if (Status.IsRunning)
            {
                Status = Status.Fail;
                OnStop();
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        // 每次开始行为时，调用OnStart
        protected virtual void OnStart()
        {
        }

        // 行为停止时，调用OnStop
        // 包含一般的停止，即返回成功或失败时，和在Running的时候，被迫Reset的停止
        protected virtual void OnStop()
        {
        }



        protected virtual void OnReset()
        {

        }

        protected abstract Status OnDo();

        public Status Do()
        {
            if (!Status.IsDone)
            {
                if (Status == Status.Reset)
                {
                    OnStart();
                }
                Status = OnDo();
                if (Status == Status.Reset)
                {
                    throw new System.InvalidOperationException("不能返回Reset");
                }
                else if (Status.IsDone)
                {
                    OnStop();
                }
                return Status;
            }
            // 非常重要，可以避免写错代码
            throw new System.InvalidOperationException("已经执行完毕了，为什么还要执行");
        }

        public void Reset()
        {
            if (Status == Status.Reset)
                return;
            if (Status == Status.Run)
            {
                OnStop();
            }
            OnReset();
            Status = Status.Reset;
        }
    }

    /// <summary>
    /// 行为树类
    /// </summary>
    public class Tree
    {

        public Node Root { get; private set; }

        /// <summary>
        /// 构造Tree
        /// </summary>
        /// <param name="root"></param>
        public Tree(Node root)
        {
            if (root == null)
            {
                throw new System.ArgumentNullException("root");
            }
            Root = root;
            Root.SetTree(this);
        }

        /// <summary>
        /// 执行这个Tree
        /// </summary>
        public void Update()
        {
            if (!Root.Status.IsDone)
            {
                Root.Do();
            }
        }

        /// <summary>
        /// Tree状态
        /// </summary>
        public Status Status
        {
            get { return Root.Status; }
        }

        /// <summary>
        /// 重置Tree
        /// </summary>
        public void Reset()
        {
            Root.Reset();
        }

        void _excudeAllNodeForInspector(Node node)
        {
            node.InspectorExcude();
            if (node is Decision)
            {
                var d = node as Decision;
                foreach (var item in d.Children)
                {
                    _excudeAllNodeForInspector(item);
                }
            }
        }
        bool isExcudedAllNodeForInspector = false;
        public void ExcudeAllNodeForInspector()
        {
            if (!isExcudedAllNodeForInspector)
            {
                _excudeAllNodeForInspector(Root);
                isExcudedAllNodeForInspector = true;
            }
        }
    }
}
