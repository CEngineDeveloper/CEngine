//------------------------------------------------------------------------------
// BaseTerrainGridMgr.cs
// Copyright 2023 2023/1/20 
// Created by CYM on 2023/1/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using System;
using System.IO;
using TGS;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using CYM.Diplomacy.NodeMap;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace CYM.Diplomacy
{
    [Serializable]
    public class SplatData
    {
        public int Index = 0;
        public float val = 0;
    }
    [Serializable]
    public class SplatMapIntData
    {
        public SplatMapIntData(int width, int height)
        {
            Data = new int[width, height];
        }
        public int[,] Data;
    }
    public class BaseTerrainGridMgr : BaseGFlowMgr
    {
        #region prop
        protected virtual string SplatMapIntDataFileName => "Temp/SplatMapIntData";
        SplatMapIntData SplatMapIntData;
        TerrainGridSystem TGS;
        TweenerCore<float, float, FloatOptions> mapAlphaTween;
        Tweener frontierColor;
        Tweener disputedfrontierColor;
        protected bool IsEnterUI { get; private set; } = false;
        public bool IsShowMap { get; private set; } = false;
        public bool IsHaveTSG { get; private set; } = false;
        public List<BaseTerritoryMgr> Territories { get; private set; } = new List<BaseTerritoryMgr>();
        //领土归属的主要城市
        public Dictionary<Territory, BaseUnit> TerritorieUnit { get; private set; } = new Dictionary<Territory, BaseUnit>();
        //领土归属的附属城市
        public Dictionary<Territory, HashList<BaseUnit>> TerritorieAttachedUnit { get; private set; } = new Dictionary<Territory, HashList<BaseUnit>>();
        public Dictionary<int, BaseUnit> TerritorieIndexUnit { get; private set; } = new Dictionary<int, BaseUnit>();
        public Dictionary<Node, BaseUnit> NodeUnit { get; private set; } = new Dictionary<Node, BaseUnit>();
        public Dictionary<int, string> SplatMap { get; private set; } = new Dictionary<int, string>();
        public Dictionary<int, int> SplatMapInt { get; private set; } = new Dictionary<int, int>();
        //基础地图纹理
        public HashSet<int> BaseSplatMap { get; private set; } = new HashSet<int>();
        public BinData<SplatMapIntData> SplatMapIntBinData { get; private set; } = null;
        #endregion

        #region life
        protected virtual float MapTweenDuration => 0.3f;
        protected virtual float MapAlpha => 0.7f;
        protected virtual float MapDisputedAlpha => 0.15f;
        protected virtual float MapFrontierAlpha => 0.15f;
        public int TerrainHeight => 1024;
        public int TerrainWidth => 1024;
        public int TerrainResolution => TerrainObj.Ins == null ? 0 : TerrainObj.Ins.Data.heightmapResolution;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnAffterStart()
        {
            base.OnAffterStart();
            SplatMapIntBinData = new BinData<SplatMapIntData>(SplatMapIntDataFileName);
        }
        public override void OnGameStart1()
        {
            base.OnGameStart1();
            CloseMap();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (BaseInputMgr.IsStayInUI)
            {
                if (!IsEnterUI)
                {
                    OnEnterUI();
                    IsEnterUI = true;
                }
            }
            else
            {
                IsEnterUI = false;
            }
        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            TGS = TerrainGridSystem.instance; //BaseSceneObject.TGS;
            if (TGS)
            {
                IsHaveTSG = BaseGlobal.SettingsMgr.Settings.IsTGS;
                TGS.gameObject.SetActive(IsHaveTSG);
                TGS.gameObject.layer = (int)SysConst.Layer_UI;
                TGS.cameraMain = SelfMono.GetComponentInChildren<Camera>();
                OnSetTerrainGridSystem();
            }
        }
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
            if (TGS)
            {
                TGS.OnTerritoryEnter += OnTerritoryEnter;
                TGS.OnTerritoryExit += OnTerritoryExit;
                TGS.OnTerritoryClick += OnTerritoryClick;

                TGS.OnCellEnter += OnCellEnter;
                TGS.OnCellExit += OnCellExit;
                TGS.OnCellClick += OnCellClick;
            }
            foreach (var item in Territories)
            {
                item.UpdateTerritory(false);
                item.CalcNodeNeighbours();
            }
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            if (TGS)
            {
                TGS.OnTerritoryEnter -= OnTerritoryEnter;
                TGS.OnTerritoryExit -= OnTerritoryExit;
                TGS.OnTerritoryClick -= OnTerritoryClick;

                TGS.OnCellEnter -= OnCellEnter;
                TGS.OnCellExit -= OnCellExit;
                TGS.OnCellClick -= OnCellClick;
            }
            Territories.Clear();
            TerritorieUnit.Clear();
            TerritorieIndexUnit.Clear();
            TerritorieAttachedUnit.Clear();
            NodeUnit.Clear();
        }
        #endregion

        #region get
        public BaseUnit GetUnitByTerr(Territory index)
        {
            if (index == null) return null;
            if (!TerritorieUnit.ContainsKey(index))
                return null;
            return TerritorieUnit[index];
        }
        public BaseUnit GetUnitByTerrIndex(int index)
        {
            if (!TerritorieIndexUnit.ContainsKey(index))
                return null;
            return TerritorieIndexUnit[index];
        }
        public BaseUnit GetUnitByNode(Node node)
        {
            if (!NodeUnit.ContainsKey(node))
                return null;
            return NodeUnit[node];
        }
        //containBase：是否包含基础贴图
        public List<SplatData> GetSplat(int x, int y, bool containBase = true)
        {
            if (TerrainObj.Ins == null) return new List<SplatData>();
            var realX = (TerrainResolution / TerrainWidth) * x;
            var realY = (TerrainResolution / TerrainHeight) * y;
            var map = TerrainObj.Ins.Data.GetAlphamaps(realX, realY, 1, 1);
            List<SplatData> ret = new List<SplatData>();
            for (int i = 0; i < map.GetLength(2); ++i)
                ret.Add(new SplatData { Index = i, val = map[0, 0, i] });
            if (!containBase)
            {
                ret.RemoveAll(data => BaseSplatMap.Contains(data.Index));
            }
            ret = ret.OrderByDescending(data => data.val).ToList();
            return ret;
        }
        public int GetSplatMapInt(int x, int y)
        {
            return BaseSceneRoot.Ins.GetSplatMapInt(x, y);
        }
        public int GetSplatMapIntFromBinData(int x, int y)
        {
            var data = SplatMapIntBinData.Data.Data;
            if (x < 0 || y < 0)
                return 0;
            if (x >= data.Length || y >= data.GetLength(1))
                return 0;
            return data[x, y];
        }
        #endregion

        #region set
        public void Show(bool b, bool isForce = false)
        {
            if (!IsHaveTSG)
                return;
            if (!isForce)
            {
                if (IsShowMap == b) return;
            }
            if (TGS) TGS.cameraMain = BaseGlobal.MainCamera;
            if (TGS == null) return;
            IsShowMap = b;
            if (mapAlphaTween != null) mapAlphaTween.Kill();
            if (disputedfrontierColor != null) disputedfrontierColor.Kill();
            if (frontierColor != null) frontierColor.Kill();
            //为了防止行政图内部,不发生dirty,外部强制改下alpha值
            if (TGS.colorizedTerritoriesAlpha == 0.0f)
                TGS.colorizedTerritoriesAlpha = 0.01f;
            else if (TGS.colorizedTerritoriesAlpha == 1.0f)
                TGS.colorizedTerritoriesAlpha = 0.99f;
            if (IsShowMap)
            {
                TGS.gameObject.SetActive(true);
            }
            //tween地图颜色
            mapAlphaTween = DOTween.To(() => TGS.colorizedTerritoriesAlpha, x => TGS.colorizedTerritoriesAlpha = x, b ? MapAlpha : 0.0f, MapTweenDuration);
            mapAlphaTween.OnComplete(OnMapShowed);
            //tween冲突边界
            disputedfrontierColor = DOTween.ToAlpha(() => TGS.territoryDisputedFrontierColor, x => TGS.territoryDisputedFrontierColor = x, b ? MapDisputedAlpha : 0.0f, MapTweenDuration);
            //tween地图边界
            frontierColor = DOTween.ToAlpha(() => TGS.territoryFrontiersColor, x => TGS.territoryFrontiersColor = x, b ? MapFrontierAlpha : 0.0f, MapTweenDuration);
        }
        public void CloseMap()
        {
            if (!IsHaveTSG)
                return;
            if (TGS == null) return;
            TGS.colorizedTerritoriesAlpha = 0.0f;
            Color tempColor = TGS.territoryDisputedFrontierColor;
            tempColor.a = 0.0f;
            TGS.territoryDisputedFrontierColor = tempColor;
            TGS.cellBorderAlpha = 0.2f;
            Show(false, true);
        }
        public void AddTerrDatas(BaseTerritoryMgr mgr)
        {
            //添加主要城市
            if (mgr.IsTerritory)
            {
                if (!TerritorieIndexUnit.ContainsKey(mgr.TerritoryIndex))
                    TerritorieIndexUnit.Add(mgr.TerritoryIndex, mgr.SelfBaseUnit);
                if (mgr.Territory != null)
                {
                    Territories.Add(mgr);
                    if (!TerritorieUnit.ContainsKey(mgr.Territory))
                        TerritorieUnit.Add(mgr.Territory, mgr.SelfBaseUnit);
                }
            }
            //添加附属城市
            else
            {
                if (mgr.Territory != null)
                {
                    if (!TerritorieAttachedUnit.ContainsKey(mgr.Territory))
                        TerritorieAttachedUnit.Add(mgr.Territory, new HashList<BaseUnit>());
                    TerritorieAttachedUnit[mgr.Territory].Add(mgr.SelfBaseUnit);
                }
            }
            if (mgr.Node != null)
            {
                if (!NodeUnit.ContainsKey(mgr.Node))
                    NodeUnit.Add(mgr.Node, mgr.SelfBaseUnit);
            }
        }
        public void AddSplatMap(int index, string tdid)
        {
            if (!SplatMap.ContainsKey(index)) SplatMap.Add(index, tdid);
            else SplatMap[index] = tdid;
        }
        public void AddSplatMap(int index, int tdid)
        {
            if (!SplatMapInt.ContainsKey(index)) SplatMapInt.Add(index, tdid);
            else SplatMapInt[index] = tdid;
        }
        public void AddBaseSplatMap(int index)
        {
            BaseSplatMap.Add(index);
        }
        public void ExportSplatMapIntData()
        {
            SplatMapIntData = new SplatMapIntData(TerrainWidth, TerrainHeight);
            for (int i = 0; i < TerrainWidth; ++i)
            {
                for (int j = 0; j < TerrainHeight; ++j)
                {
                    SplatMapIntData.Data[i, j] = GetSplatMapInt(i, j);
                }
            }
            FileUtil.SaveBin(Path.Combine(SysConst.Path_Resources, SplatMapIntDataFileName + ".bytes"), SplatMapIntData);
        }
        #endregion

        #region get
        public HashList<BaseUnit> GetAttachedCastles(Territory terr)
        {
            if (!TerritorieUnit.ContainsKey(terr))
                return new HashList<BaseUnit>();
            return TerritorieAttachedUnit[terr];
        }
        #endregion

        #region Callback
        protected virtual void OnSetTerrainGridSystem()
        {

        }
        private void OnMapShowed()
        {
            if (!IsShowMap && IsHaveTSG)
            {
                TGS.gameObject.SetActive(false);
            }
        }
        protected virtual void OnTerritoryClick(TerrainGridSystem sender, int territoryIndex, int buttonIndex) { }
        protected virtual void OnTerritoryExit(TerrainGridSystem sender, int territoryIndex) { }
        protected virtual void OnTerritoryEnter(TerrainGridSystem sender, int territoryIndex) { }
        protected virtual void OnCellClick(TerrainGridSystem sender, int cellIndex, int buttonIndex) { }
        protected virtual void OnCellExit(TerrainGridSystem sender, int cellIndex) { }
        protected virtual void OnCellEnter(TerrainGridSystem sender, int cellIndex) { }
        protected virtual void OnEnterUI() { }
        #endregion
    }
}