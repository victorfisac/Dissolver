/*using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(SpikeTrap))]
public class SpikeTrapEditor : Editor 
{
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		SpikeTrap myScript = (SpikeTrap)target;

		GUILayout.Space(20);

		if (GUILayout.Button("Enable Spikes")) myScript.SetTrapState(true);
		if (GUILayout.Button("Disable Spikes")) myScript.SetTrapState(false);
	}
}
*/