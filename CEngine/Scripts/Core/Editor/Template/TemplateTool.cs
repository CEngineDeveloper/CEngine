//------------------------------------------------------------------------------
// XenoTemplateTool.cs
//
// Copyright 2015 Xenobrain Games LLC 
//
// Created by Habib Loew on 2/10/2014
// Owner: Habib Loew
//
// Tools for creating the menu items for the various XenoTemplates
//
//------------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
namespace CYM.Template
{

    //==============================================================================
    //
    // Process XenoTemplate files and generate menu items
    //
    //==============================================================================

    public static class TemplateTool {

        //
        // Public data
        //

        public static readonly String MapLabel               = "TemplateMap";
        public static readonly String TemplateLabel          = "TemplateFile";
        public static readonly String TemplateFileFolderName = "TemplateFiles";
        public static readonly float  Version                = 1.2f;
          
                            
        //
        // Private data
        // 

        private static readonly String s_templateFilePathPrefixPattern      = String.Format(@"(.*\b{0}[/]+)|(.*\b{0}$)", TemplateFileFolderName);
        private static readonly String s_templateMenuImplementationFileName = "TemplatesMenu.cs";

        private static int s_priority = 0;


        //
        // Public methods
        // 

        public static void PromptUserForNameAuto()
        {
            string templateFilePath = "";
            String targetPath = EditorUtil.GetDirectoryPathOfSelectedAsset();

            if (targetPath.Contains(SysConst.Dir_CEngine))
            {
                templateFilePath = SysConst.RPath_TempCYMMonobehaviour;
            }
            else
            {
                templateFilePath = SysConst.RPath_TempMonobehaviour;
            }

            if (String.IsNullOrEmpty(targetPath))
                targetPath = "Assets";

            if (!File.Exists(templateFilePath))
                return;

            String templateName = Path.GetFileNameWithoutExtension(templateFilePath);
            templateName = Regex.Replace(templateName, @"\W", "");

            TemplateUtilityWindow.ShowWindow(templateFilePath, templateName, targetPath);

        }
        public static void ShowTemplate(string name)
        {
            String targetPath = EditorUtil.GetDirectoryPathOfSelectedAsset();
            string templateFilePath = Path.Combine(SysConst.RPath_CEngine, "_Res/Configs/TemplateFiles/" + name + ".asset"); 
            if (String.IsNullOrEmpty(targetPath))
                targetPath = "Assets";
            if (!File.Exists(templateFilePath))
                return;
            String templateName = Path.GetFileNameWithoutExtension(templateFilePath);
            templateName = Regex.Replace(templateName, @"\W", "");
            TemplateUtilityWindow.ShowWindow(templateFilePath, templateName, targetPath);
        }
        public static void CreateTemplate()
        {
            Template template = ScriptableObject.CreateInstance<Template>();
            string rawPath = Path.Combine(SysConst.RPath_CustomTempScript, "NewTemplate.asset");
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(rawPath);
            AssetDatabase.CreateAsset(template, uniquePath);
            AssetDatabase.SetLabels(template, new string[] { TemplateLabel });
            Selection.activeObject = template;
        }

        //------------------------------------------------------------------------------
        public static void CreateMappingSet()
        {
            TemplateMapping mapping = ScriptableObject.CreateInstance<TemplateMapping>();
            string rawPath = Path.Combine(SysConst.RPath_CustomTempScript, "NewTemplateMapping.asset"); 
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(rawPath);
            AssetDatabase.CreateAsset(mapping, uniquePath);
            AssetDatabase.SetLabels(mapping, new string[] { MapLabel });
            Selection.activeObject = mapping;
        }


        //------------------------------------------------------------------------------
        public static void PromptUserForName (String templateFilePath) {

            String targetPath = EditorUtil.GetDirectoryPathOfSelectedAsset();
            if (String.IsNullOrEmpty(targetPath))
                targetPath = "Assets";

            if (!File.Exists(templateFilePath))
                return;

            String templateName = Path.GetFileNameWithoutExtension(templateFilePath);
            templateName = Regex.Replace(templateName, @"\W", "");
                        
            TemplateUtilityWindow.ShowWindow(templateFilePath, templateName, targetPath);

        }

        //------------------------------------------------------------------------------
        public static void GenerateFileNames (String templateFilePath, String userSuppliedName, ref List<String> fileNames, ref String error) {

            fileNames.Clear();
            error = String.Empty;

            if (!File.Exists(templateFilePath))
                return;

            String targetPath = EditorUtil.GetDirectoryPathOfSelectedAsset();
            if (targetPath == null)
                targetPath = "Assets";

            String templateExtension = Path.GetExtension(templateFilePath);

            if (templateExtension == ".asset") {
                Template template = ExtractTemplatesNew(templateFilePath);
                if (template == null) {
                    error = String.Format(
                        "Incompatible template \"{0}\". Is it from a later version of Unity?",
                        templateFilePath
                    );
                    return;
                }

                foreach (TemplateElement element in template.TemplateElements) {
                    element.GenerateInstanceData(userSuppliedName, targetPath);
                    fileNames.Add(String.Format("{0}{1}", element.InstanceName, element.Extension));
                }

            } else {
                error = String.Format(
                    "Unable to generate files from unknown template type: \"{0}\"",
                    templateFilePath
                );
            }

        }

