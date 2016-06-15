using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Destroyable : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private float hits;
	[SerializeField] private Vector2 dissolveLimits;
	[SerializeField] private float dissolveDelay;
	[SerializeField] private float alphaDuration;
	[SerializeField] private bool justTank;
	[SerializeField] private bool destroyOnShatter;

	[Header("Particles")]
	[SerializeField] private ParticleSystem hitSystem;

	[Header("Physics")]
	[SerializeField] private float explosionForce;
	[SerializeField] private float explosionRadius;

	[Header("Pickup")]
	[SerializeField] private bool randomPickup;
	[SerializeField] private int specificPickup;
	[SerializeField] private GameObject[] pickups;

	[Header("Audio")]
	[SerializeField] private AudioClip[] clips;

	[Header("Events")]
	[SerializeField] private UnityEvent shatterEvent;

	[Header("References")]
	[SerializeField] private GameObject visualObject;
	[SerializeField] private GameObject shatterObject;
	[SerializeField] private AudioSource source;
	[SerializeField] private Collider coll;
	#endregion

	#region Private Attributes
	private int state;								// Shatter behaviour state
	private int currentHit;							// Current hit count
	private float timeCounter;						// Shatter fade out time counter
	private bool destroyed;							// Destroyable destroyed state
	private MeshRenderer[] shatterRenderers;		// Shatter objects mesh renderer references
	private Rigidbody[] rbs;						// Shatter rigidbodies references
	private GameplayManager gameplayManager;		// Gameplay manager reference
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Get references
		gameplayManager = GameObject.FindWithTag("GameController").GetComponent<GameplayManager>();
		shatterRenderers = shatterObject.GetComponentsInChildren<MeshRenderer>();
		rbs = shatterObject.GetComponentsInChildren<Rigidbody>();

		// Initialize values
		shatterObject.SetActive(false);
	}

	public void UpdateBehaviour()
	{
		if(shatterObject.activeSelf)
		{
			switch(state)
			{
				case 0:
				{
					// Update time counter
					timeCounter += Time.deltaTime;

					if(timeCounter >= dissolveDelay)
					{
						// Reset time counter
						timeCounter = 0f;

						// Update shatter state
						state = 1;
					}
				} break;
				case 1:
				{
					// Update dissolve amount based on time counter
					for(int i = 0; i < shatterRenderers.Length; i++) shatterRenderers[i].material.SetFloat("_DissolveAmount", Mathf.Lerp(dissolveLimits.x, dissolveLimits.y, timeCounter / alphaDuration));

					// Update time counter
					timeCounter += Time.deltaTime;

					if(timeCounter >= alphaDuration)
					{
						// Remove destroyable from gameplay manager list
						gameplayManager.Destroyables.Remove(this);

						// Destroy destroyable game object if needed
						if(destroyOnShatter) Destroy(gameObject);
					}
				} break;
				default: break;
			}
		}
	}
	#endregion

	#region DestroyableMethods
	public void Shatter(bool isTank)
	{
		// Check if destroyable is already shatted
		if(!shatterObject.activeSelf)
		{
			bool canShatter = true;

			if(justTank && !isTank) canShatter = false;

			if(canShatter)
			{
				// Update current hit count
				currentHit++;

				// Check if hits are enough to shatter
				if(currentHit >= hits)
				{
					// Update destroyed state
					destroyed = true;

					// Disable visual object
					visualObject.SetActive(false);

					// Disable destroyable main collider
					coll.enabled = false;

					// Set a random smash sound
					source.clip = clips[Random.Range((int)0, (int)clips.Length)];

					// Enable shatter object
					shatterObject.SetActive(true);

					// Invoke all shatter event methods
					shatterEvent.Invoke();

					// Apply explosion to all rigidbodies
					for(int i = 0; i < rbs.Length; i++) rbs[i].AddExplosionForce(explosionForce, shatterObject.transform.position, explosionRadius);

					// Instantiate new pickup if needed
					if(pickups.Length > 0)
					{
						// Instantiate new pickup game object
						GameObject newObject = (GameObject)Instantiate((randomPickup ? pickups[Random.Range((int)0, (int)pickups.Length)] : pickups[specificPickup]), transform.position, Quaternion.identity);

						// Get new pickup component reference and initialize it
						Pickup newPickup = newObject.GetComponent<Pickup>();
						newPickup.StartBehaviour();

						// Add new pickup reference to gameplay manager list
						gameplayManager.Pickups.Add(newPickup);
					}
				}
				else if(hitSystem) hitSystem.Play();	// Play hit particle system
			}
		}
	}
	#endregion

	#region Properties
	public bool Destroyed
	{
		get { return destroyed; }
	}
	#endregion
}
