using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Sprites;
using UnityEngine.UI;
namespace CYM.UI.Particle
{
    public class UIParticleDepthObjectInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Vector3 rectSize;
        public Vector3[] rectVerticesTmp = new Vector3[4];
    }
    public enum UIParticleMaskRenderMode
    {
        JustDepth, CullingMask
    };

    public enum UIParticleMaskSourceMode
    {
        Image, RawImage, MaskTexture
    };

    public enum UIParticleMaskAlphaMode
    {
        AlphaTest, Dithering, NoAlpha, Translucency
    }

    //[ExecuteInEditMode]
    public class UIParticle : MonoBehaviour
    {
        public const string depthRenderShaderName = "MODev/UIParticle/Mask/DepthRender";
        public const string cullRenderShaderName = "MODev/UIParticle/Mask/CullRender";
        public UIParticleMaskRenderMode renderMode;
        public UIParticleMaskSourceMode source;
        public UIParticleMaskAlphaMode alphaMode;
        public bool willRectResizeInRuntime;//enables check if rect size was changed, enable ony if needed because it makes higher performance hit per depth obj
        [HideInInspector]
        public Texture2D maskTexture;
        [HideInInspector]
        public int cullingMaskVal = 255;

        [Range(1, 255)]
        [HideInInspector]
        public int ditheringSteps = 255;
        [Range(0f, 1f)]
        public float alphaTestTreshold = 0.05f;
        [Range(0f, 1f)]
        [HideInInspector]
        public float translucencyFactor = 1f;
        [HideInInspector]
        public Shader depthRenderShader;
        [HideInInspector]
        public Shader cullRenderShader;
        [HideInInspector]
        private GameObject maskRendererObj;
        private bool needEditorRefresh;
        private UIParticleCanvas particleCanvas;
        private UIParticleDepthObjectInfo previousObjectInfo = new UIParticleDepthObjectInfo();

        protected void Awake()
        {
            RectTrans = GetComponent<RectTransform>();
        }

        protected void OnEnable()
        {
            Util.Invoke(() => {
                particleCanvas = GetComponentInParent<UIParticleCanvas>();
                RefreshRenderer(true);
                particleCanvas.RegisterUIParticleDepthObject(this);
            }, 0.1f);


        }

        protected void OnDisable()
        {
            if (particleCanvas == null)
                return;
            particleCanvas.UnregisterUIParticleDepthObject(this);
            if (maskRendererObj != null)
                DestroyImmediate(maskRendererObj);
        }


        public void CheckForRefeshRenderer()
        {
            Profiler.BeginSample("UI Particle depth object CheckForRefeshRenderer");
            bool recreateRenerer = false;
            bool refreshRenderer = false;

            if (willRectResizeInRuntime || !Application.isPlaying)
            {
                RectTrans.GetLocalCorners(previousObjectInfo.rectVerticesTmp);
                Vector3 newRectSize = previousObjectInfo.rectVerticesTmp[2] - previousObjectInfo.rectVerticesTmp[0];

                if (newRectSize != previousObjectInfo.rectSize)
                {
                    recreateRenerer = true;
                    refreshRenderer = true;
                }
            }

            if (!refreshRenderer && RectTrans.position != previousObjectInfo.position)
                refreshRenderer = true;


            if (!refreshRenderer && RectTrans.localScale != previousObjectInfo.scale)
                refreshRenderer = true;

            if (!refreshRenderer && RectTrans.rotation != previousObjectInfo.rotation)
                refreshRenderer = true;

            if (refreshRenderer)
            {
                RefreshRenderer(recreateRenerer);
            }
            Profiler.EndSample();
        }

        public void RefreshRenderer(bool recreateRenderer = false, bool afterRetry = false)
        {
#if UNITY_EDITOR
            EditorErrorLog = "";
#endif
            Profiler.BeginSample("RefreshRenderer, recreateRenderer = " + recreateRenderer);
            if (recreateRenderer && maskRendererObj != null)
            {
                if (Application.isPlaying)
                    Destroy(maskRendererObj);
                else
                    DestroyImmediate(maskRendererObj);
                maskRendererObj = null;
            }

            if (particleCanvas == null)
                particleCanvas = GetComponentInParent<UIParticleCanvas>();
            if (particleCanvas == null)
            {
                ErrorLog("Missing UIParticleCanvas in parents");
                Profiler.EndSample();
                return;
            }

            if (maskRendererObj == null)
            {
                CreateMaskRendererObject(particleCanvas);
            }

            MeshFilter mf = maskRendererObj.GetComponent<MeshFilter>();
            MeshRenderer mr = null;
            if (mf == null)
            {
                mf = maskRendererObj.AddComponent<MeshFilter>();
                mr = maskRendererObj.AddComponent<MeshRenderer>();

                RecreateMaterial(mr);
            }

            if (mr == null)
                mr = maskRendererObj.GetComponent<MeshRenderer>();


            Texture2D texture = null;
            Mesh mesh = null;
            if (mf.sharedMesh != null)
            {
                mesh = mf.sharedMesh;
                mesh.Clear();
            }

            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.MarkDynamic();
                mesh.name = "Mesh" + maskRendererObj.name;
            }

            switch (source)
            {
                case UIParticleMaskSourceMode.Image:
                    if (!GetMeshAndTextureFromImage(ref mesh, out texture))
                    {
                        if (!afterRetry)
                            RetryRefreshRenderer(recreateRenderer);
                        return;
                    }
                    break;
                case UIParticleMaskSourceMode.RawImage:
                    if (!GetMeshAndTextureFromRawImage(ref mesh, out texture))
                    {
                        if (!afterRetry)
                            RetryRefreshRenderer(recreateRenderer);
                        return;
                    }
                    break;
                case UIParticleMaskSourceMode.MaskTexture:
                    if (!GetMeshAndTextureFromMaskTexture(ref mesh, out texture))
                    {
                        if (!afterRetry)
                            RetryRefreshRenderer(recreateRenderer);
                        return;
                    }
                    break;
            }

            mesh.RecalculateBounds();
            mf.sharedMesh = mesh;
            if (mr == null)
            {
                Debug.LogError("mr 为 null!!");
                return;
            }
            mr.sharedMaterial.SetTexture("_MainTex", texture);
            RefreshMaterialProperties(mr.sharedMaterial);

            previousObjectInfo.position = RectTrans.position;
            previousObjectInfo.scale = RectTrans.localScale;
            previousObjectInfo.rotation = RectTrans.rotation;

            particleCanvas.SetDirty();
            Profiler.EndSample();
        }
        public void RefreshMaterial()
        {
            var mr = maskRendererObj.GetComponent<MeshRenderer>();
            if (mr == null)
            {
                Debug.LogError("mr 为 null!!");
                return;
            }
            if (mr.sharedMaterial == null)
            {
                Debug.LogError("mr sharedMaterial 为 null!!");
                return;
            }
            RefreshMaterialProperties(mr.sharedMaterial);
        }

        private void RetryRefreshRenderer(bool recreateRenderer)
        {
            StartCoroutine(RetryRefreshRendererCor(recreateRenderer));
#if UNITY_EDITOR
            if (!Application.isPlaying)
                needEditorRefresh = true;
#endif
        }

        private IEnumerator RetryRefreshRendererCor(bool recreateRenderer)
        {
            yield return new WaitForEndOfFrame();
            RefreshRenderer(recreateRenderer, true);
        }

        private void RecreateMaterial(MeshRenderer mr)
        {
            if (depthRenderShader == null)
                depthRenderShader = Shader.Find(depthRenderShaderName);

            if (cullRenderShader == null)
                cullRenderShader = Shader.Find(cullRenderShaderName);

            Shader shader = renderMode == UIParticleMaskRenderMode.JustDepth ? depthRenderShader : cullRenderShader;
            mr.sharedMaterial = new Material(shader);
        }

        private void RefreshMaterialProperties(Material mat)
        {
            if (renderMode == UIParticleMaskRenderMode.CullingMask)
            {
                mat.SetFloat("_MaskVal", cullingMaskVal / 255f);
            }

            if (alphaMode == UIParticleMaskAlphaMode.AlphaTest)
            {
                mat.EnableKeyword("ALPHAMODE_ALPHATEST");
                mat.SetFloat("_AlphaTestTreshold", alphaTestTreshold);
            }
            else if (alphaMode == UIParticleMaskAlphaMode.Dithering)
            {
                mat.EnableKeyword("ALPHAMODE_DITHERING");
                mat.SetFloat("_DitheringStep", 1f / ditheringSteps);
            }
            else if (alphaMode == UIParticleMaskAlphaMode.Translucency)
            {
                mat.EnableKeyword("ALPHAMODE_TRANSLUCENCY");
                mat.SetFloat("_TranslucencyFactor", translucencyFactor);
            }
            else
            {
                mat.EnableKeyword("ALPHAMODE_NOALPHA");
            }
        }

        private void CreateMaskRendererObject(UIParticleCanvas particleCanvas)
        {
            maskRendererObj = new GameObject("UIParticleMask" + name);
            maskRendererObj.transform.parent = transform;
            maskRendererObj.transform.localPosition = Vector3.zero;
            maskRendererObj.transform.localRotation = Quaternion.identity;
            maskRendererObj.transform.localScale = Vector3.one;
            maskRendererObj.layer = particleCanvas.maskLayer;
            maskRendererObj.hideFlags = HideFlags.HideAndDontSave;
        }

        private bool GetMeshAndTextureFromMaskTexture(ref Mesh mesh, out Texture2D texture)
        {
            texture = maskTexture;
            /*if(this.maskTexture == null)
            {
                Debug.LogError("Missing texture to generate", this);
                return false;
            }*/

            RectTransform rectTrans = GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                ErrorLog("Missing RectTransform in game object. Object is propably out of canvas");
                return false;
            }

            Vector3[] vertices;
            int[] indices;
            Vector2[] uvs;
            if (!GenerateMeshDataFromRectTransform(null, rectTrans, out vertices, out indices, out uvs))
                return false;

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = indices;

            return true;
        }

        private bool GetMeshAndTextureFromRawImage(ref Mesh mesh, out Texture2D texture)
        {
            texture = null;
            RawImage image = GetComponent<RawImage>();
            if (image == null)
            {
                ErrorLog("Missing RawImage in game object");
                return false;
            }
            texture = image.texture as Texture2D;

            RectTransform rectTrans = GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                ErrorLog("Missing RectTransform in game object. Object is propably out of canvas");
                return false;
            }

            Vector3[] vertices;
            int[] indices;
            Vector2[] uvs;
            if (!GenerateMeshDataFromRectTransform(null, rectTrans, out vertices, out indices, out uvs))
                return false;

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = indices;

            return true;
        }
        private bool GetMeshAndTextureFromImage(ref Mesh mesh, out Texture2D texture)
        {
            texture = null;
            Image image = GetComponent<Image>();
            if (image == null)
            {
                ErrorLog("Missing Image in game object");
                return false;
            }

            if (image.sprite == null)
            {
                ErrorLog("Missing sprite in Image in game object");
                return false;
            }

            Sprite sprite = image.sprite;
            texture = sprite.texture;

            RectTransform rectTrans = GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                ErrorLog("Missing RectTransform in game object. Object is propably out of canvas");
                return false;
            }

            Vector3[] vertices = null;
            int[] indices = null;
            Vector2[] uvs = null;

            bool useSimpleSprite = image.type == Image.Type.Simple;
            if (!useSimpleSprite)
            {
                switch (image.type)
                {
                    case Image.Type.Sliced:
                        useSimpleSprite = GenerateMeshDataFromSlicedSprite(sprite, rectTrans, out vertices, out indices, out uvs);
                        break;
                    case Image.Type.Tiled:
                        useSimpleSprite = true;
                        break;
                    case Image.Type.Filled:
                        useSimpleSprite = GenerateMeshDataFromSlicedSprite(sprite, rectTrans, out vertices, out indices, out uvs);
                        break;
                }
            }

            if (useSimpleSprite)
            {
                if (sprite.packed)
                {
                    GenerateMeshDataFromPackedSprite(sprite, rectTrans, out vertices, out indices, out uvs);
                }
                else
                {
                    if (!GenerateMeshDataFromRectTransform(sprite, rectTrans, out vertices, out indices, out uvs))
                        return false;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = indices;

            return true;
        }
        private bool GenerateMeshDataFromSlicedSprite(Sprite sprite, RectTransform rectTrans, out Vector3[] vertices, out int[] indices, out Vector2[] uvs)
        {
            vertices = null;
            indices = null;
            uvs = null;
            if (sprite.border.sqrMagnitude <= 0f)
                return true;//use simple sprite

            Vector3[] rectVertices = previousObjectInfo.rectVerticesTmp;
            rectTrans.GetLocalCorners(rectVertices);
            if (rectVertices[0] == rectVertices[1] && rectVertices[0] == rectVertices[2] && rectVertices[0] == rectVertices[3])
            {
                return false;
            }

            Vector3 newRectSize = rectVertices[2] - rectVertices[0];
            previousObjectInfo.rectSize = newRectSize;

            Vector4 spriteOuterUv = DataUtility.GetOuterUV(sprite);
            Vector4 spriteInnerUV = DataUtility.GetInnerUV(sprite);
            //Vector4 spritePadding = DataUtility.GetPadding(sprite);
            Vector4 spriteBorder = sprite.border * particleCanvas.Canvas.referencePixelsPerUnit / sprite.pixelsPerUnit;

            bool horizontalSplit = newRectSize.x > spriteBorder.x + spriteBorder.z;
            bool verticalSplit = newRectSize.y > spriteBorder.y + spriteBorder.w;

            Vector3 halfRectSize = newRectSize * 0.5f;
            Vector3[] borderAddVals = new Vector3[]
            {
            new Vector3(horizontalSplit ? spriteBorder.x : halfRectSize.x, verticalSplit ? spriteBorder.y : halfRectSize.y, 0),
            new Vector3(horizontalSplit ? spriteBorder.x : halfRectSize.x, verticalSplit ? -spriteBorder.w : -halfRectSize.y, 0),
            new Vector3(horizontalSplit ? -spriteBorder.z : -halfRectSize.x, verticalSplit ? -spriteBorder.w : -halfRectSize.y, 0),
            new Vector3(horizontalSplit ? -spriteBorder.z : -halfRectSize.x, verticalSplit ? spriteBorder.y : halfRectSize.y, 0)
            };

            Vector2[] outerUVs = new Vector2[]
            {
            new Vector2(spriteOuterUv.x, spriteOuterUv.y), new Vector2(spriteOuterUv.x, spriteOuterUv.w),//outer
			new Vector2(spriteOuterUv.z, spriteOuterUv.w), new Vector2(spriteOuterUv.z, spriteOuterUv.y),//outer
            };

            Vector2[] innerUVs = new Vector2[]
            {
            new Vector2(spriteInnerUV.x, spriteInnerUV.y), new Vector2(spriteInnerUV.x, spriteInnerUV.w),//inner
			new Vector2(spriteInnerUV.z, spriteInnerUV.w), new Vector2(spriteInnerUV.z, spriteInnerUV.y),//inner
            };

            List<Vector3> verticesList = new List<Vector3>();
            List<Vector2> uvsList = new List<Vector2>();
            List<int> indicesList = new List<int>();
            Dictionary<Vector3, int> alreadyUsedVectors = new Dictionary<Vector3, int>();

            AddQuadToTriangles(alreadyUsedVectors, verticesList, uvsList, indicesList, rectVertices[0], rectVertices[0] + borderAddVals[0], outerUVs[0], innerUVs[0]);//left bot
            AddQuadToTriangles(alreadyUsedVectors, verticesList, uvsList, indicesList,
                                rectVertices[1] + new Vector3(0, borderAddVals[1].y, 0), rectVertices[1] + new Vector3(borderAddVals[1].x, 0, 0),
                                new Vector2(outerUVs[0].x, innerUVs[1].y), new Vector2(innerUVs[0].x, outerUVs[1].y));//left top
            AddQuadToTriangles(alreadyUsedVectors, verticesList, uvsList, indicesList, rectVertices[2] + borderAddVals[2], rectVertices[2], innerUVs[1], outerUVs[1]);//right top
            AddQuadToTriangles(alreadyUsedVectors, verticesList, uvsList, indicesList,
                                rectVertices[3] + new Vector3(borderAddVals[3].x, 0, 0), rectVertices[3] + new Vector3(0, borderAddVals[3].y, 0),
                                new Vector2(innerUVs[1].x, outerUVs[0].y), new Vector2(outerUVs[0].x, innerUVs[0].y));//right bot

            if (horizontalSplit)
            {
                AddQuadToTriangles(alreadyUsedVectors, verticesList, uvsList, indicesList,
                                    rectVertices[0] + new Vector3(borderAddVals[0].x, 0, 0), rectVertices[3] + borderAddVals[3],
                                    new Vector2(innerUVs[0].x, outerUVs[0].y), new Vector2(innerUVs[1].x, innerUVs[0].y));//bot

                AddQuadToTriangles(alreadyUsedVectors, verticesList, uvsList, indicesList,
                                    rectVertices[1] + borderAddVals[1], rectVertices[2] + new Vector3(borderAddVals[2].x, 0, 0),
                                    innerUVs[1], new Vector2(innerUVs[2].x, outerUVs[2].y));//top
            }

            if (verticalSplit)
            {
                AddQuadToTriangles(alreadyUsedVectors, verticesList, uvsList, indicesList,
                                    rectVertices[0] + new Vector3(0, borderAddVals[0].y, 0), rectVertices[1] + borderAddVals[1],
                                    new Vector2(outerUVs[0].x, innerUVs[0].y), innerUVs[1]);//left

                AddQuadToTriangles(alreadyUsedVectors, verticesList, uvsList, indicesList,
                                    rectVertices[3] + borderAddVals[3], rectVertices[2] + new Vector3(0, borderAddVals[2].y, 0),
                                    innerUVs[3], new Vector2(outerUVs[2].x, innerUVs[2].y));//right

                if (horizontalSplit)
                {
                    AddQuadToTriangles(alreadyUsedVectors, verticesList, uvsList, indicesList,
                                        rectVertices[0] + borderAddVals[0], rectVertices[2] + borderAddVals[2],
                                        innerUVs[0], innerUVs[2]);//center
                }
            }

            vertices = verticesList.ToArray();
            uvs = uvsList.ToArray();
            indices = indicesList.ToArray();

            return false;//don't use simple sprite
        }
        private void AddQuadToTriangles(Dictionary<Vector3, int> alreadyUsedVectors, List<Vector3> verticesList, List<Vector2> uvsList, List<int> indicesList,
                                            Vector3 leftBotVert, Vector3 rightTopVert, Vector2 leftBotUv, Vector2 rightTopUv)
        {
            Vector3 leftTopVert = new Vector3(leftBotVert.x, rightTopVert.y, 0f);
            Vector3 rightBotVert = new Vector3(rightTopVert.x, leftBotVert.y, 0f);
            Vector2 leftTopUv = new Vector2(leftBotUv.x, rightTopUv.y);
            Vector2 rightBotUv = new Vector2(rightTopUv.x, leftBotUv.y);

            int leftBotId = GetOrCreateVertexIdInTriangles(alreadyUsedVectors, verticesList, uvsList, leftBotVert, leftBotUv);
            int leftTopId = GetOrCreateVertexIdInTriangles(alreadyUsedVectors, verticesList, uvsList, leftTopVert, leftTopUv);
            int rightTopId = GetOrCreateVertexIdInTriangles(alreadyUsedVectors, verticesList, uvsList, rightTopVert, rightTopUv);
            int rightBotId = GetOrCreateVertexIdInTriangles(alreadyUsedVectors, verticesList, uvsList, rightBotVert, rightBotUv);

            indicesList.Add(leftBotId);
            indicesList.Add(leftTopId);
            indicesList.Add(rightTopId);

            indicesList.Add(leftBotId);
            indicesList.Add(rightTopId);
            indicesList.Add(rightBotId);
        }
        private int GetOrCreateVertexIdInTriangles(Dictionary<Vector3, int> alreadyUsedVectors, List<Vector3> verticesList, List<Vector2> uvsList, Vector3 vert, Vector2 uv)
        {
            int id = 0;
            if (!alreadyUsedVectors.TryGetValue(vert, out id))
            {
                id = verticesList.Count;
                verticesList.Add(vert);
                uvsList.Add(uv);
                alreadyUsedVectors[vert] = id;
            }
            return id;
        }
        private bool GenerateMeshDataFromPackedSprite(Sprite sprite, RectTransform rectTrans, out Vector3[] vertices, out int[] indices, out Vector2[] uvs)
        {
            Vector3[] rectVertices = previousObjectInfo.rectVerticesTmp;
            rectTrans.GetLocalCorners(rectVertices);
            if (rectVertices[0] == rectVertices[1] && rectVertices[0] == rectVertices[2] && rectVertices[0] == rectVertices[3])
            {
                vertices = null;
                indices = null;
                uvs = null;
                return false;
            }

            Vector3 newRectSize = rectVertices[2] - rectVertices[0];
            previousObjectInfo.rectSize = newRectSize;

            Vector2[] spriteVerts = sprite.vertices;
            ushort[] spriteTris = sprite.triangles;
            uvs = sprite.uv;
            vertices = new Vector3[spriteVerts.Length];
            indices = new int[spriteTris.Length];

            Vector2 minSpriteBounds = sprite.bounds.min;
            Vector2 maxSpriteBounds = sprite.bounds.max;
            Vector2 spriteBoundSize = maxSpriteBounds - minSpriteBounds;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vertProgress = (spriteVerts[i] - minSpriteBounds);
                vertProgress.x /= spriteBoundSize.x;
                vertProgress.y /= spriteBoundSize.y;
                Vector3 vert = new Vector3(Mathf.Lerp(rectVertices[0].x, rectVertices[2].x, vertProgress.x),
                                            Mathf.Lerp(rectVertices[0].y, rectVertices[2].y, vertProgress.y),
                                            Mathf.Lerp(rectVertices[0].z, rectVertices[2].z, vertProgress.x));
                vertices[i] = vert;
            }

            for (int i = 0; i < indices.Length; i++)
                indices[i] = spriteTris[i];

            return true;
        }

        private bool GenerateMeshDataFromRectTransform(Sprite sprite, RectTransform rectTrans, out Vector3[] vertices, out int[] indices, out Vector2[] uvs)
        {
            vertices = previousObjectInfo.rectVerticesTmp;
            rectTrans.GetLocalCorners(vertices);
            indices = new int[]
            {
            0, 1, 2,
            0, 2, 3
            };
            if (sprite != null && !sprite.packed)
            {
                Rect texRect = sprite.rect;
                float texWidth = sprite.texture.width;
                float texHeight = sprite.texture.height;
                uvs = new Vector2[]
                {
                new Vector2(texRect.xMin / texWidth, texRect.yMin / texHeight),
                new Vector2(texRect.xMin / texWidth, texRect.yMax / texHeight),
                new Vector2(texRect.xMax / texWidth, texRect.yMax / texHeight),
                new Vector2(texRect.xMax / texWidth, texRect.yMin / texHeight),
                };
            }
            else
            {
                uvs = new Vector2[]
                {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
                };
            }
            if (vertices[0] == vertices[1] && vertices[0] == vertices[2] && vertices[0] == vertices[3])
                return false;

            Vector3 newRectSize = vertices[2] - vertices[0];
            previousObjectInfo.rectSize = newRectSize;
            return true;
        }
        protected void ErrorLog(string error)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                Debug.LogError(error, this);
            else
            {
                if (EditorErrorLog.Length > 0)
                    EditorErrorLog += "\n" + error;
                else
                    EditorErrorLog += error;
            }
#else
            Debug.LogError(error);
#endif
        }
        public RectTransform RectTrans { get; private set; }

#if UNITY_EDITOR
        public string EditorErrorLog { get; private set; }
#endif
    }
}