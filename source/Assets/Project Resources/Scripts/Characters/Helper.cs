using UnityEngine;
using System.Collections;

public class Helper : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private string cameraChild;
	[SerializeField] private Vector3 offset;

	[Header("Motion")]
	[SerializeField] private float smoothness;

	[Header("Scale")]
	[SerializeField] private ScaleType startCurve;
	[SerializeField] private ScaleType loopCurve;

	[Header("Materials")]
	[SerializeField] private Color rimColor;
	[SerializeField] private AnimationCurve rimCurve;
	[SerializeField] private float rimDuration;
	#endregion

	#region Private Attributes
	// Motion
	private Transform target;				// Helper target reference
	private Vector3 targetPosition;			// Calculated target position to move
	private Transform cameraTrans;			// Main camera transform reference

	// Scale
	private float scaleCounter;				// Scale animation time counter
	private int scaleState;					// Scale animations current state
	private Vector3 initScale;				// Transform scale at start

	// Materials
	private float timeCounter;				// Materials animation time counter
	private MeshRenderer[] renderers;		// Mesh renderers references
	private Material[] mats;				// Mesh renderers materials references
	private Color rimInit;					// Material rim color at start

	// Behaviour
	private bool needFadeOut;				// Temporal fade in state
	private float fadeOutCounter;			// Fade out delay timer
	private float fadeOutDuration;			// Fade out delay amount
	#endregion

	#region Main Methods
	public void AwakeBehaviour (Transform newTarget) 
	{
		// Get references
		cameraTrans = Camera.main.transform;
		renderers = GetComponentsInChildren<MeshRenderer>();
		target = newTarget.FindChild(cameraChild);

		// Initialize values
		initScale = transform.localScale;
		transform.localScale = Vector3.zero;
		gameObject.SetActive(false);

		mats = new Material[renderers.Length];
		for(int i = 0; i < mats.Length; i++) mats[i] = renderers[i].material;
		if (mats.Length > 0) rimInit = mats[0].GetColor("_RimColor");
	}

	public void UpdateBehaviour () 
	{
		// Update helper position based on player position and target with smooth movement
		transform.position = Vector3.Lerp(transform.position, target.position + cameraTrans.TransformDirection(Vector3.Scale(offset, new Vector3(1, 0, 1))) + Vector3.up * offset.y, Time.deltaTime * smoothness);

		// Update materials rim color based on animation curve
		for(int i = 0; i < mats.Length; i++) mats[i].SetColor("_RimColor", Color.Lerp(rimInit, rimColor, rimCurve.Evaluate(timeCounter / rimDuration)));

		// Update time counter
		timeCounter += Time.deltaTime;

		// Reset time counter to make loop
		if(timeCounter >= rimDuration) timeCounter = 0f;

		switch(scaleState)
		{
			case 1:
			{
				// Update transform scale based on animation curve
				transform.localScale = Vector3.Lerp(Vector3.zero, initScale, startCurve.Curve.Evaluate(scaleCounter / startCurve.Duration));

				// Update scale time counter
				scaleCounter += Time.deltaTime;

				if(scaleCounter >= startCurve.Duration)
				{
					// Fix final local scale
					transform.localScale = initScale;

					// Reset scale time counter
					scaleCounter = 0f;

					// Update scale state
					scaleState = 2;
				}
			} break;
			case 2:
			{
				// Update transform scale based on animation curve
				transform.localScale = initScale * loopCurve.Curve.Evaluate(scaleCounter / loopCurve.Duration);

				// Update scale time counter
				scaleCounter += Time.deltaTime;

				// Reset scale time counter to make loop
				if(scaleCounter >= loopCurve.Duration) scaleCounter = 0f;

				// Check if helper needs to do a fade out
				if(needFadeOut)
				{
					// Update fade out time counter
					fadeOutCounter += Time.deltaTime;

					if(fadeOutCounter >= fadeOutDuration)
					{
						// Reset fade out counter
						fadeOutCounter = 0f;

						// Disable need fade out state
						needFadeOut = false;

						// Reset fade out duration
						fadeOutDuration = 0f;

						// Set fade out logic
						HelperFadeOut();
					}
				}
			} break;
			case 3:
			{
				// Update transform scale based on animation curve
				transform.localScale = Vector3.Lerp(initScale, Vector3.zero, startCurve.Curve.Evaluate(scaleCounter / startCurve.Duration));

				// Update scale time counter
				scaleCounter += Time.deltaTime;

				if(scaleCounter >= startCurve.Duration)
				{
					// Fix final local scale
					transform.localScale = Vector3.zero;

					// Reset scale time counter
					scaleCounter = 0f;

					// Update scale state
					scaleState = 0;

					// Disable helper game object
					gameObject.SetActive(false);
				}
			} break;
			default: break;
		}
	}
	#endregion

	#region Helper Methods
	public void HelperFadeIn()
	{
		// Enable helper game object
		gameObject.SetActive(true);

		// Update scale state
		scaleState = 1;

		// Reset scale time counter
		scaleCounter = 0f;
	}

	public void HelperFadeIn(float delayAmount)
	{
		if(scaleState != 2)
		{
			// Enable helper game object
			gameObject.SetActive(true);

			// Update scale state
			scaleState = 1;

			// Reset scale time counter
			scaleCounter = 0f;

			// Update need fade out state
			needFadeOut = true;

			// Reset fade out time counter
			fadeOutCounter = 0f;

			// Update fade out delay amount
			fadeOutDuration = delayAmount;
		}
		else fadeOutDuration += delayAmount;
	}

	public void HelperFadeOut()
	{
		// Update scale state
		scaleState = 3;

		// Reset scale time counter
		scaleCounter = 0f;
	}
	#endregion

	#region Serializable
	[System.Serializable]
	public class ScaleType
	{
		#region Inspector Attributes
		[Header("Animation")]
		[SerializeField] private AnimationCurve curve;
		[SerializeField] private float duration;
		#endregion

		#region Properties
		public AnimationCurve Curve
		{
			get { return curve; }
		}

		public float Duration
		{
			get { return duration; }
		}
		#endregion
	}
	#endregion
}
