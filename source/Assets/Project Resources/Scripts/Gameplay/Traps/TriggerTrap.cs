using UnityEngine;
using System.Collections;

public class TriggerTrap : MonoBehaviour 
{
	#region Enums
	public enum Axis { X, Y, Z };
	#endregion

	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private float damage;
	[SerializeField] private bool checkLayer;
	[SerializeField] private LayerMask mask;

	[Header("Motion")]
	[SerializeField] private Axis axis;
	[SerializeField] private Transform target;
	[SerializeField] private float duration;
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float delay;
	#endregion

	#region Private Attributes
	private Vector3 initPosition;		// Init target local position
	private float counter;				// Motion time counter
	private bool canWork;				// Can work state
	private bool reverse;				// Trap animation reverse state
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Initialize values
		initPosition = target.localPosition;
		counter = delay;
	}

	public void UpdateBehaviour()
	{
		if(canWork)
		{
			if(reverse)
			{
				// Update y axis position based on animation curve value
				switch(axis)
				{
					case Axis.X: target.transform.localPosition = initPosition + Vector3.right * curve.Evaluate(1f) - Vector3.right * curve.Evaluate(counter / duration); break;
					case Axis.Y: target.transform.localPosition = initPosition + Vector3.up * curve.Evaluate(1f) - Vector3.up * curve.Evaluate(counter / duration); break;
					case Axis.Z: target.transform.localPosition = initPosition + Vector3.forward * curve.Evaluate(1f) - Vector3.forward * curve.Evaluate(counter / duration); break;
					default: break;
				}
			}
			else
			{
				// Update y axis position based on animation curve value
				switch(axis)
				{
					case Axis.X: target.transform.localPosition = initPosition + Vector3.right * curve.Evaluate(counter / duration); break;
					case Axis.Y: target.transform.localPosition = initPosition + Vector3.up * curve.Evaluate(counter / duration); break;
					case Axis.Z: target.transform.localPosition = initPosition + Vector3.forward * curve.Evaluate(counter / duration); break;
					default: break;
				}
			}

			// Update motion counter
			counter += Time.deltaTime;

			// Reset counter value if it is out of bounds
			if(counter >= duration) 
			{
				// Reset time counter
				counter = 0f;

				// Disable behaviour
				canWork = false;
			}
		}
	}
	#endregion

	#region Trap Methods
	public void ApplyTrigger(bool rev)
	{
		// Enable behaviour
		canWork = true;

		// Update reverse state
		reverse = rev;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("TriggerTrap: trap enabled with reverse: " + reverse.ToString());
	#endif
	}
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		// Apply damage to any triggered object
		Character character = other.GetComponent<Character>();
		if(character) character.SetDamage(damage, Vector3.zero, 0f, null);
	}
	#endregion
}
