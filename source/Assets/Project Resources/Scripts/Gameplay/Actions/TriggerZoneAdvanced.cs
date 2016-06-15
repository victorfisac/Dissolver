using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TriggerZoneAdvanced : MonoBehaviour 
{
	#region Inspector Methods
	[Header("Settings")]
	[SerializeField] private bool checkTag;
	[SerializeField] private string targetTag;
	[SerializeField] private bool disableOnExit;
	[SerializeField] private bool checkProgress;
	[SerializeField] private int maxProgress;

	[Header("Events")]
	[SerializeField] private UnityEvent triggerEnter;
	[SerializeField] private UnityEvent triggerExit;
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		bool canDetect = true;

		if(checkProgress) canDetect = (GameManager.Instance.GameProgress <= maxProgress);

		if(canDetect)
		{
			// Handle trigger event conditions
			if(!checkTag) triggerEnter.Invoke();
			else if(other.tag == targetTag) triggerEnter.Invoke();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		bool canDetect = true;

		if(checkProgress) canDetect = (GameManager.Instance.GameProgress <= maxProgress);

		if(canDetect)
		{
			// Handle trigger event conditions
			if(!checkTag) 
			{
				triggerExit.Invoke();

			#if DEBUG_BUILD
				// Trace debug message
				Debug.Log("TriggerZone: executed advanced trigger " + gameObject.name);
			#endif

				// Disable if needed
				if(disableOnExit) gameObject.SetActive(false);
			}
			else if(other.tag == targetTag)
			{
				triggerExit.Invoke();

			#if DEBUG_BUILD
				// Trace debug message
				Debug.Log("TriggerZone: executed advanced trigger " + gameObject.name);
			#endif

				// Disable if needed
				if(disableOnExit) gameObject.SetActive(false);
			}
		}
	}
	#endregion
}
