//------------------------------------------------------------------------------
// TerrainObj.cs
// Created by CYM on 2022/4/15
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public class TerrainObj : SceneObj<Terrain, TerrainObj>
    {
        public TerrainData Data { get; private set; }
        protected override void Awake()
        {
            base.Awake();
            Data = Obj.terrainData;
        }

        public float GetAbsHeight(Vector3 point)
        {
            if (Obj == null) return 0;
            if (Obj.terrainData == null) return 0;
            return Obj.terrainData.GetHeight((int)point.x, (int)point.z);
        }
        public float SampleHeight(Vector3 point)
        {
            if (Obj == null) return 0;
            if (Obj.terrainData == null) return 0;
            return Obj.SampleHeight(point) + Obj.transform.position.y;
        }
    }
}