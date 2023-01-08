//------------------------------------------------------------------------------
// XenoTemplateCustomMappingData.cs
//
// Copyright 2015 Xenobrain Games LLC 
//
// Created by Habib Loew on 5/28/2015
// Owner: Habib Loew
// 
// 
//
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CYM.Template
{

    [Serializable]
    public class TemplateCustomMappingData {

        //
        // Private data
        //

        [SerializeField]
        private MonoScript m_targetScript;

        [SerializeField]
        private String m_methodName;


        //
        // Public properties
        //

        //------------------------------------------------------------------------------
        public MonoScript TargetScript {
            get {
                return m_targetScript;
            }
        }

        //------------------------------------------------------------------------------
        public String MethodName {
            get {
                return m_methodName;
            }
        }


        //
        // Construction
        //

        //------------------------------------------------------------------------------
        public TemplateCustomMappingData () {

            m_targetScript = null;
            m_methodName = String.Empty;

        }

        //------------------------------------------------------------------------------
        public TemplateCustomMappingData (MonoScript targetScript, String methodName) {

            m_targetScript = targetScript;
            m_methodName = methodName;

        }


        //
        // Public methods
        //

        //------------------------------------------------------------------------------
        public String Execute () {

            if (m_targetScript == null || String.IsNullOrEmpty(m_methodName)) {
                return String.Empty;
            }

            MethodInfo method = m_targetScript.GetClass().GetMethod(m_methodName, BindingFlags.Public | BindingFlags.Static);
            if (method == null || method.GetParameters().Length > 0 || method.ReturnType != typeof(String)) {
                return String.Empty;
            }

            String result = (String)method.Invoke(null, null);
            return result;

        }

    }

}
