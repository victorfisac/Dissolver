using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ObjExporter))]
public class ObjExporterEditor : Editor 
{
	public override void OnInspectorGUI ()
	{
		ObjExporter script = (ObjExporter)target;

		DrawDefaultInspector();

		GUILayout.Space(20);

		if(GUILayout.Button("Build OBJ file"))
		{
			script.MeshToFile();
		}
	}
}