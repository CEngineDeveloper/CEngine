//**********************************************
// Class Name	: BaseSpawner
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class BaseSpawner<T> : BaseCoreMono where T : BaseMono
    {
        #region inspector
        [FoldoutGroup("Base"), SerializeField, OnValueChanged("OnPropChanged", true)]
        protected string CustomName = "";
        [FoldoutGroup("Base"), SerializeField, OnValueChanged("OnPropChanged", true)]
        public List<string> TDIDs;
        [FoldoutGroup("Base"), MinValue(0), SerializeField, OnValueChanged("OnPropChanged", true)]
        public int Team = 1;
        [FoldoutGroup("Base"), MinValue(0), SerializeField]
        protected float Delay = 0.0f;
        [FoldoutGroup("Base"), MinValue(1), MaxValue(10), SerializeField, Tooltip("小于等于0表示不限次数")]
        protected int MaxSpawnCount = 1;
        [FoldoutGroup("Trigger"), SerializeField, ShowIf("Inspector_ShowIsAutoSpawn")]
        public bool IsAutoSpawn = false;
        #endregion

        #region prop
        protected CoroutineHandle coroutineHandle;
        public int SpawnedCount { get; protected set; } = 0;
        /// <summary>
        /// 至少已经Spawn了一个单位,会被标记为IsActived=true
        /// </summary>
        public bool IsActived { get; protected set; } = false;
        #endregion

        #region life
        public override void Start()
        {
            base.Start();
            if (IsAutoSpawn)
                DoSpawn();
        }
        #endregion

        #region set
        /// <summary>
        /// 手动调用
        /// </summary>
        /// <param name="delay"></param>
        public void DoSpawn()
        {
            if (!IsCanSpawn())
                return;
            IsActived = true;
            if (MaxSpawnCount > 0)
            {
                if (SpawnedCount >= MaxSpawnCount)
                    return;
            }
            SpawnedCount++;
            if (Delay == 0)
            {
                SpawnInternel();
            }
            else
            {
                BaseGlobal.BattleCorouter.Kill(coroutineHandle);
                coroutineHandle = BaseGlobal.BattleCorouter.Run(_Spawn(Delay));
            }
        }
        /// <summary>
        /// 生成单位
        /// </summary>
        protected virtual void SpawnInternel()
        {

        }
        /// <summary>
        /// 检查是否死亡
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckIsDeath()
        {
            return false;
        }
        #endregion

        #region get 
        protected string RandTDID
        {
            get
            {
                if (TDIDs == null || TDIDs.Count <= 0)
                    return TDID;
                return RandUtil.RandArray(TDIDs);
            }
        }
        protected string FirstTDID
        {
            get
            {
                return TDID;
            }
        }
        public override string GOName
        {
            get
            {
                if (!CustomName.IsInv())
                    return CustomName;
                return name;
            }
        }
        #endregion

        #region is
        /// <summary>
        /// 是否可以Spawn
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsCanSpawn()
        {
            return true;
        }
        public bool IsMaxSpawnCount
        {
            get
            {
                if (MaxSpawnCount <= 0)
                    return false;
                return SpawnedCount >= MaxSpawnCount;
            }
        }
        #endregion

        #region inspector
        protected override void OnFirstDrawGizmos()
        {
            base.OnFirstDrawGizmos();
            RefreshGizmos();
        }
        string[] spliteNames;
        protected virtual void OnPropChanged()
        {
            RefreshGizmos();
        }
        public virtual void RefreshGizmos()
        {
            if (!CustomName.IsInv())
            {
                gameObject.name = CustomName;
            }
            else
            {
                if (FirstTDID == null)
                    return;
                spliteNames = FirstTDID.Split('_');
                if (spliteNames.Length >= 2)
                    gameObject.name = spliteNames[1];
                else
                    gameObject.name = FirstTDID;
            }
        }
        #endregion

        #region IEnumerator
        IEnumerator<float> _Spawn(float delay)
        {
            yield return Timing.WaitForSeconds(delay);
            SpawnInternel();
        }
        #endregion

        #region inspector
        protected virtual bool Inspector_ShowIsAutoSpawn()
        {
            return true;
        }
        [Button(ButtonSizes.Small), PropertyOrder(-10)]
        private void DuplicateName()
        {
            Util.CopyTextToClipboard(name);
        }
        #endregion
    }
}