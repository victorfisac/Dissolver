using UnityEngine;
using System.Collections;

public class PlatformWeak : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private float delay;
	[SerializeField] private float destroyDelay;

	[Header("Shake")]
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float shakeDuration;
	[SerializeField] private float shakeAmount;

	[Header("References")]
	[SerializeField] private Collider coll;
	#endregion

	#region Private Attributes
	private int state;				// Weak platform state
	private float counter;			// Weak platform counter
	private float shakeCounter;		// Shake animation time counter
	private int rbCounter;			// Rigidbody enabled process counter
	private Rigidbody[] rbs;		// Shatter rigidbodies references
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Get references
		rbs = GetComponentsInChildren<Rigidbody>();
	}

	public void UpdateBehaviour()
	{
		switch(state)
		{
			case 1:
			{
				transform.Translate(curve.Evaluate(shakeCounter / shakeDuration) * shakeAmount, 0f, curve.Evaluate(shakeCounter / shakeDuration) * shakeAmount, Space.World);

				// Update shake counter
				shakeCounter += Time.deltaTime;
				if(shakeCounter > shakeDuration) shakeCounter = 0f;

				// Update counter value
				counter += Time.deltaTime;

				if(counter >= delay)
				{
					// Reset time counter
					counter = 0f;

					// Disable behaviour
					state = 2;
				}
			} break;
			case 2:
			{
				// Disable current rigidbody kinematic state
				rbs[rbCounter].isKinematic = false;

				// Update rigidbody counter
				rbCounter++;

				if(rbCounter == rbs.Length) state = 3;
			} break;
			case 3:
			{
				// Update time counter
				counter += Time.deltaTime;

				if(counter >= destroyDelay)
				{
					// Reset time counter
					counter = 0f;

					// Delete reference from gameplay manager list
					GameObject.FindWithTag("GameController").GetComponent<GameplayManager>().PlatformWeaks.Remove(this);

					// Destroy game object
					Destroy(gameObject);
				}
			} break;
			default: break;
		}
	} 
	#endregion

	#region Platform Methods
	public void MakeWeak()
	{
		// Enable behaviour
		state = 1;

		// Disable detection collider
		coll.enabled = false;

		// Disable current rigidbody kinematic state
		transform.GetChild(0).GetComponent<Rigidbody>().isKinematic = false;
	}

	private void ShatterPlatform()
	{
		
	}
	#endregion
}
