//------------------------------------------------------------------------------
// BaseTerrainColorMgr.cs
// Copyright 2019 2019/12/31 
// Created by CYM on 2019/12/31
// Owner: CYM
// 通过一张颜色图,来获得某个坐标点的颜色
//------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    [Unobfus]
    public class TextureColorData<T>
    {
        #region prop
        protected Dictionary<Color, T> Data { get; set; } = new Dictionary<Color, T>();
        public virtual Texture2D ColorMap { get; protected set; }
        public string Path { get; set; } = "";
        #endregion

        #region life
        public TextureColorData(string res)
        {
            if (res != null)
                LoadTexture(res);
        }
        public TextureColorData(Texture2D texture2D)
        {
            ColorMap = texture2D;
        }
        #endregion

        #region set
        protected Texture2D LoadTexture(string path)
        {
            Path = path;
            ColorMap = Resources.Load<Texture2D>(path);
            ColorMap.filterMode = FilterMode.Point;
            ColorMap.Apply();
            return ColorMap;
        }
        public void AddConfig(Color col, T val)
        {
            Data.Add(col, val);
        }
        public void AddConfig(string colStr, T val)
        {
            Color color = new Color();
            ColorUtility.TryParseHtmlString(colStr, out color);
            if (Data.ContainsKey(color))
            {
                Debug.LogError(string.Format("重复了Key:{0},{1}", colStr, color));
                return;
            }
            Data.Add(color, val);
        }
        #endregion

        #region get
        public T GetConfig(int x, int y)
        {
            var col = GetColor(x, y);
            if (Data.ContainsKey(col))
            {
                return Data[col];
            }
            else
            {
                Debug.LogError(string.Format("{4},错误!没有此颜色的配置:{0},Hex:{1} 坐标:{2},{3}", col, ColToStr(col), x, y, Path));
                return default;
            }
        }
        public Color GetColor(int x, int y)
        {
            if (ColorMap == null) return Color.black;
            var col = ColorMap.GetPixel(x, y);
            return col;
        }
        #endregion

        static string ColToStr(Color32 color)
        {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
        }
    }
}