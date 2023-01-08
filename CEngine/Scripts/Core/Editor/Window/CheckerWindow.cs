using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CYM
{
    public class BuildChecker
    {
        public static string ErrorStr;
        public static string InfoStr;
        public virtual bool Check()
        {
            return false;
        }
        public virtual void Dispose()
        {

        }
        public static void ClearLog()
        {
            Debug.ClearDeveloperConsole();
            ErrorStr = "";
            InfoStr = "";
        }
        public void AddErrorLog(string str)
        {
            CLog.Error(str);
            ErrorStr += str + "\n";
        }
        public void AddInfo(string str)
        {
            CLog.Info(str);
            InfoStr += str + "\n";
        }
    }
    public class CheckerWindow : EditorWindow
    {
        List<BuildChecker> Checkers = new List<BuildChecker>();
        void OnEnable()
        {
            titleContent.text = "资源检查";
            AddChecker();
        }
        void OnDisable()
        {
            Checkers.Clear();
        }
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("检查所有"))
            {
                CheckAll();
            }
            foreach(var item in Checkers)
            {
                if (GUILayout.Button(item.GetType().Name))
                {
                    item.Check();
                    item.Dispose();
                }
            }
            EditorGUILayout.EndVertical();
        }

        public string CheckAll()
        {
            BuildChecker.ClearLog();
            foreach (var item in Checkers)
            {
                item.Check();
                item.Dispose();
            }
            return BuildChecker.ErrorStr;
        }
        public bool IsHaveError()
        {
            return BuildChecker.ErrorStr != "";
        }

        #region

        protected virtual void AddChecker()
        {

        }
        #endregion

    }
}
