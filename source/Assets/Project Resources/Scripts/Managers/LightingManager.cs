using UnityEngine;
using System.Collections;

public class LightingManager : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Light Settings")]
	[SerializeField] private LightSettings calmSettings;
	[SerializeField] private LightSettings combatSettings;

	[Header("Animation")]
	[SerializeField] private AnimationCurve lightCurve;
	[SerializeField] private float duration;

	[Header("Audio")]
	[SerializeField] private AudioSource combatSource;
	[SerializeField] private float combatVolume;

	[Header("References")]
	[SerializeField] private Light directionalLight;
	#endregion

	#region Private Attributes
	private float timeCounter;					// Lighting settings time counter
	private LightSettings previousSettings;		// Lighting previous state values
	#endregion

	#region Main Methods
	public void AwakeBehaviour ()
	{
		// Initialize values
		previousSettings = new LightSettings(directionalLight.intensity, directionalLight.color);
	}

	public void UpdateBehaviour () 
	{
		if(combatSettings.Enabled)
		{
			// Update light settings based on animation curve
			directionalLight.intensity = Mathf.Lerp(previousSettings.Intensity, combatSettings.Intensity, lightCurve.Evaluate(timeCounter / duration));
			directionalLight.color = Color.Lerp(previousSettings.Col, combatSettings.Col, lightCurve.Evaluate(timeCounter / duration));

			// Update combat audio source settings based on animation curve
			combatSource.volume = Mathf.Lerp(0f, combatVolume, lightCurve.Evaluate(timeCounter / duration));

			// Update time counter
			timeCounter += Time.deltaTime;

			if(timeCounter >= duration)
			{
				// Reset time counter
				timeCounter = 0f;

				// Disable light settings state
				combatSettings.Enabled = false;
			}
		}

		if(calmSettings.Enabled)
		{
			// Update light settings based on animation curve
			directionalLight.intensity = Mathf.Lerp(previousSettings.Intensity, calmSettings.Intensity, lightCurve.Evaluate(timeCounter / duration));
			directionalLight.color = Color.Lerp(previousSettings.Col, calmSettings.Col, lightCurve.Evaluate(timeCounter / duration));

			// Update combat audio source settings based on animation curve
			combatSource.volume = Mathf.Lerp(combatVolume, 0f, lightCurve.Evaluate(timeCounter / duration));

			// Update time counter
			timeCounter += Time.deltaTime;

			if(timeCounter >= duration)
			{
				// Reset time counter
				timeCounter = 0f;

				// Disable light settings state
				calmSettings.Enabled = false;
			}
		}
	}
	#endregion

	#region Lighting Methods
	public void SetCalm()
	{
		// Disable combat setting
		combatSettings.Enabled = false;
		
		// Enable calm setting
		calmSettings.Enabled = true;

		// Reset time counter
		timeCounter = 0f;

		// Set current lighting settings to previous
		previousSettings.Intensity = directionalLight.intensity;
		previousSettings.Col = directionalLight.color;
	}

	public void SetCombat()
	{
		// Disable combat setting
		calmSettings.Enabled = false;
		
		// Enable calm setting
		combatSettings.Enabled = true;

		// Reset time counter
		timeCounter = 0f;

		// Play combat sound
		combatSource.volume = 0f;
		combatSource.Play();

		// Set current lighting settings to previous
		previousSettings.Intensity = directionalLight.intensity;
		previousSettings.Col = directionalLight.color;
	}
	#endregion

	[System.Serializable]
	public class LightSettings
	{
		#region Inspector Attributes
		[Header("Settings")]
		[SerializeField] private float intensity;
		[SerializeField] private Color color;
		#endregion

		#region Private Attributes
		private bool enabled;			// Lighting setting enabled state
		#endregion

		#region Main Methods
		public LightSettings(float intens, Color col)
		{
			// Initialize values
			intensity = intens;
			color = col;
		}
		#endregion

		#region Properties
		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}

		public float Intensity
		{
			get { return intensity; }
			set { intensity = value; }
		}

		public Color Col
		{
			get { return color; }
			set { color = value; }
		}
		#endregion
	}
}