        //------------------------------------------------------------------------------
        public static void GenerateFiles (String templateFilePath, String userSuppliedName, String targetPath) {

            if (!File.Exists(templateFilePath))
                return;

            String templateExtension = Path.GetExtension(templateFilePath);

            if (templateExtension == ".asset") {

                Template template = ExtractTemplatesNew(templateFilePath);

                // Generate per-instance data
                foreach (TemplateElement element in template.TemplateElements) {
                    element.GenerateInstanceData(userSuppliedName, targetPath);
                }

                // Build a token replacement table for all sub-templates
                Dictionary<String, String> tokenReplacements = new Dictionary<String, String>();
                LoadCustomMappings(ref tokenReplacements);

                tokenReplacements.Add(@"%TEMPLATE_TOKEN%", userSuppliedName);

                foreach (TemplateElement element in template.TemplateElements) {
                    String token = String.Format(@"%TEMPLATE_NAME\({0}\)%", element.ElementName);
                    tokenReplacements.Add(token, element.InstanceName);
                }

                // Generate one new asset file for each subTemplate
                foreach (TemplateElement element in template.TemplateElements) {
                    GenerateFileFromTemplateElement(element, tokenReplacements);
                }

            }
            else {
                Debug.LogErrorFormat(
                    "Unable to generate files from unknown template type: \"{0}\"",
                    templateFilePath
                );
            }

        }

        //------------------------------------------------------------------------------
        public static List<String> GetValidSubstitutionMethods (MonoScript script) {

            List<String> methods = new List<String>();

            Type scriptClass = script.GetClass();
            if (scriptClass == null) {
                return methods;
            }

            System.Reflection.MethodInfo[] rawMethods = scriptClass.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            for (int i = 0; i < rawMethods.Length; ++i) {
                System.Reflection.MethodInfo info = rawMethods[i];

                if (info.GetParameters().Length > 0 || info.ReturnType != typeof(String)) {
                    continue;
                }

                methods.Add(info.Name);
            }

            return methods;

        }

        //------------------------------------------------------------------------------
        public static void RefreshTemplates (string path=null,bool force=true) {

            // Build a list of all the template asset paths
            List<String> templateFilePaths = EditorUtil.FindAssetPaths(TemplateTool.TemplateLabel);

            if (path != null)
            {
                path = Path.Combine(path, s_templateMenuImplementationFileName);
            }

            // Generate the new version of menu implementation file to create 
            // menu items for each of the templates.

            String templateMenuScriptPath = EditorUtil.GetAssetPathFromName(s_templateMenuImplementationFileName);
            if (!force && templateMenuScriptPath != null)
                return;
            if (templateMenuScriptPath != null&& path==null)
            {
                path = templateMenuScriptPath;
            }

            GenerateTemplateMenuImplementation(
                path,
                templateFilePaths
            );

            // Force an immediate import of the newly generated menu script.
            // This will cause the context menu to update immediately
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            //AssetDatabase.Refresh();
        }

        //
        // Private methods
        // 

        //------------------------------------------------------------------------------
        private static String ExtractSubMenuPath (String templateFilePath) {

            String templateName = Path.GetFileNameWithoutExtension(templateFilePath).Trim();
            String prettyTemplateName = Regex.Replace(templateName, "[A-Z]", " $&").Trim();

            String rawDirectoryName = Path.GetDirectoryName(templateFilePath);
            String subDirectoryName = String.Empty;

            if (!String.IsNullOrEmpty(rawDirectoryName)) {
                subDirectoryName = Regex.Replace(rawDirectoryName, s_templateFilePathPrefixPattern, String.Empty);
                if (!String.IsNullOrEmpty(subDirectoryName)) {
                    return String.Format(
                        "{0}/{1}",
                        subDirectoryName,
                        prettyTemplateName
                    );
                }
            }

            return prettyTemplateName;

        }

        //------------------------------------------------------------------------------
        private static Template ExtractTemplatesNew (String templateFilePath) {

            Template template = AssetDatabase.LoadAssetAtPath(
                templateFilePath, 
                typeof(Template)
            ) as Template;

            return template;

        }

        //------------------------------------------------------------------------------
        private static void GenerateTemplateMenuImplementation (
            String templateMenuScriptPath,
            List<String> templateFilePaths
        ) {

            using (FileStream fs = new FileStream(templateMenuScriptPath, FileMode.Create, FileAccess.Write)) {

                using (StreamWriter writer = new StreamWriter(fs)) {

                    writer.WriteLine(
                        String.Format(
                            @"// Generated by XenoTemplateTool v{0} on {1} at {2}",
                            Version,
                            DateTime.Now.ToShortDateString(),
                            DateTime.Now.ToShortTimeString()
                        )
                    );

                    writer.WriteLine(@"using UnityEditor;");
                    writer.WriteLine(@"using Xeno.Tools;");
                    writer.WriteLine(@"using Xeno.Tools.Templates;");
                    writer.WriteLine();
                    //writer.WriteLine(@"namespace Xeno.Tools.Templates.XenoTemplateMenuItems {");
                    writer.WriteLine();

                    s_priority = 0;

                    GenerateTemplateMenuClass(writer, "Extended", templateFilePaths);

                    //writer.WriteLine(@"} // namespace");

                    writer.Flush();

                }

            }

        }

