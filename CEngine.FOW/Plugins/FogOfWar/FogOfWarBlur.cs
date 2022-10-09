using UnityEngine;

namespace FoW
{
    public enum FogOfWarBlurType
    {
        Gaussian3,
        Gaussian5,
        Antialias
    }

    public class FogOfWarBlur
    {
        RenderTexture _target;
        RenderTexture _source;
        static Material _blurMaterial = null;

        void SetupRenderTarget(Vector2Int resolution, ref RenderTexture tex)
        {
            if (tex == null)
                tex = new RenderTexture(resolution.x, resolution.y, 0);
            else if (tex.width != resolution.x || tex.height != resolution.y)
            {
                tex.width = resolution.x;
                tex.height = resolution.y;
            }
        }

        public Texture Apply(Texture2D fogtexture, Vector2Int resolution, int amount, int iterations, FogOfWarBlurType type)
        {
            if (amount <= 0 || iterations <= 0)
                return fogtexture;

            if (_blurMaterial == null)
                _blurMaterial = new Material(FogOfWarUtils.FindShader("Hidden/FogOfWarBlurShader"));

            _blurMaterial.SetFloat("_BlurAmount", amount);
            _blurMaterial.SetKeywordEnabled("GAUSSIAN3", type == FogOfWarBlurType.Gaussian3);
            _blurMaterial.SetKeywordEnabled("GAUSSIAN5", type == FogOfWarBlurType.Gaussian5);
            _blurMaterial.SetKeywordEnabled("ANTIALIAS", type == FogOfWarBlurType.Antialias);

            SetupRenderTarget(resolution, ref _target);
            if (iterations > 1)
                SetupRenderTarget(resolution, ref _source);
            
            _target.MarkRestoreExpected();
            Graphics.Blit(fogtexture, _target, _blurMaterial);

            for (int i = 1; i < iterations; ++i)
            {
                FogOfWarUtils.Swap(ref _target, ref _source);
                _target.MarkRestoreExpected();
                Graphics.Blit(_source, _target, _blurMaterial);
            }

            return _target;
        }
    }
}
