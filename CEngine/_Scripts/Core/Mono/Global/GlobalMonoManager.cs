using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public sealed class MonoUpdateData
    {
        #region Normal
        public List<BaseCoreMono> UpdateIns = new List<BaseCoreMono>();
        public List<BaseCoreMono> FixedUpdateIns = new List<BaseCoreMono>();
        public List<BaseCoreMono> LateUpdateIns = new List<BaseCoreMono>();
        public List<BaseCoreMono> GUIIns = new List<BaseCoreMono>();
        public List<BaseCoreMono> GizmosIns = new List<BaseCoreMono>();

        List<BaseCoreMono> update_addList = new List<BaseCoreMono>();
        List<BaseCoreMono> update_removeList = new List<BaseCoreMono>();

        List<BaseCoreMono> fixedupdate_addList = new List<BaseCoreMono>();
        List<BaseCoreMono> fixedupdate_removeList = new List<BaseCoreMono>();

        List<BaseCoreMono> lateupdate_addList = new List<BaseCoreMono>();
        List<BaseCoreMono> lateupdate_removeList = new List<BaseCoreMono>();

        List<BaseCoreMono> gui_addList = new List<BaseCoreMono>();
        List<BaseCoreMono> gui_removeList = new List<BaseCoreMono>();

        List<BaseCoreMono> gizmos_addList = new List<BaseCoreMono>();
        List<BaseCoreMono> gizmos_removeList = new List<BaseCoreMono>();
        #endregion

        #region prop
        string Name;
        int JobPerFrame = 100;
        string JobKeyName;
        string JobKeyNameFixedUpdate;
        #endregion

        #region life
        public MonoUpdateData(string name,int jobPerFrame)
        {
            Name = name;
            JobKeyName = name + "JobUpdate";
            JobKeyNameFixedUpdate = name + "FixedUpdate";
            JobPerFrame = jobPerFrame;
        }
        public void LateUpdate()
        {
            AddList(UpdateIns, update_addList);
            RemoveList(UpdateIns, update_removeList);

            AddList(FixedUpdateIns, fixedupdate_addList);
            RemoveList(FixedUpdateIns, fixedupdate_removeList);

            AddList(LateUpdateIns, lateupdate_addList);
            RemoveList(LateUpdateIns, lateupdate_removeList);

            AddList(GUIIns, gui_addList);
            RemoveList(GUIIns, gui_removeList);

            AddList(GizmosIns, gizmos_addList);
            RemoveList(GizmosIns, gizmos_removeList);

            foreach (var item in LateUpdateIns)
            {
                item.OnLateUpdate();
            }
        }
        public void Update()
        {
            foreach (var item in UpdateIns)
            {
                item.OnUpdate();
            }
        }
        public void FixedUpdate()
        {
            //转到Job System
            //foreach (var item in FixedUpdateIns)
            //{
            //    item.OnFixedUpdate();
            //}
        }
        public void OnGUI()
        {
            foreach (var item in GUIIns)
            {
                item.OnGUIPaint();
            }
        }
        public void OnDestroy()
        {
            QueueHub.DestroyQueue(JobKeyName, true);
            QueueHub.DestroyQueue(JobKeyNameFixedUpdate, true);
            UpdateIns.Clear();
            FixedUpdateIns.Clear();
            LateUpdateIns.Clear();
            GUIIns.Clear();
            GizmosIns.Clear();
        }
        #endregion

        #region set
        void RemoveList(List<BaseCoreMono> ins, List<BaseCoreMono> list)
        {
            if (list.Count <= 0) 
                return;
            foreach (var temp in list) 
                ins.Remove(temp);
            list.Clear();
        }
        void AddList(List<BaseCoreMono> ins, List<BaseCoreMono> list)
        {
            if (list.Count <= 0) 
                return;
            foreach (var temp in list) 
                ins.Add(temp);
            list.Clear();
        }
        public void AddMono(BaseCoreMono mono)
        {
            if (mono.NeedJobUpdate)
            {
                JoinQueue(mono,JobKeyName, mono.OnJobUpdate,0, JobPerFrame);
            }
            if (mono.NeedUpdate)
            {
                update_addList.Add(mono);
            }
            if (mono.NeedLateUpdate)
            {
                lateupdate_addList.Add(mono);
            }
            if (mono.NeedGUI)
            {
                gui_addList.Add(mono);
            }
            if (mono.NeedFixedUpdate)
            {
                //fixedupdate_addList.Add(mono);
                JoinQueue(mono, JobKeyNameFixedUpdate, mono.OnFixedUpdate, 0.02f, JobPerFrame);
            }
        }
        public void RemoveMono(BaseCoreMono mono)
        {
            if (mono.NeedJobUpdate)
            {
                QuitQueue(mono, JobKeyName);
            }
            if (mono.NeedUpdate)
            {
                update_removeList.Add(mono);
            }
            if (mono.NeedLateUpdate)
            {
                lateupdate_removeList.Add(mono);
            }
            if (mono.NeedGUI)
            {
                gui_removeList.Add(mono);
            }
            if (mono.NeedFixedUpdate)
            {
                //fixedupdate_removeList.Add(mono);
                QuitQueue(mono, JobKeyNameFixedUpdate);
            }
        }
        public void RemoveAllNull()
        {
            UpdateIns.RemoveAll((p) => p == null);
            FixedUpdateIns.RemoveAll((p) => p == null);
            LateUpdateIns.RemoveAll((p) => p == null);
            GUIIns.RemoveAll((p) => p == null);
            GizmosIns.RemoveAll((p) => p == null);
        }
        void JoinQueue(BaseCoreMono mono,string jobName,QueueSpotDelegate update,float timer,int jobsPerFrame)
        {
            //string realName = jobName + Name;
            if (QueueHub.DoesQueueExist(jobName))
            {
                QueueHub.AddJobToQueue(jobName, mono.GO, update);
            }
            else
            {
                QueueHub.CreateQueue(jobName, jobsPerFrame, true);
                QueueHub.SetQueueUpdateRate(jobName, timer);
                QueueHub.AddJobToQueue(jobName, mono.GO, update);
            }
        }
        void QuitQueue(BaseCoreMono mono, string jobName)
        {
            //string realName = jobName + Name;
            if (QueueHub.DoesQueueExist(jobName))
                QueueHub.RemoveJobFromQueue(jobName, mono.GO);
        }
        #endregion
    }
    [HideMonoScript]
    public sealed class GlobalMonoManager : MonoBehaviour
    {
        #region prop
        Timer GCTimer = new Timer(1000);
        public static MonoType PauseType { get; private set; } = MonoType.None;
        public static GlobalMonoManager Ins { get; private set; }
        #endregion

        #region Inspector
        public static bool ToggleUpdate { get; set; } = true;
        public static bool ToggleFixedUpdate { get; set; } = true;
        public static bool ToggleLateUpdate { get; set; } = true;
        #endregion

        #region data
        public static MonoUpdateData Unit;//= new MonoUpdateData(nameof(Unit),GameConfig.Ins.UnitJobPerFrame);
        public static MonoUpdateData Global;//= new MonoUpdateData(nameof(Global), GameConfig.Ins.GlobalJobPerFrame);
        public static MonoUpdateData View;//= new MonoUpdateData(nameof(View), GameConfig.Ins.ViewJobPerFrame);
        public static MonoUpdateData Normal;//= new MonoUpdateData(nameof(Normal), GameConfig.Ins.NormalJobPerFrame);
        #endregion

        #region set
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            if (Ins == null)
            {
                GameObject temp = new GameObject("GlobalMonoMgr");
                DontDestroyOnLoad(temp);
                temp.AddComponent<GlobalMonoManager>();
            }
        }
        // 设置暂停类型
        public static void SetPauseType(MonoType type) => PauseType = type;
        public static void ActiveMono(BaseCoreMono mono)
        {
            if (mono.MonoType == MonoType.Unit)
                Unit.AddMono(mono);
            else if (mono.MonoType == MonoType.Global)
                Global.AddMono(mono);
            else if (mono.MonoType == MonoType.View)
                View.AddMono(mono);
            else if (mono.MonoType == MonoType.Normal)
                Normal.AddMono(mono);
        }
        public static void DeactiveMono(BaseCoreMono mono)
        {
            if (mono.MonoType == MonoType.Unit)
                Unit.RemoveMono(mono);
            else if (mono.MonoType == MonoType.Global)
                Global.RemoveMono(mono);
            else if (mono.MonoType == MonoType.View)
                View.RemoveMono(mono);
            else if (mono.MonoType == MonoType.Normal)
                Normal.RemoveMono(mono);
        }
        public static void RemoveAllNull()
        {
            Normal.RemoveAllNull();
            Unit.RemoveAllNull();
            Global.RemoveAllNull();
            View.RemoveAllNull();
        }
        #endregion

        #region life
        private void Awake()
        {
            DontDestroyOnLoad(this);
            Ins = this;
            Unit = new MonoUpdateData(nameof(Unit), GameConfig.Ins.UnitJobPerFrame);
            Global = new MonoUpdateData(nameof(Global), GameConfig.Ins.GlobalJobPerFrame);
            View = new MonoUpdateData(nameof(View), GameConfig.Ins.ViewJobPerFrame);
            Normal = new MonoUpdateData(nameof(Normal), GameConfig.Ins.NormalJobPerFrame);
        }
        void Update()
        {
            if (!ToggleUpdate)
                return;
            Normal.Update();
            Unit.Update();
            Global.Update();
            View.Update();

            if (GCTimer.CheckOver())
                BaseGlobal.GCCollect();
        }
        private void FixedUpdate()
        {
            if (!ToggleFixedUpdate)
                return;
            Normal.FixedUpdate();
            Unit.FixedUpdate();
            Global.FixedUpdate();
            View.FixedUpdate();
        }
        public void LateUpdate()
        {
            if (!ToggleLateUpdate)
                return;
            Normal.LateUpdate();
            Unit.LateUpdate();
            Global.LateUpdate();
            View.LateUpdate();
        }
        void OnGUI()
        {
            Normal.OnGUI();
            Unit.OnGUI();
            Global.OnGUI();
            View.OnGUI();
        }
        //private void OnDestroy()
        //{
        //    Normal.OnDestroy();
        //    Unit.OnDestroy();
        //    Global.OnDestroy();
        //    View.OnDestroy();
        //}
        #endregion
    }
}