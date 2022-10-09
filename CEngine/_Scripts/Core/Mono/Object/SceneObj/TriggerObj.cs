//------------------------------------------------------------------------------
// SubTriggerObject.cs
// Copyright 2018 2018/4/4 
// Created by CYM on 2018/4/4
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public interface ITriggerObj
    {
        void DoTriggerObjectEnter(Collider other, TriggerObj triggerObj, bool forceSense);
        void DoTriggerObjectExit(Collider other, TriggerObj triggerObj, bool forceSense);
    }
    public sealed class TriggerObj : BaseMono
    {
        [SerializeField]
        GameObject TriggerGO;
        //[SerializeField]
        //bool IsSense = false;

        ITriggerObj triggerObject;
        Collider col;

        public override void Awake()
        {
            base.Awake();
            col = GetComponent<Collider>();
            col.isTrigger = true;
            SetTriggerObj(TriggerGO);
        }

        public void SetTriggerObj(GameObject go)
        {
            if (go == null)
                return;
            TriggerGO = go;
            triggerObject = go.GetComponent<ITriggerObj>();
            //if (IsSense)
            //{
            //    SetLayer(Const.Layer_Sense, false);
            //}
        }

        public void OnTriggerEnter(Collider other)
        {
            if (triggerObject == null)
                return;
            triggerObject.DoTriggerObjectEnter(other, this, false);
        }

        public void OnTriggerExit(Collider other)
        {
            if (triggerObject == null)
                return;
            triggerObject.DoTriggerObjectExit(other, this, false);
        }

        //public bool IsSenseObj()
        //{
        //    return Layer == (int)Const.Layer_Sense;
        //}

    }
}