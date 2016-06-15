using UnityEngine;
using System.Collections;

public class PositionTransition : MonoBehaviour 
{
	#region Enums
	public enum Axis { X, Y, Z, XY, XZ, YZ, XYZ };
	#endregion

	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private Axis axis;
	[SerializeField] private float delayRange;

	[Header("Motion")]
	[SerializeField] private Vector3 startPosition;
	[SerializeField] private Vector3 finalPosition;
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float duration;

	[Header("References")]
	[SerializeField] private RectTransform[] buttons;
	#endregion

	#region Private Attributes
	private float counter;			// Interpolation time counter
	private Vector3 auxVector;		// Current button scale vector
	private bool isReverse;			// Playing reverse state
	private bool canPlay;			// Can play state
	#endregion

	#region Main Methods
	public void UpdateBehaviour () 
	{
		if(canPlay)
		{
			// Update buttons position based on curve interpolation (every button has its own delay range)
			for(int i = 0; i < buttons.Length; i++)
			{
				auxVector = buttons[i].localPosition;

				switch(axis)
				{
					case Axis.X: auxVector.x = Mathf.Lerp(startPosition.x, finalPosition.x, curve.Evaluate((isReverse ? ((duration + i * delayRange) - counter) : counter) / (duration + i * delayRange))); break;
					case Axis.Y: auxVector.y = Mathf.Lerp(startPosition.y, finalPosition.y, curve.Evaluate((isReverse ? ((duration + i * delayRange) - counter) : counter) / (duration + i * delayRange))); break;
					case Axis.Z: auxVector.z = Mathf.Lerp(startPosition.z, finalPosition.z, curve.Evaluate((isReverse ? ((duration + i * delayRange) - counter) : counter) / (duration + i * delayRange))); break;
					case Axis.XY:
					{
						auxVector.x = Mathf.Lerp(startPosition.x, finalPosition.x, curve.Evaluate((isReverse ? ((duration + i * delayRange) - counter) : counter) / (duration + i * delayRange)));
						auxVector.y = Mathf.Lerp(startPosition.y, finalPosition.y, curve.Evaluate((isReverse ? ((duration + i * delayRange) - counter) : counter) / (duration + i * delayRange)));
						break;
					}
					case Axis.XZ:
					{
						auxVector.x = Mathf.Lerp(startPosition.x, finalPosition.x, curve.Evaluate((isReverse ? ((duration + i * delayRange) - counter) : counter) / (duration + i * delayRange)));
						auxVector.z = Mathf.Lerp(startPosition.z, finalPosition.z, curve.Evaluate((isReverse ? ((duration + i * delayRange) - counter) : counter) / (duration + i * delayRange)));
						break;
					}
					case Axis.YZ:
					{
						auxVector.y = Mathf.Lerp(startPosition.y, finalPosition.y, curve.Evaluate((isReverse ? ((duration + i * delayRange) - counter) : counter) / (duration + i * delayRange)));
						auxVector.z = Mathf.Lerp(startPosition.z, finalPosition.z, curve.Evaluate((isReverse ? ((duration + i * delayRange) - counter) : counter) / (duration + i * delayRange)));
						break;
					}
					case Axis.XYZ: auxVector = Vector3.Lerp(startPosition, finalPosition, curve.Evaluate((isReverse ? ((duration + i * delayRange) - counter) : counter) / (duration + i * delayRange))); break;
					default: break;
				}

				buttons[i].localPosition = auxVector;
			}

			// Update time counter
			if(counter < (duration + buttons.Length * delayRange)) counter += Time.deltaTime;
			else counter = duration + buttons.Length * delayRange;
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

	#region Properties
	public float Duration
	{
		get { return duration; }
	}

	public float MaxDelay
	{
		get { return delayRange; }
	}
	#endregion
}
