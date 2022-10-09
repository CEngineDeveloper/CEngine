using UnityEngine;

namespace CYM
{
    public static class Extension
    {
        public static bool IsInv(this long i) => i == SysConst.LONG_Inv;
        public static bool IsInv(this int i) => i == SysConst.INT_Inv;
        public static bool IsInv(this float f) => f == SysConst.FLOAT_Inv;
        public static bool IsValid(this long i) => i != SysConst.LONG_Inv;
        public static bool IsValid(this int i) => i != SysConst.INT_Inv;
        public static bool IsValid(this float f) => f != SysConst.FLOAT_Inv;

        public static bool IsNone(this string str)
        {
            if (str == null) return true;
            return str == SysConst.STR_None;
        }
        public static bool IsUnknow(this string str)
        {
            if (str == null) return true;
            return str == SysConst.STR_Unkown;
        }
        public static bool IsInv(this string str)
        {
            if (str == null) return true;
            return str == SysConst.STR_Inv || str == SysConst.STR_None || str == SysConst.STR_Unkown || str == string.Empty || str == "";
        }
        public static bool IsValid(this string str) => !IsInv(str);
        public static bool IsInv(this Vector3 pos)
        {
            if (pos == SysConst.VEC_GlobalPos ||
                pos == SysConst.VEC_FarawayPos)
                return true;
            return false;
        }
    }
}
