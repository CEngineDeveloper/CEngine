using System;
using UnityEngine;
namespace CYM.Sense
{
    public interface ISenseMgr
    {
        float Radius { get; }
        string SenseName { get; }
        void DoTestEnter(Component col);
        void DoTestExit(Component col);
        void DoCollect();
    }
    public class BaseSenseMgr<TUnit> : BaseMgr, ISenseMgr
        where TUnit : BaseUnit
    {
        #region col
        protected Collider SelfCol;
        protected SphereCollider SphereCollider;
        protected Collider[] ColliderResults;
        #endregion

        #region col 2D
        protected Collider2D SelfCol2D;
        protected CircleCollider2D CircleCollider2D;
        protected Collider2D[] Collider2DResults;
        #endregion

        #region prop
        protected GameObject SenseGameObj;
        protected SenseObj SenseObject;
        protected Timer Timer = new Timer();
        #endregion

        //#region mgr
        //IRelationMgr RelationMgr => BaseGlobal.RelationMgr;
        //#endregion

        #region list
        public HashList<TUnit> Units { get; private set; } = new HashList<TUnit>();//视野中的Unit
        public HashList<TUnit> UnitsEnemy { get; private set; } = new HashList<TUnit>();//并不安全
        public HashList<TUnit> UnitsAlly { get; private set; } = new HashList<TUnit>();//并不安全
        public HashList<TUnit> UnitsSelf { get; private set; } = new HashList<TUnit>();//并不安全
        #endregion

        #region life
        public virtual float Radius => 4;
        protected virtual float UpdateTimer => float.MaxValue;
        protected virtual int MaxColliderResults => 20;
        protected virtual bool Is3D => true;
        //protected virtual bool IsMonitorRelation => true;
        //视野的名称，必须重载，DetectionMgr需要用到，比如Sense，Vision
        public virtual string SenseName => throw new NotImplementedException();
        protected virtual LayerData CheckLayer => throw new NotImplementedException("必须重载");
        public sealed override MgrType MgrType => MgrType.Unit;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            SelfCol = SelfBaseUnit.Collider;
            SelfCol2D = SelfBaseUnit.Collider2D;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            //if (IsMonitorRelation && RelationMgr != null)
            //    RelationMgr.Callback_OnChangeRelationBaseUnits += OnBaseChangeRelation;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            //if (IsMonitorRelation && RelationMgr != null)
            //    RelationMgr.Callback_OnChangeRelationBaseUnits -= OnBaseChangeRelation;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            Timer = new Timer(UpdateTimer);
            SenseGameObj = new GameObject("SenseObj");
            SenseGameObj.layer = (int)SysConst.Layer_Sense;
            SenseGameObj.transform.SetParent(SelfMono.Trans);
            SenseGameObj.transform.localPosition = Vector3.zero;
            SenseGameObj.transform.localScale = Vector3.one;
            SenseGameObj.transform.localRotation = Quaternion.identity;
            SenseObject = BaseMono.GetUnityComponet<SenseObj>(SenseGameObj);
            SenseObject.Init(this);
            if (Is3D)
            {
                SphereCollider = SenseGameObj.AddComponent<SphereCollider>();
                SphereCollider.isTrigger = true;
                SphereCollider.radius = 0;
                ColliderResults = new Collider[MaxColliderResults];
            }
            else
            {
                CircleCollider2D = SenseGameObj.AddComponent<CircleCollider2D>();
                CircleCollider2D.isTrigger = true;
                CircleCollider2D.radius = 0;
                Collider2DResults = new Collider2D[MaxColliderResults];
            }
            SenseGameObj.SetActive(false);
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (SelfBaseUnit == null)
                return;
            if (BaseGlobal.BattleMgr == null ||
                !BaseGlobal.BattleMgr.IsLoadBattleEnd)
                return;
            if (Timer.CheckOver())
                DoCollect();

            if (SphereCollider != null && SphereCollider.radius != Radius)
            {
                SphereCollider.radius = Mathf.Lerp(Radius, SphereCollider.radius, Time.deltaTime * 2);
            }
            else if (CircleCollider2D != null && CircleCollider2D.radius != Radius)
            {
                CircleCollider2D.radius = Mathf.Lerp(Radius, CircleCollider2D.radius, Time.deltaTime * 2);
            }
        }
        public override void OnBirth()
        {
            base.OnBirth();
            Clear();
            SenseGameObj.SetActive(true);

        }

