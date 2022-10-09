//------------------------------------------------------------------------------
// EmojConfig.cs
// Copyright 2018 2018/3/23 
// Created by CYM on 2018/3/23
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    [Serializable]
    public class SpritesData
    {
        [PropertyOrder(0), ShowInInspector, ReadOnly]
        public string Name
        {
            get
            {
                if (First != null)
                    return First.name;
                return "";
            }
        }
        [PropertyOrder(1)]
        public Sprite[] Sprites;
        public Sprite First
        {
            get
            {
                if (Sprites == null)
                    return null;
                if (Sprites.Length <= 0)
                    return null;
                return Sprites[0];
            }
        }
        public bool IsAnim
        {
            get
            {
                if (Sprites == null)
                    return false;
                return Sprites.Length > 1;
            }
        }
        [ShowIf("IsAnim")]
        public float AnimSpeed = 0.05f;//多少秒播放一个图片

        public bool IsHave
        {
            get
            {
                return First != null;
            }
        }
    }

    [Serializable]
    public class SpriteConfig : ScriptableObject
    {
        public string Name = "";
        public SpritesData[] SpritesData;
        [NonSerialized]
        public Dictionary<string, SpritesData> KeySpritesData = new Dictionary<string, SpritesData>();

        private void OnEnable()
        {
            KeySpritesData.Clear();
            if (SpritesData != null)
            {
                foreach (var Item in SpritesData)
                {
                    if (!KeySpritesData.ContainsKey(Item.Name))
                    {
                        //Item.Init();
                        KeySpritesData.Add(Item.Name, Item);
                    }
                }
            }
        }

        public SpriteConfig()
        {

        }

        public void OnAfterDeserialize()
        {

        }
    }

}