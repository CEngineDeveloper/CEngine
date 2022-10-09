using UnityEngine;

namespace HighlightPlus {

    [CreateAssetMenu(menuName = "Highlight Plus Profile", fileName = "Highlight Plus Profile", order = 100)]
    [HelpURL("https://www.dropbox.com/s/v9qgn68ydblqz8x/Documentation.pdf?dl=0")]
    public class HighlightProfile : ScriptableObject {

		[Tooltip("Different options to specify which objects are affected by this Highlight Effect component.")]
        public TargetOptions effectGroup = TargetOptions.Children;
		[Tooltip("The layer that contains the affected objects by this effect when effectGroup is set to LayerMask.")]
        public LayerMask effectGroupLayer = -1;
		[Tooltip("Only include objects whose names contains this text.")]
		public string effectNameFilter;
		[Tooltip("Combine meshes of all objects in this group affected by Highlight Effect reducing draw calls.")]
        public bool combineMeshes;
		[Tooltip("The alpha threshold for transparent cutout objects. Pixels with alpha below this value will be discarded.")]
        [Range(0, 1)]
		public float alphaCutOff;
		[Tooltip("If back facing triangles are ignored.Backfaces triangles are not visible but you may set this property to false to force highlight effects to act on those triangles as well.")]
        public bool cullBackFaces = true;
        public bool depthClip;
		[Tooltip("Normals handling option:\nPreserve original: use original mesh normals.\nSmooth: average normals to produce a smoother outline/glow mesh based effect.\nReorient: recomputes normals based on vertex direction to centroid.")]
        public NormalsOption normalsOption;

        public float fadeInDuration;
        public float fadeOutDuration;

		[Tooltip("Keeps the outline/glow size unaffected by object distance.")]
        public bool constantWidth = true;

        [Range(0, 1)]
		[Tooltip("Intensity of the overlay effect. A value of 0 disables the overlay completely.")]
        public float overlay;
		[ColorUsage(true, true)] public Color overlayColor = Color.yellow;
        public float overlayAnimationSpeed = 1f;
        [Range(0, 1)]
        public float overlayMinIntensity = 0.5f;
        [Range(0, 1)]
		[Tooltip("Controls the blending or mix of the overlay color with the natural colors of the object.")]
        public float overlayBlending = 1.0f;
        [Tooltip("Optional overlay texture.")]
        public Texture2D overlayTexture;
        public float overlayTextureScale = 1f;

        [Range(0, 1)]
		[Tooltip("Intensity of the outline. A value of 0 disables the outline completely.")]
        public float outline = 1f;
		[ColorUsage(true, true)] public Color outlineColor = Color.black;
        public float outlineWidth = 0.45f;
        public QualityLevel outlineQuality = QualityLevel.High;
        [Range(1, 8)]
		[Tooltip("Reduces the quality of the outline but improves performance a bit.")]
        public int outlineDownsampling = 2;
        public bool outlineOptimalBlit = true;
        public Visibility outlineVisibility = Visibility.Normal;
		[Tooltip("If enabled, this object won't combine the outline with other objects.")]
        public bool outlineIndependent;

        [Range(0, 5)]
		[Tooltip("The intensity of the outer glow effect. A value of 0 disables the glow completely.")]
        public float glow;
        public float glowWidth = 0.4f;
        public QualityLevel glowQuality = QualityLevel.High;
        [Range(1, 8)]
		[Tooltip("Reduces the quality of the glow but improves performance a bit.")]
        public int glowDownsampling = 2;
		[ColorUsage(true, true)] public Color glowHQColor = new Color (0.64f, 1f, 0f, 1f);
		[Tooltip("When enabled, outer glow renders with dithering. When disabled, glow appears as a solid color.")]
        public bool glowDithering = true;
        public bool glowOptimalBlit = true;
		[Tooltip("Seed for the dithering effect")]
        public float glowMagicNumber1 = 0.75f;
		[Tooltip("Another seed for the dithering effect that combines with first seed to create different patterns")]
        public float glowMagicNumber2 = 0.5f;
        public float glowAnimationSpeed = 1f;
        public Visibility glowVisibility = Visibility.Normal;
        public GlowBlendMode glowBlendMode = GlowBlendMode.Additive;
		[Tooltip("Blends glow passes one after another. If this option is disabled, glow passes won't overlap (in this case, make sure the glow pass 1 has a smaller offset than pass 2, etc.)")]
        public bool glowBlendPasses = true;
        public GlowPassData[] glowPasses;
        [Tooltip("If enabled, glow effect will not use a stencil mask. This can be used to render the glow effect alone.")]
        public bool glowIgnoreMask;

