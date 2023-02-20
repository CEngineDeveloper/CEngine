using UnityEngine;
using System.Collections;

namespace TGS {

    public enum FaderStyle {
        FadeOut = 0,
        Blink = 1,
        Flash = 2,
        ColorTemp = 3
    }

    public class SurfaceFader : MonoBehaviour {

        Material fadeMaterial;
        float startTime, duration;
        TerrainGridSystem grid;
        Color color, initialColor;
        Region region;
        Renderer _renderer;
        FaderStyle style;
        int repetitions;

        public static void Animate(FaderStyle style, TerrainGridSystem grid, GameObject surface, Region region, Material fadeMaterial, Color color, float duration, int repetitions) {
            SurfaceFader fader = surface.GetComponent<SurfaceFader>();
            if (fader != null) {
                fader.Finish(0);
                DestroyImmediate(fader);
            }
            fader = surface.AddComponent<SurfaceFader>();
            fader.grid = grid;
            fader.startTime = Time.time;
            fader.duration = duration + 0.0001f;
            fader.color = color;
            fader.region = region;
            fader.fadeMaterial = fadeMaterial;
            fader.style = style;
            fader.initialColor = fadeMaterial.color;
            fader.repetitions = repetitions;
        }

        void Start() {
            _renderer = GetComponent<Renderer>();
            _renderer.sharedMaterial = fadeMaterial;
        }

        void Update() {
            float elapsed = Time.time - startTime;
            switch (style) {
                case FaderStyle.FadeOut:
                    UpdateFadeOut(elapsed);
                    break;
                case FaderStyle.Blink:
                    UpdateBlink(elapsed);
                    break;
                case FaderStyle.Flash:
                    UpdateFlash(elapsed);
                    break;
                case FaderStyle.ColorTemp:
                    UpdateColorTemp(elapsed);
                    break;
            }
        }

        void ToggleActiveByAlpha() {
            bool current = _renderer.gameObject.activeSelf;
            bool should = _renderer.sharedMaterial.color.a > 0;
            if (current != should) {
                _renderer.gameObject.SetActive(should);
            }
        }


        #region Fade Out effect

        public void Finish(float fadeOutDuration) {
            repetitions = 0;

            if (fadeOutDuration <= 0) {
                startTime = float.MinValue;
                duration = 1f;
                Update();
            } else {
                style = FaderStyle.FadeOut;
                color = fadeMaterial.color;
                startTime = Time.time;
                duration = fadeOutDuration;
            }
        }

        void UpdateFadeOut(float elapsed) {
            float newAlpha = Mathf.Clamp01(1.0f - elapsed / duration);
            SetAlpha(newAlpha);
            if (elapsed >= duration) {
                if (--repetitions > 0) {
                    startTime = Time.time;
                    return;
                }
                SetAlpha(0);
                region.customMaterial = null;
                DestroyImmediate(this);
            }
        }

        void SetAlpha(float newAlpha) {
            if (grid.highlightedObj == gameObject || _renderer == null)
                return;
            Color newColor = new Color(color.r, color.g, color.b, color.a * newAlpha);
            fadeMaterial.color = newColor;
            if (_renderer.sharedMaterial != fadeMaterial) {
                if (fadeMaterial.HasProperty(ShaderParams.MainTex) && _renderer.sharedMaterial.HasProperty(ShaderParams.MainTex)) {
                    fadeMaterial.mainTexture = _renderer.sharedMaterial.mainTexture;
                }
                _renderer.sharedMaterial = fadeMaterial;
            }
            ToggleActiveByAlpha();
        }

        #endregion

        #region Flash effect

        void UpdateFlash(float elapsed) {
            SetFlashColor(elapsed / duration);
            if (elapsed >= duration) {
                if (--repetitions > 0) {
                    startTime = Time.time;
                    return;
                }
                SetFlashColor(1f);
                if (region.customMaterial != null && _renderer != null)
                    _renderer.sharedMaterial = region.customMaterial;
                DestroyImmediate(this);
            }
        }


        void SetFlashColor(float t) {
            if (_renderer == null)
                return;
            if (t < 0)
                t = 0;
            else if (t > 1)
                t = 1f;
            Color newColor = Color.Lerp(color, initialColor, t);
            fadeMaterial.color = newColor;
            if (_renderer.sharedMaterial != fadeMaterial) {
                if (fadeMaterial.HasProperty(ShaderParams.MainTex) && _renderer.sharedMaterial.HasProperty(ShaderParams.MainTex)) {
                    fadeMaterial.mainTexture = _renderer.sharedMaterial.mainTexture;
                }
                _renderer.sharedMaterial = fadeMaterial;
            }
            ToggleActiveByAlpha();
        }

        #endregion

        #region Blink effect

        void UpdateBlink(float elapsed) {
            SetBlinkColor(elapsed / duration);
            if (elapsed >= duration) {
                if (--repetitions > 0) {
                    startTime = Time.time;
                    return;
                }
                SetBlinkColor(0);
                if (region.customMaterial != null && _renderer != null)
                    _renderer.sharedMaterial = region.customMaterial;
                DestroyImmediate(this);
            }
        }

        void SetBlinkColor(float t) {
            if (_renderer == null)
                return;
            Color newColor;
            if (t < 0.5f) {
                if (t < 0)
                    t = 0;
                newColor = Color.Lerp(initialColor, color, t * 2f);
            } else {
                if (t > 1)
                    t = 1;
                newColor = Color.Lerp(color, initialColor, (t - 0.5f) * 2f);
            }
            fadeMaterial.color = newColor;
            if (_renderer.sharedMaterial != fadeMaterial) {
                if (fadeMaterial.HasProperty(ShaderParams.MainTex) && _renderer.sharedMaterial.HasProperty(ShaderParams.MainTex)) {
                    fadeMaterial.mainTexture = _renderer.sharedMaterial.mainTexture;
                }
                _renderer.sharedMaterial = fadeMaterial;
            }
            ToggleActiveByAlpha();
        }

        #endregion

        #region Color Temp effect

        void UpdateColorTemp(float elapsed) {
            SetColorTemp(1);
            if (elapsed >= duration) {
                if (--repetitions > 0) {
                    startTime = Time.time;
                    return;
                }
                SetColorTemp(0);
                if (region.customMaterial != null && _renderer != null)
                    _renderer.sharedMaterial = region.customMaterial;
                DestroyImmediate(this);
            }
        }


        void SetColorTemp(float t) {
            if (_renderer == null)
                return;
            fadeMaterial.color = t == 0 ? initialColor : color;
            if (_renderer.sharedMaterial != fadeMaterial) {
                if (fadeMaterial.HasProperty(ShaderParams.MainTex) && _renderer.sharedMaterial.HasProperty(ShaderParams.MainTex)) {
                    fadeMaterial.mainTexture = _renderer.sharedMaterial.mainTexture;
                }
                _renderer.sharedMaterial = fadeMaterial;
            }
            ToggleActiveByAlpha();
        }

        #endregion


    }

}