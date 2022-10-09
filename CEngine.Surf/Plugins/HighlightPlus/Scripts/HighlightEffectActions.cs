using UnityEngine;

namespace HighlightPlus {

    public enum HitFxMode {
        Overlay = 0,
        InnerGlow = 1,
        LocalHit = 2
    }

    public partial class HighlightEffect : MonoBehaviour {

        [Range(0,1)] public float hitFxInitialIntensity;
        public HitFxMode hitFxMode = HitFxMode.Overlay;
        public float hitFxFadeOutDuration = 0.25f;
        [ColorUsage(true, true)] public Color hitFxColor = Color.white;
        public float hitFxRadius = 0.5f;

        float hitInitialIntensity;
        float hitStartTime;
        float hitFadeOutDuration;
        Color hitColor;
        bool hitActive;
        Vector3 hitPosition;
        float hitRadius;

        /// <summary>
        /// Performs a hit effect using default values
        /// </summary>
        public void HitFX() {
            HitFX(hitFxColor, hitFxFadeOutDuration, hitFxInitialIntensity);
        }

        /// <summary>
        /// Performs a hit effect localized at hit position and radius with default values
        /// </summary>
        public void HitFX(Vector3 position) {
            HitFX(hitFxColor, hitFxFadeOutDuration, hitFxInitialIntensity, position, hitFxRadius);

        }

        /// <summary>
        /// Performs a hit effect using desired color, fade out duration and optionally initial intensity (0-1)
        /// </summary>
        public void HitFX(Color color, float fadeOutDuration, float initialIntensity = 1f) {
            hitInitialIntensity = initialIntensity;
            hitFadeOutDuration = fadeOutDuration;
            hitColor = color;
            hitStartTime = Time.time;
            hitActive = true;
            if (overlay == 0) {
                UpdateMaterialProperties();
            }
        }


        /// <summary>
        /// Performs a hit effect using desired color, fade out duration, initial intensity (0-1), hit position and radius of effect
        /// </summary>
        public void HitFX(Color color, float fadeOutDuration, float initialIntensity, Vector3 position, float radius) {
            hitInitialIntensity = initialIntensity;
            hitFadeOutDuration = fadeOutDuration;
            hitColor = color;
            hitStartTime = Time.time;
            hitActive = true;
            hitPosition = position;
            hitRadius = radius;
            if (overlay == 0) {
                UpdateMaterialProperties();
            }
        }

        /// <summary>
        /// Initiates the target FX on demand using predefined configuration (see targetFX... properties)
        /// </summary>
        public void TargetFX() {
            targetFxStartTime = Time.time;
            if (!targetFX) {
                targetFX = true;
                UpdateMaterialProperties();
            }
        }


    }
}