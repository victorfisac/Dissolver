using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(AchievementLogic))]
public class AchievementLogicEditor : Editor 
{
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		AchievementLogic myScript = (AchievementLogic)target;

		GUILayout.Space(20);

		if (GUILayout.Button("Get Achievement")) myScript.GetAchievement();
	}
}
