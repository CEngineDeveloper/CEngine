using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
namespace CYM.UI.Particle
{
    [ExecuteInEditMode, RequireComponent(typeof(Canvas))]
    public class UIParticleCanvas : MonoBehaviour
    {
        [Range(0.05f, 1f)]
        public float maskResolutionScale = 1f;
        public int maskLayer;

        private List<UIParticle> depthObjects = new List<UIParticle>();
        private Canvas canvas;
        private RectTransform canvasRect;
        private Vector3[] canvasCorners = new Vector3[4];
        private RenderTexture mask;
        private Camera maskCamera;
        private float minDepth, maxDepth;
        private Vector2 lastScreenSize;

        protected void Awake()
        {
            SetDirty();
            Trans = transform;

            canvas = GetComponent<Canvas>();
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        protected void OnEnable()
        {
            SetDirty();
            Trans = transform;
            Camera.onPreRender += OnCameraPreRender;
            //Camera.onPostRender += OnCameraPostRender;
        }

        protected void OnDisable()
        {
            Camera.onPreRender -= OnCameraPreRender;
            //Camera.onPostRender -= OnCameraPostRender;
        }

        protected void Update()
        {
            if (lastScreenSize.x != Screen.width
                || lastScreenSize.y != Screen.height)
            {
                SetDirty();
            }

            if (!IsDirty)
                return;

#if UNITY_EDITOR
            EditorErrorLog = "";
#endif

            RefreshMask();
        }

        protected void OnDestroy()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (mask)
                    DestroyImmediate(mask);

                if (maskCamera)
                    DestroyImmediate(maskCamera.gameObject);
            };
#else
		if(mask)
            DestroyImmediate(mask);

        if (maskCamera)
            DestroyImmediate(maskCamera.gameObject);
#endif

        }

        public void SetDirty()
        {
            IsDirty = true;
        }

        public void SetToReinitialize()
        {
            if (mask)
            {
                DestroyImmediate(mask);
                mask = null;
            }
            SetDirty();
        }

        public void Reinitialize()
        {
            Profiler.BeginSample("Reinitialize UI Particle Canvas");
            canvas = GetComponent<Canvas>();
            canvasRect = canvas.GetComponent<RectTransform>();

            if (canvas.worldCamera == null)
            {
                ErrorLog("Canvas is missing camera setup", canvas);
                Profiler.EndSample();
                return;
            }

            mask = new RenderTexture(Mathf.RoundToInt(Screen.width * maskResolutionScale),
                                        Mathf.RoundToInt(Screen.height * maskResolutionScale),
                                        16, RenderTextureFormat.ARGB32);
            mask.name = "UIParticleCanvasMask";


            if (maskCamera != null)
                DestroyImmediate(maskCamera.gameObject);

            GameObject maskCameraGO = new GameObject("MaskCamera");
            maskCameraGO.hideFlags = HideFlags.HideAndDontSave;
            maskCameraGO.layer = maskLayer;
            maskCameraGO.transform.parent = canvas.worldCamera.transform;
            maskCameraGO.transform.localPosition = Vector3.zero;
            maskCameraGO.transform.localRotation = Quaternion.identity;
            maskCameraGO.transform.localScale = Vector3.one;
            maskCamera = maskCameraGO.AddComponent<Camera>();
            maskCamera.CopyFrom(canvas.worldCamera);
            maskCamera.cullingMask = 1 << maskLayer;
            maskCamera.clearFlags = CameraClearFlags.Color;
            maskCamera.backgroundColor = new Color(1f, 0f, 0f, 0f);
            maskCamera.targetTexture = mask;

            for (int i = 0; i < depthObjects.Count; i++)
            {
                depthObjects[i].RefreshRenderer(true);
            }

            SetDirty();
            Profiler.EndSample();
        }

        public void UnregisterUIParticleDepthObject(UIParticle obj)
        {
            depthObjects.Remove(obj);
            SetDirty();
        }

