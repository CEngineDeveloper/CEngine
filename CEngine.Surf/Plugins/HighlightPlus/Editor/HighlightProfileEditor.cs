using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HighlightPlus {

    [CustomEditor(typeof(HighlightProfile))]
    [CanEditMultipleObjects]
    public class HighlightProfileEditor : Editor {

        SerializedProperty effectGroup, effectGroupLayer, effectNameFilter, combineMeshes, alphaCutOff, cullBackFaces, depthClip, normalsOption;
        SerializedProperty overlay, overlayColor, overlayAnimationSpeed, overlayMinIntensity, overlayTexture, overlayTextureScale, overlayBlending;
        SerializedProperty fadeInDuration, fadeOutDuration, constantWidth;
        SerializedProperty outline, outlineColor, outlineWidth, outlineQuality, outlineDownsampling, outlineOptimalBlit, outlineVisibility, outlineIndependent;
        SerializedProperty glow, glowWidth, glowQuality, glowOptimalBlit, glowDownsampling, glowHQColor, glowDithering, glowMagicNumber1, glowMagicNumber2, glowAnimationSpeed, glowVisibility, glowBlendMode, glowBlendPasses, glowPasses, glowIgnoreMask;
        SerializedProperty innerGlow, innerGlowWidth, innerGlowColor, innerGlowVisibility;
        SerializedProperty targetFX, targetFXTexture, targetFXColor, targetFXRotationSpeed, targetFXInitialScale, targetFXEndScale, targetFXScaleToRenderBound;
        SerializedProperty targetFXAlignToGround, targetFXFadePower, targetFXGroundMaxDistance, targetFXGroundLayerMask, targetFXScaleToRenderBounds, targetFXTransitionDuration, targetFXStayDuration, targetFXVisibility;
        SerializedProperty seeThrough, seeThroughOccluderMask, seeThroughOccluderMaskAccurate, seeThroughOccluderThreshold, seeThroughOccluderCheckInterval, seeThroughOccluderCheckIndividualObjects, seeThroughDepthOffset, seeThroughMaxDepth;
        SerializedProperty seeThroughIntensity, seeThroughTintAlpha, seeThroughTintColor, seeThroughNoise, seeThroughBorder, seeThroughBorderWidth, seeThroughBorderColor, seeThroughOrdered;
        SerializedProperty hitFxInitialIntensity, hitFxMode, hitFxFadeOutDuration, hitFxColor, hitFxRadius;

        void OnEnable() {
            effectGroup = serializedObject.FindProperty("effectGroup");
            effectGroupLayer = serializedObject.FindProperty("effectGroupLayer");
            effectNameFilter = serializedObject.FindProperty("effectNameFilter");
            combineMeshes = serializedObject.FindProperty("combineMeshes");
            alphaCutOff = serializedObject.FindProperty("alphaCutOff");
            cullBackFaces = serializedObject.FindProperty("cullBackFaces");
            depthClip = serializedObject.FindProperty("depthClip");
            normalsOption = serializedObject.FindProperty("normalsOption");
            fadeInDuration = serializedObject.FindProperty("fadeInDuration");
            fadeOutDuration = serializedObject.FindProperty("fadeOutDuration");
            constantWidth = serializedObject.FindProperty("constantWidth");
            overlay = serializedObject.FindProperty("overlay");
            overlayColor = serializedObject.FindProperty("overlayColor");
            overlayAnimationSpeed = serializedObject.FindProperty("overlayAnimationSpeed");
            overlayMinIntensity = serializedObject.FindProperty("overlayMinIntensity");
            overlayBlending = serializedObject.FindProperty("overlayBlending");
            overlayTexture = serializedObject.FindProperty("overlayTexture");
            overlayTextureScale = serializedObject.FindProperty("overlayTextureScale");
            outline = serializedObject.FindProperty("outline");
            outlineColor = serializedObject.FindProperty("outlineColor");
            outlineWidth = serializedObject.FindProperty("outlineWidth");
            outlineQuality = serializedObject.FindProperty("outlineQuality");
            outlineOptimalBlit = serializedObject.FindProperty("outlineOptimalBlit");
            outlineDownsampling = serializedObject.FindProperty("outlineDownsampling");
            outlineVisibility = serializedObject.FindProperty("outlineVisibility");
            outlineIndependent = serializedObject.FindProperty("outlineIndependent");
            glow = serializedObject.FindProperty("glow");
            glowWidth = serializedObject.FindProperty("glowWidth");
            glowQuality = serializedObject.FindProperty("glowQuality");
            glowOptimalBlit = serializedObject.FindProperty("glowOptimalBlit");
            glowDownsampling = serializedObject.FindProperty("glowDownsampling");
            glowHQColor = serializedObject.FindProperty("glowHQColor");
            glowAnimationSpeed = serializedObject.FindProperty("glowAnimationSpeed");
            glowDithering = serializedObject.FindProperty("glowDithering");
            glowMagicNumber1 = serializedObject.FindProperty("glowMagicNumber1");
            glowMagicNumber2 = serializedObject.FindProperty("glowMagicNumber2");
            glowAnimationSpeed = serializedObject.FindProperty("glowAnimationSpeed");
            glowBlendPasses = serializedObject.FindProperty("glowBlendPasses");
            glowVisibility = serializedObject.FindProperty("glowVisibility");
            glowBlendMode = serializedObject.FindProperty("glowBlendMode");
            glowPasses = serializedObject.FindProperty("glowPasses");
            glowIgnoreMask = serializedObject.FindProperty("glowIgnoreMask");
            innerGlow = serializedObject.FindProperty("innerGlow");
            innerGlowColor = serializedObject.FindProperty("innerGlowColor");
            innerGlowWidth = serializedObject.FindProperty("innerGlowWidth");
            innerGlowVisibility = serializedObject.FindProperty("innerGlowVisibility");
            targetFX = serializedObject.FindProperty("targetFX");
            targetFXTexture = serializedObject.FindProperty("targetFXTexture");
            targetFXRotationSpeed = serializedObject.FindProperty("targetFXRotationSpeed");
            targetFXInitialScale = serializedObject.FindProperty("targetFXInitialScale");
            targetFXEndScale = serializedObject.FindProperty("targetFXEndScale");
            targetFXScaleToRenderBounds = serializedObject.FindProperty("targetFXScaleToRenderBounds");
            targetFXAlignToGround = serializedObject.FindProperty("targetFXAlignToGround");
            targetFXGroundMaxDistance = serializedObject.FindProperty("targetFXGroundMaxDistance");
            targetFXGroundLayerMask = serializedObject.FindProperty("targetFXGroundLayerMask");
            targetFXFadePower = serializedObject.FindProperty("targetFXFadePower");
            targetFXColor = serializedObject.FindProperty("targetFXColor");
            targetFXTransitionDuration = serializedObject.FindProperty("targetFXTransitionDuration");
            targetFXStayDuration = serializedObject.FindProperty("targetFXStayDuration");
            targetFXVisibility = serializedObject.FindProperty("targetFXVisibility");
            seeThrough = serializedObject.FindProperty("seeThrough");
            seeThroughOccluderMask = serializedObject.FindProperty("seeThroughOccluderMask");
            seeThroughOccluderMaskAccurate = serializedObject.FindProperty("seeThroughOccluderMaskAccurate");
            seeThroughOccluderThreshold = serializedObject.FindProperty("seeThroughOccluderThreshold");
            seeThroughOccluderCheckInterval = serializedObject.FindProperty("seeThroughOccluderCheckInterval");
            seeThroughOccluderCheckIndividualObjects = serializedObject.FindProperty("seeThroughOccluderCheckIndividualObjects");
            seeThroughDepthOffset = serializedObject.FindProperty("seeThroughDepthOffset");
            seeThroughMaxDepth = serializedObject.FindProperty("seeThroughMaxDepth");
            seeThroughIntensity = serializedObject.FindProperty("seeThroughIntensity");
            seeThroughTintAlpha = serializedObject.FindProperty("seeThroughTintAlpha");
            seeThroughTintColor = serializedObject.FindProperty("seeThroughTintColor");
            seeThroughNoise = serializedObject.FindProperty("seeThroughNoise");
            seeThroughBorder = serializedObject.FindProperty("seeThroughBorder");
            seeThroughBorderWidth = serializedObject.FindProperty("seeThroughBorderWidth");
            seeThroughBorderColor = serializedObject.FindProperty("seeThroughBorderColor");
            seeThroughOrdered = serializedObject.FindProperty("seeThroughOrdered");
            hitFxInitialIntensity = serializedObject.FindProperty("hitFxInitialIntensity");
            hitFxMode = serializedObject.FindProperty("hitFxMode");
            hitFxFadeOutDuration = serializedObject.FindProperty("hitFxFadeOutDuration");
            hitFxColor = serializedObject.FindProperty("hitFxColor");
            hitFxRadius = serializedObject.FindProperty("hitFxRadius");
        }

        public override void OnInspectorGUI() {

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Highlight Options", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(effectGroup, new GUIContent("Include"));
            if (effectGroup.intValue == (int)TargetOptions.LayerInScene || effectGroup.intValue == (int)TargetOptions.LayerInChildren) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(effectGroupLayer, new GUIContent("Layer"));
                EditorGUI.indentLevel--;
            }
            if (effectGroup.intValue != (int)TargetOptions.OnlyThisObject && effectGroup.intValue != (int)TargetOptions.Scripting) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(effectNameFilter, new GUIContent("Object Name Filter"));
                EditorGUILayout.PropertyField(combineMeshes);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(alphaCutOff);
            EditorGUILayout.PropertyField(cullBackFaces);
            EditorGUILayout.PropertyField(normalsOption);
            EditorGUILayout.PropertyField(depthClip);
            EditorGUILayout.PropertyField(fadeInDuration);
            EditorGUILayout.PropertyField(fadeOutDuration);
            EditorGUILayout.PropertyField(constantWidth);
            EditorGUILayout.PropertyField(outlineIndependent, new GUIContent("Independent", "Do not combine outline with other highlighted objects."));

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawSectionField(outline, "Outline", outline.floatValue > 0);
            if (outline.floatValue > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(outlineWidth, new GUIContent("Width"));
                EditorGUILayout.PropertyField(outlineColor, new GUIContent("Color"));
                HighlightEffectEditor.QualityPropertyField(outlineQuality);
                if (outlineQuality.intValue == (int)QualityLevel.Highest) {
                    EditorGUILayout.PropertyField(outlineDownsampling, new GUIContent("Downsampling"));
                    EditorGUILayout.PropertyField(outlineOptimalBlit, new GUIContent("Optimal Blit", "Blits result over a section of the screen instead of rendering to the full screen buffer."));
                }
                if (outlineQuality.intValue == (int)QualityLevel.Highest && glowQuality.intValue == (int)QualityLevel.Highest) {
                    EditorGUILayout.PropertyField(glowVisibility, new GUIContent("Visibility"));
                } else {
                    EditorGUILayout.PropertyField(outlineVisibility, new GUIContent("Visibility"));
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawSectionField(glow, "Outer Glow", glow.floatValue > 0);
            if (glow.floatValue > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(glowWidth, new GUIContent("Width"));
                HighlightEffectEditor.QualityPropertyField(glowQuality);
                if (glowQuality.intValue == (int)QualityLevel.Highest) {
                    EditorGUILayout.PropertyField(glowDownsampling, new GUIContent("Downsampling"));
                    EditorGUILayout.PropertyField(glowHQColor, new GUIContent("Color"));
                    EditorGUILayout.PropertyField(glowOptimalBlit, new GUIContent("Optimal Blit", "Blits result over a section of the screen instead of rendering to the full screen buffer."));
                }
                EditorGUILayout.PropertyField(glowAnimationSpeed, new GUIContent("Animation Speed"));
                EditorGUILayout.PropertyField(glowVisibility, new GUIContent("Visibility"));
                EditorGUILayout.PropertyField(glowBlendMode, new GUIContent("Blend Mode"));
                if (glowQuality.intValue != (int)QualityLevel.Highest) {
                    EditorGUILayout.PropertyField(glowDithering, new GUIContent("Dithering"));
                    if (glowDithering.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(glowMagicNumber1, new GUIContent("Magic Number 1"));
                        EditorGUILayout.PropertyField(glowMagicNumber2, new GUIContent("Magic Number 2"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.PropertyField(glowBlendPasses, new GUIContent("Blend Passes"));
                    EditorGUILayout.PropertyField(glowPasses, true);
                    EditorGUILayout.PropertyField(glowIgnoreMask, new GUIContent("Ignore Mask"));
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawSectionField(innerGlow, "Inner Glow", innerGlow.floatValue > 0);
            if (innerGlow.floatValue > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(innerGlowColor, new GUIContent("Color"));
                EditorGUILayout.PropertyField(innerGlowWidth, new GUIContent("Width"));
                EditorGUILayout.PropertyField(innerGlowVisibility, new GUIContent("Visibility"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawSectionField(overlay, "Overlay", overlay.floatValue > 0);
            if (overlay.floatValue > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(overlayColor, new GUIContent("Color"));
                EditorGUILayout.PropertyField(overlayTexture, new GUIContent("Texture"));
                if (overlayTexture.objectReferenceValue != null) {
                    EditorGUILayout.PropertyField(overlayTextureScale, new GUIContent("Texture Scale"));
                }
                EditorGUILayout.PropertyField(overlayBlending, new GUIContent("Blending"));
                EditorGUILayout.PropertyField(overlayMinIntensity, new GUIContent("Min Intensity"));
                EditorGUILayout.PropertyField(overlayAnimationSpeed, new GUIContent("Animation Speed"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawSectionField(targetFX, "Target", targetFX.boolValue);
            if (targetFX.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(targetFXTexture, new GUIContent("Texture"));
                EditorGUILayout.PropertyField(targetFXColor, new GUIContent("Color"));
                EditorGUILayout.PropertyField(targetFXRotationSpeed, new GUIContent("Rotation Speed"));
                EditorGUILayout.PropertyField(targetFXInitialScale, new GUIContent("Initial Scale"));
                EditorGUILayout.PropertyField(targetFXEndScale, new GUIContent("End Scale"));
                EditorGUILayout.PropertyField(targetFXScaleToRenderBounds, new GUIContent("Scale To Object Bounds"));
                EditorGUILayout.PropertyField(targetFXAlignToGround, new GUIContent("Align To Ground"));
                if (targetFXAlignToGround.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(targetFXGroundMaxDistance, new GUIContent("Ground Max Distance"));
                    EditorGUILayout.PropertyField(targetFXGroundLayerMask, new GUIContent("Ground Layer Mask"));
                    EditorGUILayout.PropertyField(targetFXFadePower, new GUIContent("Fade Power"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(targetFXTransitionDuration, new GUIContent("Transition Duration"));
                EditorGUILayout.PropertyField(targetFXStayDuration, new GUIContent("Stay Duration"));
                EditorGUILayout.PropertyField(targetFXVisibility, new GUIContent("Visibility"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.PropertyField(seeThrough);
            if (seeThrough.intValue != (int)SeeThroughMode.Never) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(seeThroughOccluderMask, new GUIContent("Occluder Layer"));
                if (seeThroughOccluderMask.intValue > 0) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(seeThroughOccluderMaskAccurate, new GUIContent("Accurate"));
                    EditorGUILayout.PropertyField(seeThroughOccluderThreshold, new GUIContent("Radius Threshold", "Multiplier to the object bounds. Making the bounds smaller prevents false occlusion tests."));
                    EditorGUILayout.PropertyField(seeThroughOccluderCheckInterval, new GUIContent("Check Interval", "Interval in seconds between occlusion tests."));
                    EditorGUILayout.PropertyField(seeThroughOccluderCheckIndividualObjects, new GUIContent("Check Individual Objects", "Interval in seconds between occlusion tests."));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(seeThroughDepthOffset, new GUIContent("Depth Offset" + ((seeThroughDepthOffset.floatValue > 0) ? " •" : "")));
                EditorGUILayout.PropertyField(seeThroughMaxDepth, new GUIContent("Max Depth" + ((seeThroughMaxDepth.floatValue > 0) ? " •" : "")));
                EditorGUILayout.PropertyField(seeThroughIntensity, new GUIContent("Intensity"));
                EditorGUILayout.PropertyField(seeThroughTintColor, new GUIContent("Color"));
                EditorGUILayout.PropertyField(seeThroughTintAlpha, new GUIContent("Color Blend"));
                EditorGUILayout.PropertyField(seeThroughNoise, new GUIContent("Noise"));
                EditorGUILayout.PropertyField(seeThroughBorder, new GUIContent("Border When Hidden"));
                if (seeThroughBorder.floatValue > 0) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(seeThroughBorderWidth, new GUIContent("Width"));
                    EditorGUILayout.PropertyField(seeThroughBorderColor, new GUIContent("Color"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(seeThroughOrdered, new GUIContent("Ordered"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawSectionField(hitFxInitialIntensity, "Hit FX", hitFxInitialIntensity.floatValue > 0);
            if (hitFxInitialIntensity.floatValue > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(hitFxInitialIntensity, new GUIContent("Initial Intensity"));
                EditorGUILayout.PropertyField(hitFxMode, new GUIContent("Mode"));
                EditorGUILayout.PropertyField(hitFxFadeOutDuration, new GUIContent("Fade Out Duration"));
                EditorGUILayout.PropertyField(hitFxColor, new GUIContent("Color"));
                if ((HitFxMode)hitFxMode.intValue == HitFxMode.LocalHit) {
                    EditorGUILayout.PropertyField(hitFxRadius, new GUIContent("Radius"));
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            if (serializedObject.ApplyModifiedProperties() || (Event.current.type == EventType.ValidateCommand &&
                Event.current.commandName == "UndoRedoPerformed")) {

                // Triggers profile reload on all Highlight Effect scripts
                HighlightEffect[] effects = FindObjectsOfType<HighlightEffect>();
                for (int t = 0; t < targets.Length; t++) {
                    HighlightProfile profile = (HighlightProfile)targets[t];
                    for (int k = 0; k < effects.Length; k++) {
                        if (effects[k] != null && effects[k].profile == profile && effects[k].profileSync) {
                            profile.Load(effects[k]);
                            effects[k].Refresh();
                        }
                    }
                }
                EditorUtility.SetDirty(target);
            }

        }

        void DrawSectionField(SerializedProperty property, string label, bool active) {
            EditorGUILayout.PropertyField(property, new GUIContent(active ? label + " •" : label));
        }

    }

}