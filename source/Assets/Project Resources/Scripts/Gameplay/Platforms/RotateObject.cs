using UnityEngine;
using System.Collections;

public class RotateObject : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private bool random;
	[SerializeField] private Vector3 axis;
	[SerializeField] private float speed;

	[Header("References")]
	[SerializeField] private Transform trans;
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Set random axis value if needed
		if(random) axis = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
	}

	public void UpdateBehaviour()
	{
		trans.Rotate(axis * speed * Time.deltaTime, Space.Self);
	}
	#endregion
}
