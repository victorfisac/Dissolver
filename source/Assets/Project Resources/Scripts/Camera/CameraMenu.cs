using UnityEngine;
using System.Collections;

public class CameraMenu : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Motion")]
	[SerializeField] private Vector3 startPosition;
	[SerializeField] private AnimationCurve motionCurve;
	[SerializeField] private float motionDuration;

	[Header("References")]
	[SerializeField] private Transform trans;
	#endregion

	#region Private Attributes
	private bool canWork;			// Camera animation can work state
	private float timeCounter;		// Camera animation time counter
	private Vector3 endPosition;	// Camera animation end position
	#endregion

	#region Main Methods
	public void AwakeBehaviour () 
	{
		// Initialize values
		canWork = true;
		endPosition = trans.position;
		trans.position = startPosition;
	}

	public void UpdateBehaviour () 
	{
		if(canWork)
		{
			// Update camera position based on animation curve
			trans.position = Vector3.Lerp(startPosition, endPosition, motionCurve.Evaluate(timeCounter / motionDuration));

			// Update time counter
			timeCounter += Time.deltaTime;

			if(timeCounter >= motionDuration) canWork = false;
		}
	}
	#endregion

	#region Properties
	public float Duration
	{
		get { return motionDuration; }
	}
	#endregion
}
