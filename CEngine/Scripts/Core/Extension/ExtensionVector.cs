using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public static class ExtensionVector
    {
        // VectorInt => Vector
        public static Vector3 Vector3(this Vector3Int v)
        {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }

        public static Vector3 Vector3(this Vector2Int v)
        {
            return new Vector3((float)v.x, (float)v.y);
        }

        public static Vector2 Vector2(this Vector2Int v)
        {
            return new Vector2((float)v.x, (float)v.y);
        }

        public static Vector2 Vector2(this Vector3Int v)
        {
            return new Vector2((float)v.x, (float)v.y);
        }
        // Vector => VectorInt
        public static Vector3Int Vector3Int(this Vector3 v)
        {
            return new Vector3Int((int)v.x, (int)v.y, (int)v.z);
        }

        public static Vector2Int Vector2Int(this Vector2 v)
        {
            return new Vector2Int((int)v.x, (int)v.y);
        }

        public static Vector2Int Vector2Int(this Vector3 v)
        {
            return new Vector2Int((int)v.x, (int)v.y);
        }
        //Snap
        public static Vector3 Snap(this Vector3 v)
        {
            return new Vector3((int)v.x, (int)v.y, (int)v.z);
        }
    }
}