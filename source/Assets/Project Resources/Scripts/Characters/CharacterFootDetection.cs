using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class CharacterFootDetection : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Footprints")]
	[SerializeField] private float groundOffset;
	[SerializeField] private float groundDistance;
	[SerializeField] private LayerMask groundMask;

	[Header("Audio")]
	[SerializeField] private AudioClip[] clips;
	[SerializeField] private AudioMixerGroup group;
	#endregion

	#region Private Attributes
	private bool detected;					// Current ground detected state
	private bool lastDetected;				// Previous ground detected state
	#endregion

	#region Main Methods
	public void UpdateBehaviour()
	{
		// Update last detected state
		lastDetected = detected;

		// Get current detected state
		if(transform) detected = Physics.Raycast(transform.position + Vector3.up * groundOffset, Vector3.down, groundDistance, groundMask);

	#if DEBUG_BUILD
		// Trace debug ray to check inspector values
		Debug.DrawRay(transform.position + Vector3.up * groundOffset, Vector3.down * groundDistance, Color.white);
	#endif

		// Check if foot is grounded by first time
		if(!lastDetected && detected)
		{
			// Create new audio source in foot
			AudioSource newSource = gameObject.AddComponent<AudioSource>();
			newSource.clip = clips[Random.Range((int)0, (int)clips.Length)];
			newSource.spatialBlend = 1f;
			newSource.outputAudioMixerGroup = group;

		#if DEBUG_BUILD
			// Trace debug message (overhead)
			// Debug.Log("CharacterFootDetection: detected new object ");
		#endif
		
			// Play the new audio source
			newSource.Play();

			// Destroy the audio source after playing sound
			Destroy(newSource, newSource.clip.length);
		}
	}
	#endregion
}