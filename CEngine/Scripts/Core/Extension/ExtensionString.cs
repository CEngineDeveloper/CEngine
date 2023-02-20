using System.Text.RegularExpressions;

namespace CYM
{
    public static class ExtensionString
    {
        #region str
        /// <summary>
        /// Wraps a class around a json array so that it can be deserialized by JsonUtility
        /// </summary>
        /// <param name="source"></param>
        /// <param name="topClass"></param>
        /// <returns></returns>
        public static string WrapToClass(this string source, string topClass)
        {
            return string.Format("{{\"{0}\": {1}}}", topClass, source);
        }
        ///<summary>
        /// 移除前缀字符串
        ///</summary>
        ///<param name="val">原字符串</param>
        ///<param name="str">前缀字符串</param>
        ///<returns></returns>
        public static string TrimStart(this string val, string str)
        {
            string strRegex = @"^(" + str + ")";
            return Regex.Replace(val, strRegex, "");
        }
        ///<summary>
        /// 移除后缀字符串
        ///</summary>
        ///<param name="val">原字符串</param>
        ///<param name="str">后缀字符串</param>
        ///<returns></returns>
        public static string TrimEnd(this string val, string str)
        {
            string strRegex = @"(" + str + ")" + "$";
            return Regex.Replace(val, strRegex, "");
        }
        public static string ToUnityPath(this string mp)
        {
            mp = mp.Substring(mp.IndexOf("Assets"));
            mp = mp.Replace('\\', '/');
            return mp;
        }
        #endregion
        public static string Truncate(this string value, int maxLength, string truncateString = "...")
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Length <= maxLength ? value : $"{value.Substring(0, maxLength)}{truncateString}";
        }
    }
}
