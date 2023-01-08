//------------------------------------------------------------------------------
// Prefers.cs
// Copyright 2021 2021/3/21 
// Created by CYM on 2021/3/21
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
namespace CYM
{
    [HideInInspector]
    public sealed class Prefers : MonoBehaviour
    {
        #region key
        public const string Key_LastAchiveID = "LastAchiveID";
        public const string Key_LastAchiveLocal = "LastAchiveLocal";
        public const string Key_LastPrefsVer = "LastPrefsVer";
        public const string Key_CustomLanguage = "CustomLanguage";

        #endregion

        private void Start()
        {
            if (GetInt(Key_LastPrefsVer) != BuildConfig.Ins.Prefs)
            {
                DeleteAll();
                SetInt(Key_LastPrefsVer, BuildConfig.Ins.Prefs);
            }
        }


        #region set
        public static void SetStr(string key, string data)
        {
            PlayerPrefs.SetString(key, data);
        }
        public static void SetInt(string key, int data)
        {
            PlayerPrefs.SetInt(key, data);
        }
        public static void SetFloat(string key, float data)
        {
            PlayerPrefs.SetFloat(key, data);
        }
        public static void SetBool(string key, bool data)
        {
            PlayerPrefs.SetInt(key, data ? 1 : 0);
        }
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
        #endregion

        #region get
        public static string GetStr(string key, string defaultVal = "")
        {
            return PlayerPrefs.GetString(key, defaultVal);
        }
        public static int GetInt(string key, int defaultVal = 0)
        {
            return PlayerPrefs.GetInt(key, defaultVal);
        }
        public static float GetFloat(string key, float defaultVal = 0.0f)
        {
            return PlayerPrefs.GetFloat(key, defaultVal);
        }
        public static bool GetBool(string key, bool defaultVal = false)
        {
            var temp = defaultVal ? 1 : 0;
            return PlayerPrefs.GetInt(key, temp) == 0 ? false : true;
        }
        public static void SetLastAchiveID(string id)
        {
            SetStr(Key_LastAchiveID, id);
        }
        public static void SetLastAchiveLocal(bool b)
        {
            SetBool(Key_LastAchiveLocal, b);
        }
        public static string GetLastAchiveID()
        {
            return GetStr(Key_LastAchiveID, "");
        }
        public static bool GetLastAchiveLocal()
        {
            return GetBool(Key_LastAchiveLocal, true);
        }
        #endregion

        #region check
        public static bool Check(string id)
        {
            if (!GetBool(id))
            {
                SetBool(id, true);
                return false;
            }
            return true;
        }
        public static void SetCustomLanguage()
        {
            SetBool(Key_CustomLanguage, true);
        }
        #endregion

        #region is
        public static bool IsCustomLanguage()
        {
            return GetBool(Key_CustomLanguage);
        }
        #endregion
    }
}