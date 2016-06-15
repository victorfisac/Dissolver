using UnityEngine;
using System.Collections;

public class TankExplosion : MonoBehaviour 
{
	#region Enums
	public enum DelayType { RANDOM, SORTED };
	#endregion

	#region Public Attributes
	[Header("Settings")]
	[SerializeField] private DelayType type;
	[SerializeField] private float minDelay;
	[SerializeField] private float maxDelay;

	[Header("Collision")]
	[SerializeField] private float detectionOffset;
	[SerializeField] private LayerMask mask;
	[SerializeField] private float detectionDelay;

	[Header("Dissolve")]
	[SerializeField] private float dissolveDelay;
	[SerializeField] private AnimationCurve dissolveCurve;
	[SerializeField] private float dissolveDuration;

	[Header("Animation")]
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float duration;

	[Header("Particles")]
	[SerializeField] private ParticleSystem explosionSystem;

	[Header("References")]
	[SerializeField] private Character character;
	[SerializeField] private TankCombat tankCombat;
	[SerializeField] private Collider coll;
	#endregion

	#region Private Attributes
	private Transform[] childsTransform;	// Childs transform component references
	private Vector3[] initPositions;		// Initial local positions
	private Vector3[] desiredPositions;		// Current desired positions

	private float[] delay;					// Explosion animation delay
	private bool[] canWork;					// Explosion animation can work state
	private float[] timer;					// Explosion animation timer
	private bool[] finished;				// Explosion animation finished state

	private RaycastHit hitInfo;				// Ground raycast to fix y axis position

	private float damage;					// Damage value from character
	private Vector3 direction;				// Movement direction from character
	private float force;					// Force from character
	private float detectionCounter;			// Physics detection time counter
	private bool dissolveState;				// Dissolve transition state
	private float dissolveCounter;			// Dissolve animation time counter
	private Material[] mats;				// Spikes materials references
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Get initial positions
		initPositions = new Vector3[transform.childCount];
		for(int i = 0; i < transform.childCount; i++) initPositions[i] = transform.GetChild(i).localPosition;

		// Get character values
		damage = tankCombat.Radial.Damage;
		direction = tankCombat.Radial.Movement;
		force = tankCombat.Radial.Force;

		MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
		mats = new Material[renderers.Length];
		for(int i = 0; i < mats.Length; i++) mats[i] = renderers[i].material;

		InitializeExplosion();

		// Ignore physics between player collider and explosion collider
		Physics.IgnoreCollision(coll, transform.root.GetComponent<CharacterController>());

		// Disable object after awake
		gameObject.SetActive(false);
	}

	public void InitializeExplosion () 
	{
		// Initialize values
		timer = new float[transform.childCount];
		childsTransform = new Transform[transform.childCount];
		desiredPositions = new Vector3[transform.childCount];
		delay = new float[transform.childCount];
		canWork = new bool[transform.childCount];
		finished = new bool[transform.childCount];
		coll.enabled = false;

		for(int i = 0; i < childsTransform.Length; i++)
		{
			childsTransform[i] = transform.GetChild(i);
			childsTransform[i].localPosition = initPositions[i];
			delay[i] = ((type == DelayType.RANDOM) ? Random.Range(minDelay, maxDelay) : (maxDelay * i));

		#if UNITY_EDITOR
			// Correct y axis position
			Debug.DrawLine(childsTransform[i].position + (Vector3.up * detectionOffset), childsTransform[i].position + (Vector3.down * detectionOffset * 2), Color.gray, 1f);
		#endif

			if (Physics.Raycast(childsTransform[i].position + (Vector3.up * detectionOffset), Vector3.down, out hitInfo, detectionOffset * 2, mask)) 
			{
				childsTransform[i].position = hitInfo.point + Vector3.down * 0.9f;
				desiredPositions[i] = childsTransform[i].position;
			}
		}
	}

	public void UpdateBehaviour () 
	{
		// Update physics detection counter
		if(!coll.enabled)
		{
			if(detectionCounter < detectionDelay) detectionCounter += Time.deltaTime;
			else coll.enabled = true;
		}

		for(int i = 0; i < childsTransform.Length; i++)
		{
			if(!canWork[i])
			{
				// Update timer counter
				timer[i] += Time.deltaTime;

				if(timer[i] >= delay[i])
				{
					// Reset timer for explosion animation
					timer[i] = 0f;

					// Update can work state
					canWork[i] = true;

					// Update spike animation duration
					delay[i] = duration + Random.Range(minDelay, maxDelay);
				}
			}
			else
			{
				// Update child positions based on animation curve
				childsTransform[i].position = desiredPositions[i] + Vector3.up * curve.Evaluate(timer[i] / delay[i]);

				// Update explosion timer
				timer[i] += Time.deltaTime;

				if(timer[i] > delay[i])
				{
					// Reset can work state
					canWork[i] = false;

					// Reset animation timer
					timer[i] = 0f;

					// Update finished state
					finished[i] = true;
				}
			}
		}

		if(gameObject.activeSelf)
		{
			if(dissolveState)
			{
				// Check object null reference
				if(explosionSystem)
				{
					// Play explosion particle system
					if(!explosionSystem.isPlaying) explosionSystem.Play();
				}

				// Update materials dissolve amount based on animation curve
				for(int i = 0; i < mats.Length; i++) mats[i].SetFloat("_DissolveAmount", dissolveCurve.Evaluate(dissolveCounter / dissolveDuration));

				// Update dissolve time counter
				dissolveCounter += Time.deltaTime;

				if(dissolveCounter >= dissolveDuration)
				{
					// Reset dissolve counter
					dissolveCounter = 0f;

					// Reset dissolve state
					dissolveState = false;
				}
			}
			else
			{
				// Update dissolve time counter
				dissolveCounter += Time.deltaTime;

				if(dissolveCounter >= dissolveDelay)
				{
					// Reset dissolve counter
					dissolveCounter = 0f;

					// Update dissolve state
					dissolveState = true;
				}
			}
		}

		// Check if all animation finished
		for(int i = 0; i < childsTransform.Length; i++)
		{
			if(!finished[i]) break;

			// Update materials dissolve amount based on animation curve
			for(int k = 0; k < mats.Length; k++) mats[k].SetFloat("_DissolveAmount", -0.1f);

			// Enable physics collision
			coll.enabled = false;

			// Disable game object if all spikes finished its animation
			gameObject.SetActive(false);
		}
	}
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.layer == LayerMask.NameToLayer("Characters")) 
		{
			Character otherChar = other.GetComponent<Character>();

			if(otherChar)
			{
				if(otherChar != character) otherChar.SetDamage(damage, direction, force, character);
			}
		}

		if(other.gameObject.tag == "Destroyable")
		{
			Destroyable otherDestroy = other.GetComponent<Destroyable>();
			if(otherDestroy) otherDestroy.Shatter(true);
		}
	}
	#endregion

	#region Properties
	public float Duration
	{
		get { return duration; }
	}
	#endregion
}
