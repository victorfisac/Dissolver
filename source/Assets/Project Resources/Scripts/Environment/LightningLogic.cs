using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class LightningLogic : MonoBehaviour 
{
	#region Enums
	public enum LightningType { INSTANT, PROGRESSIVE };
	#endregion

	#region Inspector Methods
	[Header("Settings")]
	[SerializeField] private bool isManual;
	[SerializeField] private LightningType type;
	[SerializeField] private int distance;
	[SerializeField] private int divisions;
	[SerializeField] private float randomNoise;

	[Header("Construction")]
	[SerializeField] private float positionDelay;

	[Header("Visual")]
	[SerializeField] private AnimationCurve alphaCurve;
	[SerializeField] private float alphaDuration;

	[Header("Behaviour")]
	[SerializeField] private Vector2 nextDelayLimits;

	[Header("Audio")]
	[SerializeField] private float volume;
	[SerializeField] private Vector2 distances;
	[SerializeField] private AudioMixerGroup group;
	[SerializeField] private AudioClip[] clips;

	[Header("Explosion")]
	[SerializeField] private MeshRenderer staticRenderer;
	[SerializeField] private GameObject explosionObject;

	[Header("References")]
	[SerializeField] private LineRenderer line;
	#endregion

	#region Private Attributes
	private float timeCounter;				// Alpha animation and delay time counter
	private float nextDelay;				// Current next lightning delay
	private int state;						// Current lightning state
	private Color lineColor;				// Line renderer material color
	private Vector3[] positions;			// Line renderer positions
	private int positionCounter;			// Lightning progressive construction counter
	private float positionTimeCounter;		// Lightning progressive construction time counter
	#endregion

	#region Main Methods
	public void AwakeBehaviour () 
	{
		// Initialize values
		positions = new Vector3[divisions];

		// Calculate position based on distance and divisions
		for(int i = 0; i < positions.Length; i++) positions[i] = Vector3.up * distance;

		// Set positions to line renderer positions array
		line.SetVertexCount(divisions);
		line.SetPositions(positions);

		nextDelay = Random.Range(nextDelayLimits.x, nextDelayLimits.y);
		line.enabled = false;

		lineColor = line.material.GetColor("_Color");
		lineColor.a = 0f;
		line.material.SetColor("_Color", lineColor);
	}

	public void UpdateBehaviour () 
	{
		switch(state)
		{
			case 0:
			{
				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= nextDelay && !isManual)
				{
					// Reset time counter and update lightning state
					timeCounter = 0f;
					state = 1;

					if(type == LightningType.INSTANT)
					{
						// Calculate position based on distance and divisions
						for(int i = 0; i < positions.Length; i++) positions[i] = new Vector3(Random.Range(-randomNoise, randomNoise) * i / divisions, i * distance / divisions, Random.Range(-randomNoise, randomNoise) * i / divisions);

						// Set positions to line renderer positions array
						line.SetPositions(positions);
					}

					// Enable line renderer
					line.enabled = true;

					// Create a new audio source
					AudioSource newSource = gameObject.AddComponent<AudioSource>();
					newSource.clip = clips[Random.Range((int)0, (int)clips.Length)];
					newSource.volume = volume;
					newSource.spatialBlend = 1f;
					newSource.outputAudioMixerGroup = group;
					newSource.minDistance = distances.x;
					newSource.maxDistance = distances.y;

					// Play new audio source
					newSource.Play();

					// Destroy new source after playing sound
					Destroy(newSource, newSource.clip.length);
				}
			} break;
			case 1:
			{
				if(type == LightningType.PROGRESSIVE)
				{
					if(positionCounter < divisions)
					{
						// Update positions time counter
						positionTimeCounter += Time.fixedDeltaTime;

						if(positionTimeCounter >= positionDelay)
						{
							// Calculate position based on distance and divisions
							positions[positionCounter] = new Vector3(Random.Range(-randomNoise, randomNoise) * (divisions - (positionCounter + 1)) / divisions, distance - (positionCounter + 1) * (distance / divisions), Random.Range(-randomNoise, randomNoise) * (divisions - (positionCounter + 1)) / divisions);

							// Make all the following positions the current position
							for(int i = positionCounter + 1; i < positions.Length; i++) positions[i] = positions[positionCounter];

							// Set positions to line renderer positions array
							line.SetPositions(positions);

							// Reset positions time counter
							positionTimeCounter = 0f;

							// Update position counter
							positionCounter++;
						}
					}
				}

				// Get current material color
				lineColor = line.material.GetColor("_Color");

				// Update alpha value based on animation curve
				lineColor.a = alphaCurve.Evaluate(timeCounter / alphaDuration);

				// Update material color based on calculated value
				line.material.SetColor("_Color", lineColor);

				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= alphaDuration)
				{
					// Reset time counter and update lightning state
					timeCounter = 0f;
					state = 0;

					// Calculate position based on distance and divisions
					Vector3[] positions = new Vector3[divisions];
					for(int i = 0; i < positions.Length; i++) positions[i] = Vector3.up * distance;

					// Reset lightning counter
					positionCounter = 0;

					// Set positions to line renderer positions array
					line.SetPositions(positions);

					// Disable line renderer
					line.enabled = false;

					// Calculate new next lightning delay
					nextDelay = Random.Range(nextDelayLimits.x, nextDelayLimits.y);
				}
			} break;
			default: break;
		}
	}
	#endregion

	#region Lightning Methods
	public void EnableLightning()
	{
		// Reset time counter and update lightning state
		timeCounter = 0f;
		state = 1;

		if(type == LightningType.INSTANT)
		{
			// Calculate position based on distance and divisions
			for(int i = 0; i < positions.Length; i++) positions[i] = new Vector3(Random.Range(-randomNoise, randomNoise) * i / divisions, i * distance / divisions, Random.Range(-randomNoise, randomNoise) * i / divisions);

			// Set positions to line renderer positions array
			line.SetPositions(positions);
		}

		// Enable line renderer
		line.enabled = true;

		// Create a new audio source
		AudioSource newSource = gameObject.AddComponent<AudioSource>();
		newSource.clip = clips[Random.Range((int)0, (int)clips.Length)];
		newSource.volume = volume;
		newSource.spatialBlend = 1f;
		newSource.outputAudioMixerGroup = group;
		newSource.minDistance = distances.x;
		newSource.maxDistance = distances.y;

		// Play new audio source
		newSource.Play();

		// Destroy new source after playing sound
		Destroy(newSource, newSource.clip.length);
	}

	public void ExplodeStructure()
	{
		// Disable static structure game object
		staticRenderer.enabled = false;

		// Disable lightning line renderer
		line.enabled = false;

		// Update can work state
		state = -1;

		// Enable structure shatter game object
		explosionObject.SetActive(true);
	}
	#endregion
}
