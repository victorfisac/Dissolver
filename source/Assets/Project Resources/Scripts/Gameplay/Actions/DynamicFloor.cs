using UnityEngine;
using System.Collections;

public class DynamicFloor : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private float maxDelay;
	[SerializeField] private float distance;

	[Header("Motion")]
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float duration;
	#endregion

	#region Private Attributes
	private Transform[] floorChilds;		// Child transform reference
	private float[] counters;				// Transform animation time counter
	private float[] finalDurations;			// Animation final durations with random delay
	private Vector3[] startPositions;		// Animation start childs positions
	private Vector3[] endPositions;			// Animation final childs positions
	private bool finished;					// Finished dynamic state
	private bool canWork;					// Can work state
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		floorChilds = new Transform[transform.childCount];
		counters = new float[floorChilds.Length];
		startPositions = new Vector3[floorChilds.Length];
		endPositions = new Vector3[floorChilds.Length];
		finalDurations = new float[floorChilds.Length];

		for(int i = 0; i < floorChilds.Length; i++)
		{
			floorChilds[i] = transform.GetChild(i);
			counters[i] = 0f;
			startPositions[i] = floorChilds[i].localPosition - Vector3.down * distance;
			endPositions[i] = floorChilds[i].localPosition;
			finalDurations[i] = duration + Random.Range(0f, maxDelay);
			floorChilds[i].position = startPositions[i];
			floorChilds[i].gameObject.SetActive(false);
		}
	}

	public void UpdateBehaviour()
	{
		if(canWork)
		{
			for(int i = 0; i < floorChilds.Length; i++)
			{
				// Update childs positions based on curve animation
				floorChilds[i].localPosition = Vector3.Lerp(startPositions[i], endPositions[i], curve.Evaluate(counters[i] / finalDurations[i]));

				// Update time counter
				counters[i] += Time.deltaTime;
			}

			// Check if all animations finished
			finished = true;
			for(int i = 0; i < counters.Length; i++)
			{
				if(counters[i] < finalDurations[i]) finished = false;
			}

			// Disable behaviour if all animations are finished
			if(finished) canWork = false;
		}
	}
	#endregion

	#region Dynamic Methods
	public void EnableDynamic()
	{
		if(!finished && gameObject.activeSelf)
		{
			// Enable childs game objects
			for(int i = 0; i < floorChilds.Length; i++) floorChilds[i].gameObject.SetActive(true);

			// Enable behaviour
			canWork = true;
		}
	}

	public void EndDynamic()
	{
		// Enable childs game objects
		for(int i = 0; i < floorChilds.Length; i++) floorChilds[i].gameObject.SetActive(true);

		// Update dynamic values
		canWork = false;
		finished = true;

		// Set all objects to final position
		for(int i = 0; i < floorChilds.Length; i++) floorChilds[i].localPosition = endPositions[i];
	}
	#endregion
}
