//------------------------------------------------------------------------------
// PluginGameConfig.cs
// Copyright 2023 2023/1/20 
// Created by CYM on 2023/1/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using CYM.Diplomacy;
using System;

namespace CYM
{
    [Serializable, Unobfus, HideReferenceObjectPicker]
    public class RelationShip: Range
    {
        public RelationShip(float min, float max):base(min, max)
        {
        }
        public RelationShipType Type { get; set; } = RelationShipType.Close;
        [SerializeField]
        public Sprite Icon;
    }
    public partial class GameConfig : CustomScriptableObjectConfig<GameConfig>
    {
        #region Person
        [SerializeField, FoldoutGroup("Diplomacy")]
        public int ArmisticeCD = 10;
        [SerializeField, FoldoutGroup("Diplomacy"), DictionaryDrawerSettings(IsReadOnly = true), HideReferenceObjectPicker]
        public Dictionary<RelationShipType, RelationShip> RelationShip = new Dictionary<RelationShipType, RelationShip>()
        {
            { RelationShipType.Close,   new RelationShip(90,100) },
            { RelationShipType.Friendly,new RelationShip(10,90) },
            { RelationShipType.Neutral, new RelationShip(-10,10) },
            { RelationShipType.Disgust, new RelationShip(-50,-10) },
            { RelationShipType.Feud,    new RelationShip(-100,-50) },
        };
        #endregion
    }
}