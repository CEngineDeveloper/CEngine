//------------------------------------------------------------------------------
// UIPrefabWindow.cs
// Created by CYM on 2022/11/3
// 填写类的描述...
//------------------------------------------------------------------------------
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CYM
{
    public class UIPrefabWindow : OdinEditorWindow
    {
        class Item
        {
            public string AbsPath;
            public string RelPath;
        }

        #region prop
        public static UIPrefabWindow Ins { get; private set; }
        protected static string VerticalStyle = "HelpBox";
        static Dictionary<string, Item> FileRaw = new Dictionary<string, Item>();
        static Dictionary<string, Item> FileComponent = new Dictionary<string, Item>();
        static Dictionary<string, Item> FileView = new Dictionary<string, Item>();
        static GameObject Preview;
        static Vector2 scrollVal;
        static UIConfig UIConfig => UIConfig.Ins;
        #endregion

        #region menu
        [MenuItem("Tools/UIPrefab")]
        public static void ShowBuildWindow()
        {
            Ins = GetWindow<UIPrefabWindow>();
            Ins.ShowPopup();
            Ins.minSize = new Vector2(200, 500);
            if (Ins != null)
            {
                Ins.titleContent = new GUIContent("UIPrefab");
                Ins.Repaint();
            }
            RefreshData();
        }
        private void OnValidate()
        {
            RefreshData();
        }
        static void RefreshData()
        {
            CollectData("_Bundles/UI/Raw/", FileRaw);
            CollectData("_Bundles/UI/Presenter/", FileComponent);
            CollectData("_Bundles/UI/View/", FileView);
        }
        #endregion

        #region life
        protected override void OnGUI()
        {
            scrollVal = EditorGUILayout.BeginScrollView(scrollVal);
            DrawRawComponent();
            DrawComponent();
            DrawView();
            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region Draw item
        private static void CollectData(string path, Dictionary<string, Item> dicFiles)
        {
            var tempPath = Path.Combine(SysConst.Path_CEngine, path);
            dicFiles.Clear();
            var fileComponet = Directory.GetFiles(tempPath, "*.prefab", SearchOption.AllDirectories);
            foreach (var item in fileComponet)
            {
                var temp = Path.GetFileNameWithoutExtension(item);
                var tempExt = Path.GetFileName(item);
                dicFiles.Add(temp, new Item { AbsPath = item, RelPath = Path.Combine("Assets/Plugins/", SysConst.Dir_CEngine, path, tempExt) });
            }
        }
        static void DrawItem(Dictionary<string, Item> dicFiles)
        {
            foreach (var item in dicFiles)
            {
                if (GUILayout.Button(item.Key))
                {
                    Preview = AssetDatabase.LoadAssetAtPath(item.Value.RelPath, typeof(GameObject)) as GameObject;
                    if (Preview)
                    {
                        var temp = GameObject.Instantiate(Preview);
                        temp.name = temp.name.Replace("(Clone)","");
                        PlaceUIElementRoot(temp, new MenuCommand(Selection.activeGameObject, 0));
                    }
                }
            }
        }
        static void DrawRawComponent()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldRawComponent = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.Ins.FoldRawComponent, "元件"))
            {
                if (GUILayout.Button("Canvas"))
                {
                    CreateNewUI();
                }
                DrawItem(FileRaw);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        static void DrawComponent()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldComponent = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.Ins.FoldComponent, "组件"))
            {
                DrawItem(FileComponent);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        static void DrawView()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldView = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.Ins.FoldView, "界面"))
            {
                DrawItem(FileView);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Utile
        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;
            // Create world space Plane from canvas position.
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;
                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);
                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;
                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;
                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;
                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }
            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }
        public static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            bool explicitParentChoice = true;
            if (parent == null)
            {
                parent = GetOrCreateCanvasGameObject();
                explicitParentChoice = false;
                // If in Prefab Mode, Canvas has to be part of Prefab contents,
                // otherwise use Prefab root instead.
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent))
                    parent = prefabStage.prefabContentsRoot;
            }
            if (parent.GetComponentsInParent<Canvas>(true).Length == 0)
            {
                // Create canvas under context GameObject,
                // and make that be the parent which UI element is added under.
                GameObject canvas = CreateNewUI();
                canvas.transform.SetParent(parent.transform, false);
                parent = canvas;
            }

            // Setting the element to be a child of an element already in the scene should
            // be sufficient to also move the element to that scene.
            // However, it seems the element needs to be already in its destination scene when the
            // RegisterCreatedObjectUndo is performed; otherwise the scene it was created in is dirtied.
            SceneManager.MoveGameObjectToScene(element, parent.scene);
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            if (element.transform.parent == null)
            {
                Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            }
            GameObjectUtility.EnsureUniqueNameForSibling(element);
            // We have to fix up the undo name since the name of the object was only known after reparenting it.
            Undo.SetCurrentGroupName("Create " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (!explicitParentChoice) // not a context click, so center in sceneview
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());
            Selection.activeGameObject = element;
        }
        static public GameObject CreateNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer(SysConst.kUILayerName);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(UIConfig.Width, UIConfig.Height);
            // Works for all stages.
            StageUtility.PlaceGameObjectInCurrentStage(root);
            bool customScene = false;
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                root.transform.SetParent(prefabStage.prefabContentsRoot.transform, false);
                customScene = true;
            }

            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            // If there is no event system add one...
            // No need to place event system in custom scene as these are temporary anyway.
            // It can be argued for or against placing it in the user scenes,
            // but let's not modify scene user is not currently looking at.
            if (!customScene)
                CreateEventSystem(false);
            return root;
        }
        private static void CreateEventSystem(bool select)
        {
            CreateEventSystem(select, null);
        }
        private static void CreateEventSystem(bool select, GameObject parent)
        {
            StageHandle stage = parent == null ? StageUtility.GetCurrentStageHandle() : StageUtility.GetStageHandle(parent);
            var esys = stage.FindComponentOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                if (parent == null)
                    StageUtility.PlaceGameObjectInCurrentStage(eventSystem);
                else
                    GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }
        // Helper function that returns a Canvas GameObject; preferably a parent of the selection, or other existing Canvas.
        static public GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (IsValidCanvas(canvas))
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use any valid canvas.
            // We have to find all loaded Canvases, not just the ones in main scenes.
            Canvas[] canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
            for (int i = 0; i < canvasArray.Length; i++)
                if (IsValidCanvas(canvasArray[i]))
                    return canvasArray[i].gameObject;

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }
        static bool IsValidCanvas(Canvas canvas)
        {
            if (canvas == null || !canvas.gameObject.activeInHierarchy)
                return false;

            // It's important that the non-editable canvas from a prefab scene won't be rejected,
            // but canvases not visible in the Hierarchy at all do. Don't check for HideAndDontSave.
            if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0)
                return false;

            if (StageUtility.GetStageHandle(canvas.gameObject) != StageUtility.GetCurrentStageHandle())
                return false;

            return true;
        }
        #endregion
    }
}