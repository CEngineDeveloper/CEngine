using System;
namespace CYM.AI.BT
{
    public class Var<T>
    {
        Func<T> Get;

        public Var(Func<T> getter) { Get = getter; }

        public T Value
        {
            get { return Get(); }
            set { throw new InvalidOperationException("不允许set"); }
        }
    }

    public class Float : Var<float>
    {
        public Float(float val) : base(() => val) { }
    }

    // 语言
    public class BT
    {
        public static Tree CreateTree(Func<Node> action) => new Tree(action.Invoke());
        public static Tree CreateTree(Node root) => new Tree(root);
        public static Node DoSucc(Func<Status> action) => new DoStatus(action, Status.Succ);
        public static Node DoSucc(params Func<Status>[] action) => new DoListStatus(Status.Succ, action);
        public static Node DoSuccRand(params Func<Status>[] action) => new DoRandStatus(Status.Succ, action);
        public static Node DoFail(Func<Status> action) => new DoStatus(action, Status.Fail);
        public static Node DoFail(params Func<Status>[] action) => new DoListStatus(Status.Fail, action);
        public static Node DoFailRand(params Func<Status>[] action) => new DoRandStatus(Status.Fail, action);
        public static Node Do(Func<Status> action) => new Do(action);
        public static Node DoRand(params Func<Status>[] action) => new DoRand(action);
        public static Node Do(Action start, Func<Status> action) => new Do(start, action);
        public static Node If(Node iis, Node node, Node elseNode = null) => new If(iis, node, elseNode != null ? elseNode : Fail());
        public static Node If(Func<Status> iis, Node node, Node elseNode = null) => new If(Do(iis), node, elseNode != null ? elseNode : Fail());
        public static Node Succ() => new Empty(Status.Succ);
        public static Node Fail() => new Empty(Status.Fail);
        public static Node Succ(Node node) => new Result(Status.Succ, node);
        public static Node Fail(Node node) => new Result(Status.Fail, node);
        public static Node Not(Node node) => new Not(node);
        public static Node Not(Func<Status> node) => new Not(Do(node));
        public static Node Seq(params Node[] children) => new Sequence(children);
        public static Node Fall(params Node[] children) => new Fallback(children);
        public static Node All(params Node[] children) => new All(children);
        public static Node Prob(float prob) => new Prob(prob);
        public static Node ProbAction(Var<float> prob, Func<Status> node) => new ProbAction(prob, node);
        public static Node ProbNode(Var<float> prob, Node node) => new ProbNode(prob, node);
        public static Node Log(string format, params object[] ps) => DoSucc(() => { CYM.CLog.Log(format, ps); return Status.Succ; });
        public static Node Log(Func<string> log) => DoSucc(() => { CYM.CLog.Log(log.Invoke()); return Status.Succ; });
        public static Node RepFall(params Node[] children)
        {
            var ret = new Repeat(Fall(children));
            ret.CustomTypeName = "RepFall";
            return ret;
        }
        public static Node RepSeq(params Node[] children)
        {
            var ret = new Repeat(Seq(children));
            ret.CustomTypeName = "RepSeq";
            return ret;
        }
        public static Node RepAll(params Node[] children)
        {
            var ret = new Repeat(All(children));
            ret.CustomTypeName = "RepAll";
            return ret;
        }
        public static Node UntilFail(Node child) => new UntilFail(child);
        public static Node UntilFail(Func<Status> child) => new UntilFail(Do(child));
        public static Node UntilSucc(Node child) => new UntilSuccess(child);
        public static Node UntilSucc(Func<Status> child)
        {
            var temp = new UntilSuccess(Do(child));
            temp.IsNewLine = false;
            return temp;
        }
        public static Node Wait(Var<float> seconds) => new Wait(seconds);
        public static Node WaitFixed(Var<float> seconds) => new WaitFixed(seconds);
    }
}
