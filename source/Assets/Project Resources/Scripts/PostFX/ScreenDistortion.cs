using UnityEngine;
using System.Collections;

public class ScreenDistortion : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Attributes")]
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float duration;

	[SerializeField] private float[] amplitudes;

	[Header("References")]
	[SerializeField] private Material mat;
	#endregion

	#region Private Attributes
	private float radius;		// Screen wave radius
	private bool canWork;		// Behaviour can work state
	private float counter;		// Distortion animation counter
	#endregion

	#region Main Methods
	public void AwakeDistortion()
	{
		// Initialize values
		radius = -1;
		mat.SetFloat("_Radius", radius);
	}

	public void UpdateDistortion()
	{
		if(canWork)
		{
			// Calculate radius value based on animation curve
			radius = curve.Evaluate(counter / duration);

			// Send radius to shader
			mat.SetFloat("_Radius", radius);

			// Update time counter
			counter += Time.deltaTime;

			if(counter >= duration) canWork = false;
		}
	}
	#endregion

	#region Effect Methods
	private void OnRenderImage(RenderTexture src, RenderTexture dest) 
	{
		Graphics.Blit(src, dest, mat);
	}

	public void StartDistortion(Vector2 center, bool soft) 
	{
		// Send screen position to shader
		mat.SetFloat("_CenterX", (center.x + Screen.width / 2) / Screen.width);
		mat.SetFloat("_CenterY", (center.y + Screen.height / 2) / Screen.height);

		// Send amplitude to shader
		mat.SetFloat("_Amplitude", soft ? amplitudes[0] : amplitudes[1]);

		// Reset radius value
		radius = 0f;

		// Update can work state
		canWork = true;

		// Reset time counter
		counter = 0f;
	}
	#endregion
}