        [Range(0, 5f)]
		[Tooltip("The intensity of the inner glow effect. A value of 0 disables the glow completely.")]
        public float innerGlow;
        [Range(0, 2)]
        public float innerGlowWidth = 1f;
		[ColorUsage(true, true)] public Color innerGlowColor = Color.white;
        public Visibility innerGlowVisibility = Visibility.Normal;

		[Tooltip("Enables the targetFX effect. This effect draws an animated sprite over the object.")]
        public bool targetFX;
        public Texture2D targetFXTexture;
		[ColorUsage(true, true)] public Color targetFXColor = Color.white;
        public float targetFXRotationSpeed = 50f;
        public float targetFXInitialScale = 4f;
        public float targetFXEndScale = 1.5f;
        [Tooltip("Makes target scale relative to object renderer bounds.")]
        public bool targetFXScaleToRenderBounds;
        [Tooltip("Places target FX sprite at the bottom of the highlighted object.")]
        public bool targetFXAlignToGround;
        [Tooltip("Max distance from the center of the highlighted object to the ground.")]
        public float targetFXGroundMaxDistance = 15f;
        public LayerMask targetFXGroundLayerMask = -1;
		[Tooltip("Fade out effect with altitude")]
		public float targetFXFadePower = 32;
        public float targetFXTransitionDuration = 0.5f;
        public float targetFXStayDuration = 1.5f;
        public Visibility targetFXVisibility = Visibility.AlwaysOnTop;

		[Tooltip("See-through mode for this Highlight Effect component.")]
        public SeeThroughMode seeThrough = SeeThroughMode.Never;
		[Tooltip("This mask setting let you specify which objects will be considered as occluders and cause the see-through effect for this Highlight Effect component. For example, you assign your walls to a different layer and specify that layer here, so only walls and not other objects, like ground or ceiling, will trigger the see-through effect.")]
        public LayerMask seeThroughOccluderMask = -1;
		[Tooltip("Uses stencil buffers to ensure pixel-accurate occlusion test. If this option is disabled, only physics raycasting is used to test for occlusion.")]
        public bool seeThroughOccluderMaskAccurate;
		[Tooltip("A multiplier for the occluder volume size which can be used to reduce the actual size of occluders when Highlight Effect checks if they're occluding this object.")]
        [Range(0.01f, 0.9f)] public float seeThroughOccluderThreshold = 0.4f;
		[Tooltip("The interval of time between occlusion tests.")]
        public float seeThroughOccluderCheckInterval = 1f;
		[Tooltip("If enabled, occlusion test is performed for each children element. If disabled, the bounds of all children is combined and a single occlusion test is performed for the combined bounds.")]
		public bool seeThroughOccluderCheckIndividualObjects;
        [Tooltip("Shows the see-through effect only if the occluder if at this 'offset' distance from the object.")]
        public float seeThroughDepthOffset;
		[Tooltip("Hides the see-through effect if the occluder is further than this distance from the object (0 = infinite)")]
		public float seeThroughMaxDepth;
        [Range(0, 5f)] public float seeThroughIntensity = 0.8f;
        [Range(0, 1)] public float seeThroughTintAlpha = 0.5f;
        public Color seeThroughTintColor = Color.red;
        [Range(0, 1)] public float seeThroughNoise = 1f;
        [Range(0, 1)] public float seeThroughBorder;
        public Color seeThroughBorderColor = Color.black;
        public float seeThroughBorderWidth = 0.45f;
        [Tooltip("Renders see-through effect on overlapping objects in a sequence that's relative to the distance to the camera")]
        public bool seeThroughOrdered = true;

