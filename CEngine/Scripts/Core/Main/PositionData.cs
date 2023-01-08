//------------------------------------------------------------------------------
// PositionData.cs
// Copyright 2020 2020/6/17 
// Created by CYM on 2020/6/17
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    #region 可序列化trans
    [Serializable]
    public struct Vec3
    {
        public float x;
        public float y;
        public float z;

        public Vec3(Vector3 v3)
        {
            x = v3.x;
            y = v3.y;
            z = v3.z;
        }

        public void Fill(Vector3 v3)
        {
            x = v3.x;
            y = v3.y;
            z = v3.z;
        }

        public Vector3 V3
        { get { return new Vector3(x, y, z); } }
    }
    [Serializable]
    public struct Qua
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Qua(Quaternion q3)
        {
            x = q3.x;
            y = q3.y;
            z = q3.z;
            w = q3.w;
        }

        public void Fill(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public Quaternion Q
        { get { return new Quaternion(x, y, z, w); } }
    }
    #endregion

    [Serializable]
    [Unobfus]
    public class PosData
    {
        public List<PosItem> Datas = new List<PosItem>();
    }
    [Serializable]
    [Unobfus]
    public class PosItem
    {
        [SerializeField]
        public string ID;
        [SerializeField]
        public Vec3 Pos;
    }
    [Unobfus]
    public class PositionData
    {
        #region prop
        protected float MapNormal { get; set; }
        protected float MapWidth { get; set; }
        protected float MapHeight { get; set; }
        public PosData PosData { get; private set; } = new PosData();
        public Dictionary<string, Vector3> DicPosData { get; private set; } = new Dictionary<string, Vector3>();
        #endregion

        #region life
        protected string PosFile = "CastlePosData";
        public PositionData(string fileName, float mapNormal, float mapWidth, float mapHeight)
        {
            PosFile = fileName;
            TextAsset ta = Resources.Load(PosFile) as TextAsset;
            Load(ta, mapNormal, mapWidth, mapHeight);
        }
        public PositionData(TextAsset ta, float mapNormal, float mapWidth, float mapHeight)
        {
            Load( ta,  mapNormal,  mapWidth,  mapHeight);
        }
        void Load(TextAsset ta, float mapNormal, float mapWidth, float mapHeight)
        {
            MapNormal = mapNormal;
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            if (ta != null)
            {
                PosData = JsonUtility.FromJson<PosData>(ta.text);
                foreach (var item in PosData.Datas)
                {
                    DicPosData.Add(item.ID, item.Pos.V3);
                }
            }
        }
        #endregion

        #region get
        public Vector2 GetMinMapPos(string tdid)
        {
            if (!DicPosData.ContainsKey(tdid))
                return new Vector2();
            Vector3 MinmapPos = DicPosData[tdid];
            Vector2 mapPos = new Vector2(MinmapPos.x / MapNormal * MapWidth, MinmapPos.z / MapNormal * MapHeight);
            return mapPos;
        }
        public Vector2 GetCastleMinMapPos(string tdid)
        {
            return GetMinMapPos(tdid);
        }
        #endregion
    }
}