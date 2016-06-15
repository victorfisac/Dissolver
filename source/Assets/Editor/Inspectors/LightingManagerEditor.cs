using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(LightingManager))]
public class LightingManagerEditor : Editor 
{
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		LightingManager myScript = (LightingManager)target;

		GUILayout.Space(20);

		if (GUILayout.Button("Calm Lighting")) myScript.SetCalm();
		if (GUILayout.Button("Combat Lighting")) myScript.SetCombat();
	}
}