        [Range(0, 1)] public float hitFxInitialIntensity;
        public HitFxMode hitFxMode = HitFxMode.Overlay;
        public float hitFxFadeOutDuration = 0.25f;
        [ColorUsage(true, true)] public Color hitFxColor = Color.white;
        public float hitFxRadius = 0.5f;

        public void Load(HighlightEffect effect) {
            effect.effectGroup = effectGroup;
            effect.effectGroupLayer = effectGroupLayer;
			effect.effectNameFilter = effectNameFilter;
            effect.combineMeshes = combineMeshes;
            effect.alphaCutOff = alphaCutOff;
            effect.cullBackFaces = cullBackFaces;
            effect.depthClip = depthClip;
            effect.normalsOption = normalsOption;
            effect.fadeInDuration = fadeInDuration;
            effect.fadeOutDuration = fadeOutDuration;
            effect.constantWidth = constantWidth;
            effect.overlay = overlay;
            effect.overlayColor = overlayColor;
            effect.overlayAnimationSpeed = overlayAnimationSpeed;
            effect.overlayMinIntensity = overlayMinIntensity;
            effect.overlayBlending = overlayBlending;
            effect.overlayTexture = overlayTexture;
            effect.overlayTextureScale = overlayTextureScale;
            effect.outline = outline;
            effect.outlineColor = outlineColor;
            effect.outlineWidth = outlineWidth;
            effect.outlineQuality = outlineQuality;
            effect.outlineOptimalBlit = outlineOptimalBlit;
            effect.outlineDownsampling = outlineDownsampling;
            effect.outlineVisibility = outlineVisibility;
            effect.outlineIndependent = outlineIndependent;
            effect.glow = glow;
            effect.glowWidth = glowWidth;
            effect.glowQuality = glowQuality;
            effect.glowOptimalBlit = glowOptimalBlit;
            effect.glowDownsampling = glowDownsampling;
            effect.glowHQColor = glowHQColor;
            effect.glowDithering = glowDithering;
            effect.glowMagicNumber1 = glowMagicNumber1;
            effect.glowMagicNumber2 = glowMagicNumber2;
            effect.glowAnimationSpeed = glowAnimationSpeed;
            effect.glowVisibility = glowVisibility;
            effect.glowBlendMode = glowBlendMode;
            effect.glowBlendPasses = glowBlendPasses;
            effect.glowPasses = GetGlowPassesCopy(glowPasses);
            effect.glowIgnoreMask = glowIgnoreMask;
            effect.innerGlow = innerGlow;
            effect.innerGlowWidth = innerGlowWidth;
            effect.innerGlowColor = innerGlowColor;
            effect.innerGlowVisibility = innerGlowVisibility;
            effect.targetFX = targetFX;
            effect.targetFXColor = targetFXColor;
            effect.targetFXInitialScale = targetFXInitialScale;
            effect.targetFXEndScale = targetFXEndScale;
            effect.targetFXScaleToRenderBounds = targetFXScaleToRenderBounds;
            effect.targetFXAlignToGround = targetFXAlignToGround;
            effect.targetFXGroundMaxDistance = targetFXGroundMaxDistance;
            effect.targetFXGroundLayerMask = targetFXGroundLayerMask;
			effect.targetFXFadePower = targetFXFadePower;
            effect.targetFXRotationSpeed = targetFXRotationSpeed;
            effect.targetFXStayDuration = targetFXStayDuration;
            effect.targetFXTexture = targetFXTexture;
            effect.targetFXTransitionDuration = targetFXTransitionDuration;
            effect.targetFXVisibility = targetFXVisibility;
            effect.seeThrough = seeThrough;
            effect.seeThroughOccluderMask = seeThroughOccluderMask;
            effect.seeThroughOccluderMaskAccurate = seeThroughOccluderMaskAccurate;
            effect.seeThroughOccluderThreshold = seeThroughOccluderThreshold;
            effect.seeThroughOccluderCheckInterval = seeThroughOccluderCheckInterval;
			effect.seeThroughOccluderCheckIndividualObjects = seeThroughOccluderCheckIndividualObjects;
            effect.seeThroughIntensity = seeThroughIntensity;
            effect.seeThroughTintAlpha = seeThroughTintAlpha;
            effect.seeThroughTintColor = seeThroughTintColor;
            effect.seeThroughNoise = seeThroughNoise;
            effect.seeThroughBorder = seeThroughBorder;
            effect.seeThroughBorderColor = seeThroughBorderColor;
            effect.seeThroughBorderWidth = seeThroughBorderWidth;
            effect.seeThroughDepthOffset = seeThroughDepthOffset;
			effect.seeThroughMaxDepth = seeThroughMaxDepth;
            effect.seeThroughOrdered = seeThroughOrdered;
            effect.hitFxInitialIntensity = hitFxInitialIntensity;
            effect.hitFxMode = hitFxMode;
            effect.hitFxFadeOutDuration = hitFxFadeOutDuration;
            effect.hitFxColor = hitFxColor;
            effect.hitFxRadius = hitFxRadius;
            effect.UpdateMaterialProperties();
        }

