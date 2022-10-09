//**********************************************
// Class Name	: CYMBaseSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
// 物件名称后缀加上_eff就可以忽略
//**********************************************
using HighlightPlus;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace CYM.Surf
{
    public interface ISurfaceMgr<out TModel>
    {
        #region prop
        Renderer[] ModelRenders { get; }
        SkinnedMeshRenderer[] SkinnedMeshRenderers { get; }
        SkinnedMeshRenderer MainSkinnedMesh { get; }
        GameObject GOModel { get; }
        public TModel Model { get; }
        SurfSource SurfSource { get; }
        BaseSurf CurSurface { get; set; }
        #endregion

        #region is
        bool IsEnableRenders { get; }
        bool IsUseSurfaceMaterial { get; }
        #endregion

        #region set
        public void EnableRender(bool enable);
        public void SetShadowMode(ShadowCastingMode mode);
        public void EnableReceiveShadows(bool b);
        #endregion
    }
    public class BaseSurfaceMgr<TModel> : BaseMgr, ISurfaceMgr<TModel>
        where TModel : BaseModel
    {
        #region member variable
        public Renderer[] ModelRenders { get; protected set; }//模型自身的渲染器
        public SkinnedMeshRenderer[] SkinnedMeshRenderers { get; protected set; }//蒙皮渲染
        public SkinnedMeshRenderer MainSkinnedMesh { get; protected set; }//主要的蒙皮
        public GameObject GOModel { get; protected set; }//模型自身的渲染器的跟节点
        public TModel Model { get; protected set; }
        public bool IsEnableRenders { get; private set; }
        public virtual bool IsUseSurfaceMaterial => false; //禁用材质效果,这样可以使用GPUInstance
        protected virtual bool IsUseHighlighter => false;
        #endregion

        #region Highlighter
        private HighlightEffect highlighter;
        protected Dictionary<string, HighlightProfile> HighlightProfileObjs = new Dictionary<string, HighlightProfile>();
        #endregion

        #region property
        public SurfSource SurfSource { get; private set; } = new SurfSource();
        public BaseSurf CurSurface { get; set; }
        #endregion

        #region life
        public virtual string HighlightProfile => "HighlightProfile";
        public sealed override MgrType MgrType => MgrType.Unit;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedFixedUpdate = true;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            SelfBaseUnit.Callback_OnBeSelected += OnBeSelected;
            SelfBaseUnit.Callback_OnUnBeSelected += OnUnBeSelected;
            SelfBaseUnit.Callback_OnMouseEnter += OnMouseEnter;
            SelfBaseUnit.Callback_OnMouseExit += OnMouseExit;
            SelfBaseUnit.Callback_OnSetOwner += OnSetOwner;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            SelfBaseUnit.Callback_OnBeSelected += OnBeSelected;
            SelfBaseUnit.Callback_OnUnBeSelected += OnUnBeSelected;
            SelfBaseUnit.Callback_OnMouseEnter -= OnMouseEnter;
            SelfBaseUnit.Callback_OnMouseExit -= OnMouseExit;
            SelfBaseUnit.Callback_OnSetOwner -= OnSetOwner;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            AssianModel();
            if (GOModel == null)
            {
                CLog.Error("Unit 没有model");
                return;
            }
            ModelRenders = GOModel.GetComponentsInChildren<Renderer>();
            {
                if (ModelRenders != null)
                    ModelRenders = ModelRenders.Where((x) =>
                    !(x is ParticleSystemRenderer) &&
                    !x.gameObject.name.EndsWith(SysConst.Suffix_Eff)).ToArray();
            }
            SkinnedMeshRenderers = GOModel.GetComponentsInChildren<SkinnedMeshRenderer>();
            {
                if (SkinnedMeshRenderers != null)
                    SkinnedMeshRenderers = SkinnedMeshRenderers.Where((x) =>
                    !x.gameObject.name.EndsWith(SysConst.Suffix_Eff)).ToArray();
                float lastSize = 0.0f;
                float curSize = 0.0f;
                foreach (var item in SkinnedMeshRenderers)
                {
                    Vector3 extents = item.bounds.extents;
                    curSize = extents.x + extents.y + extents.z;
                    if (curSize > lastSize)
                    {
                        lastSize = curSize;
                        MainSkinnedMesh = item;
                    }
                }
            }
            IsEnableRenders = true;
            if (IsUseSurfaceMaterial)
            {
                SurfSource.InitByMgr(this);
            }
            if (IsUseHighlighter)
            {
                HighlightProfile temp = null;
                if (HighlightProfileObjs.ContainsKey(HighlightProfile))
                    temp = HighlightProfileObjs[HighlightProfile];
                else
                {
                    temp = Resources.Load<HighlightProfile>(HighlightProfile);
                    HighlightProfileObjs.Add(HighlightProfile, temp);
                }
                if (temp == null)
                    CLog.Error("没有创建:" + HighlightProfile);

                highlighter = SelfMono.EnsureComponet<HighlightEffect>();
                highlighter.ProfileLoad(temp);
            }
        }
        public override void OnBirth()
        {
            base.OnBirth();
            if (IsUseSurfaceMaterial)
            {
                SurfSource.Use();
            }
            CloseHighlight();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (IsUseSurfaceMaterial)
            {
                CurSurface?.Update();
            }
        }
        protected virtual void AssianModel()
        {
            var temp = SelfMono.transform.Find("Model");
            if (temp != null)
                GOModel = temp.gameObject;
            if (GOModel == null)
                GOModel = SelfMono.GO;
            Model = GOModel.GetComponent<TModel>();
        }
        public virtual void EnableRender(bool enable)
        {
            if (IsEnableRenders == enable)
                return;
            if (ModelRenders != null)
            {
                for (int i = 0; i < ModelRenders.Length; ++i)
                    ModelRenders[i].enabled = enable;
                IsEnableRenders = enable;
            }
        }
        public virtual void SetShadowMode(ShadowCastingMode mode)
        {
            if (ModelRenders != null)
            {
                foreach (var item in ModelRenders)
                {
                    item.shadowCastingMode = mode;
                }
            }
        }
        public void EnableReceiveShadows(bool b)
        {
            if (ModelRenders != null)
            {
                foreach (var item in ModelRenders)
                {
                    item.receiveShadows = b;
                }
            }
        }
        #endregion

        #region set
        public virtual void ShowSelectEffect(bool b)
        {
            throw new System.NotImplementedException();
        }
        public virtual void ShowHint(bool b)
        {

        }
        #endregion

        #region highlight
        public void ShowHighlight(Color col)
        {
            if (highlighter)
            {
                if (BaseGlobal.MainCamera == null) return;
                if (BaseGlobal.MainCamera.orthographic) return;
                highlighter.glowHQColor = col;
                highlighter.outlineColor = col;
                highlighter.SetHighlighted(true);
            }
        }
        public void CloseHighlight()
        {
            if (highlighter)
            {
                if (BaseGlobal.MainCamera == null) return;
                if (BaseGlobal.MainCamera.orthographic) return;
                highlighter.SetHighlighted(false);
            }
        }
        #endregion

        #region Callback
        protected virtual void OnBeSelected(bool arg1)
        {
            ShowSelectEffect(true);
        }
        protected virtual void OnUnBeSelected()
        {
            ShowSelectEffect(false);
        }
        protected virtual void OnMouseEnter()
        {

        }
        protected virtual void OnMouseExit()
        {
            CloseHighlight();
        }
        protected virtual void OnSetOwner(BaseUnit owner)
        {
        }
        #endregion
    }
}
