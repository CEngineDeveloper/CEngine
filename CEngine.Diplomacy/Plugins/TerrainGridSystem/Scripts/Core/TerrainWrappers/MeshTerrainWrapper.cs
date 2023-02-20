//#define DEBUG_TEXTURES

using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace TGS {
    public class MeshTerrainWrapper : ITerrainWrapper {

        class RendererInfo {
            public MeshRenderer renderer;
            public int layer;
            public Material[] materials;
            public Terrain terrain;
            public Material terrainMaterial;
            public bool terrainInstanced;
        }

        GameObject _gameObject;
        bool _enabled = true;
        string _prefix;
        LayerMask _layerMask;
        bool _searchGlobal;
        int _heightmapWidth = 513, _heightmapHeight = 513;
        float[,] heights;
        Vector3[] normals;
        Bounds _bounds;
        const int TEMP_LAYER = 30;
        readonly List<RendererInfo> rr = new List<RendererInfo>();
        readonly HashSet<GameObject> gameObjects = new HashSet<GameObject>();
        Terrain[] terrains;
        Vector3 boundsCenter, boundsMin, boundsMax, boundsSize, lossyScale;
        static Texture2D tex;

        public bool supportsMultipleObjects {
            get { return true; }
        }

        public bool supportsCustomHeightmap {
            get { return true; }
        }

        public bool supportsPivot {
            get { return true; }
        }

        public GameObject gameObject {
            get { return _gameObject; }
        }

        public Bounds bounds {
            get { return _bounds; }
        }

        public bool enabled {
            get { return _enabled; }
            set {
                _enabled = value;
                int rrCount = rr.Count;
                for (int k = 0; k < rrCount; k++) {
                    RendererInfo ri = rr[k];
                    if (ri.renderer != null) {
                        ri.renderer.enabled = _enabled;
                    } else if (ri.terrain != null) {
                        ri.terrain.drawHeightmap = _enabled;
                        ri.terrain.drawTreesAndFoliage = _enabled;
                    }
                }
            }
        }

        /// <summary>
        /// Meshes that have pivot not centered need to use this parameter for correct highligting
        /// </summary>
        public Vector2 pivot = new Vector2(0.5f, 0.5f);

        public MeshTerrainWrapper(GameObject gameObject, string prefix, LayerMask layerMask, bool searchGlobal, int heightmapWidth, int heightmapHeight) {
            _gameObject = gameObject;
            _prefix = prefix;
            _layerMask = layerMask;
            _layerMask &= ~(1 << 5); // exclude UI layer always
            _searchGlobal = searchGlobal;
            _heightmapWidth = heightmapWidth;
            _heightmapHeight = heightmapHeight;
            GetRenderers();
        }

        public void Dispose() {
            if (tex != null) {
                try {
                    Object.DestroyImmediate(tex);
                    gameObjects.Clear();
                } catch {

                }
            }
        }


        public bool includesGameObject(GameObject go) {
            int rrCount = rr.Count;
            for (int k = 0; k < rrCount; k++) {
                RendererInfo ri = rr[k];
                if (ri.renderer != null) {
                    if (ri.renderer.gameObject == go) return true;
                } else if (ri.terrain != null) {
                    if (ri.terrain.gameObject == go) return true;
                }
            }
            return false;
        }

        void GetRenderers() {
            // Gets all renderers
            MeshRenderer[] renderers;
            gameObjects.Clear();
            rr.Clear();
            terrains = Terrain.activeTerrains;
            if (_searchGlobal) {
                renderers = Object.FindObjectsOfType<MeshRenderer>();
            } else {
                renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            }

            int totalObjects = renderers.Length + terrains.Length;
            bool first = true;
            bool usesPrefix = !string.IsNullOrEmpty(_prefix);

            // Add renderers
            for (int k = 0; k < renderers.Length; k++) {
                MeshRenderer r = renderers[k];
                if (r != null) {
                    bool matchesLayer = (_layerMask & (1 << r.gameObject.layer)) != 0;
                    bool matchesPrefix = !usesPrefix || r.name.StartsWith(_prefix, System.StringComparison.InvariantCulture);
                    if (matchesLayer && matchesPrefix) {
                        RendererInfo ri = new RendererInfo();
                        rr.Add(ri);
                        ri.renderer = r;
                        if (first) {
                            first = false;
                            _bounds = r.bounds;
                        } else {
                            _bounds.Encapsulate(r.bounds);
                        }
                        gameObjects.Add(r.gameObject);
                    }
                }
            }

            // Add terrain elements
            for (int k = 0; k < terrains.Length; k++) {
                Terrain t = terrains[k];
                if (t != null) {
                    bool matchesLayer = (_layerMask & (1 << t.gameObject.layer)) != 0;
                    bool matchesPrefix = !usesPrefix || t.name.StartsWith(_prefix, System.StringComparison.InvariantCulture);
                    if (matchesLayer && matchesPrefix) {
                        RendererInfo ri = new RendererInfo();
                        rr.Add(ri);
                        ri.terrain = t;
                        Bounds bounds = GetTerrainBounds(t);
                        if (first) {
                            first = false;
                            _bounds = bounds;
                        } else {
                            _bounds.Encapsulate(bounds);
                        }
                        gameObjects.Add(t.gameObject);
                    }
                }
            }

            boundsCenter = _bounds.center;
            boundsMin = _bounds.min;
            boundsMax = _bounds.max;
            boundsSize = boundsMax - boundsMin;
        }

        Bounds GetTerrainBounds(Terrain terrain) {
            Bounds bounds = terrain.terrainData.bounds;
            bounds.center += terrain.GetPosition();
            return bounds;
        }

        public void SetupTriggers(TerrainGridSystem tgs) {
#if !ENABLE_INPUT_SYSTEM
            int rrCount = rr.Count;
            for (int k = 0; k < rrCount; k++) {
                if (rr[k].renderer == null)
                    continue;
                TerrainTrigger trigger = rr[k].renderer.GetComponent<TerrainTrigger>();
                if (trigger == null) {
                    trigger = rr[k].renderer.gameObject.AddComponent<TerrainTrigger>();
                }
                trigger.Init<MeshCollider>(tgs);
            }
#endif
        }

        public void Refresh() {
            if (_gameObject != null) {
                GetRenderers();
                ComputeDepth();
            }
        }

        public UnityEngine.TerrainData terrainData {
            get {
                return null;
            }
        }

        public int heightmapMaximumLOD {
            get { return 0; }
            set {
            }
        }

        public int heightmapWidth {
            get { return _heightmapWidth; }
        }

        public int heightmapHeight {
            get { return _heightmapHeight; }
        }

        public Vector3 size {
            get { return boundsSize; }
        }

        public Vector3 center {
            get { return boundsCenter; }
        }


        public float[,] GetHeights(int xBase, int yBase, int width, int height) {
            if (heights == null) {
                ComputeDepth();
            }
            if (width == _heightmapWidth && height == _heightmapHeight) {
                return heights;
            } else {
                float[,] hh = new float[height, width];
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        hh[y, x] = heights[y + yBase, x + xBase];
                    }
                }
                return hh;
            }
        }

        public void SetHeights(int xBase, int yBase, float[,] heights) {
        }

        public T GetComponent<T>() {
            return gameObject.GetComponent<T>();
        }

        /// <summary>
        /// Returns the height at a world space position. Height is in local space.
        /// </summary>
        /// <returns>The height.</returns>
        public float SampleInterpolatedHeight(Vector3 worldPosition) {
            if (worldPosition.x < boundsMin.x) worldPosition.x = boundsMin.x;
            if (worldPosition.x > boundsMax.x) worldPosition.x = boundsMax.x;
            if (worldPosition.z < boundsMin.z) worldPosition.z = boundsMin.z;
            if (worldPosition.z > boundsMax.z) worldPosition.z = boundsMax.z;

            // Depth texture is squared so we need to fit within bounds
            float size = boundsSize.z > boundsSize.x ? boundsSize.z : boundsSize.x;
            if (size == 0) return 0;
            float yPos = (_heightmapHeight - 1) * (worldPosition.z - (boundsCenter.z - size * 0.5f)) / size;
            float xPos = (_heightmapWidth - 1) * (worldPosition.x - (boundsCenter.x - size * 0.5f)) / size;

            int yPos0 = (int)(yPos);
            int xPos0 = (int)(xPos);
            int yPos1 = yPos0 == _heightmapHeight - 1 ? yPos0 : yPos0 + 1;
            int xPos1 = xPos0 == _heightmapWidth - 1 ? xPos0 : xPos0 + 1;
            float h00 = heights[yPos0, xPos0];
            float h01 = heights[yPos0, xPos1];
            float h10 = heights[yPos1, xPos0];
            float h11 = heights[yPos1, xPos1];
            // interpolate
            float fx = xPos - xPos0;
            float fy = yPos - yPos0;
            float h = (1f - fx) * (fy * h10 + (1f - fy) * h01) +
                      fx * (fy * h11 + (1f - fy) * h00);
            return h / lossyScale.y;
        }

        /// <summary>
        /// Returns the height at a world space position. Height is in local space.
        /// </summary>
        /// <returns>The height.</returns>
        public float SampleHeight(Vector3 worldPosition) {
            if (worldPosition.x < boundsMin.x) worldPosition.x = boundsMin.x;
            if (worldPosition.x > boundsMax.x) worldPosition.x = boundsMax.x;
            if (worldPosition.z < boundsMin.z) worldPosition.z = boundsMin.z;
            if (worldPosition.z > boundsMax.z) worldPosition.z = boundsMax.z;

            // Depth texture is squared so we need to fit within bounds
            float size = boundsSize.z > boundsSize.x ? boundsSize.z : boundsSize.x;
            if (size == 0) return 0;
            float yPos = (_heightmapHeight - 1) * (worldPosition.z - (boundsCenter.z - size * 0.5f)) / size;
            float xPos = (_heightmapWidth - 1) * (worldPosition.x - (boundsCenter.x - size * 0.5f)) / size;

            int yPos0 = (int)(yPos);
            int xPos0 = (int)(xPos);
            float h = heights[yPos0, xPos0];
            return h / lossyScale.y;
        }


        public Transform transform {
            get {
                return _gameObject.transform;
            }
        }

        /// <summary>
        /// Returns normalized terrain normal at x, y position where x and y are values from 0-1.
        /// </summary>
        /// <returns>The interpolated normal.</returns>
        public Vector3 GetInterpolatedNormal(float x, float y) {
            // Normals texture is squared so we need to fit within bounds
            float size = boundsSize.z > boundsSize.x ? boundsSize.z : boundsSize.x;
            x = (x - 0.5f) * boundsSize.x / size + 0.5f;
            y = (y - 0.5f) * boundsSize.z / size + 0.5f;
            if (x < 0 || x > 1f || y < 0 || y > 1)
                return Misc.Vector3up;

            float yPos = y * (_heightmapHeight - 1);
            float xPos = x * (_heightmapWidth - 1);
            int yPos0 = (int)(yPos);
            int xPos0 = (int)(xPos);
            int yPos1 = yPos0 == _heightmapHeight - 1 ? yPos0 : yPos0 + 1;
            int xPos1 = xPos0 == _heightmapWidth - 1 ? xPos0 : xPos0 + 1;
            Vector3 n00 = normals[yPos0 * _heightmapWidth + xPos0];
            Vector3 n01 = normals[yPos0 * _heightmapWidth + xPos1];
            Vector3 n10 = normals[yPos1 * _heightmapWidth + xPos0];
            Vector3 n11 = normals[yPos1 * _heightmapWidth + xPos1];
            // interpolate
            float fx = xPos - xPos0;
            float fy = yPos - yPos0;
            Vector3 norm = Misc.Vector3zero;
            norm.x = (1f - fx) * (fy * n10.x + (1f - fy) * n01.x) + fx * (fy * n11.x + (1f - fy) * n00.x);
            norm.y = (1f - fx) * (fy * n10.y + (1f - fy) * n01.y) + fx * (fy * n11.y + (1f - fy) * n00.y);
            norm.z = (1f - fx) * (fy * n10.z + (1f - fy) * n01.z) + fx * (fy * n11.z + (1f - fy) * n00.z);
            return norm.normalized;
        }


        /// <summary>
        /// Returns normalized terrain normal at x, y position where x and y are values from 0-1.
        /// </summary>
        /// <returns>The interpolated normal.</returns>
        public Vector3 GetNormal(float x, float y) {
            // Normals texture is squared so we need to fit within bounds
            float size = boundsSize.z > boundsSize.x ? boundsSize.z : boundsSize.x;
            x = (x - 0.5f) * boundsSize.x / size + 0.5f;
            y = (y - 0.5f) * boundsSize.z / size + 0.5f;
            if (x < 0 || x > 1f || y < 0 || y > 1)
                return Misc.Vector3up;

            float yPos = y * (_heightmapHeight - 1);
            float xPos = x * (_heightmapWidth - 1);
            int yPos0 = (int)(yPos);
            int xPos0 = (int)(xPos);
            return normals[yPos0 * _heightmapWidth + xPos0];
        }


        public Vector3 localCenter {
            get {
                Vector3 lossyScale = _gameObject.transform.lossyScale;
                float x = (boundsCenter.x - _gameObject.transform.position.x + (0.5f - pivot.x) * boundsSize.x) / lossyScale.x;
                float z = (boundsCenter.z - _gameObject.transform.position.z + (0.5f - pivot.y) * boundsSize.z) / lossyScale.z;
                float y = (boundsMin.y - _gameObject.transform.position.y) / lossyScale.y;
                return new Vector3(x, y, z);
            }
        }


        void ComputeDepth() {
            lossyScale = _gameObject.transform.lossyScale;
            GameObject camObject = Object.Instantiate(Resources.Load<GameObject>("Prefabs/DepthCam"));
            Camera cam = camObject.GetComponent<Camera>();
            Vector3 top = boundsMax;
            top.x = boundsCenter.x;
            top.z = boundsCenter.z;
            top.y += 1f;
            cam.transform.position = top;
            cam.transform.rotation = Quaternion.Euler(90, 0, 0);
            cam.renderingPath = RenderingPath.Forward;
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.orthographic = true;
            float size = Mathf.Max(boundsSize.x, boundsSize.z) * 0.5f;
            cam.orthographicSize = size;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.white;
            cam.cullingMask = 1 << TEMP_LAYER;
            cam.nearClipPlane = 0.5f;
            float farClipPlane = _bounds.size.y + 2f;
            cam.farClipPlane = farClipPlane;

            // Temporarily switch layers
            BackupMaterials();

            // Render depth for heightmap
            RenderTexture rt = RenderTexture.GetTemporary(_heightmapWidth, _heightmapHeight, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, 1);
            cam.targetTexture = rt;
            SetShader("Terrain Grid System/DepthCopy");
            cam.Render();
            RenderTexture.active = rt;

            if (tex == null || tex.width != _heightmapWidth || tex.height != _heightmapHeight) {
                tex = new Texture2D(_heightmapWidth, _heightmapHeight, TextureFormat.RGBAFloat, false);
            }
            // Read heightmap
            tex.ReadPixels(new Rect(0, 0, _heightmapWidth, _heightmapHeight), 0, 0);
            if (heights == null || heights.GetUpperBound(0) != _heightmapHeight - 1 || heights.GetUpperBound(1) != _heightmapHeight - 1) {
                heights = new float[_heightmapHeight, _heightmapWidth];
            }

#if DEBUG_TEXTURES
            System.IO.File.WriteAllBytes("depth.png", tex.EncodeToPNG());
#endif

            Color32[] colors = tex.GetPixels32();
            float minY = _bounds.min.y;
            for (int index = 0, y = 0; y < _heightmapHeight; y++) {
                for (int x = 0; x < _heightmapWidth; x++) {
                    float depth = colors[index].r / 255f;
                    float far = top.y - depth * farClipPlane;
                    float h = far - minY;
                    //if (h < 0) {
                    //    h = -0.001f;
                    //}
                    heights[y, x] = h;
                    index++;
                }
            }

            // Render normals
            SetShader("Terrain Grid System/NormalsCopy");
            cam.Render();
            // Read normals
            if (normals == null || normals.Length != _heightmapWidth * _heightmapHeight) {
                normals = new Vector3[_heightmapHeight * _heightmapWidth];
            }
            tex.ReadPixels(new Rect(0, 0, _heightmapWidth, _heightmapHeight), 0, 0);

            RestoreMaterials();

#if DEBUG_TEXTURES
            System.IO.File.WriteAllBytes("normals.png", tex.EncodeToPNG());
#endif

            colors = tex.GetPixels32();
            Vector3 norm;
            for (int index = 0, y = 0; y < _heightmapHeight; y++) {
                for (int x = 0; x < _heightmapWidth; x++) {
                    Color32 color = colors[index];
                    if (color.r == 255 && color.g == 255) {
                        normals[index] = Misc.Vector3up;
                    } else {
                        norm.x = (color.r / 255f) * 2f - 1f;
                        norm.y = (color.g / 255f) * 2f - 1f;
                        norm.z = (color.b / 255f) * 2f - 1f;
                        normals[index] = norm;
                    }
                    index++;
                }
            }

            // Release
            cam.targetTexture = null;
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            Object.DestroyImmediate(camObject);
        }


        void BackupMaterials() {
            int rrCount = rr.Count;
            for (int k = 0; k < rrCount; k++) {
                RendererInfo ri = rr[k];
                if (ri.renderer != null) {
                    ri.layer = ri.renderer.gameObject.layer;
                    ri.renderer.gameObject.layer = TEMP_LAYER;
                    ri.materials = ri.renderer.sharedMaterials;
                } else if (ri.terrain != null) {
                    ri.layer = ri.terrain.gameObject.layer;
                    ri.terrain.gameObject.layer = TEMP_LAYER;
                    ri.terrainMaterial = ri.terrain.materialTemplate;
                    ri.terrainInstanced = ri.terrain.drawInstanced;
                }
            }
        }

        void SetShader(string shaderName) {
            Material material = new Material(Shader.Find(shaderName));
            int rrCount = rr.Count;
            for (int k = 0; k < rrCount; k++) {
                RendererInfo ri = rr[k];
                if (ri.renderer != null) {
                    int matCount = rr[k].renderer.sharedMaterials.Length;
                    if (matCount == 0) matCount = 1;
                    Material[] newMaterials = new Material[matCount];
                    for (int m = 0; m < matCount; m++) {
                        newMaterials[m] = material;
                    }
                    ri.renderer.sharedMaterials = newMaterials;
                } else if (ri.terrain != null) {
                    ri.terrain.materialTemplate = material;
                    ri.terrain.drawInstanced = false;
                }
            }
        }

        void RestoreMaterials() {
            int rrCount = rr.Count;
            for (int k = 0; k < rrCount; k++) {
                RendererInfo ri = rr[k];
                if (ri.renderer != null) {
                    ri.renderer.sharedMaterials = ri.materials;
                    ri.renderer.gameObject.layer = ri.layer;
                } else if (ri.terrain != null) {
                    ri.terrain.materialTemplate = ri.terrainMaterial;
                    ri.terrain.gameObject.layer = ri.layer;
                    ri.terrain.drawInstanced = ri.terrainInstanced;
                }
            }
        }

        public bool Contains(GameObject gameObject) {
            return gameObjects.Contains(gameObject);
        }

        public Vector3 GetLocalPoint(GameObject gameObject, Vector3 worldSpacePosition) {
            Vector3 localPoint;
            localPoint.x = (worldSpacePosition.x - boundsMin.x) / boundsSize.x - 0.5f;
            localPoint.y = (worldSpacePosition.z - boundsMin.z) / boundsSize.z - 0.5f;
            localPoint.z = 0;
            return localPoint;
        }

    }
}

