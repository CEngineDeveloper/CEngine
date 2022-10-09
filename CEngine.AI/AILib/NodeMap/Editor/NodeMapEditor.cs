
using UnityEditor;
using UnityEngine;
namespace CYM.AI.NodesMap
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(NodeMap))]
    public class NodeMapEditor : Editor
    {
        private NodeMap map;
        private bool showSettingsSection = true;

        private void OnEnable()
        {
            map = target as NodeMap;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            showSettingsSection = EditorGUILayout.Foldout(showSettingsSection, "Settings");
            if (showSettingsSection)
            {
                GUILayout.BeginVertical();
                map.showPath = EditorGUILayout.Toggle("Draw Path", map.showPath);
                map.redrawThreshhold = EditorGUILayout.FloatField("Redraw Threshold", map.redrawThreshhold);
                float nodeScale = EditorGUILayout.FloatField("Node Scale", map.nodeScale);
                if (System.Math.Abs(nodeScale - map.nodeScale) > Mathf.Epsilon)
                {
                    map.nodeScale = nodeScale;
                }
                GUILayout.EndVertical();
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (GUILayout.Button("RefreshAdd"))
            {
                map.Refresh();
            }
            if (GUILayout.Button("RefreshRemove"))
            {
                map.Refresh(false);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}