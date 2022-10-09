using UnityEngine;

namespace FoW
{
    class FogOfWarLegacyManager : FogOfWarPostProcessManager
    {
        Material _material;
        RenderTexture _source = null;
        RenderTexture _destination = null;

        public FogOfWarLegacyManager()
        {
            _material = new Material(FogOfWarUtils.FindShader("Hidden/FogOfWarLegacy"));
            _material.name = "FogOfWarLegacy";
        }

        public void Setup(RenderTexture source, RenderTexture destination)
        {
            _source = source;
            _destination = destination;
        }

        protected override void SetTexture(int id, Texture value) { _material.SetTexture(id, value); }
        protected override void SetVector(int id, Vector4 value) { _material.SetVector(id, value); }
        protected override void SetColor(int id, Color value) { _material.SetColor(id, value); }
        protected override void SetFloat(int id, float value) { _material.SetFloat(id, value); }
        protected override void SetMatrix(int id, Matrix4x4 value) { _material.SetMatrix(id, value); }
        protected override void SetKeyword(string keyword, bool enabled)
        {
            if (enabled)
                _material.EnableKeyword(keyword);
            else
                _material.DisableKeyword(keyword);
        }

        protected override void GetTargetSize(out int width, out int height, out int depth)
        {
            width = _source.width;
            height = _source.height;
            depth = _source.depth;
        }

        protected override void BlitToScreen()
        {
            if (_destination != null)
                _destination.MarkRestoreExpected();
            Graphics.Blit(_source, _destination, _material);
        }
    }

    [AddComponentMenu("FogOfWar/FogOfWarLegacy")]
    public class FogOfWarLegacy : MonoBehaviour
    {
        public int team = 0;
        public bool fogFarPlane = true;
        [Range(0.0f, 1.0f)]
        public float outsideFogStrength = 1;
        public bool pointFiltering = false;

        [Header("Color")]
        public Color fogColor = Color.black;
        public Texture2D fogColorTexture = null;
        public bool fogTextureScreenSpace = false;
        public float fogColorTextureScale = 1;
        public float fogColorTextureHeight = 0;

        // core stuff
        FogOfWarLegacyManager _postProcess = null;
        Camera _camera;

        void Awake()
        {
            _postProcess = new FogOfWarLegacyManager();
        }

        void Start()
        {
            _camera = GetComponent<Camera>();
            _camera.depthTextureMode |= DepthTextureMode.Depth;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _postProcess.Setup(source, destination);
            _postProcess.team = team;
            _postProcess.camera = _camera;
            _postProcess.fogFarPlane = fogFarPlane;
            _postProcess.outsideFogStrength = outsideFogStrength;
            _postProcess.pointFiltering = pointFiltering;
            _postProcess.fogColor = fogColor;
            _postProcess.fogColorTexture = fogColorTexture;
            _postProcess.fogTextureScreenSpace = fogTextureScreenSpace;
            _postProcess.fogColorTextureScale = fogColorTextureScale;
            _postProcess.fogColorTextureHeight = fogColorTextureHeight;
            _postProcess.Render();
        }
    }
}
