using System.Collections.Generic;
namespace CYM.AI.BT
{
    /// <summary>
    /// 条件节点
    /// </summary>
    public abstract class Decision : Node
    {
        protected Decision(params Node[] children)
        {
            Children = new List<Node>();
            for (int i = 0; i < children.Length; i++)
            {
                Children.Add(children[i]);
            }
        }

        public Node First { get { return Children[0]; } }
        public Node Second { get { return Children[1]; } }

        void StopChildren()
        {
            foreach (var child in Children)
            {
                if (child.Status.IsRunning)
                {
                    child.Stop();
                }
            }
        }

        public override void SetTree(Tree tree)
        {
            base.SetTree(tree);
            foreach (var child in Children)
            {
                child?.SetTree(tree);
            }
        }

        protected override void OnReset()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Reset();
            }
        }

        public List<Node> Children { get; private set; }

        protected override void OnStop()
        {
            base.OnStop();
            // 有些decision会在孩子还没有执行完毕时退出，这时候孩子的OnStop不会被调用
            // 所以这里必须手动调用stop
            StopChildren();
        }
    }

    /// <summary>
    /// 返回指定的Result,并且执行child
    /// </summary>
    public class Result : Decision
    {
        Status _result;

        public Result(Status result, Node child) : base(child)
        {
            _result = result;
            if (!result.IsDone)
            {
                throw new System.ArgumentOutOfRangeException();
            }
        }

        protected override Status OnDo()
        {
            Node child = Children[0];
            child.Do();
            if (child.Status.IsDone)
            {
                return _result;
            }
            else
            {
                return Status.Run;
            }
        }
    }

    /// <summary>
    /// 顺序节点
    /// 一个成功执行下一个
    /// </summary>
    public class Sequence : Decision
    {
        int _index;

        public Sequence(params Node[] children) : base(children) { }

        protected override void OnStart()
        {
            base.OnStart();
            _index = 0;
        }

        protected override Status OnDo()
        {
            if (Children.Count == 0)
            {
                return Status.Succ;
            }
            Node child = Children[_index];
            Status status = child.Do();
            while (status == Status.Succ)
            {
                _index++;
                if (_index >= Children.Count)
                {
                    return Status.Succ;
                }
                child = Children[_index];
                status = child.Do();
            }
            if (status == Status.Fail)
            {
                return Status.Fail;
            }
            if (status == Status.Run)
            {
                return Status.Run;
            }
            throw new System.Exception("不可能到这里");
        }
    }

    /// <summary>
    /// 选择节点
    /// 一个失败执行下一个
    /// </summary>
    public class Fallback : Decision
    {
        int _index;

        public Fallback(params Node[] children) : base(children) { }

        protected override void OnStart()
        {
            base.OnStart();
            _index = 0;
        }

        protected override Status OnDo()
        {
            if (Children.Count == 0)
            {
                return Status.Fail;
            }
            Node child = Children[_index];
            Status status = child.Do();
            while (status == Status.Fail)
            {
                _index++;
                if (_index >= Children.Count)
                {
                    return Status.Fail;
                }
                child = Children[_index];
                status = child.Do();
            }
            if (status == Status.Succ)
            {
                return Status.Succ;
            }
            if (status == Status.Run)
            {
                return Status.Run;
            }
            throw new System.Exception("不可能到这里");
        }
    }

    /// <summary>
    /// 执行所有节点
    /// </summary>
    public class All : Decision
    {
        int _index;

        public All(params Node[] children) : base(children) { }

        protected override void OnStart()
        {
            base.OnStart();
            _index = 0;
        }

        protected override Status OnDo()
        {
            if (Children.Count == 0)
            {
                return Status.Fail;
            }
            Node child = Children[_index];
            Status status = child.Do();
            while (status == Status.Fail ||
                status == Status.Succ)
            {
                _index++;
                if (_index >= Children.Count)
                {
                    return Status.Succ;
                }
                child = Children[_index];
                status = child.Do();
            }
            return status;
        }

    }

    /// <summary>
    /// 无限重复执行:child
    /// </summary>
    public class Repeat : Decision
    {
        //孩子结束时立刻再执行一遍:节点内部循环
        public bool IsImmediate;
        // 避免死循环，一般上只需要一次
        public int MaxImmediateDoCount = 1;
        // 只允许一个child
        public Repeat(Node child) : base(child)
        {
        }

        protected virtual bool IsQuit()
        {
            return false;
        }

        protected virtual Status QuitStatus()
        {
            if (Child.Status.IsDone)
            {
                return Child.Status;
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        protected Node Child
        {
            get { return Children[0]; }
        }

        protected override Status OnDo()
        {
            Node child = Child;
            int doCount = 0;
            do
            {
                if (IsImmediate && doCount > 0)
                {
                    CLog.Error("repeattttt immmmmmeddddiateeeeee");
                }

                if (child.Status.IsDone)
                {
                    child.Reset();
                }
                child.Do();
                if (child.Status.IsDone)
                {
                    if (IsQuit())
                    {
                        return QuitStatus();
                    }
                }
                doCount++;

            } while (IsImmediate && child.Status.IsDone && doCount <= MaxImmediateDoCount);

            return Status.Run;
        }
    }

    /// <summary>
    /// 无限重复,直到执行成功
    /// </summary>
    public class UntilSuccess : Repeat
    {
        public UntilSuccess(Node child) : base(child)
        {
        }

        protected override bool IsQuit()
        {
            return Child.Status == Status.Succ;
        }
    }

    /// <summary>
    /// 无限重复,直到执行失败
    /// </summary>
    public class UntilFail : Repeat
    {
        public UntilFail(Node child) : base(child)
        {
        }

        protected override bool IsQuit()
        {
            return Child.Status == Status.Fail;
        }
    }

    /// <summary>
    /// if节点
    /// 条件成功执行:then,条件失败执行:elseNode
    /// </summary>
    public class If : Decision
    {
        readonly Node _cond;
        readonly Node _then;
        readonly Node _else;
        bool _result;
        public If(Node condNode, Node thenNode, Node elseNode = null) : base(new Node[] { condNode, thenNode, elseNode })
        {
            _cond = condNode;
            _then = thenNode;
            _else = elseNode;

            _cond.IsNewLine = false;
        }

        protected override void OnStart()
        {
            base.OnStart();

            Status status = _cond.Do();
            if (status == Status.Succ)
            {
                _result = true;
            }
            else if (status == Status.Fail)
            {
                _result = false;
            }
            else
            {
                throw new System.ArgumentException("BT:If 条件接地点返回值错误:" + status.ToString());
            }
        }

        protected override Status OnDo()
        {
            if (_result) return _then.Do();
            else return _else.Do();
        }

    }

    /// <summary>
    /// 取反
    /// </summary>
    public class Not : Decision
    {
        public Not(params Node[] children) : base(children)
        {
            if (Children.Count != 1)
            {
                throw new System.ArgumentException("_child数量必须等于1，而实际是:" + Children.Count);
            }
            First.IsNewLine = false;
        }

        protected override Status OnDo()
        {
            First.Do();
            Status status = First.Status;
            if (status == Status.Succ)
            {
                return Status.Fail;
            }
            else if (status == Status.Fail)
            {
                return Status.Succ;
            }
            else
            {
                throw new System.Exception("不允许Running");
            }
        }
    }

    /// <summary>
    /// 概率Action 0-1 概率成功后执行节点,概率成功返回true 否则返回false
    /// </summary>
    public class ProbNode : Decision
    {
        readonly Var<float> _prob;
        public ProbNode(Var<float> prob, params Node[] children) : base(children)
        {
            if (Children.Count != 1)
            {
                throw new System.ArgumentException("_child数量必须等于1，而实际是:" + Children.Count);
            }
            _prob = prob;

            First.IsNewLine = false;
        }

        protected override Status OnDo()
        {
            float v = UnityEngine.Random.value;
            bool ret = (v < _prob.Value);
            if (_prob.Value >= 1.0f)
                ret = true;
            if (_prob.Value <= 0.0f)
                ret = false;
            if (ret)
                First.Do();
            return Status.Bool(ret);
        }

        public override void InspectorExcude()
        {
            base.InspectorExcude();
            AppendInpsectorStr(UIUtil.Per(_prob.Value));
        }
    }
}

