namespace CYM
{
    /// <summary>
    /// 脏值处理器
    /// </summary>
    [Unobfus]
    public class DirtyAction
    {
        Callback Action;
        bool IsDirty = false;
        int UpdateCount = 0;
        public void SetDirty(bool immediately = false)
        {
            if (immediately)
            {
                Action?.Invoke();
            }
            else
            {
                IsDirty = true;
            }
        }
        public void SetDirty(int updateCount)
        {
            IsDirty = true;
            UpdateCount = updateCount;
        }

        public DirtyAction(Callback action)
        {
            Action = action;
        }

        public void OnUpdate()
        {
            if (IsDirty)
            {
                UpdateCount--;
                if (UpdateCount <= 0)
                {
                    Action?.Invoke();
                    IsDirty = false;
                }
            }
        }
    }
}
