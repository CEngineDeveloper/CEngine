namespace CYM.AI.BT
{
    public class Event
    {
        public object[] Arg { get; private set; }
        public void Set(params object[] arg)
        {
            Trigger();
            Arg = arg;
        }
        bool IsOn
        {
            get;
            set;
        }
        public Event()
        {

        }
        public Status IsTrigger()
        {
            if (IsOn)
                return Status.Succ;
            return Status.Fail;
        }
        public Status Reset()
        {
            IsOn = false;
            return Status.Succ;
        }

        public Status Trigger()
        {
            IsOn = true;
            return Status.Succ;
        }
    }
}
