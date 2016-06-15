using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour 
{
	#region Enums
	public enum PickupType { HEALTH, SKILL };
	#endregion

	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private PickupType type;
	[SerializeField] private int amount;
	[SerializeField] private float destroyDelay;

	[Header("Motion")]
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float duration;

	[Header("Audio")]
	[SerializeField] private AudioSource pickupSource;

	[Header("References")]
	[SerializeField] private Transform trans;
	[SerializeField] private SphereCollider detectionColl;
	[SerializeField] private SphereCollider pickupColl;
	[SerializeField] private GameObject pickup;
	[SerializeField] private GameObject explosion;
	#endregion

	#region Private Attributes
	private int state;				// Pickup behaviour state
	private Vector3 initPosition;	// Initial transform position
	private Character playerChar;	// Player character reference
	private float counter;			// Animation time counter
	#endregion

	#region Main Methods
	public void StartBehaviour()
	{
		// Initialize values
		initPosition = transform.position;
	}

	public void UpdateBehaviour()
	{
		switch(state)
		{
			case 1:
			{
				// Update pickup position based on animation curve
				trans.position = Vector3.Lerp(initPosition, playerChar.Trans.position + Vector3.up, curve.Evaluate(counter / duration));

				// Update time counter
				counter += Time.deltaTime;
			} break;
			case 2:
			{
				// Update time counter
				counter += Time.deltaTime;

				if(counter >= destroyDelay)
				{
					// Delete pickup reference from gameplay manager
					GameObject.FindWithTag("GameController").GetComponent<GameplayManager>().Pickups.Remove(this);

					// Destroy pickup game object
					Destroy(gameObject);
				}
			} break;
			default: break;
		}
	}
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag == "Player")
		{
			switch(state)
			{
				case 0:
				{
					// Get player character reference
					playerChar = other.GetComponent<Character>();

					// Disable detection collider
					detectionColl.enabled = false;

					// Enable pickup collider
					pickupColl.enabled = true;

					// Update state value
					state = 1;

					// Play pickup sound
					pickupSource.Play();
				} break;
				case 1:
				{
					switch(type)
					{
						case PickupType.HEALTH:
						{
							if(playerChar)
							{
								// Add slot to player character
								playerChar.AddHealth((float)amount);

								// Apply health feedback to player
								playerChar.Feedback.EnableEvent(4);
							}
						} break;
						case PickupType.SKILL:
						{
							if(playerChar)
							{
								// Add slot to player character
								playerChar.AddSkill(amount);

								// Apply health feedback to player
								playerChar.Feedback.EnableEvent(5);
							}
						} break;
						default: break;
					}

					// Update state value
					state = 2;

					// Reset time counter for destroy delay
					counter = 0f;

					// Disable pickup collider
					pickupColl.enabled = false;

					// Parent pickup to player transform
					if(playerChar) transform.SetParent(playerChar.Trans);

					// Fix pickup position to character
					transform.position = other.transform.position + Vector3.up;

					// Disable pickup visuals
					pickup.SetActive(false);

					// Enable pickup explosion
					explosion.SetActive(true);
				} break;
				default: break;
			}
		}
	}
	#endregion
}
