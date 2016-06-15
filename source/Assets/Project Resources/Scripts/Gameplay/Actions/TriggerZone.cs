using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TriggerZone : MonoBehaviour 
{
	#region Inspector Methods
	[Header("Settings")]
	[SerializeField] private bool checkTag;
	[SerializeField] private string targetTag;
	[SerializeField] private bool checkLayer;
	[SerializeField] private string targetLayer;
	[SerializeField] private bool disableOnTrigger;
	[SerializeField] private bool checkProgress;
	[SerializeField] private int maxProgress;
	[SerializeField] private bool killCharacters;

	[Header("Events")]
	[SerializeField] private float eventDelay;
	[SerializeField] private UnityEvent triggerEvent;
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		bool canDetect = true;

		// Check settings requeriments to handle trigger detection
		if(checkProgress) canDetect = (GameManager.Instance.GameProgress <= maxProgress);
		if(checkTag) canDetect = (other.tag == targetTag);
		if(checkLayer) canDetect = (other.gameObject.layer == LayerMask.NameToLayer(targetLayer));

		// Check if trigger event is valid
		if(canDetect)
		{
			if(killCharacters && other.gameObject.tag != "Player") Destroy(other.gameObject);
			else
			{
				// Invoke events method after delay
				Invoke("InvokeEvents", eventDelay);

			#if DEBUG_BUILD
				// Trace debug message
				Debug.Log("TriggerZone: executed trigger " + gameObject.name);
			#endif

				// Disable if needed
				if(disableOnTrigger) gameObject.SetActive(false);
			}
		}
	}
	#endregion

	#region Trigger Methods
	private void InvokeEvents()
	{
		// Invoke all trigger event methods
		triggerEvent.Invoke();
	}
	#endregion
}
