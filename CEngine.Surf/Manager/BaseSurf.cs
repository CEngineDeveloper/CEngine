//**********************************************
// Class Name	: BaseSurface
// Discription	：地表贴图类
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
// 物件名称后缀加上_eff就可以忽略
//**********************************************
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace CYM.Surf
{
    public class BaseSurf
    {
        #region mgr
        BaseGRMgr GRMgr => BaseGlobal.GRMgr;
        public HashSet<Shader> Shaders { get; private set; } = new HashSet<Shader>();
        protected Renderer[] ModelRenders { get;private set; }//模型自身的渲染器
        protected Material DefaultUsedMaterial { get; private set; }
        protected Dictionary<Shader, Material> UsedMaterials = new Dictionary<Shader, Material>();
        protected Dictionary<Renderer, Material> SourceMats = new Dictionary<Renderer, Material>();
        protected Dictionary<Renderer, Material[]> SourceMatsArray = new Dictionary<Renderer, Material[]>();
        protected ISurfaceMgr<BaseModel> SurfaceMgr;
        public bool IsInstance { get; private set; }
        #endregion

        ~BaseSurf()
        {
            if (UsedMaterials != null)
            {
                foreach (var item in UsedMaterials)
                    GameObject.Destroy(item.Value);
            }
        }

        #region init
        public void InitByMgr(ISurfaceMgr<BaseModel> mgr, string matName = "", bool isInstance = true)
        {
            IsInstance = isInstance;
            SurfaceMgr = mgr;
            SourceMats.Clear();
            SourceMatsArray.Clear();
            Shaders.Clear();

            var renders = GetRenderers();

            if (renders != null)
            {
                for (int i = 0; i < renders.Length; ++i)
                {
                    GetMaterial(renders[i],out Material material);
                    SourceMats.Add(renders[i], material);
                }

                for (int i = 0; i < renders.Length; ++i)
                {
                    GetMaterials(renders[i],out Material[] materials);
                    SourceMatsArray.Add(renders[i], materials);
                    if (materials != null)
                    {
                        foreach (var item in materials)
                        {
                            if(!Shaders.Contains(item.shader))
                                Shaders.Add(item.shader);
                        }
                    }
                }
                //获得材质
                string tempMatName = matName.IsInv() ? GetDefaultMatName() : matName;
                if (UsedMaterials.Count == 0 && !tempMatName.IsInv())
                {
                    foreach (var key in Shaders)
                    {
                        Material material = BaseGlobal.RsMaterial.Spawn(tempMatName);
                        if (DefaultUsedMaterial == null)
                            DefaultUsedMaterial = material;
                        UsedMaterials.Add(key, material);
                    }
                }
            }
            OnInit();
        }
        public void InitByGO(GameObject go,string matName = "",bool isInstance=true)
        {
            ModelRenders = go.GetComponentsInChildren<Renderer>();
            if (ModelRenders != null)
            {
                if (ModelRenders != null)
                    ModelRenders = ModelRenders.Where((x) =>
                    !(x is ParticleSystemRenderer) &&
                    !x.gameObject.name.EndsWith(SysConst.Suffix_Eff)).ToArray();
            }
            InitByMgr(null,matName, isInstance);
        }
        public void InitByUnit(BaseUnit unit,string matName = "", bool isInstance = true)
        {
            InitByGO(unit.GO,matName, isInstance);
        }
        protected virtual void OnInit()
        { 
        
        }
        #endregion

        #region set
        public virtual void SetParam(params object[] param)
        {
            throw new NotImplementedException("必须重载此函数,设置用户自定义参数");
        }
        protected virtual void CopyProp(Material preMat, Material mat)
        {
            throw new NotImplementedException("必须重载此函数,拷贝材质各类参数,一般复制MainTex");
        }
        public virtual string GetDefaultMatName()
        {
            throw new NotImplementedException("必须重载此函数,默认材质");
        }
        #endregion

        #region get
        private void GetMaterial(Renderer render,out Material material)
        {
            if (IsInstance)
                material = render.material;
             else
                material = render.sharedMaterial;
        }
        private void GetMaterials(Renderer render, out Material[] materials)
        {
            if (IsInstance)
                materials = render.materials;
            else
                materials = render.sharedMaterials;
        }
        #endregion

        #region use & revert
        public virtual void Revert()
        {
            var renders = GetRenderers();
            if (renders != null)
            {
                foreach (var item in renders)
                {
                    item.materials = SourceMatsArray[item];
                }
            }
        }
        public virtual void Use()
        {
            if (SurfaceMgr != null)
                SurfaceMgr.CurSurface = this;
            var renders = GetRenderers();
            if (renders != null && UsedMaterials != null)
            {
                for (int i = 0; i < renders.Length; ++i)
                {
                    GetMaterials(renders[i],out Material[] materials);
                    if (materials != null && materials.Length>0)
                    {
                        int count = materials.Length;
                        Material[] matArray = new Material[count];
                        Material[] matSourceArray = new Material[count];
                        for (int j = 0; j < materials.Length; ++j)
                        {
                            matArray[j] = GetUsedMaterial(materials[j].shader);//UsedMaterials[materials[j].shader];
                            matSourceArray[j] = materials[j];        
                        }
                        renders[i].materials = matArray;
                        Util.For(count,(index)=> {
                            CopyProp(matSourceArray[index], matArray[index]);
                        });
                        
                    }
                }
            }
        }
        #endregion

        #region other
        public virtual void Fade(float alpha)        
        {
            throw new NotImplementedException("必须重载此函数");
        }
        public virtual void FadeOut() 
        {
            Fade(1.0f);
        }
        public virtual void FadeIn() 
        {
            Fade(0.0f);
        }
        public virtual void Update() { }
        #endregion

        #region 工具
        protected virtual void ForeachMaterial(Callback<Material> setMaterial)
        {
            var renders = GetRenderers();
            if (renders != null)
            {
                for (int i = 0; i < renders.Length; ++i)
                {
                    if (renders[i].materials == null)
                        continue;
                    for(int j=0;j<renders[i].materials.Length;++j)
                        setMaterial(renders[i].materials[j]);
                }
            }
        }
        protected Renderer[] GetRenderers()
        {
            if (SurfaceMgr != null)
                return SurfaceMgr.ModelRenders;
            return ModelRenders;
        }
        Material GetUsedMaterial(Shader shader)
        {
            if (UsedMaterials.ContainsKey(shader))
                return UsedMaterials[shader];
            return DefaultUsedMaterial;
        }
        #endregion
    }
}