using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LogoUI : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private float wooshDelay;
	[SerializeField] private float titleDelay;
	[SerializeField] private float explosionDelay;
	[SerializeField] private float dissolveDelay;
	[SerializeField] private float endDelay;

	[Header("Dissolve")]
	[SerializeField] private Vector2 dissolveLimits;
	[SerializeField] private AnimationCurve dissolveCurve;
	[SerializeField] private float dissolveDuration;

	[Header("Audio")]
	[SerializeField] private AudioSource wooshSource;
	[SerializeField] private AudioSource distortionSource;

	[Header("Distortion")]
	[SerializeField] private ScreenDistortion distortion;

	[Header("UI")]
	[SerializeField] private RectTransform uiTrans;
	[SerializeField] private AnimationCurve uiCurve;
	[SerializeField] private float uiDuration;
	[SerializeField] private float distance;

	[Header("References")]
	[SerializeField] private Transform animationRoot;
	#endregion

	#region Private Attributes
	private int state;				// Current logo animation state
	private float timeCounter;		// Logo animation time counter
	private Material[] mats;		// Dissolve animation materials references
	private bool wooshPlayed;		// Woosh sound played state
	private bool explosionPlayed;	// Screen explosion played state

	// UI
	private Vector2 initPosition;	// Webpage text position at start
	private Text uiText;			// Webpage text reference
	private bool uiPlayed;			// UI animation played state
	private float uiCounter;		// UI animation time counter
	private Color startColor;		// UI text animation color at start
	private Color endColor;			// UI text animation end color
	#endregion

	#region Main Methods
	private void Awake () 
	{
		// Get references
		uiText = uiTrans.GetComponent<Text>();

		// Find all mesh renderers in animation transform childs
		MeshRenderer[] renderers = animationRoot.GetComponentsInChildren<MeshRenderer>();

		// Initialize values
		mats = new Material[renderers.Length];
		for(int i = 0; i < mats.Length; i++) mats[i] = renderers[i].material;

		// Initialize UI values
		initPosition = uiTrans.anchoredPosition;
		uiTrans.anchoredPosition = initPosition + Vector2.down * distance;
		endColor = uiText.color;
		startColor = endColor;
		startColor.a = 0f;
		uiText.color = startColor;

		// Disable and block cursor
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		// Apply frame rate limitation (v-sync)
		Application.targetFrameRate = 60;
	}

	private void Update () 
	{
		// Update distortion behaviour
		distortion.UpdateDistortion();

		if(uiPlayed)
		{
			// Update UI position based on animation curve
			uiTrans.anchoredPosition = Vector2.Lerp(initPosition + Vector2.down * distance, initPosition, uiCurve.Evaluate(uiCounter / uiDuration));

			// Update UI color based on animation curve
			uiText.color = Color.Lerp(startColor, endColor, uiCurve.Evaluate(uiCounter / uiDuration));

			// Update UI animation time counter
			uiCounter += Time.deltaTime;

			if(uiCounter >= uiDuration)
			{
				// Reset UI animation time counter
				uiCounter = 0f;

				// Disable UI animation
				uiPlayed = false;

				// Fix UI position end value
				uiTrans.anchoredPosition = initPosition;
			}
		}

		switch(state)
		{
			case 0:
			{
				if(!wooshPlayed)
				{
					if(timeCounter >= wooshDelay)
					{
						// Play woosh sound
						if(wooshSource) wooshSource.Play();

						// Update woosh played state
						wooshPlayed = true;
					}
				}

				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= dissolveDelay)
				{
					// Reset time counter
					timeCounter = 0f;

					// Update state
					state = 1;
				}
			} break;
			case 1:
			{
				// Update time counter
				timeCounter += Time.deltaTime;

				if(!explosionPlayed)
				{
					if(timeCounter >= explosionDelay)
					{
						// Update explosion played state
						explosionPlayed = true;

						// Play distortion source
						if(distortionSource) distortionSource.Play();

						// Apply screen distortion
						distortion.StartDistortion(new Vector2(0.5f, 0.5f), true);

						// Update UI played state
						uiPlayed = true;
					}
				}

				if(timeCounter >= titleDelay)
				{
					// Update state
					state = 2;

					// Reset time counter
					timeCounter = 0f;
				}
			} break;
			case 2:
			{
				// Update all materials dissolve amount based on animation curve
				for(int i = 0; i < mats.Length; i++) mats[i].SetFloat("_DissolveAmount", Mathf.Lerp(dissolveLimits.x, dissolveLimits.y, dissolveCurve.Evaluate(timeCounter / dissolveDuration)));

				// Update UI animation color based on animation curve
				uiText.color = Color.Lerp(endColor, startColor, dissolveCurve.Evaluate(timeCounter / dissolveDuration));

				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= dissolveDuration)
				{
					// Reset time counter
					timeCounter = 0f;

					// Fix all materials dissolve amount final value
					for(int i = 0; i < mats.Length; i++) mats[i].SetFloat("_DissolveAmount", dissolveLimits.y);
					uiText.color = startColor;

					// Update state
					state = 3;
				}
			} break;
			case 3:
			{
				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= endDelay)
				{
					// Reset time counter
					timeCounter = 0f;

					// Update state
					state = 4;

					// Load main menu scene
					SceneManager.LoadScene("menu");
				}
			} break;
			default: break;
		}
	}
	#endregion
}
