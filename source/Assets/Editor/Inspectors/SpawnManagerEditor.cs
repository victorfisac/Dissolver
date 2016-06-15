using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(SpawnManager))]
public class SpawnManagerEditor : Editor 
{
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		SpawnManager myScript = (SpawnManager)target;

		GUILayout.Space(20);
        GUILayout.Label("Spawned enemies: " + myScript.EnemiesCount);

		if (GUILayout.Button("Start Spawner")) myScript.StartSpawner();
		if (GUILayout.Button("Align Points")) myScript.AlignSpawners();
	}
}
