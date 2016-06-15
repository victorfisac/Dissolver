using UnityEngine;
using UnityEditor;
using System.Collections;

public class Test : MonoBehaviour 
{
	#region Data
	[MenuItem("Tools/Data/Delete Player Prefs")]
	public static void DeletePlayerPreeeefs()
	{
		Debug.Log("Editor: player prefs deleted successfully");
		PlayerPrefs.DeleteAll();
	}
	#endregion

	#region PressKit
	[MenuItem("Tools/Screenshot/Capture Screenshot")]
	public static void CaptureScreenshot()
	{
		Application.CaptureScreenshot("Screenshot.png");
		Debug.Log("Editor: screenshot captured with size (" + Screen.width + "x" + Screen.height + ") successfully");
	}

	[MenuItem("Tools/Screenshot/Capture Screenshot (x2)")]
	public static void CaptureScreenshotX2()
	{
		Application.CaptureScreenshot("Screenshot.png", 2);
		Debug.Log("Editor: screenshot captured with size (" + (Screen.width * 2) + "x" + (Screen.height * 2) + ") successfully");
	}

	[MenuItem("Tools/Screenshot/Capture Screenshot (x4)")]
	public static void CaptureScreenshotX4()
	{
		Application.CaptureScreenshot("Screenshot.png", 4);
		Debug.Log("Editor: screenshot captured with size (" + (Screen.width * 4) + "x" + (Screen.height * 4) + ") successfully");
	}

	[MenuItem("Tools/Screenshot/Capture Screenshot (x8)")]
	public static void CaptureScreenshotX8()
	{
		Application.CaptureScreenshot("Screenshot.png", 8);
		Debug.Log("Editor: screenshot captured with size (" + (Screen.width * 8) + "x" + (Screen.height * 8) + ") successfully");
	}
	#endregion

	#region Export
	[MenuItem("Tools/Export/Export static meshes")]
	public static void ExportStaticMeshes()
	{
		Selection.activeGameObject.AddComponent<ObjExporter>();
		Selection.activeGameObject.GetComponent<ObjExporter>().SetPath("C:/dissolver_exported_object.obj");
		Selection.activeGameObject.GetComponent<ObjExporter>().MeshToFile();
		Destroy(Selection.activeGameObject.GetComponent<ObjExporter>());
		Debug.Log("Editor: exported static meshes");
	}

	[MenuItem("Tools/Export/Export skinned meshes")]
	public static void ExportSkinnedMeshes()
	{
		Selection.activeGameObject.AddComponent<SkinnedObjExporter>();
		Selection.activeGameObject.GetComponent<SkinnedObjExporter>().SetPath("C:/dissolver_exported_object.obj");
		Selection.activeGameObject.GetComponent<SkinnedObjExporter>().MeshToFile();
		Destroy(Selection.activeGameObject.GetComponent<SkinnedObjExporter>());
		Debug.Log("Editor: exported skinned meshes");
	}
	#endregion

	#region Cursor
	[MenuItem("Tools/Cursor/Lock Cursor %F1")]
	private static void LockCursor()
	{
		// Disable and lock cursor
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	[MenuItem("Tools/Cursor/Unlock Cursor #F1")]
	private static void UnlockCursor()
	{
		// Disable and lock cursor
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
	#endregion
}