        public override void OnDeath()
        {
            foreach (var item in SelfBaseUnit.DetectionMgr.Units.ToArray())
            {
                foreach (var sense in item.SenseMgrs)
                {
                    if (Is3D) sense.DoTestExit(SelfCol);
                    else sense.DoTestExit(SelfCol2D);
                }
            }
            Clear();
            SenseGameObj.SetActive(false);
            base.OnDeath();
        }
        #endregion

        #region set
        public void Clear()
        {
            foreach (var item in Units)
            {
                item.DetectionMgr.Remove(this, SelfBaseUnit);
            }
            Units.Clear();
            UnitsEnemy.Clear();
            UnitsAlly.Clear();
            UnitsSelf.Clear();
            for (int i = 0; i < ColliderResults.Length; i++)
            {
                ColliderResults[i] = null;
            }
        }
        #endregion

        #region is
        public bool IsInSense(BaseUnit unit) => Units.Contains(unit as TUnit);
        public bool IsInSensePos(TUnit unit) => SphereCollider.bounds.Contains(unit.Pos);
        public bool IsHaveEnemy() => UnitsEnemy.Count > 0;
        public bool IsHave() => Units.Count > 0;
        #endregion

        #region utile
        public void DoCollect()
        {
            Clear();
            if (Is3D)
            {
                int count = Physics.OverlapSphereNonAlloc(SelfBaseUnit.Pos, Radius, ColliderResults, (LayerMask)CheckLayer, QueryTriggerInteraction.Collide);
                if (count > 0)
                {
                    if (ColliderResults == null) return;
                    foreach (var item in ColliderResults)
                        DoTestEnter(item);
                }
            }
            else
            {
                Collider2DResults = Physics2D.OverlapCircleAll(SelfBaseUnit.Pos, Radius, (LayerMask)CheckLayer);
                if (Collider2DResults != null && Collider2DResults.Length > 0)
                {
                    if (Collider2DResults == null) return;
                    foreach (var item in Collider2DResults)
                        DoTestEnter(item);
                }
            }
        }
        public void DoTestEnter(Component col)
        {
            if (!SelfBaseUnit.IsLive)
                return;
            if (col == null) return;
            TUnit unit = col.GetComponent<TUnit>();
            if (unit != null)
            {
                if (!unit.IsLive)
                    return;
                Units.Add(unit);
                if (SelfBaseUnit.IsEnemy(unit)) 
                    UnitsEnemy.Add(unit);
                if (SelfBaseUnit.IsSOF(unit)) 
                    UnitsAlly.Add(unit);
                if (SelfBaseUnit.IsSelf(unit)) 
                    UnitsSelf.Add(unit);
                unit.DetectionMgr.Add(this, SelfBaseUnit);
                OnEnter(unit);
            }
            OnEnterObject(col);
        }
        public void DoTestExit(Component col)
        {
            if (!SelfBaseUnit.IsLive)
                return;
            TUnit unit = col.GetComponent<TUnit>();
            if (unit != null)
            {
                Units.Remove(unit);
                UnitsEnemy.Remove(unit);
                UnitsAlly.Remove(unit);
                UnitsSelf.Remove(unit);
                unit.DetectionMgr.Remove(this, SelfBaseUnit);
                OnExit(unit);
            }
            OnExitObject(col);
        }
        public void RefreshUnitState()
        {
            if (!SelfBaseUnit.IsLive)
                return;
            UnitsEnemy.Clear();
            UnitsAlly.Clear();
            UnitsSelf.Clear();
            foreach (var item in Units)
            {
                if (SelfBaseUnit.IsEnemy(item)) 
                    UnitsEnemy.Add(item);
                if (SelfBaseUnit.IsSOF(item)) 
                    UnitsAlly.Add(item);
                if (SelfBaseUnit.IsSelf(item)) 
                    UnitsSelf.Add(item);
            }
        }
        #endregion

        #region Callback
        protected virtual void OnEnter(TUnit col) { }
        protected virtual void OnExit(TUnit col) { }
        protected virtual void OnEnterObject(Component col) { }
        protected virtual void OnExitObject(Component col) { }
        private void OnBaseChangeRelation(IHashList list)
        {
            if(list.Contains(SelfBaseUnit.BaseOwner))
                RefreshUnitState();
        }
        #endregion
    }

}