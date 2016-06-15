using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ConditionZone : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private int maxConditions;
	[SerializeField] private UnityEvent conditionEvent;
	#endregion

	#region Private Attributes
	private float conditionCounter;		// Current conditions count
	#endregion

	#region Condition Methods
	public void AddCount()
	{
		// Update condition counter value
		conditionCounter++;

		if(conditionCounter >= maxConditions)
		{
			// Fix condition counter value out of bounds
			conditionCounter = maxConditions;

			// Invoke conditions event
			conditionEvent.Invoke();

			// Destroy component
			Destroy(this);
		}

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("ConditionZone: condition updated to " + conditionCounter + " / " + maxConditions);
	#endif
	}
	#endregion
}
