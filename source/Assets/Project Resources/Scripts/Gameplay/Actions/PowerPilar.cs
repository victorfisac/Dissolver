using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PowerPilar : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private Color endColor;
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float duration;

	[Header("Player")]
	[SerializeField] private bool applyFeedback;
	[SerializeField] private int playerFeedback;

	[Header("Visual")]
	[SerializeField] private MeshRenderer pilarRenderer;
	[SerializeField] private int index;
	[SerializeField] private ParticleSystem pSystem;

	[Header("Return")]
	[SerializeField] private bool canReturn;
	[SerializeField] private float delayReturn;

	[Header("Audio")]
	[SerializeField] private Vector2 audioLimits;
	[SerializeField] private AudioSource audioSource;

	[Header("UI")]
	[SerializeField] private float uiDistance;
	[SerializeField] private Transform interactRoot;
	[SerializeField] private GameObject interactPrefab;

	[Header("Events")]
	[SerializeField] private UnityEvent powerEvent;
	[SerializeField] private UnityEvent returnEvent;

	[Header("Laser")]
	[SerializeField] private LineRenderer lineRenderer;
	[SerializeField] private Transform laserEnd;

	[Header("Vibration")]
	[SerializeField] private bool applyVibration;
	[SerializeField] private float distanceVibration;
	[SerializeField] private float powerVibration;
	[SerializeField] private AnimationCurve powerVibrationCurve;

	[Header("References")]
	[SerializeField] private Transform trans;
	[SerializeField] private InterfaceManager interfaceManager;
	#endregion

	#region Private Attributes
	private Transform interactUI;			// Current interact UI transform reference
	private RectTransform interactRect;		// Current interact UI rect transform reference
	private Character playerChar;			// Player character reference
	private int state;						// Power pilar current state
	private Material brilliantMat;			// Environment brilliant material reference
	private Color shadowColor;				// Brilliant material shadow color
	private Color rimColor;					// Brilliant material rim color
	private float timeCounter;				// Animation curve time counter
	private float vibrationCounter;			// Vibration value animation time counter
	private Vector3[] linePositions;		// Line renderer positions array
	private GameManager gameManager;		// Game manager reference
	#endregion

	#region Main Methods
	public void AwakeBehaviour (Character player) 
	{
		// Get references
		gameManager = GameManager.Instance;
		playerChar = player;

		// Initialize values
		brilliantMat = pilarRenderer.materials[index];
		shadowColor = brilliantMat.GetColor("_SColor");
		rimColor = brilliantMat.GetColor("_RimColor");

		// Instantiate interact game object
		GameObject newObject = (GameObject)Instantiate(interactPrefab, Vector3.zero, Quaternion.identity);

		// Get input UI reference from new object
		InputUI newUI = newObject.GetComponent<InputUI>();

		// Add new input UI reference to interface manager inputs list
		interfaceManager.Inputs.Add(newUI);

		// Initialize new input UI reference
		newUI.AwakeBehaviour();

		// Set new interact object parent to the interact root transform
		newObject.transform.SetParent(interactRoot, false);

		// Get transform and rect transform references from new object
		interactUI = newObject.transform;
		interactRect = newObject.GetComponent<RectTransform>();
		interactRect.anchoredPosition = Vector2.zero;

		// Disable interact game object by default
		interactUI.gameObject.SetActive(false);

		if(lineRenderer)
		{
			// Initialize line renderer positions and get values
			linePositions = new Vector3[lineRenderer.transform.childCount];
			for(int i = 0; i < linePositions.Length; i++) linePositions[i] = lineRenderer.transform.GetChild(i).position;

			// Initialize line renderer
			lineRenderer.useWorldSpace = true;
			lineRenderer.SetVertexCount(linePositions.Length);
			lineRenderer.SetPositions(linePositions);
			laserEnd.position = linePositions[1];
		}
	}

	public void UpdateBehaviour () 
	{
		switch(state)
		{
			case 0:
			{
				if(interactUI)
				{
					if(Vector3.Distance(playerChar.Trans.position, trans.position) < uiDistance)
					{
						// Enable ui game object
						interactUI.gameObject.SetActive(true);

						// Calculate pilar position converted to viewport point (values between 0 and 1)
						Vector3 viewportPoint = Camera.main.WorldToViewportPoint(trans.position + Vector3.up);

						if(viewportPoint.x >= 0f && viewportPoint.x <= 1f && viewportPoint.x >= 0f && viewportPoint.y <= 1f && viewportPoint.z >= 0f)
					    {
					    	// Enable interact game object if it is hidden
							if(!interactUI.gameObject.activeSelf) interactUI.gameObject.SetActive(true);

							// Update interact position based on viewport point and screen size
							interactRect.anchoredPosition = new Vector2(viewportPoint.x * 1280f, viewportPoint.y * 720f);
						}
						else if(interactUI.gameObject.activeSelf) interactUI.gameObject.SetActive(false);

						// Apply gamepad vibration based on distance to power pilar if needed
						if(applyVibration) gameManager.SetVibration(Mathf.Lerp(0f, distanceVibration, (1f - (Vector3.Distance(playerChar.Trans.position, trans.position) / uiDistance))));
					}
					else if(interactUI.gameObject.activeSelf) interactUI.gameObject.SetActive(false);
				}
			} break;
			case 1:
			{
				// Update material emission based on animation curve
				brilliantMat.SetColor("_SColor", Color.Lerp(shadowColor, endColor, curve.Evaluate(timeCounter / duration)));
				brilliantMat.SetColor("_RimColor", Color.Lerp(rimColor, endColor, curve.Evaluate(timeCounter / duration)));

				// Update audio source volume based on time
				audioSource.volume = Mathf.Lerp(audioLimits.y, audioLimits.x, timeCounter / duration);

				// Update line positions based on interpolation
				if(lineRenderer)
				{
					lineRenderer.SetPosition(1, Vector3.Lerp(linePositions[1], linePositions[0], (timeCounter / duration)));
					laserEnd.position = Vector3.Lerp(linePositions[1], linePositions[0], (timeCounter / duration));
				}

				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= duration) DisablePower();
			} break;
			case 2:
			{
				if(canReturn)
				{
					// Update line positions based on interpolation
					if(lineRenderer)
					{
						lineRenderer.SetPosition(1, Vector3.Lerp(linePositions[0], Vector3.Lerp(linePositions[0], linePositions[1], 0.5f), (timeCounter / delayReturn)));
						laserEnd.position = Vector3.Lerp(linePositions[0], Vector3.Lerp(linePositions[0], linePositions[1], 0.5f), (timeCounter / delayReturn));
					}

					// Update time counter
					timeCounter += Time.deltaTime;

					if(timeCounter >= delayReturn)
					{
						// Reset time counter
						timeCounter = 0f;

						// Update power pilar state
						state = 3;
					}
				}

				if(applyVibration && vibrationCounter < duration)
				{
					// Apply vibration to gamepad based on animation curve
					gameManager.SetVibration(Mathf.Lerp(0f, powerVibration, powerVibrationCurve.Evaluate(vibrationCounter / duration)));

					// Update vibration time counter
					vibrationCounter += Time.deltaTime;
				}
			} break;
			case 3:
			{
				// Update line positions based on interpolation
				if(lineRenderer)
				{
					lineRenderer.SetPosition(1, Vector3.Lerp(Vector3.Lerp(linePositions[0], linePositions[1], 0.5f), linePositions[1], (timeCounter / duration)));
					laserEnd.position = Vector3.Lerp(Vector3.Lerp(linePositions[0], linePositions[1], 0.5f), linePositions[1], (timeCounter / duration));
				}

				// Update material emission based on animation curve
				brilliantMat.SetColor("_SColor", Color.Lerp(shadowColor, endColor, curve.Evaluate(timeCounter / duration)));
				brilliantMat.SetColor("_RimColor", Color.Lerp(rimColor, endColor, curve.Evaluate(timeCounter / duration)));

				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= duration)
				{
					// Fix final color value
					brilliantMat.SetColor("_SColor", shadowColor);
					brilliantMat.SetColor("_RimColor", rimColor);

					// Reset power pilar state
					state = 0;

					// Reset time counter
					timeCounter = 0f;

					// Enable particles effects game object
					trans.GetChild(0).gameObject.SetActive(true);

					// Invoke return power pilar methods
					returnEvent.Invoke();
				}
			} break;
			default: break;
		}
	}
	#endregion

	#region Pilar Methods
	public void DisablePower()
	{
		// Fix final color value
		brilliantMat.SetColor("_SColor", endColor);
		brilliantMat.SetColor("_RimColor", endColor);

		// Disable power particles game object
		trans.GetChild(0).gameObject.SetActive(false);

		// Disable audio source
		audioSource.Stop();
		audioSource.enabled = false;

		// Reset vibration time counter
		vibrationCounter = 0f;

		// Update power pilar state
		state = 2;

		// Reset time counter
		timeCounter = 0f;
	}

	public void DisablePilar()
	{
		if(state == 0)
		{
			// Update power pilar state
			state = 1;

			// Apply character pilar feedback
			if(applyFeedback) playerChar.Feedback.EnableEvent(playerFeedback);

			// Disable player behaviour for animation
			playerChar.CanInteract = false;

			// Disable particle system
			pSystem.Stop();

			// Reset vibration time counter
			vibrationCounter = 0f;

			// Restore player character can interact state after animation
			Invoke("InteractPlayer", duration);

			// Disable interact UI game object
			interactUI.gameObject.SetActive(false);
		}
	}

	private void InteractPlayer()
	{
		// Restore player character can interact state
		playerChar.CanInteract = true;

		// Invoke end power event
		powerEvent.Invoke();
	}
	#endregion

	#region Properties
	public bool CanDisable
	{
		get { return (state == 0); }
	}
	#endregion
}
