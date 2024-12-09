
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GradientBackground))]
public class GradientBackgroundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GradientBackground myScript = (GradientBackground)target;
        if (GUILayout.Button("Apply"))
        {
            myScript.Apply();
        }
    }
}
