//------------------------------------------------------------------------------
// ScenePreloadObj.cs
// Created by CYM on 2022/5/20
// 填写类的描述...
// 用来管理场景预加载的对象,有些Prefab通过AssetBundle加载就会出错
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CYM
{
    [HideMonoScript]
    public class PreloadObj : MonoBehaviour
    {
        public static Dictionary<string, GameObject> Prefabs { get; private set; } = new Dictionary<string, GameObject>();

        private void Awake()
        {
            Prefabs.Clear();
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform trans = transform.GetChild(i);
                {
                    if (!Prefabs.ContainsKey(trans.name))
                        Prefabs.Add(trans.name, trans.gameObject);
                    else 
                    {
                        CLog.Error($"错误!!PreloadObj:相同的名称{trans.name}");
                    }
                }
            }
        }
        private void OnDestroy()
        {
            Prefabs.Clear();
        }

        public static GameObject GetPrefab(string name)
        {
            if (Prefabs.TryGetValue(name, out GameObject ret))
            {
                return ret;
            }
            return null;
        }
    }
}