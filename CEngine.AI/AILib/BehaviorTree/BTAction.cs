using System;
using UnityEngine;
namespace CYM.AI.BT
{
    /// <summary>
    /// is 节点
    /// </summary>
    public class Is : Node
    {
        System.Func<bool> _is;
        public Is(System.Func<bool> iis)
        {
            _is = iis;
        }

        protected override Status OnDo()
        {
            bool result = _is();
            return result ? Status.Succ : Status.Fail;
        }

        public override void InspectorExcude()
        {
            base.InspectorExcude();
            AppendInpsectorStr(_is.Method.Name);
        }
    }

    /// <summary>
    /// 空节点
    /// </summary>
    public class Empty : Node
    {
        Status _status;
        public Empty(Status status)
        {
            IsHide = true;
            if (!status.IsDone)
            {
                throw new System.ArgumentOutOfRangeException();
            }
            _status = status;
        }

        protected override Status OnDo()
        {
            return _status;
        }
    }

    /// <summary>
    /// 执行节点
    /// </summary>
    public class Do : Node
    {
        System.Action _start;
        System.Func<Status> _do;

        public Do(System.Action start, System.Func<Status> action)
        {
            if (action == null)
            {
                throw new System.ArgumentNullException();
            }
            _start = start;
            _do = action;

            IsDrawTypeName = false;
        }

        public Do(System.Func<Status> action) : this(null, action)
        {
        }

        protected override Status OnDo()
        {
            return _do();
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (_start != null)
            {
                _start();
            }
        }

        public override void InspectorExcude()
        {
            base.InspectorExcude();
            AppendInpsectorStr(_do.Method.Name);
        }
    }

    /// <summary>
    /// 执行节点,返回指定返回值
    /// </summary>
    public class DoStatus : Node
    {
        System.Func<Status> _action;
        Status _s;

        public DoStatus(System.Func<Status> action, Status s)
        {
            if (!s.IsDone)
            {
                throw new System.ArgumentOutOfRangeException("节点没有执行完成");
            }
            if (action == null)
            {
                throw new System.ArgumentNullException();
            }
            _action = action;
            _s = s;
        }

        protected override Status OnDo()
        {
            _action();
            return _s;
        }

        public override void InspectorExcude()
        {
            base.InspectorExcude();
            AppendInpsectorStr(_action.Method.Name);
        }
    }
    public class DoActStatus : Node
    {
        System.Action _action;
        Status _s;

        public DoActStatus(System.Action action, Status s)
        {
            if (!s.IsDone)
            {
                throw new System.ArgumentOutOfRangeException("节点没有执行完成");
            }
            if (action == null)
            {
                throw new System.ArgumentNullException();
            }
            _action = action;
            _s = s;
        }

        protected override Status OnDo()
        {
            _action();
            return _s;
        }

        public override void InspectorExcude()
        {
            base.InspectorExcude();
            AppendInpsectorStr(_action.Method.Name);
        }
    }

    /// <summary>
    /// 执行所有节点,返回指定返回值
    /// </summary>
    public class DoListStatus : Node
    {
        System.Func<Status>[] _action;
        Status _s;

        public DoListStatus(Status s, params System.Func<Status>[] action)
        {
            if (!s.IsDone)
            {
                throw new System.ArgumentOutOfRangeException("节点没有执行完成");
            }
            if (action == null)
            {
                throw new System.ArgumentNullException();
            }
            _action = action;
            _s = s;
        }

        protected override Status OnDo()
        {
            foreach (var item in _action)
            {
                item();
            }
            return _s;
        }

        public override void InspectorExcude()
        {
            base.InspectorExcude();
            string str = "";
            foreach (var item in _action)
            {
                str += item.Method.Name + " ";
            }
            AppendInpsectorStr(str);
        }
    }

    /// <summary>
    /// 随机执行,返回指定值
    /// </summary>
    public class DoRandStatus : Node
    {
        Func<Status>[] _action;
        Status _s;

        Func<Status> _doAction;

        public DoRandStatus(Status s, params System.Func<Status>[] action)
        {
            if (!s.IsDone)
            {
                throw new System.ArgumentOutOfRangeException("节点没有执行完成");
            }
            if (action == null)
            {
                throw new System.ArgumentNullException();
            }
            _action = action;
            _s = s;
        }

        protected override Status OnDo()
        {
            _doAction = RandUtil.RandArray(_action);
            _doAction.Invoke();
            return _s;
        }

        public override void InspectorExcude()
        {
            base.InspectorExcude();

            string str = "";
            foreach (var item in _action)
            {
                str += item.Method.Name + " ";
            }
            AppendInpsectorStr(str);

        }
    }

    /// <summary>
    /// 随机执行,返回值
    /// </summary>
    public class DoRand : Node
    {
        Func<Status>[] _action;

        Func<Status> _doAction;

        public DoRand(params Func<Status>[] action)
        {
            if (action == null)
            {
                throw new System.ArgumentNullException();
            }
            _action = action;
        }

        protected override Status OnDo()
        {
            _doAction = RandUtil.RandArray(_action);
            return _doAction.Invoke(); ;
        }

        public override void InspectorExcude()
        {
            base.InspectorExcude();

            string str = "";
            foreach (var item in _action)
            {
                str += item.Method.Name + " ";
            }
            AppendInpsectorStr(str);

        }
    }

    /// <summary>
    /// 等待Action
    /// </summary>
    public class Wait : Node
    {
        protected readonly Var<float> _time;
        protected float _elapsed;

        public Wait(Var<float> time)
        {
            _time = time;
        }

        protected override void OnStart()
        {
            base.OnStart();
            _elapsed = 0;
        }

        protected override Status OnDo()
        {
            _elapsed += DeltaTime;
            return _elapsed < _time.Value ? Status.Run : Status.Succ;
        }

        protected virtual float DeltaTime
        {
            get
            {
                return Time.deltaTime;
            }
        }

        public override void InspectorExcude()
        {
            base.InspectorExcude();
            AppendInpsectorStr(_time.Value.ToString("f1"));
        }
    }

    /// <summary>
    /// 等待Action
    /// </summary>
    public class WaitFixed : Wait
    {
        public WaitFixed(Var<float> time) : base(time)
        {

        }
        protected override float DeltaTime => Time.fixedDeltaTime;//BaseGlobalMonoMgr.FixedDeltaTime;
    }

    /// <summary>
    /// 概率Action 0-1
    /// </summary>
    public class Prob : Node
    {
        readonly float _prob;

        public Prob(float prob)
        {
            _prob = prob;
        }

        protected override Status OnDo()
        {
            float v = UnityEngine.Random.value;
            return Status.Bool(v < _prob);
        }

        public override void InspectorExcude()
        {
            base.InspectorExcude();
            AppendInpsectorStr(UIUtil.Per(_prob));
        }
    }

    /// <summary>
    /// 概率Action 0-1 概率成功后执行节点,概率成功返回true 否则返回false
    /// </summary>
    public class ProbAction : Node
    {
        readonly Var<float> _prob;
        Func<Status> _action;
        public ProbAction(Var<float> prob, Func<Status> action)
        {
            _prob = prob;
            _action = action;
        }

        protected override Status OnDo()
        {
            float v = UnityEngine.Random.value;
            bool ret = (v < _prob.Value);
            if (ret)
                _action.Invoke();
            return Status.Bool(ret);
        }

        public override void InspectorExcude()
        {
            base.InspectorExcude();
            AppendInpsectorStr(UIUtil.Per(_prob.Value) + " " + _action.Method.Name);
        }
    }


}