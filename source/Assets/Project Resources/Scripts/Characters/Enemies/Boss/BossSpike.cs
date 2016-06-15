using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossSpike : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private float damageDuration;
	[SerializeField] private float damage;
	[SerializeField] private Vector3 forceDirection;
	[SerializeField] private float forceAmount;

	[Header("Motion")]
	[SerializeField] private AnimationCurve motionCurve;
	[SerializeField] private float motionDuration;
	[SerializeField] private float distance;

	[Header("Delay")]
	[SerializeField] private float delay;

	[Header("Dissolver")]
	[SerializeField] private AnimationCurve dissolverCurve;
	[SerializeField] private float dissolverDuration;

	[Header("References")]
	[SerializeField] private Transform trans;
	#endregion

	#region Private Attributes
	// Motion
	private float timeCounter;			// Spike animation time counter
	private Vector3 initPosition;		// Spike position at start
	private bool canWork;				// Spike can work state
	private int state;					// Spike animation current state

	// Dissolver
	private Material[] materials;		// Spike visual materials

	// Targets
	private float damageCounter;		// Damage time counter
	private List<Character> targets;	// Target character references list
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Get references
		MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
		materials = new Material[renderers.Length];
		for(int i = 0; i < materials.Length; i++) materials[i] = renderers[i].material;

		// Initialize values
		initPosition = trans.position;
		targets = new List<Character>();

		// Disable game object by default
		gameObject.SetActive(false);
	}

	public void UpdateBehaviour()
	{
		if(canWork)
		{
			switch(state)
			{
				case 0:
				{
					// Update spike position based on animation curve
					trans.position = initPosition + Vector3.up * motionCurve.Evaluate(timeCounter / motionDuration) * distance;

					// Update time counter
					timeCounter += Time.deltaTime;

					if(timeCounter >= motionDuration)
					{
						// Reset time counter
						timeCounter = 0f;

						// Update current state
						state  = 1;
					}
				} break;
				case 1:
				{
					// Update time counter
					timeCounter += Time.deltaTime;

					if(timeCounter >= delay)
					{
						// Reset time counter
						timeCounter = 0f;

						// Update current state
						state = 2;
					}
				} break;
				case 2:
				{
					// Update materials dissolve amount based on animation curve
					for(int i = 0; i < materials.Length; i++) materials[i].SetFloat("_DissolveAmount", dissolverCurve.Evaluate(timeCounter / dissolverDuration));

					// Update time counter
					timeCounter += Time.deltaTime;

					if(timeCounter >= dissolverDuration)
					{
						// Reset current state
						state = 0;

						// Reset materials dissolve amount
						for(int i = 0; i < materials.Length; i++) materials[i].SetFloat("_DissolveAmount", -0.1f);

						// Disable spike
						DisableSpike();
					}
				} break;
				default: break;
			}

			if(targets.Count > 0)
			{
				// Update time counter
				damageCounter -= Time.deltaTime;

				if(damageCounter <= 0)
				{
					// Reset time counter
					damageCounter = damageDuration;

					// Make damage to all targets
					for(int i = 0; i < targets.Count; i++) targets[i].SetDamage(damage, transform.root.TransformDirection(forceDirection), forceAmount, null);
				}
			}
		}
	}
	#endregion

	#region Spike Methods
	public void EnableSpike(Vector3 position)
	{
		// Update position
		trans.position = new Vector3(position.x, initPosition.y, position.z);

		// Update init animation position
		initPosition = trans.position;

		// Update can work state
		canWork = true;

		// Reset time counter
		timeCounter = 0f;

		// Enable spike game object
		gameObject.SetActive(true);
	}

	public void DisableSpike()
	{
		// Update can work state
		canWork = false;

		// Reset time counter
		timeCounter = 0f;

		// Disable spike game object
		gameObject.SetActive(false);
	}
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag == "Player" || other.gameObject.tag == "Ally")
		{
			Character otherChar = other.GetComponent<Character>();
			if(otherChar)
			{
				targets.Add(otherChar);
				timeCounter = 0f;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if(other.gameObject.tag == "Player" || other.gameObject.tag == "Ally")
		{
			Character otherChar = other.GetComponent<Character>();
			if(otherChar) targets.Remove(otherChar);
		}
	}
	#endregion
}
