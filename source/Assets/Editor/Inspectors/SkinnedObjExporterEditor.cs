using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SkinnedObjExporter))]
public class SkinnedObjExporterEditor : Editor 
{
	public override void OnInspectorGUI ()
	{
		SkinnedObjExporter script = (SkinnedObjExporter)target;

		DrawDefaultInspector();

		GUILayout.Space(20);

		if(GUILayout.Button("Build OBJ file"))
		{
			script.MeshToFile();
		}
	}
}