using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationStudio;
#endif

//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    [ExecuteInEditMode,HideMonoScript]
    public class BaseSceneRoot : BaseMono
    {
        #region inspector
        [FoldoutGroup("Active"), SerializeField, SceneObjectsOnly]
        protected GameObject[] EnableOnPlay;
        [FoldoutGroup("Active"), SerializeField, SceneObjectsOnly]
        protected GameObject[] DisableOnPlay;
        [FoldoutGroup("Active"), SerializeField, AssetsOnly]
        protected GameObject[] InstantiateOnPlay;
        [FoldoutGroup("Root"), SerializeField, SceneObjectsOnly]
        public Transform RootPoints;
        #endregion

        #region prop
        public static BaseSceneRoot Ins { get; protected set; }
        public List<Transform> Points { get; private set; } = new List<Transform>();
        protected Dictionary<string, Transform> PointsDic { get; private set; } = new Dictionary<string, Transform>();
        public int TerrainX => Terrain.activeTerrain == null ? 0 : (int)Terrain.activeTerrain.terrainData.size.x;
        public int TerrainZ => Terrain.activeTerrain == null ? 0 : (int)Terrain.activeTerrain.terrainData.size.z;
        public int TerrainResolution=> Terrain.activeTerrain == null ? 0 : (int)Terrain.activeTerrain.terrainData.heightmapResolution;
        #endregion

        #region life
        public override void Awake()
        {
            Ins = this;
            base.Awake();
            Parse();
        }
        #endregion

        #region bake pathmap
        [FoldoutGroup(nameof(BakePathfindingMap))][SerializeField]
        List<int> ObstacleSplatIndex = new List<int>();        
        [FoldoutGroup(nameof(BakePathfindingMap))][Button(nameof(BakePathfindingMap))]
        void BakePathfindingMap()
        {
            Texture2D texture = new Texture2D(TerrainX, TerrainZ);

            for (int i = 0; i < texture.width; ++i)
            {
                for (int j = 0; j < texture.height; ++j)
                {
                    var isObs = IsContainSplatMapInt(i, j,0.2f, ObstacleSplatIndex);
                    if (isObs)
                    {
                        texture.SetPixel(i, j, Color.black);
                    }
                    else
                    {
                        texture.SetPixel(i, j, Color.white);
                    }
                }
            }
            texture.Apply();
            FileUtil.SaveTextureToPNG(texture, Path.Combine(SysConst.Path_ResourcesTemp, nameof(BakePathfindingMap)+".png"));
        }
        #endregion

        #region bake collider
        [FoldoutGroup(nameof(BakeCollider))]
        [SerializeField]
        LayerMask BakeColliderLayer;
        [FoldoutGroup(nameof(BakeCollider))]
        [FoldoutGroup(nameof(BakeCollider))]
        [SerializeField,ReadOnly]
        public Array<Array<Vec3>> BakeColliderPos =new Array<Array<Vec3>>();
        [FoldoutGroup(nameof(BakeCollider))]
        [Button(nameof(BakeCollider))]
        void BakeCollider()
        {
            BakeColliderPos = new Array<Array<Vec3>>();
            
            for (int i = 0; i < TerrainX; ++i)
            {
                BakeColliderPos.Add(new Array<Vec3>());
                for (int j = 0; j < TerrainZ; ++j)
                {
                    Vec3 temp = new Vec3();
                    if (Physics.Raycast(new Ray {origin = new Vector3(i, 1000, j),direction =  -Vector3.up }, out RaycastHit hitInfo, 99999, BakeColliderLayer))
                    {
                        temp = new Vec3( hitInfo.point);
                    }
                    else
                    {
                        temp = new Vec3(SysConst.VEC_Inv);
                    }
                    BakeColliderPos[i].Add(temp);
                }
            }
        }
        public Vector3 GetBakedColliderPos(BaseMono mono)
        {
            return GetBakedColliderPos(mono.Pos);
        }
        public Vector3 GetBakedColliderPos(Vector3 mono)
        {
            if (BakeColliderPos.Count <= 0)
                return SysConst.VEC_Inv;
            var data = BakeColliderPos[(int)mono.x][(int)mono.z].V3;
            return data;
        }
        #endregion

        #region Vegetation
        #if VEGETATION_STUDIO_PRO
        [Button]
        void ExportVegetation()
        {
            Dictionary<Vector2Int, PersistentVegetationItem> PersistentVegetations = new Dictionary<Vector2Int, PersistentVegetationItem>();
            VegetationStudioManager Vegetation = VegetationStudioManager.Instance;
            AdjVegetation();
            Texture2D texture = new Texture2D(TerrainX, TerrainZ);
            texture.DrawBG(Color.black);
            foreach (var item in PersistentVegetations)
            {
                texture.DrawFilledCircle(item.Key.x, TerrainZ - item.Key.y, 1, Color.white);
            }
            texture.Apply();
            FileUtil.SaveTextureToPNG(texture, Path.Combine(Const.Path_ResourcesTemp, "Vegetation.png"));

            Vector3 PositionVegetationItem(Vector3 position, PersistentVegetationStorage storeage)
            {
                Ray ray = new Ray(position + new Vector3(0, 2000f, 0), Vector3.down);

                var hits = Physics.RaycastAll(ray).OrderBy(h => h.distance).ToArray();
                for (int i = 0; i <= hits.Length - 1; i++)
                {
                    if (hits[i].collider is TerrainCollider ||
                        storeage.GroundLayerMask.Contains(hits[i].collider.gameObject.layer))
                    {
                        return hits[i].point;
                    }
                }

                return position;
            }
            void AdjVegetation()
            {
                if (Application.isEditor)
                {
                    PersistentVegetations.Clear();
                    if (Vegetation != null && Vegetation.VegetationSystemList.Count > 0)
                    {
                        foreach (var System in Vegetation.VegetationSystemList)
                        {
                            foreach (var Storage in System.PersistentVegetationStorage.PersistentVegetationStoragePackage.PersistentVegetationCellList)
                            {
                                foreach (var info in Storage.PersistentVegetationInfoList)
                                {
                                    for (int j = info.VegetationItemList.Count - 1; j >= 0; j--)
                                    {
                                        var item = info.VegetationItemList[j];
                                        Vector3 newPos = PositionVegetationItem(item.Position, System.PersistentVegetationStorage);
                                        item.Position = newPos - System.PersistentVegetationStorage.VegetationSystemPro.VegetationSystemPosition;
                                        Vector2Int intV2 = new Vector2Int((int)item.Position.x, (int)item.Position.z);
                                        if (!PersistentVegetations.ContainsKey(intV2))
                                        {
                                            PersistentVegetations.Add(intV2, item);
                                        }
                                        else
                                        {
                                            PersistentVegetations[intV2] = item;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }
        #endif
        #endregion

        #region set
        [Button("Parse")]
        protected virtual void Parse()
        {
            if (Application.isPlaying)
            {
                if (InstantiateOnPlay != null && BaseGlobal.BattleMgr != null)
                {
                    foreach (var item in InstantiateOnPlay)
                    {
                        if (item == null)
                            continue;
                        var tempGO = Instantiate(item);
                        SceneManager.MoveGameObjectToScene(tempGO, BaseGlobal.BattleMgr.SceneSelf);
                    }
                }
                if (EnableOnPlay != null)
                {
                    foreach (var item in EnableOnPlay)
                    {
                        if (item == null)
                            continue;
                        item.SetActive(true);
                    }
                }
                if (DisableOnPlay != null)
                {
                    foreach (var item in DisableOnPlay)
                    {
                        if (item == null)
                            continue;
                        item.SetActive(false);
                    }
                }
            }
            if (RootPoints != null)
            {
                Points.Clear();
                PointsDic.Clear();
                Points.AddRange(RootPoints.GetComponentsInChildren<Transform>());
                if (Points.Count > 0) Points.RemoveAt(0);
                foreach (var item in Points)
                {
                    if (PointsDic.ContainsKey(item.name)) 
                        continue;
                    PointsDic.Add(item.name, item);
                }
            }
        }
        #endregion

        #region get
        public Vector3 GetInterpolatedNormal(int x, int z)
        {
            if (TerrainObj.Ins == null)
                throw new Exception("ActiveTerrain == null");
            return TerrainObj.Ins.Data.GetInterpolatedNormal(x, z);
        }
        /// <summary>
        /// 获得出身点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Transform GetPoint(int index = 0)
        {
            if (Points.Count <= 0) return null;
            if (Points.Count <= index) return Points[0];
            return Points[index];
        }
        /// <summary>
        /// 获得位置点
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Transform GetPoint(string name)
        {
            if (!PointsDic.ContainsKey(name)) return null;
            return PointsDic[name];
        }
        public int GetSplatMapInt(int x, int y)
        {
            if (Terrain.activeTerrain == null)
                return 0;
            var realX = (TerrainResolution / (int)TerrainX) * x;
            var realY = (TerrainResolution / (int)TerrainZ) * y;
            var splatMap = Terrain.activeTerrain.terrainData.GetAlphamaps(realX, realY, 1, 1);
            int index = 0;
            float val = 0;
            for (int i = 0; i < splatMap.GetLength(2); ++i)
            {
                if (splatMap[0, 0, i] > val)
                {
                    val = splatMap[0, 0, i];
                    index = i;
                }
            }
            return index;
        }
        public bool IsContainSplatMapInt(int x, int y,float val,List<int> indexes)
        {
            if (Terrain.activeTerrain == null)
                return false;
            var realX = (TerrainResolution / (int)TerrainX) * x;
            var realY = (TerrainResolution / (int)TerrainZ) * y;
            var splatMap = Terrain.activeTerrain.terrainData.GetAlphamaps(realX, realY, 1, 1);
            for (int i = 0; i < splatMap.GetLength(2); ++i)
            {
                if (splatMap[0, 0, i] > val)
                {
                    if (indexes.Contains(i))
                        return true;
                }
            }
            return false;
        }
        #endregion
    }

}