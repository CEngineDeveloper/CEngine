//------------------------------------------------------------------------------
// BaseScriptableObjectConfig.cs
// Copyright 2018 2018/3/28 
// Created by CYM on 2018/3/28
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CYM
{
    public interface IScriptableObjectConfig
    {
        void OnCreate();
        void OnCreated();
        void OnInited();
        void OnEditorCreate();
        void OnUse();
        bool IsHideInBuildWindow { get; }
    }



    [HideMonoScript]
    public class ScriptableObjectConfig<T> : SerializedScriptableObject, ISerializationCallbackReceiver, IScriptableObjectConfig 
        where T : SerializedScriptableObject, IScriptableObjectConfig
    {
        public virtual bool IsHideInBuildWindow => false;
        public string FileName { get; private set; }
        protected override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
        }
        protected override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
        }
        public void Load()
        { 
        
        }

        static T _ins;
        public static T Ins
        {
            get
            {
                if (_ins == null)
                {

                    string fileName = typeof(T).Name;
                    _ins = Resources.Load<T>(SysConst.Dir_Config + "/" + fileName);
                    if (_ins == null)
                    {
                        _ins = CreateInstance<T>();
                        _ins.OnCreate();
                        _ins.OnEditorCreate();
#if UNITY_EDITOR
                        AssetDatabase.CreateAsset(_ins, string.Format(SysConst.Format_ConfigAssetPath, fileName));
#endif
                        _ins.OnCreated();
                    }
                    _ins.OnInited();
                }
                _ins.OnUse();
                return _ins;
            }
        }
        public virtual void OnCreate() { }
        public virtual void OnCreated() { }
        public virtual void OnInited() { }
        public virtual void OnUse()
        {
#if UNITY_EDITOR
            if (!BaseGlobal.ConfigWindows.ContainsKey(_ins))
            {
                BaseGlobal.ConfigWindows.Add(_ins, Editor.CreateEditor(_ins));
            }
#endif 
        }
        public virtual void OnEditorCreate()
        { 
        
        }
    }
}