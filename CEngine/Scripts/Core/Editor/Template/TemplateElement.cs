//------------------------------------------------------------------------------
// XenoTemplateElement.cs
//
// Copyright 2015 Xenobrain Games LLC 
//
// Created by Habib Loew on 5/8/2015
// Owner: Habib Loew
//
// 
//
//------------------------------------------------------------------------------

using System;
using System.IO;
using UnityEngine;

namespace CYM.Template
{

    [Serializable]
    public class TemplateElement {

        //
        // Configuration data
        // 

        public String ElementName;
        public String TemplateTokenFormat;
        public String Extension;
        public String Contents;


        //
        // Public properties
        // 

        //------------------------------------------------------------------------------
        public String InstanceAssetPath { get; set; }

        //------------------------------------------------------------------------------
        public String InstanceName { get; set; }


        //
        // Construction
        // 

        //------------------------------------------------------------------------------
        public TemplateElement () {

            Clear();

        }

        //------------------------------------------------------------------------------
        public TemplateElement (String elementName, String fileNameFormat, String extension, String contents) {
            ElementName = elementName;
            TemplateTokenFormat = fileNameFormat;
            Extension = extension;
            Contents = contents;
        }


        //
        // Public methods
        // 

        //------------------------------------------------------------------------------
        public void Clear () {

            ElementName = String.Empty;
            TemplateTokenFormat = String.Empty;
            Extension = String.Empty;
            Contents = String.Empty;

            ClearInstanceData();

        }

        //------------------------------------------------------------------------------
        public void ClearInstanceData () {

            InstanceAssetPath = String.Empty;
            InstanceName = String.Empty;

        }

        //------------------------------------------------------------------------------
        public void GenerateInstanceData (String baseName, String targetPath) {

            String undecoratedName = String.Format(TemplateTokenFormat, baseName);
            int retryCount = 0;

            do {
                InstanceName = String.Format(
                    "{0}{1}",
                    undecoratedName,
                    (retryCount > 0) ? retryCount.ToString() : ""
                );

                InstanceAssetPath = System.IO.Path.Combine(
                    targetPath,
                    String.Format("{0}{1}", InstanceName, Extension)
                );

                ++retryCount;
            }
            while (File.Exists(InstanceAssetPath));

        }

        //------------------------------------------------------------------------------
        public bool IsValid () {

            if (String.IsNullOrEmpty(ElementName))
                return false;

            if (String.IsNullOrEmpty(TemplateTokenFormat))
                return false;

            if (String.IsNullOrEmpty(Extension))
                return false;

            return true;

        }

    }

}
