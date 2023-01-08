//------------------------------------------------------------------------------
// PluginConfig.cs
// Copyright 2022 2022/11/12 
// Created by CYM on 2022/11/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using Sirenix.OdinInspector;
using System.IO;
using System.Linq;
using System;
namespace CYM
{
    public class PluginConfig : ScriptableObjectConfig<PluginConfig>
    {
        [Serializable]
        public class Item
        {
            [HideInInspector]
            public string Name;
            public string Url;
            public string Path;
        }

        [SerializeField][HideReferenceObjectPicker][DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout,KeyLabel = "")][ReadOnly]
        public SerializableDic<string, Item> Datas = new SerializableDic<string, Item>();

        [Button(nameof(Refresh))]
        public void Refresh()
        {
            Datas.Clear();
            var files = Directory.GetDirectories(SysConst.Path_Plugins,$"{SysConst.Dir_CEngine}.*",SearchOption.TopDirectoryOnly);
            foreach (var item in files)
            {
                var newItem = item.Replace("\\","/").Split('/').Last();
                if (newItem == SysConst.Dir_CEngine)
                    continue;
                var data = new Item { Name = newItem, Url = "",Path = item };
                Datas.Add(newItem, data);
            }
        }
    }
}