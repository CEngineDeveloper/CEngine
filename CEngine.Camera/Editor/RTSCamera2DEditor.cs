using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEditor.SceneManagement;

namespace CYM.Cam
{
    [CustomEditor(typeof(RTSCamera2D))]
    public class RTSCamera2DEditor : Editor
    {

        private RTSCamera2D mCam;
        private bool baseSetting;
        private bool boundSetting;
        private bool mouseSetting;
        private bool touchSetting;

        void Awake()
        {
            mCam = target as RTSCamera2D;
        }

        public override void OnInspectorGUI()
        {
            baseSetting = EditorGUILayout.Foldout(baseSetting, "Basic");
            if (baseSetting)
            {
                mCam.moveLerpSpeed = EditorGUILayout.FloatField("Move Lerp Speed", mCam.moveLerpSpeed);
                Range zoomRange = mCam.zoomRange;
                zoomRange.Min = EditorGUILayout.FloatField("Zoom Min", zoomRange.Min);
                zoomRange.Max = EditorGUILayout.FloatField("Zoom Max", zoomRange.Max);
                mCam.zoomRange = zoomRange;
                EditorGUILayout.Slider("Start Scroll Value", mCam.ScrollValue, 0f, 1f);//mCam.ScrollValue = 
                mCam.unlockWhenMove = EditorGUILayout.Toggle("  Unlock When Move", mCam.unlockWhenMove);
            }

            boundSetting = EditorGUILayout.Foldout(boundSetting, "Bound");
            if (boundSetting)
            {
                EditorGUILayout.LabelField("MinBound");
                mCam.bound.xMin = EditorGUILayout.FloatField("  Min X", mCam.bound.xMin);
                mCam.bound.xMax = EditorGUILayout.FloatField("  Max X", mCam.bound.xMax);
                mCam.bound.yMin = EditorGUILayout.FloatField("  Min Y", mCam.bound.yMin);
                mCam.bound.yMax = EditorGUILayout.FloatField("  Max Y", mCam.bound.yMax);
                
                EditorGUILayout.LabelField("MaxBound");
                mCam.maxBound.xMin = EditorGUILayout.FloatField("  Min X", mCam.maxBound.xMin);
                mCam.maxBound.xMax = EditorGUILayout.FloatField("  Max X", mCam.maxBound.xMax);
                mCam.maxBound.yMin = EditorGUILayout.FloatField("  Min Y", mCam.maxBound.yMin);
                mCam.maxBound.yMax = EditorGUILayout.FloatField("  Max Y", mCam.maxBound.yMax);

                EditorGUILayout.Space();
            }

            mouseSetting = EditorGUILayout.Foldout(mouseSetting, "Mouse Control");
            if (mouseSetting)
            {
                mCam.desktopMoveDragSpeed = EditorGUILayout.FloatField("  Drag Speed", mCam.desktopMoveDragSpeed);
                mCam.desktopMoveSpeed = EditorGUILayout.FloatField("  Move Speed", mCam.desktopMoveSpeed);
                mCam.desktopScrollSpeed = EditorGUILayout.FloatField("  Scroll Speed", mCam.desktopScrollSpeed);
                EditorGUILayout.Space();
            }
            touchSetting = EditorGUILayout.Foldout(touchSetting, "Touch Control");
            if (touchSetting)
            {
                mCam.touchMoveDragSpeed = EditorGUILayout.FloatField("  Drag Speed", mCam.touchMoveDragSpeed);
                mCam.touchScrollSpeed = EditorGUILayout.FloatField("  Scroll Speed", mCam.touchScrollSpeed);
                EditorGUILayout.Space();
            }
            EditorGUILayout.Space();

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