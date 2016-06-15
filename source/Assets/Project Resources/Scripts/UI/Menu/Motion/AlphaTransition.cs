using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AlphaTransition : MonoBehaviour 
{
	#region Enums
	public enum Axis { X, Y, Z, XY, XZ, YZ, XYZ };
	#endregion

	#region Inspector Attributes
	[Header("Motion")]
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float duration;
	#endregion

	#region Private Attributes
	private float counter;			// Interpolation time counter
	private Color auxColor;			// Transition color
	private bool isReverse;			// Playing reverse state
	private bool canPlay;			// Can play state
	private Text[] labels;			// Labels component references
	private Image[] images;			// Images component references
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Initialize values
		labels = new Text[0];
		images = new Image[0];

		Start();
	}

	private void Start()
	{
		// Get references
		labels = GetComponentsInChildren<Text>();
		images = GetComponentsInChildren<Image>();

		for(int i = 0; i < labels.Length; i++)
		{
			auxColor = labels[i].color;
			auxColor.a = 0f;
			labels[i].color = auxColor;
		}

		for(int i = 0; i < images.Length; i++)
		{
			auxColor = images[i].color;
			auxColor.a = 0f;
			images[i].color = auxColor;
		}
	}

	public void UpdateBehaviour ()
	{
		if(canPlay)
		{
			for(int i = 0; i < labels.Length; i++)
			{
				auxColor = labels[i].color;
				auxColor.a = curve.Evaluate((isReverse ? (duration - counter) : counter) / duration);
				labels[i].color = auxColor;
			}

			for(int i = 0; i < images.Length; i++)
			{
				auxColor = images[i].color;
				auxColor.a = curve.Evaluate((isReverse ? (duration - counter) : counter) / duration);
				images[i].color = auxColor;
			}

			// Update time counter
			if(counter < duration) counter += Time.deltaTime;
			else counter = duration;
		}
	}
	#endregion

	#region Sets Methods
	public void Play(bool reverse)
	{
		// Reset counter state
		counter = 0f;

		// Update can play state
		canPlay = true;

		// Update reversed state
		isReverse = reverse;
	}
	#endregion
}
