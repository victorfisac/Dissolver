using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;
using UnityStandardAssets.CinematicEffects;
using UnityEngine.EventSystems;

public class MenuUI : MonoBehaviour 
{
	#region Enums
	public enum MenuStates { MAIN, SETTINGS, EXIT };
	#endregion

	#region Inspector Attributes
	[Header("Camera")]
	[SerializeField] private bool updateCameraMenu;
	[SerializeField] private CameraMenu cameraMenu;

	[Header("Title")]
	[SerializeField] private AlphaTransition titleAlpha;
	[SerializeField] private float titleDelayDuration;
	[SerializeField] private AnimationCurve titleCurve;
	[SerializeField] private float titleDuration;
	[SerializeField] private Color titleInitColor;
	[SerializeField] private Color titleFinalColor;

	[Header("Motion")]
	[SerializeField] private Renderer bullet;
	[SerializeField] private AnimationCurve transCurve;
	[SerializeField] private string shaderAttribute;
	[SerializeField] private float transDuration;

	[Header("Settings")]
	[SerializeField] private Dropdown resolution;
	[SerializeField] private Toggle windowed;
	[SerializeField] private Dropdown quality;
	[SerializeField] private Dropdown effects;
	[SerializeField] private Slider gamma;
	[SerializeField] private Slider volume;
	[SerializeField] private Slider music;
	[SerializeField] private Slider sensitivity;

	[Header("Behaviours Settings")]
	[SerializeField] private bool updateAlphaTransitions;
	[SerializeField] private bool updatePositionTransitions;
	[SerializeField] private bool updateButtonScales;
	[SerializeField] private bool updateMenuBehaviours;
	[SerializeField] private bool updateFades;
	[SerializeField] private bool updateLightLogics;

	[Header("Behaviours")]
	[SerializeField] private List<AlphaTransition> alphaTransitions;
	[SerializeField] private List<PositionTransition> positionTransitions;
	[SerializeField] private List<ButtonScale> buttonScales;
	[SerializeField] private List<MenuBehaviour> menuBehaviours;
	[SerializeField] private List<Fade> fades;

	[Header("Flow")]
	[SerializeField] private Button backButton;

	[Header("Audio")]
	[SerializeField] private AudioMixer audioMixer;
	[SerializeField] private AudioSource positiveSource;
	[SerializeField] private AudioSource negativeSource;
	[SerializeField] private AudioSource buttonOverSource;

	[Header("References")]
	[SerializeField] private Fade fade;
	[SerializeField] private GameObject mainMenu;
	[SerializeField] private Camera[] gameCameras;
	[SerializeField] private Button continueButton;
	[SerializeField] private Button newButton;
	#endregion

	#region Private Attributes
	// Flow
	private MenuStates state;					// Current menu flow state

	// Title
	private float titleDelayCounter;			// Title fade int delay counter
	private float titleCounter;					// Title animation time counter

	// Transformation
	private float transCounter;					// Transformation animation time counter

	// Audio
	private bool fadeDone;						// Audio fade in done state

	// Managers
	private GameManager gameManager;			// Game manager reference

	// Cameras
	private AntiAliasing[] aas;					// Game cameras antialiasing references
	private Bloom[] blooms;						// Game cameras bloom references
	private DepthOfField[] depthOfField;		// Game cameras depth of field references
	private LensAberrations[] lens;				// Game cameras lens aberration references
	private TonemappingColorGrading[] tones;	// Game cameras tone mapping references
	private ScreenDistortion[] distortions;		// Game cameras screen distortion references
	private List<LightLogic> lights;			// Light logics searched references
	private bool isDemo;						// Menu demo state
	#endregion

