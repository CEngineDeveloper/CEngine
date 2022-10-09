//------------------------------------------------------------------------------
// XenoTemplateMapping.cs
//
// Copyright 2015 Xenobrain Games LLC 
//
// Created by Habib Loew on 5/18/2015
// Owner: Habib Loew
//
// 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Template
{

    public class TemplateMapping : ScriptableObject, ISerializationCallbackReceiver {

        //
        // Public data
        //

        public Dictionary<String, TemplateCustomMappingData> CustomMappings = new Dictionary<String, TemplateCustomMappingData>();
        public Dictionary<String, String> EnvironmentMappings = new Dictionary<String, String>();


        //
        // Private data
        //

        [SerializeField]
        private List<String> m_customMappingKeys = new List<String>();

        [SerializeField]
        private List<TemplateCustomMappingData> m_customMappingValues = new List<TemplateCustomMappingData>();

        [SerializeField]
        private List<String> m_envMappingKeys = new List<String>();

        [SerializeField]
        private List<String> m_envMappingValues = new List<String>();


        //
        // ISerializationCallbackReceiver implementation
        //

        //------------------------------------------------------------------------------
        public void OnBeforeSerialize () {

            m_customMappingKeys.Clear();
            m_customMappingValues.Clear();
            m_envMappingKeys.Clear();
            m_envMappingValues.Clear();

            m_customMappingKeys.AddRange(CustomMappings.Keys);
            m_customMappingValues.AddRange(CustomMappings.Values);
            m_envMappingKeys.AddRange(EnvironmentMappings.Keys);
            m_envMappingValues.AddRange(EnvironmentMappings.Values);

        }

        //------------------------------------------------------------------------------
        public void OnAfterDeserialize () {

            CustomMappings.Clear();
            int customMappingCount = Math.Min(m_customMappingKeys.Count, m_customMappingValues.Count);
            for (int i = 0; i < customMappingCount; ++i) {
                CustomMappings.Add(m_customMappingKeys[i], m_customMappingValues[i]);
            }

            EnvironmentMappings.Clear();
            int envMappingCount = Math.Min(m_envMappingKeys.Count, m_envMappingValues.Count);
            for (int i = 0; i < envMappingCount; ++i) {
                EnvironmentMappings.Add(m_envMappingKeys[i], m_envMappingValues[i]);
            }

        }

    }

}
