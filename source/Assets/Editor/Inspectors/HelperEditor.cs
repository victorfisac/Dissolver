using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Helper))]
public class HelperEditor : Editor 
{
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		Helper myScript = (Helper)target;

		GUILayout.Space(20);

		if (GUILayout.Button("Fade In")) myScript.HelperFadeIn();
		if (GUILayout.Button("Fade Out")) myScript.HelperFadeOut();
	}
}
