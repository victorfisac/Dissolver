using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(PowerPilarPuzzle))]
public class PowerPilarPuzzleEditor : Editor 
{
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		PowerPilarPuzzle myScript = (PowerPilarPuzzle)target;

		GUILayout.Space(20);

		if (GUILayout.Button("Enable Intro")) myScript.IntroPilar();
	}
}
