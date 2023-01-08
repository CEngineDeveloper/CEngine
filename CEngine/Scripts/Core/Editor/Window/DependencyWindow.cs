using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace CYM
{
    public class StringComparer : IComparer<Object>
    {
        public int Compare(Object x, Object y)
        {
            int compareResult = x.GetType().Name.CompareTo(y.GetType().Name);
            if (compareResult == 0)
            {
                compareResult = x.name.CompareTo(y.name);
            }
            return compareResult;
        }
    }

    public class DependencyWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(DependencyWindow));
            window.titleContent =new GUIContent( "Dependencies"); 
        }

        private StringComparer comparer = new StringComparer();
        private Vector2 scrollPos = new Vector2(0.0f, 0.0f);
        private Rect editorArea;
        private Object newSelection = null;
        private int depSelection = 0;
        private int page = 0;
        private const int itemPerPage = 100;

        void OnGUI()
        {
            EditorStyles.toolbarButton.alignment = TextAnchor.MiddleLeft;
            this.newSelection = null;
            this.editorArea = this.position;
            GUILayout.BeginHorizontal();
            GUILayout.Space(2);
            this.scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUILayout.BeginVertical();
            foreach (Object targetObj in Selection.objects)
            {
                GUILayout.Space(2);
                drawAssetInfo(targetObj, ref depSelection);
            }
            GUILayout.EndVertical();

            GUILayout.EndScrollView();
            GUILayout.Space(2);
            GUILayout.EndHorizontal();

            if (newSelection != null)
            {
                Selection.activeObject = newSelection;
            }
        }

        void OnSelectionChange()
        {
            this.page = 0;
            this.Repaint();
        }

        private List<Object> cleanUp(Object[] arr)
        {
            List<Object> newList = new List<Object>();

            foreach (Object obj in arr)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (!newList.Contains(obj) && assetPath != "")
                {
                    newList.Add(obj);
                }
            }
            return newList;
        }

        void drawAssetInfo(Object obj, ref int selection, bool drawDependency = true)
        {
            if (obj != null)
            {
                GUILayout.BeginVertical();
                string assetPath = AssetDatabase.GetAssetOrScenePath(obj);
                Texture2D icon = AssetPreview.GetMiniTypeThumbnail(obj.GetType());
                GUILayout.BeginHorizontal();
                GUILayout.Label(icon, GUILayout.Width(16), GUILayout.Height(16));
                GUILayout.Label(obj.name, GUILayout.Width(editorArea.width - 65), GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(24);
                GUILayout.BeginVertical();
                GUILayout.Label(new GUIContent(assetPath, "Asset Path : \n" + assetPath), GUILayout.Width(editorArea.width - 65), GUILayout.ExpandWidth(false));

                GUILayout.BeginHorizontal(GUILayout.Width(editorArea.width - 65), GUILayout.ExpandWidth(false));
                if (GUILayout.Button(new GUIContent("Locate", "Select asset on Project Window")))
                {
                    EditorGUIUtility.PingObject(obj);
                }

                if (GUILayout.Button(new GUIContent("Select", "Select asset on Hierarchy Window")))
                {
                    this.newSelection = obj;
                }

                if (GUILayout.Button(new GUIContent("Open", "Open asset with External Application")))
                {
                    AssetDatabase.OpenAsset(obj);
                }

                if (GUILayout.Button(new GUIContent("Export", "Export selected asset to UnityPackage")))
                {
                    string targetPath = EditorUtility.SaveFilePanel("export package", Application.dataPath + ".unitypackage", obj.name, "unitypackage");
                    if (targetPath != "")
                    {
                        AssetDatabase.ExportPackage(assetPath, targetPath);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                if (drawDependency)
                {
                    Object[] objects = { obj };
                    List<Object> objectList = cleanUp(EditorUtility.CollectDependencies(objects));

                    if (objectList.Count > 0)
                    {
                        objectList.Sort(comparer);
                        this.drawDependencyList(obj, ref objectList, ref selection);
                    }
                }
                GUILayout.EndVertical();
            }
        }

        bool drawDependencyList(Object currentObj, ref List<Object> assetList, ref int selection)
        {
            bool selected = false;
            TextAnchor lastAnchor = GUI.skin.button.alignment;
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;

            if (assetList.Count > 0)
            {
                GUILayout.Label("Depend on (" + (assetList.Count - 1).ToString() + ") :");
                int showCount = assetList.Count;
                if (showCount > itemPerPage)
                {
                    showCount = itemPerPage;
                }
                for (int i = page * itemPerPage; i < assetList.Count; i++)
                {
                    if (assetList[i] != currentObj)
                    {
                        string label = assetList[i].name + " [" + assetList[i].GetType().Name + "]";
                        if (i == selection)
                        {
                            drawAssetInfo(assetList[i], ref selection, false);
                        }
                        else
                        {
                            Texture2D icon = AssetPreview.GetMiniTypeThumbnail(assetList[i].GetType());
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(icon, GUILayout.Width(16), GUILayout.Height(16));
                            if (GUILayout.Toggle(false, label, "toolbarButton", GUILayout.Width(editorArea.width - 46), GUILayout.ExpandWidth(false)))
                            {
                                selection = i;
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                if (assetList.Count > itemPerPage)
                {
                    GUILayout.Space(8);
                    GUILayout.Label("Showing item " + (page * itemPerPage).ToString() + " to " + ((page + 1) * itemPerPage - 1).ToString());
                    GUILayout.BeginHorizontal();
                    if (page > 0)
                    {
                        if (GUILayout.Button("Show Prev " + itemPerPage))
                        {
                            page--;
                        }
                    }
                    if (assetList.Count / (itemPerPage + 1) < assetList.Count)
                    {
                        if (GUILayout.Button("Show Next " + itemPerPage))
                        {
                            page++;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUI.skin.button.alignment = lastAnchor;
            return selected;
        }
    }

}