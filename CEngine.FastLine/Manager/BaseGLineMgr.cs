//------------------------------------------------------------------------------
// BasePathRenderMgr.cs
// Copyright 2019 2019/4/18 
// Created by CYM on 2019/4/18
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.Pool;
using DigitalRuby.FastLineRenderer;
using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Line
{
    #region class
    //Const 路径绘制基类
    public class BaseTexConstPathDrawer
    {
        #region mgr
        BaseGlobal BaseGlobal => BaseGlobal.Ins;
        BaseGRMgr GRMgr => BaseGlobal.GRMgr;
        #endregion

        protected List<GraphNode> LastedNodes = new List<GraphNode>();
        protected GameObject ConstantPathProjectObj;
        protected Color ConstantPathMapColor;

        protected string ConstantPathPrefab;
        protected virtual float ConstantPathProjectSize => 15;
        protected virtual int ConstantPathNodeSize => 10;
        protected virtual int ConstantPathMapSize => 512;
        protected Texture2D ConstantPathTex;

        protected int Offset;
        protected int NodeSize;

        public virtual void Init(Color c, string prefab)
        {
            ConstantPathPrefab = prefab;
            ConstantPathMapColor = c;
            ConstantPathProjectObj = GRMgr.GetResources(ConstantPathPrefab, true);

            ConstantPathTex = new Texture2D(ConstantPathMapSize, ConstantPathMapSize, TextureFormat.RGBA32, true);
            ConstantPathTex.filterMode = FilterMode.Bilinear;
            ConstantPathTex.wrapMode = TextureWrapMode.Clamp;
            ConstantPathTex.anisoLevel = 1;

            Offset = (int)(ConstantPathMapSize / 2 / 2.0f);
            NodeSize = (int)(ConstantPathNodeSize / 2.0f);
        }

        public virtual void Draw(List<GraphNode> nodes, Vector3? center = null)
        {
            if (nodes == null)
            {
                ConstantPathProjectObj?.SetActive(false);
                return;
            }
            ConstantPathProjectObj?.SetActive(true);

            if (center.HasValue)
                ConstantPathProjectObj.transform.position = center.Value;

            LastedNodes = nodes;
            foreach (var item in nodes)
            {
                ApplyNode(item);
            }
            SetTexture();
            ConstantPathTex.Apply();
        }

        protected virtual void SetTexture() { }
        protected virtual void ApplyNode(GraphNode item)
        {
        }
    }
    //Const 纹理路径绘制(投影)
    public class TexProjectConstPathDrawer : BaseTexConstPathDrawer
    {
        protected override int ConstantPathMapSize => 1024;
        protected override float ConstantPathProjectSize => 30;
        protected Projector ConstantPathProjectCom;
        //protected string ConstantPathPrefab;
        protected Texture2D SourceTex;
        protected Color[] SourceCol;

        public override void Init(Color c, string prefab)
        {
            base.Init(c, prefab);
            if (ConstantPathProjectObj)
            {
                ConstantPathProjectCom = ConstantPathProjectObj.GetComponentInChildren<Projector>();
                ConstantPathProjectCom.orthographicSize = ConstantPathProjectSize;
                ConstantPathProjectCom.material = GameObject.Instantiate(ConstantPathProjectCom.material);
                SourceTex = BaseGlobal.RsTexture.Get("HexTile2");
                SourceCol = SourceTex.GetPixels();
            }
        }
        protected override void SetTexture()
        {
            base.SetTexture();
            ConstantPathProjectCom.material.SetTexture("_ShadowTex", ConstantPathTex);
        }
        protected override void ApplyNode(GraphNode item)
        {
            Vector3 pos = (Vector3)item.position;
            Vector3 localPos = ConstantPathProjectObj.transform.InverseTransformPoint(pos);
            var halfSize = ConstantPathMapSize / 2 / 2;
            var x = (int)(localPos.x / (ConstantPathProjectSize / 2) * halfSize) + halfSize;
            var y = (int)(localPos.z / (ConstantPathProjectSize / 2) * halfSize) + halfSize;
        }
    }
    //Const Prefab路径绘制(Prefab)
    public class PrefabConstPathDrawer
    {
        #region mgr
        BaseGlobal BaseGlobal => BaseGlobal.Ins;
        BaseGRMgr GRMgr => BaseGlobal.GRMgr;
        #endregion

        List<GraphNode> PathNodes;
        GOPool Pool;
        public bool IsShow
        {
            get
            {
                if (ConstantPathProjectObj == null)
                    return false;
                return ConstantPathProjectObj.activeSelf;
            }
        }
        public bool IsDraw
        {
            get
            {
                if (PathNodes == null)
                    return false;
                return true;
            }
        }
        protected GameObject ConstantPathProjectObj;
        protected string ConstantPathPrefab;

        public virtual void Init(Color c,string prefab)
        {
            ConstantPathPrefab = prefab;
            ConstantPathProjectObj = new GameObject("PrefabConstPathDrawer");
            ConstantPathProjectObj.transform.SetParent(BaseGlobal.Ins.transform);
            var tempPrefab = GRMgr.GetResources(ConstantPathPrefab, true);
            if (tempPrefab)
            {
                tempPrefab.hideFlags = HideFlags.HideInHierarchy;
                Pool = new GOPool(tempPrefab, ConstantPathProjectObj.transform, true);
            }
        }
        public void Show(bool b)
        {
            if (IsShow == b)
                return;
            //IsShow = b;
            if (ConstantPathProjectObj == null) return;
            ConstantPathProjectObj.SetActive(b);
        }
        public virtual void Draw(List<GraphNode> nodes, Vector3? center = null)
        {
            PathNodes = nodes;
            if (ConstantPathProjectObj == null)
                return;
            if (nodes == null)
            {
                ConstantPathProjectObj.transform.position = SysConst.VEC_FarawayPos;
                return;
            }

            if (center.HasValue)
                ConstantPathProjectObj.transform.position = center.Value;

            Pool.DespawnAll();
            foreach (var item in nodes)
            {
                var go = Pool.Spawn();
                go.transform.position = (Vector3)item.position;
            }

        }
    }
    #endregion

    public class BaseGLineMgr : BaseGFlowMgr
    {
        #region virtual
        protected override string ResourcePrefabKey => SysConst.STR_Inv;
        protected virtual string ConstantPathPrefab => SysConst.STR_Inv;
        #endregion

        #region prop
        BaseCameraMgr BaseCameraMgr => BaseGlobal.CameraMgr;
        protected FastLineRenderer LineTemp;
        protected ObjPool<FastLineRenderer> LinePool;
        protected TexProjectConstPathDrawer TexConstant { get; private set; } = new TexProjectConstPathDrawer();
        protected PrefabConstPathDrawer PrefabConstant { get; private set; } = new PrefabConstPathDrawer();
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (BaseCameraMgr != null && 
                BattleMgr.IsInBattle)
            {
                if (BaseCameraMgr.ScrollVal >= 0.5f)
                {
                    PrefabConstant?.Show(false);
                }
                else PrefabConstant?.Show(true);
            }
        }
        protected override void OnAllLoadEnd2()
        {
            base.OnAllLoadEnd2();
            if (ResourceObj != null)
            {
                LineTemp = ResourceObj.GetComponent<FastLineRenderer>();
                LineTemp.Camera = BaseGlobal.MainCamera;
                LineTemp.UseWorldSpace = false;
                LinePool = new ObjPool<FastLineRenderer>(CreateLineRender);
            }
            if (!ConstantPathPrefab.IsInv())
            {
                PrefabConstant?.Init(new Color(0, 0.3f, 0, 1f), ConstantPathPrefab);
            }

            FastLineRenderer CreateLineRender()
            {
                if (LineTemp == null)
                {
                    CLog.Error("lineRedner为null无法创建！");
                    return null;
                }
                return FastLineRenderer.CreateWithParent(ResourceObj, LineTemp);
            }
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            PrefabConstant?.Draw(null);
            if (LinePool != null)
            {
                foreach (var item in LinePool.Stack)
                {
                    item.Reset();
                }
                foreach (var item in LinePool.Stack)
                {
                    Object.DestroyImmediate(item.gameObject);
                }
            }
        }
        #endregion

        #region set
        public void DrawTexConstant(List<GraphNode> nodes, Vector3? center = null)
        {
            TexConstant?.Draw(nodes, center);
        }
        public void DrawPrefabConstant(List<GraphNode> nodes, Vector3? center = null)
        {
            PrefabConstant?.Draw(nodes, center);
        }
        public FastLineRenderer DrawPath(List<Vector3> points, Color color, float radius=0.1f,float endCapScale=1.0f)
        {
            if (ResourceObj == null)
            {
                CLog.Error("ResourceObj 为 null");
                return null;
            }
            if (points == null)
            {
                CLog.Error("vector Point 为 null");
                return null;
            }

            FastLineRenderer lr = LinePool.Get();
            color.a = 0.5f;
            FastLineRendererProperties props = new FastLineRendererProperties();
            props.Color = color;
            props.Radius = radius;
            props.LineJoin = FastLineRendererLineJoin.Round;
            props.End = points[points.Count-1];
            lr.EndCapScale = endCapScale;
            lr.UseWorldSpace = false;
            lr.Camera = BaseGlobal.MainCamera;
            lr.TintColor = color;
            lr.AppendSpline(props, points, points.Count*4, FastLineRendererSplineFlags.EndCap);
            lr.hideFlags = HideFlags.HideInHierarchy;
            lr.Apply();
            return lr;
        }
        public void ClearPath(ref FastLineRenderer r)
        {
            if (r == null)
            {
                return;
            }
            r.Reset();
            LinePool.Recycle(r);
            r = null;
        }
        #endregion

        #region is
        public bool IsShowPrefabConstPath
        {
            get
            {
                if (PrefabConstant == null)
                    return false;
                return PrefabConstant.IsShow;
            }
        }
        public bool IsDrawPrefabConstPath
        {
            get
            {
                if (PrefabConstant == null)
                    return false;
                return PrefabConstant.IsDraw;
            }
        }
        #endregion

        #region Callback
        protected override void OnBattleUnLoaded()
        {
            base.OnBattleUnLoaded();
        }
        #endregion
    }
}