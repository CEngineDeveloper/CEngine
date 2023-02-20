//------------------------------------------------------------------------------
// BaseTerritoryMgr.cs
// Copyright 2023 2023/1/20 
// Created by CYM on 2023/1/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM.Diplomacy.NodeMap;
using System.Collections.Generic;
using TGS;

namespace CYM.Diplomacy
{
    public class BaseTerritoryMgr : BaseMgr
    {
        #region prop
        protected TerrainGridSystem TerrainGridSystem => TerrainGridSystem.instance;
        protected BaseTerrainGridMgr TerrainGridMgr => BaseGlobal.TerrainGridMgr;
        public List<BaseUnit> Neighbours { get; protected set; } = new List<BaseUnit>();
        public Territory Territory { get; private set; }
        public int TerritoryIndex { get; private set; }
        public Cell Cell { get; private set; }
        public Node Node { get; private set; }
        #endregion

        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        public virtual Color TerritoryColor => Color.black;
        public virtual bool IsTerritory => true;
        public override void OnBeSpawned()
        {
            base.OnBeSpawned();
            if (TerrainGridSystem != null &&
                BaseGlobal.TerrainGridMgr != null &&
                BaseGlobal.TerrainGridMgr.IsHaveTSG)
            {
                Cell = TerrainGridSystem.CellGetAtPosition(SelfMono.Pos, true);
                if (Cell != null)
                {
                    TerritoryIndex = Cell.territoryIndex;
                    Territory = TerrainGridSystem.territories[Cell.territoryIndex];
                }
            }
            Node = SelfMono.GetComponent<Node>();
            TerrainGridMgr?.AddTerrDatas(this);
        }
        public override void OnEnable()
        {
            base.OnEnable();
            SelfBaseUnit.Callback_OnSetOwner += OnSetOwner;
        }
        public override void OnDisable()
        {
            SelfBaseUnit.Callback_OnSetOwner -= OnSetOwner;
            base.OnDisable();
        }
        #endregion

        #region set
        public void UpdateTerritory(bool refreshGeometry = true)
        {
            if (Territory != null && IsTerritory)
            {
                Territory.fillColor = TerritoryColor;
                TerrainGridSystem.TerritoryToggleRegionSurface(TerritoryIndex, true, Territory.fillColor, refreshGeometry, null, Vector2.one, Vector2.one, 0.0f, true, true);
            }
        }
        #endregion

        #region get
        //计算周边城市
        public void CalcNodeNeighbours()
        {
            Neighbours.Clear();
            HashSet<BaseUnit> units1 = new HashSet<BaseUnit>();
            foreach (var item in Node.Nodes)
            {
                var temp = TerrainGridMgr.GetUnitByNode(item);
                units1.Add(temp);
            }

            foreach (var item in units1)
            {
                Neighbours.Add(item);
            }
        }
        public BaseUnit GetMainCastle()
        {
            if (IsTerritory) return null;
            return TerrainGridMgr.GetUnitByTerr(Territory);
        }
        public HashList<BaseUnit> GetAttachedCastle()
        {
            return TerrainGridMgr.GetAttachedCastles(Territory);
        }
        #endregion

        #region is
        public bool IsHaveMainCastle()
        {
            return GetMainCastle() != null;
        }
        #endregion

        #region Callback
        protected virtual void OnSetOwner(BaseUnit unit)
        {
            if (SelfBaseUnit.IsSystem) return;
            if (BaseGlobal.BattleMgr.IsLoadBattleEnd)
                UpdateTerritory(true);
        }

        #endregion
    }
}