        public void RegisterUIParticleDepthObject(UIParticle obj)
        {
            if (obj == null || depthObjects.Contains(obj))
                return;

            depthObjects.Add(obj);
            depthObjects.Sort(SortDepthObjects);
            SetDirty();
        }

        private void RefreshMask()
        {
            Profiler.BeginSample("Refresh UI Particle Canvas Mask");
            if (mask == null || maskCamera == null)
                Reinitialize();

            maskCamera.enabled = true;

            minDepth = Mathf.Infinity;
            maxDepth = Trans.position.z;
            for (int i = 0; i < depthObjects.Count; i++)
            {
                float z = depthObjects[i].transform.position.z;

                if (z < minDepth)
                    minDepth = z;
                if (z > maxDepth)
                    maxDepth = z;
            }

            RenderTexture.active = maskCamera.targetTexture;
            maskCamera.Render();
            maskCamera.enabled = false;
            RenderTexture.active = null;

            IsDirty = false;
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            Profiler.EndSample();
        }

        private int SortDepthObjects(UIParticle x, UIParticle y)
        {
            return y.transform.position.z.CompareTo(x.transform.position.z);
        }

        private void OnCameraPreRender(Camera cam)
        {
            if (cam == maskCamera)
                OnMaskCameraPreRender();

            if (canvas && cam == canvas.worldCamera)
                OnCanvasCameraPreRender();
        }

        /*private void OnCameraPostRender(Camera cam)
        {
            if(cam == canvas.worldCamera)
                OnCanvasCameraPostRender();
        }*/

        private void OnCanvasCameraPreRender()
        {
            if (maskCamera == null)
                Reinitialize();
            Shader.SetGlobalFloat("_UIParticleCanvasZMin", minDepth);
            Shader.SetGlobalFloat("_UIParticleCanvasZMax", maxDepth);
            Shader.SetGlobalTexture("_UIDepthTex", maskCamera.targetTexture);
            Shader.SetGlobalVector("_UIParticleDepthTexPosParams", DepthTexPosParams);
            //Shader.EnableKeyword("_UIDepthCulling");

            for (int i = 0; i < depthObjects.Count; i++)
            {
                depthObjects[i].CheckForRefeshRenderer();
            }
        }

        private void OnCanvasCameraPostRender()
        {
            //Shader.DisableKeyword("_UIDepthCulling");
        }

        private void OnMaskCameraPreRender()
        {
            Shader.SetGlobalFloat("_UIParticleCanvasZMin", minDepth);
            Shader.SetGlobalFloat("_UIParticleCanvasZMax", maxDepth);
            Shader.SetGlobalVector("_UIParticleDepthTexPosParams", DepthTexPosParams);
        }

        protected void ErrorLog(string error, UnityEngine.Object obj = null)
        {
            if (obj == null)
                obj = this;
#if UNITY_EDITOR
            if (Application.isPlaying)
                Debug.LogError(error, obj);
            else
            {
                if (EditorErrorLog.Length > 0)
                    EditorErrorLog += "\n" + error;
                else
                    EditorErrorLog += error;
            }
#else
            Debug.LogError(error, obj);
#endif
        }

        public Vector4 DepthTexPosParams
        {
            get
            {
                canvasRect.GetWorldCorners(canvasCorners);
                return new Vector4(canvasCorners[0].x, canvasCorners[0].y, canvasCorners[2].x - canvasCorners[0].x, canvasCorners[2].y - canvasCorners[0].y);
            }
        }

#if UNITY_EDITOR
        public string EditorErrorLog { get; private set; }
#endif

        public bool IsDirty { get; private set; }

        public Transform Trans { get; private set; }

        public RenderTexture Mask
        {
            get
            {
                return mask;
            }
        }

        public Canvas Canvas
        {
            get
            {
                if (canvas == null)
                    canvas = GetComponent<Canvas>();
                return canvas;
            }
        }
    }
}