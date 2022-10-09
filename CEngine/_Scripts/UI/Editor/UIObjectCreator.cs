//------------------------------------------------------------------------------
// UIObjectCreator.cs
// Copyright 2021 2021/2/24 
// Created by CYM on 2021/2/24
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
namespace CYM.UI
{
    public static class UIObjectCreator
    {
        #region prop
        static Font DefaultFont=>UIConfig.Ins.NormalFont.Default;
        #endregion

        #region U Menu
        [MenuItem("Tools/UICompound/UProto", false, -1000)]
        static void CreateUProto(MenuCommand menu)
        {
            var root = CreateUIRoot("Proto", new Vector2(200, 200), menu);
            RectTransform textRect = CreateUIObject("text", root.gameObject);
            SetTrans(textRect, new Vector2(0f, 0.0f), new Vector2(0f, 0f), new Vector2(1.0f, 1.0f), new Vector2(0.5f, 0.5f), new Vector2(0, 0));
            var icon = AddImage(root.gameObject, Image.Type.Sliced);
            var text = AddText(textRect.gameObject, "New Text");
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 25;
        }
        [MenuItem("Tools/UICompound/UText", false, 1001)]
        static void CreateUText(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateText(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            UText text = go.AddComponent<UText>();
            text.IName.font = DefaultFont;
        }
        [MenuItem("Tools/UICompound/UImage", false, 1002)]
        static void CreateUImage(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateImage(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            go.AddComponent<UImage>();
        }
        [MenuItem("Tools/UICompound/URawImage", false, 1003)]
        static void CreateURawImage(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateRawImage(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            go.AddComponent<URawImage>();
        }
        [MenuItem("Tools/UICompound/UButton", false, 1004)]
        static void CreateUButton(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateButton(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            var temp = go.AddComponent<UButton>();
            var source = go.GetComponent<Button>();
            GameObject.DestroyImmediate(source);
        }
        [MenuItem("Tools/UICompound/UCheck", false, 1005)]
        static void CreateUCheck(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateToggle(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            go.name = "Check";
            go.AddComponent<UCheck>();
            GameObject.DestroyImmediate(go.GetComponent<Toggle>());
        }
        [MenuItem("Tools/UICompound/USlider", false, 1006)]
        static void CreateUSlider(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateSlider(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            go.AddComponent<USlider>();
        }
        [MenuItem("Tools/UICompound/UDropdown", false, 1007)]
        static void CreateUDropdown(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateDropdown(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            go.AddComponent<UDropdown>();
        }
        [MenuItem("Tools/UICompound/UInputField", false, 1008)]
        static void CreateUInputField(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateInputField(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            go.AddComponent<UInput>();
        }
        [MenuItem("Tools/UICompound/UDupplicate", false, 1010)]
        static void CreateUDupplicate(MenuCommand menu)
        {
            var root = CreateUIRoot<UDupplicate>(new Vector2(200, 200), menu);
        }
        [MenuItem("Tools/UICompound/UScroll", false, 1011)]
        static void CreateUScroll(MenuCommand menu)
        {
            UScroll root = CreateUIRoot<UScroll>(new Vector2(200, 200), menu);
            RectTransform placeholder = CreateUIObject("Placeholder", root.GO);
            RectTransform emptyDesc = CreateUIObject("EmptyDesc", root.GO);
            SetTrans(placeholder, new Vector2(0.0f, 0.0f), Vector2.up, Vector2.one, new Vector2(0.5f, 1f), new Vector2(0, 300));
            SetTrans(emptyDesc, new Vector2(0.0f, 0.0f), Vector2.up, Vector2.one, new Vector2(0.5f, 1f), new Vector2(0, 100));
            root.IContent = placeholder;
            var emptyDescText = emptyDesc.gameObject.AddComponent<Text>();
            emptyDescText.text = "Empty Desc";
            emptyDescText.font = DefaultFont;
            emptyDescText.alignment = TextAnchor.UpperCenter;
            root.IEmptyDesc = emptyDescText;
            AddImage(root.GO, Image.Type.Sliced);
            AddMask(root.GO);
        }
        [MenuItem("Tools/UICompound/UScrollRect", false, 1012)]
        static void CreateUScrollRect(MenuCommand menu)
        {
            UScrollRect root = CreateUIRoot<UScrollRect>(new Vector2(200, 200), menu);
            RectTransform placeholder = CreateUIObject("Placeholder", root.GO);
            root.IContent = placeholder;
            SetTrans(placeholder, new Vector2(0.0f, 0.0f), Vector2.up, Vector2.one, new Vector2(0.5f, 1f), new Vector2(0, 300));
            AddImage(root.GO, Image.Type.Sliced);
            AddMask(root.GO);
        }
        [MenuItem("Tools/UICompound/UProgress", false, 1013)]
        static void CreateUProgress(MenuCommand menuCommand)
        {
            UProgress root = CreateUIRoot<UProgress>(new Vector2(200, 50), menuCommand);
            RectTransform fill = CreateUIObject("Fill", root.GO);
            SetTrans(fill, new Vector2(0.0f, 0.0f), Vector2.zero, Vector2.one, new Vector2(0.0f, 0f), new Vector2(0, 0));
            var imageFill = AddImage(fill.gameObject, Image.Type.Filled);
            imageFill.fillAmount = 0.5f;
            imageFill.fillMethod = Image.FillMethod.Horizontal;
        }
        #endregion

        #region UI
        [MenuItem("Tools/UI/Text", false, -1000)]
        static void CreateText(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateText(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            var text = go.GetComponent<Text>();
            text.font = DefaultFont;
        }

        [MenuItem("Tools/UI/Image", false, 1001)]
        static void CreateImage(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateImage(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }
        [MenuItem("Tools/UI/RawImage", false, 1002)]
        static void CreateRawImage(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateRawImage(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }
        [MenuItem("Tools/UI/Scrollbar", false, 1003)]
        static void CreateScrollbar(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateScrollbar(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }
        [MenuItem("Tools/UI/Canvas", false, 1004)]
        static void CreateCanvas(MenuCommand menuCommand)
        {
            var go = CreateNewUI();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            if (go.transform.parent as RectTransform)
            {
                RectTransform rect = go.transform as RectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
            }
            Selection.activeGameObject = go;
        }
        [MenuItem("Tools/UI/Panel", false, 1005)]
        static void CreatePanel(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreatePanel(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }
        #endregion

        #region Utile
        public readonly static Color PanelColor = new Color(1f, 1f, 1f, 0.392f);
        public readonly static Color DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        public readonly static Vector2 ThinElementSize = new Vector2(160f, 20f);
        static private DefaultControls.Resources s_StandardResources;
        static private DefaultControls.Resources GetStandardResources()
        {
            if (s_StandardResources.standard == null)
            {
                s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kStandardSpritePath);
                s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kBackgroundSpritePath);
                s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kInputFieldBackgroundPath);
                s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kKnobPath);
                s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kCheckmarkPath);
                s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kDropdownArrowPath);
                s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kMaskPath);
            }
            return s_StandardResources;
        }
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
        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
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
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

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

        #region Utile2
        static T CreateUIRoot<T>(Vector2 size, MenuCommand menuCommand = null) where T : UControl
        {
            var ret = CreateUIRoot(typeof(T).Name, size, menuCommand);
            ret.AddComponent<T>();
            return ret.GetComponent<T>();
        }
        static GameObject CreateUIRoot(string name, Vector2 size, MenuCommand menuCommand = null)
        {
            GameObject child = new GameObject(name);
            child.layer = LayerMask.NameToLayer("UI");
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            GameObject parent = menuCommand != null ? menuCommand.context as GameObject : null;
            var canvas = GameObject.FindObjectOfType<Canvas>();
            var activeTrans = Selection.activeTransform;
            if (activeTrans != null)
            {
                child.transform.SetParent(activeTrans.transform, false);
            }
            else if (parent != null)
            {
                child.transform.SetParent(parent.transform, false);
            }
            else if (canvas != null)
            {
                child.transform.SetParent(canvas.transform);
            }
            Selection.activeGameObject = child;
            return child;
        }
        static RectTransform CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            SetParentAndAlign(go, parent);
            return go.GetComponent<RectTransform>();
        }
        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }
        static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
        static void SetDefaultColorTransitionValues(Selectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
        }
        static void SetTrans(RectTransform rect, Vector2 pos, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size)
        {
            rect.anchoredPosition = pos;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = size;
            rect.pivot = pivot;
        }
        #endregion

        #region add com
        static Image AddImage(GameObject go, Image.Type type)
        {
            Image temp = go.SafeAddComponet<Image>();
            temp.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kStandardSpritePath);
            temp.type = type;
            temp.color = PanelColor;
            return temp;
        }
        static RawImage AddRawImage(GameObject go)
        {
            RawImage temp = go.SafeAddComponet<RawImage>();
            temp.texture = AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kStandardSpritePath).texture;
            temp.color = PanelColor;
            return temp;
        }
        static Mask AddMask(GameObject go)
        {
            Mask temp = go.SafeAddComponet<Mask>();
            return temp;
        }
        static Text AddText(GameObject go, string name)
        {
            Text text = go.gameObject.SafeAddComponet<Text>();
            text.text = name;
            text.font = DefaultFont;
            return text;
        }
        #endregion

    }
}