	#region Main Methods
	private void Start()
	{
		// Get references
		gameManager = GameManager.Instance;

		// Initialize values
		AudioListener.volume = 0f;
		aas = new AntiAliasing[gameCameras.Length];
		blooms = new Bloom[gameCameras.Length];
		depthOfField = new DepthOfField[gameCameras.Length];
		lens = new LensAberrations[gameCameras.Length];
		tones = new TonemappingColorGrading[gameCameras.Length];
		distortions = new ScreenDistortion[gameCameras.Length];

		// Get effects references
		for(int i = 0; i < aas.Length; i++) aas[i] = gameCameras[i].GetComponent<AntiAliasing>();
		for(int i = 0; i < blooms.Length; i++) blooms[i] = gameCameras[i].GetComponent<Bloom>();
		for(int i = 0; i < depthOfField.Length; i++) depthOfField[i] = gameCameras[i].GetComponent<DepthOfField>();
		for(int i = 0; i < lens.Length; i++) lens[i] = gameCameras[i].GetComponent<LensAberrations>();
		for(int i = 0; i < tones.Length; i++) tones[i] = gameCameras[i].GetComponent<TonemappingColorGrading>();
		for(int i = 0; i < distortions.Length; i++) distortions[i] = gameCameras[i].GetComponent<ScreenDistortion>();

		if(updateLightLogics)
		{
			// Find all meshes mergers references
			LightLogic[] tempLights = FindObjectsOfType<LightLogic>();
			lights = new List<LightLogic>();
			for(int i = 0; i < tempLights.Length; i++) lights.Add(tempLights[i]);
		}

		// Disable positive source to set initial settings values
		positiveSource.enabled = false;

		// Initialize ui values based on game manager loaded data
		resolution.value = gameManager.Resolution;
		windowed.isOn = gameManager.FullScreen;
		quality.value = gameManager.Quality;
		effects.value = gameManager.Effects;
		gamma.value = gameManager.Gamma;
		volume.value = gameManager.Volume;
		music.value = gameManager.Music;
		sensitivity.value = gameManager.Sensitivity;

		// Initialize effects in cameras
		SetEffects(gameManager.Effects);

		// Disable continue button if needed
		if(gameManager.GameProgress == 0 && gameManager.GameLevel == 1) continueButton.interactable = false;

		// Enable fade in
		fade.SetFadeIn();

		// Awake camera menu animation behaviour
		if(updateCameraMenu) cameraMenu.AwakeBehaviour();

		// Awake all button scale behaviours
		if(updateButtonScales) for(int i = 0; i < buttonScales.Count; i++) buttonScales[i].AwakeBehaviour(buttonOverSource);

		// Awake all alpha transition behaviours
		if(updateAlphaTransitions) for(int i = 0; i < alphaTransitions.Count; i++) alphaTransitions[i].AwakeBehaviour();

		// Awake all lights logic behaviours
		if(updateLightLogics) for(int i = 0; i < lights.Count; i++) lights[i].AwakeBehaviour(Camera.main.transform);

		// Enable main menu after a delay
		Invoke("ShowMenu", cameraMenu.Duration + titleDelayDuration);
	}

	private void Update() 
	{
		// Audio listener fade in
		if(!fadeDone)
		{
			if(AudioListener.volume < 1f) AudioListener.volume += Time.deltaTime;
			else fadeDone = true;
		}

		// Check all flow inputs
		CheckInputs();

		// Update camera menu animation behaviour
		if(updateCameraMenu) cameraMenu.UpdateBehaviour();

		// Update menu behaviours
		if(updateButtonScales) for(int i = 0; i < buttonScales.Count; i++) buttonScales[i].UpdateBehaviour();

		// Update all alpha transition behaviours
		if(updateAlphaTransitions) for(int i = 0; i < alphaTransitions.Count; i++) alphaTransitions[i].UpdateBehaviour();

		// Update all alpha transition behaviours
		if(updatePositionTransitions) for(int i = 0; i < positionTransitions.Count; i++) positionTransitions[i].UpdateBehaviour();

		// Update buttons scale behaviours
		if(updateButtonScales) for(int i = 0; i < buttonScales.Count; i++) buttonScales[i].UpdateBehaviour();

		// Update menu behaviours
		if(updateMenuBehaviours) for(int i = 0; i < menuBehaviours.Count; i++) menuBehaviours[i].UpdateBehaviour();

		// Update fades behaviours
		if(updateFades) for(int i = 0; i < fades.Count; i++) fades[i].UpdateBehaviour();

		// Update all lights logic behaviours
		if(updateLightLogics) for(int i = 0; i < lights.Count; i++) lights[i].UpdateBehaviour();
	}
	#endregion

