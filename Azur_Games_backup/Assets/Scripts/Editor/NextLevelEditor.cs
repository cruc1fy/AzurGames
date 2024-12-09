using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Level))]
public class NextLevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Next Level"))
        {
            FinishMenu.instance.NoThankYou();
        }
    }
}
