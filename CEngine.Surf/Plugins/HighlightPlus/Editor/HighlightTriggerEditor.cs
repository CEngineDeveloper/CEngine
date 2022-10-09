using UnityEditor;
using UnityEngine;

namespace HighlightPlus {
    [CustomEditor(typeof(HighlightTrigger))]
    public class HighlightTriggerEditor : Editor {

        SerializedProperty highlightOnHover, triggerMode, raycastCamera, raycastSource, minDistance, maxDistance, respectUI, volumeLayerMask;
        SerializedProperty selectOnClick, selectedProfile, selectedAndHighlightedProfile, singleSelection, toggleOnClick;
        HighlightTrigger trigger;

        void OnEnable() {
            highlightOnHover = serializedObject.FindProperty("highlightOnHover");
            triggerMode = serializedObject.FindProperty("triggerMode");
            raycastCamera = serializedObject.FindProperty("raycastCamera");
            raycastSource = serializedObject.FindProperty("raycastSource");
            minDistance = serializedObject.FindProperty("minDistance");
            maxDistance = serializedObject.FindProperty("maxDistance");
            respectUI = serializedObject.FindProperty("respectUI");
            volumeLayerMask = serializedObject.FindProperty("volumeLayerMask");
            selectOnClick = serializedObject.FindProperty("selectOnClick");
            selectedProfile = serializedObject.FindProperty("selectedProfile");
            selectedAndHighlightedProfile = serializedObject.FindProperty("selectedAndHighlightedProfile");
            singleSelection = serializedObject.FindProperty("singleSelection");
            toggleOnClick = serializedObject.FindProperty("toggle");
            trigger = (HighlightTrigger)target;
            trigger.Init();
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            if (trigger.triggerMode == TriggerMode.RaycastOnThisObjectAndChildren) {
                if (trigger.colliders == null || trigger.colliders.Length == 0) {
                    EditorGUILayout.HelpBox("No collider found on this object or any of its children. Add colliders to allow automatic highlighting.", MessageType.Warning);
                }
            } else {
#if ENABLE_INPUT_SYSTEM
                if (trigger.triggerMode == TriggerMode.ColliderEventsOnlyOnThisObject) {
                    EditorGUILayout.HelpBox("This trigger mode is not compatible with the new input system.", MessageType.Error);
                }
#endif
                if (trigger.GetComponent<Collider>() == null) {
                    EditorGUILayout.HelpBox("No collider found on this object. Add a collider to allow automatic highlighting.", MessageType.Error);
                }
            }

            EditorGUILayout.PropertyField(triggerMode);
            switch (trigger.triggerMode) {
                case TriggerMode.RaycastOnThisObjectAndChildren:
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(raycastCamera);
                    EditorGUILayout.PropertyField(raycastSource);
                    EditorGUILayout.PropertyField(minDistance);
                    EditorGUILayout.PropertyField(maxDistance);
                    EditorGUI.indentLevel--;
                    break;
                case TriggerMode.Volume:
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(volumeLayerMask);
                    EditorGUI.indentLevel--;
                    break;
            }

            if (trigger.triggerMode != TriggerMode.Volume) {
                EditorGUILayout.PropertyField(respectUI);
            }
            EditorGUILayout.PropertyField(highlightOnHover);
            EditorGUILayout.PropertyField(selectOnClick);
            if (selectOnClick.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(selectedProfile);
                EditorGUILayout.PropertyField(selectedAndHighlightedProfile);
                EditorGUILayout.PropertyField(singleSelection);
                EditorGUILayout.PropertyField(toggleOnClick);
                EditorGUILayout.HelpBox("To deselect any object by clicking outside, add a Highlight Manager to the scene.", MessageType.Info);
                EditorGUI.indentLevel--;
            }

            if (serializedObject.ApplyModifiedProperties()) {
                trigger.Init();
            }
        }

    }

}
