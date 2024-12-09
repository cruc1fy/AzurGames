
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MazeGenerator myScript = (MazeGenerator)target;
        if (GUILayout.Button("Generate"))
        {
            myScript.Draw();
        }
    }
}