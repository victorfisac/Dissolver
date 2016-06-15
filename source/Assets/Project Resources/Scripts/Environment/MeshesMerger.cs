using UnityEngine;
using System.Collections;

public class MeshesMerger : MonoBehaviour 
{
	#region Main Methods
	public void AwakeBehaviour()
	{
		for(int i = 0; i < transform.childCount; i++) transform.GetChild(i).position += transform.position;

	    transform.position = Vector3.zero;
	    transform.rotation = Quaternion.identity;
	   
	    MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[filters.Length];

	    int index = 0;
	    for (int i = 0; i < filters.Length; i++)
	    {
	        if (filters[i].sharedMesh == null) continue;
	        combine[index].mesh = filters[i].sharedMesh;
			combine[index++].transform = filters[i].transform.localToWorldMatrix;
	    }

		for(int i = 0; i < filters.Length; i++) Destroy(filters[i].gameObject);

	    // Create mesh filter component
		MeshFilter newMeshFilter = gameObject.AddComponent<MeshFilter>();

		// Initialize and combine meshes into one
		newMeshFilter.mesh = new Mesh();
		newMeshFilter.mesh.CombineMeshes (combine);
	}
	#endregion
}