        public void Save(HighlightEffect effect) {
            effectGroup = effect.effectGroup;
            effectGroupLayer = effect.effectGroupLayer;
			effectNameFilter = effect.effectNameFilter;
            combineMeshes = effect.combineMeshes;
            alphaCutOff = effect.alphaCutOff;
            cullBackFaces = effect.cullBackFaces;
            depthClip = effect.depthClip;
            normalsOption = effect.normalsOption;
            fadeInDuration = effect.fadeInDuration;
            fadeOutDuration = effect.fadeOutDuration;
            constantWidth = effect.constantWidth;
            overlay = effect.overlay;
            overlayColor = effect.overlayColor;
            overlayAnimationSpeed = effect.overlayAnimationSpeed;
            overlayMinIntensity = effect.overlayMinIntensity;
            overlayBlending = effect.overlayBlending;
            overlayTexture = effect.overlayTexture;
            overlayTextureScale = effect.overlayTextureScale;
            outline = effect.outline;
            outlineColor = effect.outlineColor;
            outlineWidth = effect.outlineWidth;
            outlineQuality = effect.outlineQuality;
            outlineDownsampling = effect.outlineDownsampling;
            outlineVisibility = effect.outlineVisibility;
            outlineIndependent = effect.outlineIndependent;
            outlineOptimalBlit = effect.outlineOptimalBlit;
            glow = effect.glow;
            glowWidth = effect.glowWidth;
            glowQuality = effect.glowQuality;
            glowOptimalBlit = effect.glowOptimalBlit;
            glowDownsampling = effect.glowDownsampling;
            glowHQColor = effect.glowHQColor;
            glowDithering = effect.glowDithering;
            glowMagicNumber1 = effect.glowMagicNumber1;
            glowMagicNumber2 = effect.glowMagicNumber2;
            glowAnimationSpeed = effect.glowAnimationSpeed;
            glowVisibility = effect.glowVisibility;
            glowBlendMode = effect.glowBlendMode;
            glowBlendPasses = effect.glowBlendPasses;
            glowPasses = GetGlowPassesCopy(effect.glowPasses);
            glowIgnoreMask = effect.glowIgnoreMask;
            innerGlow = effect.innerGlow;
            innerGlowWidth = effect.innerGlowWidth;
            innerGlowColor = effect.innerGlowColor;
            innerGlowVisibility = effect.innerGlowVisibility;
            targetFX = effect.targetFX;
            targetFXColor = effect.targetFXColor;
            targetFXInitialScale = effect.targetFXInitialScale;
            targetFXEndScale = effect.targetFXEndScale;
            targetFXScaleToRenderBounds = effect.targetFXScaleToRenderBounds;
            targetFXAlignToGround = effect.targetFXAlignToGround;
            targetFXGroundMaxDistance = effect.targetFXGroundMaxDistance;
            targetFXGroundLayerMask = effect.targetFXGroundLayerMask;
			targetFXFadePower = effect.targetFXFadePower;
            targetFXRotationSpeed = effect.targetFXRotationSpeed;
            targetFXStayDuration = effect.targetFXStayDuration;
            targetFXTexture = effect.targetFXTexture;
            targetFXTransitionDuration = effect.targetFXTransitionDuration;
            targetFXVisibility = effect.targetFXVisibility;
            seeThrough = effect.seeThrough;
            seeThroughOccluderMask = effect.seeThroughOccluderMask;
            seeThroughOccluderMaskAccurate = effect.seeThroughOccluderMaskAccurate;
            seeThroughOccluderThreshold = effect.seeThroughOccluderThreshold;
            seeThroughOccluderCheckInterval = effect.seeThroughOccluderCheckInterval;
			seeThroughOccluderCheckIndividualObjects = effect.seeThroughOccluderCheckIndividualObjects;
            seeThroughIntensity = effect.seeThroughIntensity;
            seeThroughTintAlpha = effect.seeThroughTintAlpha;
            seeThroughTintColor = effect.seeThroughTintColor;
            seeThroughNoise = effect.seeThroughNoise;
            seeThroughBorder = effect.seeThroughBorder;
            seeThroughBorderColor = effect.seeThroughBorderColor;
            seeThroughBorderWidth = effect.seeThroughBorderWidth;
            seeThroughDepthOffset = effect.seeThroughDepthOffset;
			seeThroughMaxDepth = effect.seeThroughMaxDepth;
            seeThroughOrdered = effect.seeThroughOrdered;
            hitFxInitialIntensity = effect.hitFxInitialIntensity;
            hitFxMode = effect.hitFxMode;
            hitFxFadeOutDuration = effect.hitFxFadeOutDuration;
            hitFxColor = effect.hitFxColor;
            hitFxRadius = effect.hitFxRadius;
        }

