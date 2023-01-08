//------------------------------------------------------------------------------
// CustomScriptableObjectConfig.cs
// Created by CYM on 2022/1/2
// 填写类的描述...
//------------------------------------------------------------------------------
using Sirenix.OdinInspector;
using UnityEditor;

namespace CYM
{
    public class CustomScriptableObjectConfig<T> : ScriptableObjectConfig<T>
         where T : SerializedScriptableObject, IScriptableObjectConfig
    {
        public override void OnEditorCreate()
        {
            base.OnEditorCreate();
#if UNITY_EDITOR
            string fileName = typeof(T).Name;
            AssetDatabase.CreateAsset(Ins, string.Format(SysConst.Format_ConfigAssetPath, fileName));
#endif
        }
    }
}