#if UNITY_POST_PROCESSING_STACK_V2

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace FoW
{
    [System.Serializable]
    public class LayerMaskParameter : ParameterOverride<LayerMask> { }

    class FogOfWarPPSv2Manager : FogOfWarPostProcessManager
    {
        PostProcessRenderContext _context;
        PropertySheet _sheet;
        MaterialPropertyBlock _properties { get { return _sheet.properties; } }
        Shader _shader = null;

        public void Setup(PostProcessRenderContext context)
        {
            _context = context;
            if (_shader == null)
                _shader = FogOfWarUtils.FindShader("Hidden/FogOfWarPPSv2");
            _sheet = _context.propertySheets.Get(_shader);
        }

        protected override void SetTexture(int id, Texture value) { _properties.SetTexture(id, value); }
        protected override void SetVector(int id, Vector4 value) { _properties.SetVector(id, value); }
        protected override void SetColor(int id, Color value) { _properties.SetColor(id, value); }
        protected override void SetFloat(int id, float value) { _properties.SetFloat(id, value); }
        protected override void SetMatrix(int id, Matrix4x4 value) { _properties.SetMatrix(id, value); }
        protected override void SetKeyword(string keyword, bool enabled)
        {
            if (enabled)
                _sheet.EnableKeyword(keyword);
            else
                _sheet.DisableKeyword(keyword);
        }

        protected override void GetTargetSize(out int width, out int height, out int depth)
        {
            width = _context.width;
            height = _context.height;
            depth = 16;
        }

        protected override void BlitToScreen()
        {
            _context.command.BlitFullscreenTriangle(_context.source, _context.destination, _sheet, 0);
        }
    }

    [System.Serializable]
    [PostProcess(typeof(FogOfWarPPSv2Renderer), PostProcessEvent.BeforeStack, "FogOfWar")]
    public sealed class FogOfWarPPSv2 : PostProcessEffectSettings
    {
        public IntParameter team = new IntParameter { value = 0 };
        public BoolParameter fogFarPlane = new BoolParameter { value = true };
        [Range(0.0f, 1.0f)]
        public FloatParameter outsideFogStrength = new FloatParameter { value = 1 };
        public BoolParameter pointFiltering = new BoolParameter { value = false };

        [Header("Color")]
        public ColorParameter fogColor = new ColorParameter { value = Color.black };
        public TextureParameter fogColorTexture = new TextureParameter();
        public BoolParameter fogTextureScreenSpace = new BoolParameter { value = false };
        public FloatParameter fogColorTextureScale = new FloatParameter { value = 1 };
        public FloatParameter fogColorTextureHeight = new FloatParameter { value = 0 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && Application.isPlaying;
        }
    }

    public sealed class FogOfWarPPSv2Renderer : PostProcessEffectRenderer<FogOfWarPPSv2>
    {
        FogOfWarPPSv2Manager _postProcess = null;

        public override void Render(PostProcessRenderContext context)
        {
            if (_postProcess == null)
                _postProcess = new FogOfWarPPSv2Manager();

            _postProcess.Setup(context);
            _postProcess.team = settings.team.value;
            _postProcess.camera = context.camera;
            _postProcess.pointFiltering = settings.pointFiltering.value;
            _postProcess.fogFarPlane = settings.fogFarPlane.value;
            _postProcess.outsideFogStrength = settings.outsideFogStrength.value;
            _postProcess.fogColor = settings.fogColor.value;
            _postProcess.fogColorTexture = settings.fogColorTexture;
            _postProcess.fogColorTextureScale = settings.fogColorTextureScale.value;
            _postProcess.fogColorTextureHeight = settings.fogColorTextureHeight.value;
            _postProcess.Render();
        }
    }
}

#endif
