using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TriggerEnd : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private bool checkTag;
	[SerializeField] private string targetTag;

	[Header("Visual")]
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private Vector2 offsetSpeed;

	[Header("Events")]
	[SerializeField] private UnityEvent endEvent;
	#endregion

	#region Private Attributes
	private Material meshMat;		// Mesh renderer material reference
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Get references
		meshMat = meshRenderer.material;
	}
	
	public void UpdateBehaviour()
	{
		// Update mesh material offset
		Vector2 offset = meshMat.GetTextureOffset("_BumpMap");
		offset += offsetSpeed * Time.deltaTime;
		meshMat.SetTextureOffset("_BumpMap", offset);
	}
	#endregion

	#region Detection Methods
	public void OnTriggerEnter(Collider other)
	{
		if(checkTag)
		{
			if(other.tag == targetTag) endEvent.Invoke();
		}
		else endEvent.Invoke();

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("TriggerZone: level completed trigger detected");
	#endif
	}
	#endregion
}
