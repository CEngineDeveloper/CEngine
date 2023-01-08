namespace CYM
{
    /// <summary>
    /// 触发器,只触发一次,知道调用Reset
    /// </summary>
    [Unobfus]
    public class Triger
    {
        bool isTrue = false;
        Callback callback;
        public Triger(Callback callback)
        {
            this.callback = callback;
        }
        public void DoTriger(Callback callback)
        {
            if (isTrue == false)
            {
                callback?.Invoke();
                isTrue = true;
            }
        }
        public void DoTriger()
        {
            callback?.Invoke();
        }
        public void Reset()
        {
            isTrue = false;
        }
        public bool IsTrigered()
        {
            return isTrue;
        }
    }
}
