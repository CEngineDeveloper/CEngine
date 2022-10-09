//**********************************************
// Class Name	: CYMSingleton
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using UnityEngine;

namespace CYM
{
    [Unobfus]
    public class Singleton<T> where T : new()
    {
        static readonly object padlock = new object();
        private static T singleton;
        [HideInInspector]
        public static T Ins
        {
            get
            {
                if (singleton == null)
                {
                    lock (padlock)
                    {
                        singleton = new T();
                    }
                }
                return singleton;
            }
        }
    }
}