	#region Menu Methods
	private void CheckInputs()
	{
		// Check if back button is pressed
		if(Input.GetButtonDown("Escape") || Input.GetButtonDown("Cancel"))
		{
			switch(state)
			{
				case MenuStates.MAIN: SetQuit(); break;
				case MenuStates.SETTINGS: SetMenu(); break;
				default: break;
			}
		}
	}

	private void ShowMenu()
	{
		mainMenu.SetActive(true);
		titleAlpha.Play(false);

		// Enable positive
		positiveSource.enabled = true;
		negativeSource.enabled = true;
		buttonOverSource.enabled = true;

		// Make cursor visible
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void SetMenu()
	{
		// Update current menu state
		state = MenuStates.MAIN;

		// Update event system current selected object
		EventSystem.current.SetSelectedGameObject((gameManager.GameProgress > 0) ? continueButton.gameObject : newButton.gameObject);

		// Make cursor visible
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		// Invoke all back button on click methods
		backButton.onClick.Invoke();
	}

	public void SetSettings()
	{
		// Update current menu state
		state = MenuStates.SETTINGS;
	}

	public void SetNew()
	{
		// Set fade out
		fade.SetFadeOut();

		gameManager.ResetProgress();

		// Make cursor invisible and lock it
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		Invoke("GoIntro", 1f / fade.Speed);
	}

	public void SetContinue()
	{
		// Set fade out
		fade.SetFadeOut();

		// Make cursor invisible and lock it
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		Invoke("ChangeScene", 1f / fade.Speed);
	}

	public void SetDemo()
	{
		// Set fade out
		fade.SetFadeOut();

		// Update demo state
		isDemo = true;

		// Make cursor invisible and lock it
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		Invoke("ChangeScene", 1f / fade.Speed);
	}

	public void SetDeleteData()
	{
		gameManager.ResetProgress();
		continueButton.interactable = false;
	}

	public void SetQuit()
	{
		// Update menu state
		state = MenuStates.EXIT;

		// Set fade out
		fade.SetFadeOut();

		// Make cursor invisible and lock it
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		Invoke("GoQuit", 1f / fade.Speed);
	}
	#endregion

	#region Invoked Methods
	private void ChangeScene()
	{
		// Load demo scene
		if(!isDemo) SceneManager.LoadScene("level_" + gameManager.GameLevel.ToString("00"));
		else SceneManager.LoadScene("credits");
	}

	private void GoIntro()
	{
		// Load intro scene
		SceneManager.LoadScene("intro");
	}

	private void GoQuit()
	{
		// Quit game
		Application.Quit();
	}
	#endregion

	#region Settings Methods
	public void SetResolution(int mode)
	{
		// Update screen resolution
		switch(mode)
		{
			case 0: Screen.SetResolution(800, 600, Screen.fullScreen); break;
			case 1: Screen.SetResolution(1024, 768, Screen.fullScreen); break;
			case 2: Screen.SetResolution(1280, 720, Screen.fullScreen); break;
			case 3: Screen.SetResolution(1280, 1024, Screen.fullScreen); break;
			case 4: Screen.SetResolution(1366, 768, Screen.fullScreen); break;
			case 5: Screen.SetResolution(1440, 900, Screen.fullScreen); break;
			case 6: Screen.SetResolution(1680, 1050, Screen.fullScreen); break;
			case 7: Screen.SetResolution(1920, 1080, Screen.fullScreen); break;
			default: break;
		}

		// Update game manager resolution
		gameManager.Resolution = mode;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("MenuUI: screen resolution switched to " + Screen.width + " x " + Screen.height);
	#endif
	}

	public void SetWindowed(bool state)
	{
		// Update full screen mode
		Screen.fullScreen = state;

		// Update game manager windowed state
		gameManager.FullScreen = state;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("MenuUI: window full screen mode set to " + state.ToString());
	#endif
	}

	public void SetQuality(int quality)
	{
		// Update quality settings level
		QualitySettings.SetQualityLevel(quality);

		// Update game manager quality
		gameManager.Quality = quality;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("MenuUI: quality settings switched to " + QualitySettings.names[quality]);
	#endif
	}

	public void SetEffects(int value)
	{
		// Apply to all cameras
		switch(value)
		{
			case 0:	// Low settings
			{
				for(int i = 0; i < aas.Length; i++)	aas[i].enabled = false;
				for(int i = 0; i < blooms.Length; i++) blooms[i].enabled = false;
				for(int i = 0; i < depthOfField.Length; i++) depthOfField[i].enabled = false;
				for(int i = 0; i < lens.Length; i++) lens[i].enabled = false;
				for(int i = 0; i < tones.Length; i++) tones[i].enabled = false;
				for(int i = 0; i < distortions.Length; i++)	distortions[i].enabled = false;
				break;
			}
			case 1:	// Normal settings
			{
				for(int i = 0; i < aas.Length; i++)	aas[i].enabled = true;
				for(int i = 0; i < blooms.Length; i++) blooms[i].enabled = true;
				for(int i = 0; i < distortions.Length; i++)	distortions[i].enabled = true;

				for(int i = 0; i < depthOfField.Length; i++) depthOfField[i].enabled = false;
				for(int i = 0; i < lens.Length; i++) lens[i].enabled = false;
				for(int i = 0; i < tones.Length; i++) tones[i].enabled = false;
				break;
			}
			case 2:	// High settings
			{
				for(int i = 0; i < aas.Length; i++)	aas[i].enabled = true;
				for(int i = 0; i < blooms.Length; i++) blooms[i].enabled = true;
				for(int i = 0; i < distortions.Length; i++)	distortions[i].enabled = true;
				for(int i = 0; i < depthOfField.Length; i++) depthOfField[i].enabled = true;

				for(int i = 0; i < lens.Length; i++) lens[i].enabled = false;
				for(int i = 0; i < tones.Length; i++) tones[i].enabled = false;
				break;
			}
			case 3: // Ultra settings
			{
				for(int i = 0; i < aas.Length; i++)	aas[i].enabled = true;
				for(int i = 0; i < blooms.Length; i++) blooms[i].enabled = true;
				for(int i = 0; i < distortions.Length; i++)	distortions[i].enabled = true;
				for(int i = 0; i < depthOfField.Length; i++) depthOfField[i].enabled = true;
				for(int i = 0; i < lens.Length; i++) lens[i].enabled = true;
				for(int i = 0; i < tones.Length; i++) tones[i].enabled = true;
				break;
			}
			default: break;
		}

		// Update game manager effects
		gameManager.Effects = value;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("MenuUI: effects settings switched to " + value);
	#endif
	}

	public void SetGamma(float value)
	{
		// Update quality settings level
		for(int i = 0; i < tones.Length; i++)
		{
			TonemappingColorGrading.TonemappingSettings auxToneMapping = tones[i].tonemapping;
			auxToneMapping.exposure = value;
			tones[i].tonemapping = auxToneMapping;
		}

		// Update game manager gamma
		gameManager.Gamma = value;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("MenuUI: gamma settings switched to " + value);
	#endif
	}

	public void SetVolume(float value)
	{
		// Update audio mixer master volume
		audioMixer.SetFloat("MasterVolume", value);

		// Update game manager quality
		gameManager.Volume = value;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("MenuUI: master volume switched to " + value);
	#endif
	}

	public void SetMusic(float value)
	{
		// Update audio mixer master volume
		audioMixer.SetFloat("MusicVolume", value);

		// Update game manager music
		gameManager.Music = value;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("MenuUI: music volume switched to " + value);
	#endif
	}

	public void SetSensitivity(float value)
	{
		// Update game manager sensitivity
		gameManager.Gamma = value;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("MenuUI: sensitivity settings switched to " + value);
	#endif
	}
	#endregion
}