        GlowPassData[] GetGlowPassesCopy(GlowPassData[] glowPasses) {
            if (glowPasses == null) {
                return new GlowPassData[0];
            }
            GlowPassData[] pd = new GlowPassData[glowPasses.Length];
            for (int k = 0; k < glowPasses.Length; k++) {
                pd[k].alpha = glowPasses[k].alpha;
                pd[k].color = glowPasses[k].color;
                pd[k].offset = glowPasses[k].offset;
            }
            return pd;
        }

        public void OnValidate() {
			seeThroughDepthOffset = Mathf.Max(0, seeThroughDepthOffset);
			seeThroughMaxDepth = Mathf.Max(0, seeThroughMaxDepth);
			seeThroughBorderWidth = Mathf.Max(0, seeThroughBorderWidth);
			targetFXFadePower = Mathf.Max(0, targetFXFadePower);
            if (glowPasses == null || glowPasses.Length == 0) {
                glowPasses = new GlowPassData[4];
                glowPasses[0] = new GlowPassData() { offset = 4, alpha = 0.1f, color = new Color(0.64f, 1f, 0f, 1f) };
                glowPasses[1] = new GlowPassData() { offset = 3, alpha = 0.2f, color = new Color(0.64f, 1f, 0f, 1f) };
                glowPasses[2] = new GlowPassData() { offset = 2, alpha = 0.3f, color = new Color(0.64f, 1f, 0f, 1f) };
                glowPasses[3] = new GlowPassData() { offset = 1, alpha = 0.4f, color = new Color(0.64f, 1f, 0f, 1f) };
            }
        }

    }
}

