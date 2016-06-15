using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour 
{
	#region Enums
	public enum Axis { X, Y, Z };
	#endregion

	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private float damage;
	[SerializeField] private bool checkLayer;
	[SerializeField] private LayerMask mask;
	[SerializeField] private bool playOnAwake;

	[Header("Motion")]
	[SerializeField] private Axis axis;
	[SerializeField] private Transform target;
	[SerializeField] private float duration;
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float delay;

	[Header("Disable")]
	[SerializeField] private AnimationCurve disableCurve;
	[SerializeField] private float disableDuration;
	#endregion

	#region Private Attributes
	private Vector3 initPosition;		// Init target local position
	private float counter;				// Motion time counter
	private int state;					// Vertical trap can work state
	private Vector3 disableInit;		// Disable init position
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Initialize values
		state = -1;
		initPosition = target.localPosition;
		counter = delay;

		if(playOnAwake) state = 0;
		else 
		{
			switch(axis)
			{
				case Axis.X: target.localPosition = initPosition + Vector3.right * curve.Evaluate(0f); break;
				case Axis.Y: target.localPosition = initPosition + Vector3.up * curve.Evaluate(0f); break;
				case Axis.Z: target.localPosition = initPosition + Vector3.forward * curve.Evaluate(0f); break;
				default: break;
			}
		}
	}

	public void UpdateBehaviour()
	{
		switch(state)
		{
			case 0:
			{
				// Update y axis position based on animation curve value
				switch(axis)
				{
					case Axis.X: target.localPosition = initPosition + Vector3.right * curve.Evaluate(counter / duration); break;
					case Axis.Y: target.localPosition = initPosition + Vector3.up * curve.Evaluate(counter / duration); break;
					case Axis.Z: target.localPosition = initPosition + Vector3.forward * curve.Evaluate(counter / duration); break;
					default: break;
				}

				// Update motion counter
				counter += Time.deltaTime;

				// Reset counter value if it is out of bounds
				if(counter >= duration) counter = 0f;
				break;
			}
			case 1:
			{
				// Update y axis position based on animation curve value
				switch(axis)
				{
					case Axis.X: target.localPosition = disableInit + Vector3.right * disableCurve.Evaluate(counter / disableDuration); break;
					case Axis.Y: target.localPosition = disableInit + Vector3.up * disableCurve.Evaluate(counter / disableDuration); break;
					case Axis.Z: target.localPosition = disableInit + Vector3.forward * disableCurve.Evaluate(counter / disableDuration); break;
					default: break;
				}

				// Update motion counter
				counter += Time.deltaTime;

				// Reset counter value if it is out of bounds
				if(counter >= disableDuration)
				{
					// Update trap state
					state = 2;

					// Reset time counter
					counter = 0f;
				}
				break;
			}
			default: break;
		}
	}
	#endregion

	#region Trap Methods
	public void SetTrapState(bool value)
	{
		// Update can work state
		state = value ? 0 : 1;

		// Reset time counter
		counter = 0f;

		// Calculate disable interpolation init position
		disableInit = target.localPosition;
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
