using UnityEngine;
using UnityEditor;

namespace FoW
{
    [CustomEditor(typeof(FogOfWarChunkManager))]
    public class FogOfWarChunkManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            FogOfWarChunkManager cm = (FogOfWarChunkManager)target;
            FogOfWarTeam fow = cm.GetComponent<FogOfWarTeam>();

            if (fow.mapResolution.x != fow.mapResolution.y)
                EditorGUILayout.HelpBox("Map Resolution must be square!", MessageType.Error);
            if (fow.mapResolution.x % 2 != 0)
                EditorGUILayout.HelpBox("Map Resolution must be divisible by 2!", MessageType.Error);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox(string.Format("Chunks Loaded: {0}\nMemory Usage: {1}", cm.loadedChunkCount, EditorUtility.FormatBytes(cm.loadedChunkCount * fow.mapResolution.x * fow.mapResolution.y)), MessageType.None);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear Memory"))
                    cm.Clear();
                if (GUILayout.Button("Save"))
                {
                    string path = EditorUtility.SaveFilePanel("Save FogOfWar Chunks", null, "chunks", null);
                    if (!string.IsNullOrEmpty(path))
                        System.IO.File.WriteAllBytes(path, cm.Save());
                }
                if (GUILayout.Button("Load"))
                {
                    string path = EditorUtility.OpenFilePanel("Load FogOfWar Chunks", null, null);
                    if (!string.IsNullOrEmpty(path))
                        cm.Load(System.IO.File.ReadAllBytes(path));
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