        //------------------------------------------------------------------------------
        private static void GenerateTemplateMenuClass (
            StreamWriter writer,
            String classDecorator,
            List<String> templateFilePaths
        ) {

            writer.WriteLine(
                String.Format(
                    "\tpublic static class {0}TemplateMenuItems {{", 
                    classDecorator
                )
            );
            writer.WriteLine();

            templateFilePaths.Sort((String a, String b) => String.Compare(ExtractSubMenuPath(a), ExtractSubMenuPath(b)));

            foreach (String path in templateFilePaths) {
                GenerateMenuItem(writer, path, s_priority++);
            }

            writer.WriteLine();
            writer.WriteLine("\t}");
            writer.WriteLine();

        }

        //------------------------------------------------------------------------------
        private static void GenerateFileFromTemplateElement (TemplateElement element, Dictionary<String, String> tokenReplacements) {

            // Set template specific replacements here
            tokenReplacements[@"%TEMPLATE_NAME%"] = element.InstanceName;

            String generatedContents = element.Contents;
            foreach (KeyValuePair<String, String> kvp in tokenReplacements) {
                generatedContents = Regex.Replace(generatedContents, kvp.Key, kvp.Value);
            }

            File.WriteAllText(element.InstanceAssetPath, generatedContents);
            AssetDatabase.ImportAsset(element.InstanceAssetPath, ImportAssetOptions.ForceUpdate);

        }

        //------------------------------------------------------------------------------
        private static void GenerateMenuItem (
            StreamWriter writer,
            String templateFilePath,
            int priority
        ) {

            String templateSubMenuPath = ExtractSubMenuPath(templateFilePath);
            String templateMenuMethodName = Regex.Replace(templateSubMenuPath, @"\W", "_");

            writer.WriteLine(
                String.Format(
                    "\t\t[MenuItem(\"Assets/ScriptTemplates/{0}\", false, {1})]",
                    templateSubMenuPath,
                    priority
                )
            );

            writer.WriteLine(
                String.Format(
                    "\t\tpublic static void {0} () {{",
                    templateMenuMethodName
                )
            );

            writer.WriteLine(
                String.Format(
                    "\t\t\tXenoTemplateTool.PromptUserForName(\"{0}\");",
                    templateFilePath
                )
            );
            writer.WriteLine("\t\t}");
            writer.WriteLine();

        }

        //------------------------------------------------------------------------------
        private static void LoadCustomMappings (ref Dictionary<String, String> tokenReplacements) {

            List<String> mappingAssetPaths = EditorUtil.FindAssetPaths(TemplateTool.MapLabel);

            foreach (String path in mappingAssetPaths) {

                if (!File.Exists(path))
                    continue;

                TemplateMapping mapping = AssetDatabase.LoadAssetAtPath(path, typeof(TemplateMapping)) as TemplateMapping;

                foreach (KeyValuePair<String, TemplateCustomMappingData> kvp in mapping.CustomMappings) {
                    MapCustom(kvp.Key, kvp.Value, ref tokenReplacements);
                }

                foreach (KeyValuePair<String, String> kvp in mapping.EnvironmentMappings) {
                    MapEnvVariable(kvp.Key, kvp.Value, ref tokenReplacements);
                }

            }

        }

        //------------------------------------------------------------------------------
        private static void MapCustom (String tokenName, TemplateCustomMappingData data, ref Dictionary<String, String> tokenReplacements) {

            String decoratedKey = String.Format("%{0}%", tokenName);

            if (tokenReplacements.ContainsKey(decoratedKey)) {
                Debug.LogWarningFormat(
                    "XenoTemplates: Custom mapping of token \"{0}\" to \"{1}::{2}\" hides a previous mapping. Please check the resulting files carefully and consider renaming the conflicting mappings.",
                    decoratedKey,
                    data.TargetScript.name,
                    data.MethodName
                );
            }

            String mappedValue = data.Execute();
            tokenReplacements.Add(decoratedKey, mappedValue);

        }

        //------------------------------------------------------------------------------
        private static void MapEnvVariable (String tokenName, String envVarName, ref Dictionary<String, String> tokenReplacements) {

            String token = String.Format("%{0}%", tokenName);
            String envVarValue = Environment.GetEnvironmentVariable(envVarName);

            if (envVarValue == null) {
                Debug.LogWarningFormat(
                    "XenoTemplates: Mapped environment variable \"{0}\" (mapped to \"{1}\" does not have a value.\nSkipping.",
                    envVarName,
                    token
                );

                return;
            }

            tokenReplacements.Add(token, envVarValue);

        }

    }

}


