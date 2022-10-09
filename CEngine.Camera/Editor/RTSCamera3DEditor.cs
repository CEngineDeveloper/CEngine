
using UnityEditor;
using UnityEngine;
using System.Collections;
using CYM;
using Sirenix.OdinInspector.Editor;
using UnityEditor.SceneManagement;
namespace CYM.Cam
{
    [CustomEditor(typeof(RTSCamera3D))]
    public class RTSCamera3DEditor : OdinEditor
    {

        private RTSCamera3D mCam;
        private bool baseSetting;
        private bool boundSetting;
        private bool followSetting;
        private bool controlSetting;

        void Awake()
        {
            mCam = target as RTSCamera3D;
        }

        public override void OnInspectorGUI()
        {
            baseSetting = EditorGUILayout.Foldout(baseSetting, "Basic");
            if (baseSetting)
            {
                EditorGUILayout.LabelField("Smoothing Settings");
                mCam.movementLerpSpeed = EditorGUILayout.FloatField("  -Movement Lerp Speed", mCam.movementLerpSpeed);
                mCam.rotationLerpSpeed = EditorGUILayout.FloatField("  -Rotation Lerp Speed", mCam.rotationLerpSpeed);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Scroll Settings");

                CamScrollAnimType tempType = mCam.scrollAnimationType;
                tempType = (CamScrollAnimType)EditorGUILayout.EnumPopup("   -Animation Type", tempType);

                if (tempType != mCam.scrollAnimationType
                    && EditorUtility.DisplayDialog("Replacing Changes", "If you switch to another animation type, your settings in current mode will be replaced or modified.", "Continue", "Cancel"))
                {
                    mCam.scrollAnimationType = tempType;
                }


                switch (mCam.scrollAnimationType)
                {
                    case CamScrollAnimType.Simple:
                        mCam.minHight = EditorGUILayout.FloatField("    -Min High", mCam.minHight);
                        mCam.maxHight = EditorGUILayout.FloatField("    -Max High", mCam.maxHight);
                        mCam.minAngle = EditorGUILayout.FloatField("    -Min Angle", mCam.minAngle);
                        mCam.maxAngle = EditorGUILayout.FloatField("    -Max Angle", mCam.maxAngle);
                        break;

                    case CamScrollAnimType.Advanced:
                        mCam.scrollXAngle = EditorGUILayout.CurveField(new GUIContent("    Scroll X Angle", "Scroll X Angle Animation"), mCam.scrollXAngle);
                        mCam.scrollHigh = EditorGUILayout.CurveField(new GUIContent("    Scroll High", "Scroll High Animation"), mCam.scrollHigh);
                        break;
                }
                EditorGUILayout.Slider("    -Start Scroll Value", mCam.ScrollValue, 0f, 1f);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Casting Settings");
                mCam.groundHighTest = EditorGUILayout.Toggle("  -Ground Check", mCam.groundHighTest);
                if (mCam.groundHighTest)
                {
                    mCam.groundMask = EditorUtil.LayerMaskField("  -Ground Mask", mCam.groundMask);// PreviewUtil.LayerMaskField("  -Ground Mask", mCam.groundMask);
                    mCam.groundHighTestValMax = EditorGUILayout.FloatField("  -Ground Check Value", mCam.groundHighTestValMax);
                    mCam.groundHighTestValLerpSpeed = EditorGUILayout.FloatField("  -Ground Lerp Spped", mCam.groundHighTestValLerpSpeed);
                    mCam.seaLevel = EditorGUILayout.FloatField("  -Sea Level", mCam.seaLevel);
                }
                EditorGUILayout.Space();
            }

            boundSetting = EditorGUILayout.Foldout(boundSetting, "Bound");
            if (boundSetting)
            {
                mCam.bound.xMin = EditorGUILayout.FloatField("  -Min X", mCam.bound.xMin);
                mCam.bound.xMax = EditorGUILayout.FloatField("  -Max X", mCam.bound.xMax);
                mCam.bound.yMin = EditorGUILayout.FloatField("  -Min Z", mCam.bound.yMin);
                mCam.bound.yMax = EditorGUILayout.FloatField("  -Max Z", mCam.bound.yMax);
                EditorGUILayout.Space();
            }

            followSetting = EditorGUILayout.Foldout(followSetting, "Follow");
            if (followSetting)
            {
                mCam.unlockWhenMove = EditorGUILayout.Toggle("  -Unlock When Move", mCam.unlockWhenMove);
                EditorGUILayout.Space();
            }

            controlSetting = EditorGUILayout.Foldout(controlSetting, "Control");
            if (controlSetting)
            {
                //Screen Edge
                EditorGUILayout.LabelField("  Mouse Screen Edge Movement");
                mCam.desktopMoveSpeed = EditorGUILayout.FloatField("    -Move Speed", mCam.desktopMoveSpeed);
                mCam.deskScreenEdgeWidth = EditorGUILayout.FloatField("    -Edge Width", mCam.deskScreenEdgeWidth);
                EditorGUILayout.Space();

                //Drag
                EditorGUILayout.LabelField("  Mouse Drag Control");
                mCam.mouseDragButton = System.Convert.ToInt32(EditorGUILayout.EnumPopup("    -Move Button", (MouseButton)mCam.mouseDragButton));
                mCam.desktopMoveDragSpeed = EditorGUILayout.FloatField("    -Move Speed", mCam.desktopMoveDragSpeed);
                if (mCam.mouseDragButton == mCam.mouseRotateButton)
                {
                    EditorGUILayout.HelpBox("Control button overlapping.", MessageType.Warning);
                }
                EditorGUILayout.Space();

                //Rotation
                EditorGUILayout.LabelField("  Mouse Rotate Control");
                mCam.mouseRotateButton = System.Convert.ToInt32(EditorGUILayout.EnumPopup("    -Rotate Button", (MouseButton)mCam.mouseRotateButton));
                mCam.rotateAxis = (RotateAxis)EditorGUILayout.EnumPopup("    -Rotate Axis", mCam.rotateAxis);
                mCam.minXAxisRotateAngle = EditorGUILayout.FloatField("    -Min XAxis Rotate Angle", mCam.minXAxisRotateAngle);
                mCam.desktopRotateSpeed = EditorGUILayout.FloatField("    -Rotate Speed", mCam.desktopRotateSpeed);
                mCam.desktopRotateDelay = EditorGUILayout.FloatField("    -Rotate Delay", mCam.desktopRotateDelay);
                if (mCam.mouseDragButton == mCam.mouseRotateButton)
                {
                    EditorGUILayout.HelpBox("Control button overlapping.", MessageType.Warning);
                }
                EditorGUILayout.Space();

                //Scroll
                EditorGUILayout.LabelField("  Mouse Scroll Control");
                mCam.desktopScrollSpeed = EditorGUILayout.FloatField("    -Scroll Speed", mCam.desktopScrollSpeed);
                EditorGUILayout.Space();

                //Touch
                EditorGUILayout.LabelField("  Touch Screen Edge Movement");
                mCam.touchMoveSpeed = EditorGUILayout.FloatField("    -Move Speed", mCam.touchMoveSpeed);
                mCam.touchScreenEdgeWidth = EditorGUILayout.FloatField("    -Edge Width", mCam.touchScreenEdgeWidth);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("  Touch Drag Control");
                mCam.touchMoveDragSpeed = EditorGUILayout.FloatField("    -Move Speed", mCam.touchMoveDragSpeed);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("  Touch Rotate Control");
                mCam.touchRotateSpeed = EditorGUILayout.FloatField("    -Rotate Speed", mCam.touchRotateSpeed);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("  Touch Scroll Control");
                mCam.touchScrollSpeed = EditorGUILayout.FloatField("    -Scroll Speed", mCam.touchScrollSpeed);
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(mCam);
                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }
        }
    }

}