using System.Collections.Generic;
using UnityEngine;

namespace CYM.Unit
{
    public class BasePerformMgr : BaseMgr
    {
        #region member variable
        protected List<BasePerform> Data = new List<BasePerform>();
        #endregion

        #region life
        public sealed override MgrType MgrType => MgrType.All;
        public override void OnAffterStart()
        {
            base.OnAffterStart();
            if (IsGlobal)
            {
                if(BaseGlobal.BattleMgr!=null)
                    BaseGlobal.BattleMgr.Callback_OnUnLoad += OnBattleUnLoad;
            }
        }
        public virtual void ReBirthInit()
        {
        }
        public override void OnDeath()
        {
            base.OnDeath();
            DespawnAll();
        }
        public override void OnRealDeath()
        {
            base.OnRealDeath();
        }
        public override void OnDisable()
        {
            //删除角色身上的所有特效
            for (int i = 0; i < Data.Count; ++i)
            {
                if (Data[i].IsAttachedUnit)
                    Despawn(Data[i]);
            }
            base.OnDisable();
        }
        #endregion

        #region spawn
        public virtual BasePerform SpawnGO(GameObject prefab, BaseCoreMono target, Vector3? position = null, Quaternion? quaternion = null)
        {
            if (prefab == null) return null;
            if (BaseGlobal.PoolPerform == null) return null;
            var temp = BaseGlobal.PoolPerform.Spawn(prefab, position, quaternion);
            return Spawn<BasePerform>(temp, SelfBaseUnit, null, target, position, quaternion);
        }
        public BasePerform SpawnGO(GameObject prefab, Vector3? position = null, Quaternion? quaternion = null)
        {
            return SpawnGO(prefab, SelfMono, position, quaternion);
        }
        public virtual T Spawn<T>(string performName,BaseCoreMono self,Vector3? position = null, Quaternion? quaternion = null)
            where T : BasePerform
        {
            if (performName.IsInv()) return null;
            if (BaseGlobal.PoolPerform == null) return null;
            var temp = BaseGlobal.PoolPerform.Spawn(BaseGlobal.RsPerfom.Get(performName), position, quaternion);
            return Spawn<T>(temp, self, null, null, position, quaternion);
        }
        public virtual T Spawn<T>(string performName, BaseCoreMono cast, BaseCoreMono target, Vector3? position = null, Quaternion? quaternion = null ) 
            where T : BasePerform
        {
            if (performName.IsInv()) return null;
            if (BaseGlobal.PoolPerform == null) return null;
            var temp = BaseGlobal.PoolPerform.Spawn(BaseGlobal.RsPerfom.Get(performName), position, quaternion);
            return Spawn<T>(temp,SelfBaseUnit, cast, target, position, quaternion);
        }
        public T Spawn<T>(string performName, Vector3? position = null, Quaternion? quaternion = null)
            where T : BasePerform
        {
            return Spawn<T>(performName, SelfMono, null, position, quaternion);
        }
        public BasePerform Spawn(string performName, Vector3? position = null, Quaternion? quaternion = null)
        {
            return Spawn<BasePerform>(performName, SelfMono, null, position, quaternion);
        }
        public BasePerform Spawn(string performName, BaseCoreMono self, Vector3? position = null, Quaternion? quaternion = null)
        {
            return Spawn<BasePerform>(performName, self, null, position, quaternion);
        }
        T Spawn<T>(GameObject temp, BaseCoreMono self,BaseCoreMono cast, BaseCoreMono target, Vector3? position, Quaternion? quaternion)
            where T : BasePerform
        {
            if (temp == null) return null;
            if (position.HasValue) 
                temp.transform.position = position.Value;
            if (quaternion.HasValue) 
                temp.transform.rotation = quaternion.Value;
            T temPerform = BaseCoreMono.GetUnityComponet<T>(temp);
            //特效创建之时先将碰撞合都设为禁用,因为这个时候回掉函数还没注册
            temPerform.SetCollidersActive(false);
            if (position.HasValue || quaternion.HasValue)
                temPerform.UseFollow = false;
            else
                temPerform.UseFollow = true;
            ClearEvent(temPerform);
            temPerform.PerformMgr = this;
            temPerform.OnCreate(self==null? SelfBaseUnit: self, cast == null ? SelfBaseUnit : cast, target);
            //回掉函数注册完以后开启碰撞盒
            temPerform.SetCollidersActive(true);
            Data.Add(temPerform);
            return temPerform;
        }
        public void Despawn(BasePerform perform, bool isRemove = true)
        {
            if (perform != null)
            {
                float closeTime = perform.CloseTime;
                //GameObject mono = perform.GO;
                ClearEvent(perform);
                perform.OnClose();
                if (isRemove)
                    Data.Remove(perform);

                BaseGlobal.PoolPerform.Despawn(perform, closeTime);
            }
        }
        public void DespawnAll()
        {
            var temp = new List<BasePerform>(Data);
            foreach (var item in temp)
            {
                Despawn(item, false);
            }
            Data.Clear();
        }
        #endregion

        #region set
        public void SetVisible(bool b)
        {
            for (int i = 0; i < Data.Count; ++i)
            {
                Data[i].SetVisible(b);
            }
        }
        void ClearEvent(BasePerform perform)
        {
            perform.Callback_OnTriggerIn = null;
            perform.Callback_OnTriggering = null;
            perform.Callback_OnTriggerOut = null;
            perform.Callback_OnDoDestroy = null;
            perform.Callback_OnLifeOver = null;
        }
        #endregion

        #region CallBack
        private void OnBattleUnLoad()
        {
            DespawnAll();
        }

        #endregion
    }

}