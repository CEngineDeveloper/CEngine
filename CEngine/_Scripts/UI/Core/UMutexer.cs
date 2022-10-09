using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace CYM.UI
{
    // 用来控制子界面的打开或者关闭状态
    public class UMutexer : IUIDirty
    {
        #region prop
        int Index { get; set; } = -999;
        // 关闭界面的时候是否需要重置
        bool IsNeedReset { get; set; } = true;
        // 至少显示一个
        bool IsShowOne { get; set; } = true;
        private Func<int> GetIndex { get; set; }
        List<UControl> Controls { get; set; } = new List<UControl>();
        Dictionary<string, UControl> DicControls { get; set; } = new Dictionary<string, UControl>(); 
        #endregion

        #region life
        public UMutexer() { }
        public UMutexer(bool isNeedReset = true, bool isShowOne = true)
        {
            IsNeedReset = isNeedReset;
            IsShowOne = isShowOne; ;
        }
        public UMutexer(UControl[] controls, bool isNeedReset = true, bool isShowOne = true)
        {
            IsNeedReset = isNeedReset;
            IsShowOne = isShowOne;;
            Add(controls);
        }
        public void OnFixedUpdate()
        {
            if (IsDirtyShow) RefreshShow();
            if (IsDirtyData) RefreshData();
            if (IsDirtyRefresh) Refresh();
            if (IsDirtyCell) RefreshCell();
        }
        public void OnUpdate() { }
        #endregion

        #region set
        public void Add(UControl[] controls)
        {
            if (controls == null) throw new ArgumentNullException("Mutexer");
            if (controls.Length == 0) throw new InvalidOperationException("Mutexer.Length == 0");

            foreach (var item in controls)
                Add(item);
        }
        public void Add(UControl control)
        {
            if (control.PMutexer != null)
            {
                CLog.Error("item:{0} 已经被挂在某个Mutexer下面", control.GOName);
                return;
            }
            if (DicControls.ContainsKey(control.GOName))
                return;
            control.PMutexer = this;
            Controls.Add(control);
            DicControls.Add(control.GOName, control);
            control.SetActive(true);
        }
        public void Remove(UControl control)
        {
            control.PMutexer = null;
            Controls.Remove(control);
        }
        // 通过showable设置当前的panel
        public void Show(UControl obj)
        {
            if (Controls.Count == 0) return;
            var temp = Controls.FindIndex((x) => { return x == obj; });
            Show(temp);
        }
        public void Show(string name)
        {
            if (Controls.Count == 0) return;
            var control = Get(name);
            var temp = Controls.FindIndex((x) => { return x == control; });
            Show(temp);
        }
        public void Toggle(UControl obj)
        {
            if (Controls.Count == 0) return;
            var temp = Controls.Find((x) => { return x == obj; });
            var tempIndex = Controls.FindIndex((x) => { return x == obj; });
            if (!temp.IsShow) Show(tempIndex);
            else ShowDefault();
        }

        public void ShowDefault()
        {
            if (Controls.Count == 0) return;
            if (IsShowOne) Show(0);
            else Show(-1);
        }
        public void TestReset()
        {
            if (IsNeedReset)
                ShowDefault();
        }
        #endregion

        #region get
        public UControl Current
        {
            get
            {
                if (Controls.Count <= 0) return null;
                if (Index < 0) return null;
                if (Index >= Controls.Count) return null;
                return Controls[Index];
            }
        }
        public UControl Get(string name)
        {
            if(DicControls.ContainsKey(name))
                return DicControls[name];
            return null;
        }
        #endregion 

        #region Dirty
        public void RefreshAll()
        {
            if (Controls.Count == 0) return;
            RefreshShow();
            Refresh();
            RefreshData();
        }
        public void RefreshShow()
        {
            IsDirtyShow = false;
            if (GetIndex != null) Index = GetIndex.Invoke();
            for (int i = 0; i < Controls.Count; i++)
            {
                var item = Controls[i];
                item.ShowDirect(i == Index, false, false, true);
            }
        }
        public void Refresh()
        {
            IsDirtyRefresh = false;
            if (Controls.Count == 0) return;
            if (Index > -1)
            {
                if (Controls[Index].IsShow)
                    Controls[Index].Refresh();
            }
        }
        public void RefreshCell()
        {
            IsDirtyCell = false;
            if (Controls.Count == 0) return;
            if (Index > -1)
            {
                if (Controls[Index].IsShow)
                    Controls[Index].RefreshCell();
            }
        }
        public void RefreshData()
        {
            IsDirtyData = false;
            if (Controls.Count == 0) return;
            if (Index > -1)
            {
                if (Controls[Index].IsShow)
                    Controls[Index].RefreshData();
            }
        }
        #endregion

        #region set dirty
        public bool IsDirtyShow { get; private set; }
        public bool IsDirtyData { get; private set; }
        public bool IsDirtyCell { get; private set; }
        public bool IsDirtyRefresh { get; private set; }
        public void SetDirtyAll()
        {
            IsDirtyShow = true;
            IsDirtyData = true;
            IsDirtyRefresh = true;
            IsDirtyCell = true;
        }
        public void SetDirtyAll(float delay)
        {
            Util.Invoke(() => SetDirtyAll(), delay);
        }

        public void SetDirtyShow()
        {
            IsDirtyShow = true;
            IsDirtyRefresh = true;
        }
        public void SetDirtyData()
        {
            IsDirtyData = true;
            IsDirtyRefresh = true;
            IsDirtyCell = true;
        }
        public void SetDirtyCell() => IsDirtyCell = true;
        public void SetDirtyRefresh() => IsDirtyRefresh = true;
        #endregion

        #region utile
        // 通过index设置当前的panel
        private void Show(int state)
        {
            if (state == Index) return;
            if (Controls.Count == 0) return;
            if (state < -1 || state >= Controls.Count) return;
            if (Index != state) Index = state;
            SetDirtyAll();
        }
        #endregion
    }
}