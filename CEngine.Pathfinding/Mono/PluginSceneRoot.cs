//------------------------------------------------------------------------------
// PluginSceneRoot.cs
// Copyright 2022 2022/12/4 
// Created by CYM on 2022/12/4
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;
using System.Collections.Generic;

namespace CYM
{
    public partial class BaseSceneRoot : BaseMono
    {
        #region bake pathmap
        [FoldoutGroup(nameof(BakePathfindingMap))]
        [SerializeField]
        List<int> ObstacleSplatIndex = new List<int>();
        [FoldoutGroup(nameof(BakePathfindingMap))]
        [Button(nameof(BakePathfindingMap))]
        void BakePathfindingMap()
        {
            Texture2D texture = new Texture2D(TerrainX, TerrainZ);

            for (int i = 0; i < texture.width; ++i)
            {
                for (int j = 0; j < texture.height; ++j)
                {
                    var isObs = IsContainSplatMapInt(i, j, 0.2f, ObstacleSplatIndex);
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
            FileUtil.SaveTextureToPNG(texture, Path.Combine(SysConst.Path_ResourcesTemp, nameof(BakePathfindingMap) + ".png"));
        }
        #endregion

        #region bake collider
        [FoldoutGroup(nameof(BakeCollider))]
        [SerializeField]
        LayerMask BakeColliderLayer;
        [FoldoutGroup(nameof(BakeCollider))]
        [FoldoutGroup(nameof(BakeCollider))]
        [SerializeField, ReadOnly]
        public Array<Array<Vec3>> BakeColliderPos = new Array<Array<Vec3>>();
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
                    if (Physics.Raycast(new Ray { origin = new Vector3(i, 1000, j), direction = -Vector3.up }, out RaycastHit hitInfo, 99999, BakeColliderLayer))
                    {
                        temp = new Vec3(hitInfo.point);
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
    }
}