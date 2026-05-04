using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(FloorTileGenerator))]
public class FloorTileGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Use Generate and Clear in Edit Mode to place floor tiles directly into the scene.", MessageType.Info);

        FloorTileGenerator generator = (FloorTileGenerator)target;

        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            if (GUILayout.Button("Generate"))
            {
                generator.GenerateTiles();
            }

            if (GUILayout.Button("Clear"))
            {
                generator.ClearTiles();
            }
        }

        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("The buttons are disabled in Play Mode.", MessageType.Warning);
        }
    }
}
#endif
