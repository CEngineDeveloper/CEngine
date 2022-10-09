using System;
using UnityEngine;

//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2020-7-16
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
// Desc     ：此代码由陈宜明于2020年编写,版权归陈宜明所有
// Copyright (c) 2020 陈宜明 All rights reserved.
//**********************************************

namespace CYM
{
    [Serializable, Unobfus]
    public class LayerData
    {
        private int Layer { get; set; } = 0;
        private LayerMask Mask { get; set; }
        public string Name { get; private set; }

        public static implicit operator LayerData(string str)
        {
            LayerData ret = new LayerData();
            ret.Name = str;
            ret.Layer = NameToLayer(str);
            ret.Mask = GetMask(str);
            return ret;
        }

        public static int GetMask(params LayerData[] ps)
        {
            if (ps == null)
            {
                return default(LayerMask);
            }
            if (ps.Length == 0)
                return default(LayerMask);
            int mask = 0;
            foreach (var item in ps)
            {
                mask |= item.Mask;
            }
            return mask;
        }

        public static explicit operator int(LayerData data)
        {
            return data.Layer;
        }

        public static explicit operator LayerMask(LayerData data)
        {
            return data.Mask;
        }

        public static explicit operator string(LayerData data)
        {
            return data.Name;
        }

        public static void SetLayerAndMask(string str, out int layer, out LayerMask mask)
        {
            layer = NameToLayer(str);
            mask = GetMask(str);
        }
        public static int NameToLayer(string layerName)
        {
            return LayerMask.NameToLayer(layerName);
        }
        public static int GetMask(params string[] layerNames)
        {
            return LayerMask.GetMask(layerNames);
        }
    }
}
