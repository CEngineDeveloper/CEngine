//------------------------------------------------------------------------------
// BaseEditorUtils.cs
// Copyright 2020 2020/7/10 
// Created by CYM on 2020/7/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using System;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace CYM
{
    public partial class EditorUtil
    {
        //
        // Useful constants
        //
        public static readonly int MaxTextAreaStringLength = 15000;

        public enum EolType
        {
            Windows,
            Unix
        }


        //
        // Public methods
        //

        //------------------------------------------------------------------------------
        public static String EolTypeToEolString(EolType eolType)
        {

            switch (eolType)
            {
                case EolType.Unix:
                    return "\n";

                case EolType.Windows:
                    return "\r\n";

            }

            throw new Exception(String.Format("Unsupported EOL string!"));

        }

        //------------------------------------------------------------------------------
        public static EolType EolStringToEolType(String eol)
        {

            switch (eol)
            {
                case "\r\n":
                    return EolType.Windows;

                case "\n":
                    return EolType.Unix;
            }

            throw new Exception(String.Format("Unsupported EOL string: {0}", Regex.Escape(eol)));

        }

        //------------------------------------------------------------------------------
        public static String NormalizeLineEndings(String text, EolType eol)
        {

            String eolString = (eol == EolType.Unix) ? "\n" : "\r\n";

            String normalized = Regex.Replace(text, @"\r*\n", eolString, RegexOptions.Multiline);
            return normalized;

        }

        //------------------------------------------------------------------------------
        // Find asset paths by asset type
        public static List<String> FindAssetPaths<T>() where T : UnityEngine.Object
        {

            String assetFilter = String.Format("t:{0}", typeof(T).ToString());

            String[] assetGuids = AssetDatabase.FindAssets(assetFilter);
            List<String> assetFilePaths = new List<string>();

            foreach (String guidString in assetGuids)
            {
                assetFilePaths.Add(AssetDatabase.GUIDToAssetPath(guidString));
            }

            return assetFilePaths;

        }

        //------------------------------------------------------------------------------
        // Find asset paths by label - note that substrings match
        public static List<String> FindAssetPaths(String label)
        {

            String assetFilter = String.Format("l:{0}", label);

            String[] assetGuids = AssetDatabase.FindAssets(assetFilter);
            List<String> assetFilePaths = new List<string>();

            foreach (String guidString in assetGuids)
            {
                assetFilePaths.Add(AssetDatabase.GUIDToAssetPath(guidString));
            }

            return assetFilePaths;

        }

        //------------------------------------------------------------------------------
        // Returns the path of the first asset which fully matches the specified name.  
        // Returns null otherwise.
        public static String GetAssetPathFromName(String assetName)
        {

            foreach (String path in AssetDatabase.GetAllAssetPaths())
            {
                if (path.EndsWith(assetName))
                {
                    return path;
                }
            }

            return null;

        }

        //------------------------------------------------------------------------------
        // Returns the relative path (from the project directory) of the directory holding
        // the currently selected asset, or null if no asset is selected.
        public static String GetDirectoryPathOfSelectedAsset()
        {

            UnityEngine.Object[] selectedObjects = Selection.GetFiltered(
                typeof(UnityEngine.Object),
                SelectionMode.Assets
            );

            if (selectedObjects.Length == 0)
                return null;

            String path = AssetDatabase.GetAssetPath(selectedObjects[0]);
            if (File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }

            return path;

        }

        //------------------------------------------------------------------------------
        // Return a path based upon the input path which is guaranteed to contain 
        // requiredFolder.  If needed, requiredFolder is created and appended to 
        // the path.
        public static String ValidateAssetPath(String path, String requiredFolder)
        {

            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(path, String.Format(@"\b{0}\b", requiredFolder)))
            {
                String newPath = String.Format("{0}/{1}", path, requiredFolder);
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(path, requiredFolder);
                }

                path = newPath;
            }

            return path;

        }
        public static UnityEngine.Object DropOjbect(string meg = null)
        {
            Event aEvent;
            aEvent = Event.current;

            GUI.contentColor = Color.white;
            UnityEngine.Object temp = null;

            var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));

            GUIContent title = new GUIContent(meg);
            if (string.IsNullOrEmpty(meg))
            {
                title = new GUIContent("Drag Object here from Project view to get the object");
            }

            GUI.Box(dragArea, title);
            switch (aEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dragArea.Contains(aEvent.mousePosition))
                    {
                        break;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (aEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                        {
                            temp = DragAndDrop.objectReferences[i];

                            if (temp == null)
                            {
                                break;
                            }
                        }
                    }

                    Event.current.Use();
                    break;
                default:
                    break;
            }

            return temp;
        }

        static Dictionary<int, string[]> layerNames = new Dictionary<int, string[]>();
        static long lastUpdateTick;

        /// <summary>Displays a LayerMask field.</summary>
        /// <param name="label">Label to display</param>
        /// <param name="selected">Current LayerMask</param>
        public static LayerMask LayerMaskField(string label, LayerMask selected)
        {
            if (Event.current.type == EventType.Layout && System.DateTime.UtcNow.Ticks - lastUpdateTick > 10000000L)
            {
                layerNames.Clear();
                lastUpdateTick = System.DateTime.UtcNow.Ticks;
            }

            string[] currentLayerNames;
            if (!layerNames.TryGetValue(selected.value, out currentLayerNames))
            {
                var layers = ListPool<string>.Claim();

                int emptyLayers = 0;
                for (int i = 0; i < 32; i++)
                {
                    string layerName = LayerMask.LayerToName(i);

                    if (layerName != "")
                    {
                        for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                        layers.Add(layerName);
                    }
                    else
                    {
                        emptyLayers++;
                        if (((selected.value >> i) & 1) != 0 && selected.value != -1)
                        {
                            for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i + 1 - emptyLayers));
                        }
                    }
                }

                currentLayerNames = layerNames[selected.value] = layers.ToArray();
                ListPool<string>.Release(ref layers);
            }

            selected.value = EditorGUILayout.MaskField(label, selected.value, currentLayerNames);
            return selected;
        }

    }
}