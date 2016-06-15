using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class InvokePilar : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Burn")]
	[SerializeField] private Vector2 burnLimits;
	[SerializeField] private AnimationCurve burnCurve;
	[SerializeField] private float burnDuration;

	[Header("Dissolve")]
	[SerializeField] private Vector2 dissolveLimits;
	[SerializeField] private AnimationCurve dissolveCurve;
	[SerializeField] private float dissolveDuration;

	[Header("UI")]
	[SerializeField] private float uiDistance;
	[SerializeField] private Transform interactRoot;
	[SerializeField] private GameObject interactPrefab;

	[Header("Player")]
	[SerializeField] private bool applyFeedback;
	[SerializeField] private int feedbackIndex;
	[SerializeField] private float feedbackDuration;

	[Header("Events")]
	[SerializeField] private float eventDelay;
	[SerializeField] private UnityEvent powerEvent;
	[SerializeField] private UnityEvent endEvent;

	[Header("Vibration")]
	[SerializeField] private bool applyVibration;
	[SerializeField] private float distanceVibration;
	[SerializeField] private float powerVibration;
	[SerializeField] private AnimationCurve powerVibrationCurve;

	[Header("References")]
	[SerializeField] private Transform trans;
	[SerializeField] private Vector3 offset;
	[SerializeField] private InterfaceManager interfaceManager;
	[SerializeField] private Collider coll;
	#endregion

	#region Private Attributes
	private int pilarState;					// Current pilar state
	private Material[] materials;			// Ally materials references
	private float timeCounter;				// Material animations time counter
	private Character playerChar;			// Player character reference
	private Transform interactUI;			// Current interact UI transform reference
	private RectTransform interactRect;		// Current interact UI rect transform reference
	private bool canInvoke;					// Current can invoke state
	private bool invokeDone;				// Current invoke done state
	private GameManager gameManager;		// Game manager reference
	private float vibrationCounter;			// Gamepad vibration time counter
	#endregion

	#region Main Methods
	public void AwakeBehaviour(Character player)
	{
		// Get references
		gameManager = GameManager.Instance;
		playerChar = player;

		// Initialize values
		canInvoke = true;
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		materials = new Material[renderers.Length];

		// Get references
		for(int i = 0; i < renderers.Length; i++) materials[i] = renderers[i].material;

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
	}

	public void UpdateBehaviour()
	{
		switch(pilarState)
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
						Vector3 viewportPoint = Camera.main.WorldToViewportPoint(trans.position + Vector3.up + offset);

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

				// Update material dissolve amount based on animation curve
				for(int i = 0; i < materials.Length; i++) materials[i].SetFloat("_BurnAmount", Mathf.Lerp(burnLimits.x, burnLimits.y, burnCurve.Evaluate(timeCounter / burnDuration)));

				// Update time counter
				timeCounter += Time.deltaTime;

				// Reset time counter value to loop animation
				if(timeCounter >= burnDuration) timeCounter = 0f;
			} break;
			case 1:
			{
				// Update material dissolve amount based on animation curve
				for(int i = 0; i < materials.Length; i++) materials[i].SetFloat("_DissolveAmount", Mathf.Lerp(dissolveLimits.x, dissolveLimits.y, dissolveCurve.Evaluate(timeCounter / dissolveDuration)));

				if(applyVibration && vibrationCounter < dissolveDuration)
				{
					// Apply vibration to gamepad based on animation curve
					gameManager.SetVibration(Mathf.Lerp(0f, powerVibration, powerVibrationCurve.Evaluate(vibrationCounter / dissolveDuration)));

					// Update vibration time counter
					vibrationCounter += Time.deltaTime;
				}

				// Update time counter
				timeCounter += Time.deltaTime;

				// Reset time counter value to loop animation
				if(timeCounter >= dissolveDuration)
				{
					// Update pilar state
					pilarState = 2;

					// Reset time counter
					timeCounter = 0f;

					// Disable game object
					gameObject.SetActive(false);
				}
			} break;
			default: break;
		}
	}
	#endregion

	#region Pilar Methods
	public void GetPower()
	{
		// Check if player didn't get the power yet
		if(pilarState == 0)
		{
			// Update pilar state
			pilarState = 1;

			// Disable interact trigger collider
			coll.enabled = false;

			// Reset time counter
			timeCounter = 0f;

			// Apply character pilar feedback
			if(applyFeedback) playerChar.Feedback.EnableEvent(feedbackIndex);

			// Disable player behaviour for animation
			playerChar.CanInteract = false;

			// Restore player character can interact state after animation
			Invoke("InteractPlayer", feedbackDuration);

			// Disable interact UI game object
			interactUI.gameObject.SetActive(false);

		#if DEBUG_BUILD
			// Trace debug message
			Debug.Log("InvokePilar: power adquired from invoke pilar");
		#endif
		}
	}

	private void InteractPlayer()
	{
		// Restore player character can interact state
		playerChar.CanInteract = true;

		// Invoke power event methods
		powerEvent.Invoke();

		// Invoke end event methods after delay
		Invoke("InvokeEvents", eventDelay);
	}

	private void InvokeEvents()
	{
		// Invoke end event
		endEvent.Invoke();
	}
	#endregion

	#region Properties
	public bool CanInvoke
	{
		get { return canInvoke; }
	}
	#endregion
}
