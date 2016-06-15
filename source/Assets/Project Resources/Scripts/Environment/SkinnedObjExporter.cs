using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Linq;
 
public class SkinnedObjExporter : MonoBehaviour
{
	[Header("Attributes")]
	public string path;

	private SkinnedMeshRenderer[] meshes;
	private Transform[] transforms;

	public void SetPath(string newPath)
	{
		path = newPath;
	}
 
    public void MeshToFile() 
    {
		meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
    	transforms = new Transform[meshes.Length];

    	for(int i = 0; i < transforms.Length; i++)
    	{
    		transforms[i] = meshes[i].transform;
    	}

    	Debug.Log("ObjExporter: created OBJ file from: " + meshes.Length + " meshes");

		StreamWriter sw = new StreamWriter(path);
		char space = (char)13;
		char ret = (char)10;

		for(int i = 0; i < meshes.Length; i++)
		{
			Mesh m = new Mesh();
			meshes[i].BakeMesh(m);

			// Write obj vertex
			foreach(Vector3 vv in m.vertices) 
			{
				Vector3 v = transforms[i].TransformPoint(vv);
				sw.Write(string.Format("v {0} {1} {2}",v.x,v.y,v.z));
			    sw.Write(space);
				sw.Write(ret);
			}

			sw.Write(space);
			sw.Write(ret);

			// Write obj normals
			Quaternion r = transforms[i].localRotation;

			foreach(Vector3 nn in m.normals) 
			{
				Vector3 v = r * nn;
			    sw.Write(string.Format("vn {0} {1} {2}",v.x,v.y,v.z));
				sw.Write(space);
				sw.Write(ret);
			}

			sw.Write(space);
			sw.Write(ret);

			// Write obj texture coords
			foreach(Vector3 v in m.uv) 
			{
				sw.Write(string.Format("vt {0} {1}\n",v.x,v.y));
				sw.Write(space);
				sw.Write(ret);
			}

			sw.Write(space);
			sw.Write(ret);

			int triangleCounter = 0;

			// Write obj triangles index
			int[] triangles = m.triangles;

			for (int j = 0; j < triangles.Length; j += 3)
			{
				sw.Write(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", triangleCounter + triangles[j]+1, triangleCounter + triangles[j+1]+1, triangleCounter + triangles[j+2]+1));
				sw.Write(space);
				sw.Write(ret);
			}

			triangleCounter += triangles.Max() + 1;
		}

		sw.Close();
		

		meshes = new SkinnedMeshRenderer[0];
		transforms = new Transform[0];
    }
}