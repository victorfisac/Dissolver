using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;
using UnityStandardAssets.CinematicEffects;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InterfaceManager : MonoBehaviour 
{
	#region Enums
	public enum SlotStates { NONE, UP, DOWN };
	#endregion

	#region Inspector Attributes
	[Header("HUD")]
	[SerializeField] private Image healthImage;
	[SerializeField] private Image staminaImage;
	[SerializeField] private Image skillImage;
	[SerializeField] private ComboUI comboUI;
	[SerializeField] private ParticleSystem skillSystem;
	[SerializeField] private ParticleSystem skillActionSystem;
	[SerializeField] private GameObject waterMark;

	[Header("Boss")]
	[SerializeField] private Image bossHealth;
	[SerializeField] private ParticleSystem[] bossParticles;
	[SerializeField] private Boss boss;
	[SerializeField] private AlphaTransition bossAlpha;

	[Header("Stamina")]
	[SerializeField] private Color staminaColor;
	[SerializeField] private AnimationCurve staminaCurve;
	[SerializeField] private float staminaDuration;

	[Header("Damage")]
	[SerializeField] private Color damageColor;
	[SerializeField] private Image damageImage;

	[Header("Gameplay Menu")]
	[SerializeField] private GameObject gameplayUI;

	[Header("Pause Menu")]
	[SerializeField] private GameObject pauseUI;
	[SerializeField] private PositionTransition[] pauseTransition;

	[Header("Settings Menu")]
	[SerializeField] private GameObject settingsUI;
	[SerializeField] private PositionTransition settingsTransition;
	[SerializeField] private AlphaTransition alphaTransition;
	[SerializeField] private Dropdown resolution;
	[SerializeField] private Toggle windowed;
	[SerializeField] private Dropdown quality;
	[SerializeField] private Dropdown effects;
	[SerializeField] private Slider gamma;
	[SerializeField] private Slider volume;
	[SerializeField] private Slider music;
	[SerializeField] private Slider sensitivity;

	[Header("Gameplay End")]
	[SerializeField] private Fade gameplayFade;

	[Header("Victory Menu")]
	[SerializeField] private GameObject victoryUI;
	[SerializeField] private UnityEvent victoryEvent;

	[Header("Defeat Menu")]
	[SerializeField] private GameObject defeatUI;
	[SerializeField] private UnityEvent defeatEvent;
	[SerializeField] private Renderer[] defeatRenderers;
	[SerializeField] private AnimationCurve defeatCurve;
	[SerializeField] private float defeatDuration;
	[SerializeField] private Animator[] defeatAnimators;

	[Header("Exit Menu")]
	[SerializeField] private Fade mainFade;

	[Header("Aim")]
	[SerializeField] private AimUI aimUI;

	[Header("Effects")]
	[SerializeField] private bool awakeEffects;

	[Header("Faces")]
	[SerializeField] private MeshRenderer bulletFace;
	[SerializeField] private MeshRenderer tankFace;
	[SerializeField] private AnimationCurve facesCurve;
	[SerializeField] private float facesDuration;
	[SerializeField] private Vector2 facesLimits;

	[Header("Behaviours Settings")]
	[SerializeField] private bool updateAlphaTransitions;
	[SerializeField] private bool updatePositionTransitions;
	[SerializeField] private bool updateButtonScales;
	[SerializeField] private bool updateMenuBehaviours;
	[SerializeField] private bool updateFades;
	[SerializeField] private bool updateInputs;

	[Header("Behaviours")]
	[SerializeField] private List<AlphaTransition> alphaTransitions;
	[SerializeField] private List<PositionTransition> positionTransitions;
	[SerializeField] private List<ButtonScale> buttonScales;
	[SerializeField] private List<MenuBehaviour> menuBehaviours;
	[SerializeField] private List<Fade> fades;
	[SerializeField] private List<InputUI> inputs;

	[Header("Flow Buttons")]
	[SerializeField] private GameObject continueButton;
	[SerializeField] private GameObject backButton;
	[SerializeField] private GameObject winButton;
	[SerializeField] private GameObject tryAgainButton;

	[Header("Audio")]
	[SerializeField] private AudioMixer audioMixer;
	[SerializeField] private AudioSource positiveSource;
	[SerializeField] private AudioSource negativeSource;
	[SerializeField] private AudioSource gameOverSource;
	[SerializeField] private AudioSource defeatMenuSource;
	[SerializeField] private AudioSource winSource;
	[SerializeField] private AudioSource winMenuSource;
	[SerializeField] private AudioSource buttonSource;

	[Header("References")]
	[SerializeField] private GameplayManager gameplayManager;
	[SerializeField] private Transform camerasRoot;
	#endregion

	#region Private Attributes
	// Player
	private Character playerChar;				// Player character reference
	private bool staminaState;					// Stamina animation state
	private Color staminaColorInit;				// Stamina color at start
	private float staminaCounter;				// Stamina animation time counter
	private float currentSlots;					// Player current slots
	private float previousSlots;				// Player previous slots

	// Boss
	private bool bossUI;						// Boss UI update state					

	// Damage
	private Color damageColorInit;				// Damage image color at start

	// Cameras
	private AntiAliasing[] aas;					// Game cameras antialiasing references
	private Bloom[] blooms;						// Game cameras bloom references
	private DepthOfField[] depthOfField;		// Game cameras depth of field references
	private LensAberrations[] lens;				// Game cameras lens aberration references
	private TonemappingColorGrading[] tones;	// Game cameras tone mapping references
	private ScreenDistortion[] distortions;		// Game cameras screen distortion references
	private CameraLogic[] cameraLogics;			// Game cameras logic references

	// Faces
	private float facesCounter;					// Faces animation time counter
	private int facesState;						// Faces animation state
	private Material bulletMat;					// Bullet face material reference
	private Material tankMat;					// Tank face material reference

	// Defeat
	private Material[] defeatMats;				// Defeat renderer materials references
	private float defeatCounter;				// Defeat dissolve animation time counter
	private bool defeatWork;					// Defeat can work state

	// Effects
	private List<Camera> gameCameras;			// Gameplay cameras references

	// Managers
	private GameManager gameManager;			// Game manager instance
	#endregion

	#region Main Methods
	public void AwakeBehaviour(Character player)
	{
		// Initialize values
		playerChar = player;

		// Initialize effects references
		gameManager = GameManager.Instance;
		gameCameras = new List<Camera>();
		gameCameras.Add(Camera.main);

		// Enable cinematic camera root game object to get components references
		camerasRoot.gameObject.SetActive(true);

		// Disable watermark if needed
		waterMark.SetActive(gameManager.Help);

		// Search all scene cameras
		Camera[] auxCameras = camerasRoot.GetComponentsInChildren<Camera>();
		for(int i = 0; i < auxCameras.Length; i++) gameCameras.Add(auxCameras[i]);

		aas = new AntiAliasing[gameCameras.Count];
		blooms = new Bloom[gameCameras.Count];
		depthOfField = new DepthOfField[gameCameras.Count];
		lens = new LensAberrations[gameCameras.Count];
		tones = new TonemappingColorGrading[gameCameras.Count];
		distortions = new ScreenDistortion[gameCameras.Count];
		cameraLogics = new CameraLogic[gameCameras.Count];

		defeatMats = new Material[defeatRenderers.Length];
		for(int i = 0; i < defeatMats.Length; i++) defeatMats[i] = defeatRenderers[i].material;

		// Get effects references
		for(int i = 0; i < aas.Length; i++) aas[i] = gameCameras[i].GetComponent<AntiAliasing>();
		for(int i = 0; i < blooms.Length; i++) blooms[i] = gameCameras[i].GetComponent<Bloom>();
		for(int i = 0; i < depthOfField.Length; i++) depthOfField[i] = gameCameras[i].GetComponent<DepthOfField>();
		for(int i = 0; i < lens.Length; i++) lens[i] = gameCameras[i].GetComponent<LensAberrations>();
		for(int i = 0; i < tones.Length; i++) tones[i] = gameCameras[i].GetComponent<TonemappingColorGrading>();
		for(int i = 0; i < distortions.Length; i++) distortions[i] = gameCameras[i].GetComponent<ScreenDistortion>();
		for(int i = 0; i < cameraLogics.Length; i++) cameraLogics[i] = gameCameras[i].GetComponent<CameraLogic>();

		// Disable cinematic cameras
		for(int i = 1; i < gameCameras.Count; i++) gameCameras[i].gameObject.SetActive(false);

		if(awakeEffects)
		{
			// Initialize effects in cameras
			SetEffects(gameManager.Effects);

			// Initialize ui values based on game manager loaded data
			resolution.value = gameManager.Resolution;
			windowed.isOn = gameManager.FullScreen;
			quality.value = gameManager.Quality;
			effects.value = gameManager.Effects;
			gamma.value = gameManager.Gamma;
			volume.value = gameManager.Volume;
			music.value = gameManager.Music;
			sensitivity.value = gameManager.Sensitivity;
		}

		staminaColorInit = staminaImage.color;
		damageColorInit = damageImage.color;
		currentSlots = playerChar.Slots;
		previousSlots = currentSlots;

		// Initialize aim ui behaviour
		aimUI.AwakeBehaviour();

		// Enable fade game object to do fade in
		mainFade.gameObject.SetActive(true);

		// Awake alpha transitions behaviours
		if(updateAlphaTransitions) for(int i = 0; i < alphaTransitions.Count; i++) alphaTransitions[i].AwakeBehaviour();

		// Awake button scale behaviours
		if(updateButtonScales) for(int i = 0; i < buttonScales.Count; i++) buttonScales[i].AwakeBehaviour(buttonSource);

		// Awake all input UI behaviours
		if(updateInputs) for(int i = 0; i < inputs.Count; i++) inputs[i].AwakeBehaviour();

		// Awake combo interface behaviour
		comboUI.AwakeBehaviour(playerChar);

		// Enable gameplay UI after fade fadein
		Invoke("EnableGameplayUI", 1f / mainFade.Speed);

		// Get faces references
		bulletMat = bulletFace.material;
		tankMat = tankFace.material;

		// Initialize faces values
		bulletMat.SetFloat("_DissolveAmount", facesLimits.x);
		tankMat.SetFloat("_DissolveAmount", facesLimits.y);

		// Initialize defeat animators
		for(int i = 0; i < defeatAnimators.Length; i++) defeatAnimators[i].speed = Random.Range(0.5f, 1f);
	}

	private void EnableGameplayUI()
	{
		// Check if player is not in any pause menu to enable gameplay interface
		if(gameplayManager.State == 1)
		{
			// Enable gameplay UI
			gameplayUI.SetActive(true);

			// Enable audio UI game objects
			positiveSource.enabled = true;
			negativeSource.enabled = true;
			buttonSource.enabled = true;
		}
	}

	public void UpdateBehaviour()
	{
		UpdateHUD();
		UpdateBoss();
		UpdateStamina();
		UpdateDamage();
		UpdateFaces();

		// Switch water mark visibility
		if(Input.GetKeyDown(KeyCode.F1))
		{
			// Update watermark game object active state
			waterMark.SetActive(!waterMark.activeSelf);

			// Save current state to game manager
			gameManager.Help = waterMark.activeSelf;
		}

		// Update all cameras blur effect
		for(int i = 0; i < cameraLogics.Length; i++) cameraLogics[i].UpdateBlur();

		aimUI.UpdateBehaviour();

		// Update alpha transitions behaviours
		if(updateAlphaTransitions) for(int i = 0; i < alphaTransitions.Count; i++) alphaTransitions[i].UpdateBehaviour();

		// Update position transitions behaviours
		if(updatePositionTransitions) for(int i = 0; i < positionTransitions.Count; i++) positionTransitions[i].UpdateBehaviour();

		// Update buttons scale behaviours
		if(updateButtonScales) for(int i = 0; i < buttonScales.Count; i++) buttonScales[i].UpdateBehaviour();

		// Update menu behaviours
		if(updateMenuBehaviours) for(int i = 0; i < menuBehaviours.Count; i++) menuBehaviours[i].UpdateBehaviour();

		// Update fades behaviours
		if(updateFades) for(int i = 0; i < fades.Count; i++) fades[i].UpdateBehaviour();

		if(defeatWork && defeatCounter < defeatDuration)
		{
			// Update defeat materials dissolve amount based on animation curve
			for(int i = 0; i < defeatMats.Length; i++) defeatMats[i].SetFloat("_DissolveAmount", defeatCurve.Evaluate(defeatCounter / defeatDuration));

			// Update defeat time counter
			defeatCounter += Time.deltaTime;
		}
	}
	#endregion

	#region HUD Methods
	private void UpdateHUD()
	{
		healthImage.fillAmount = playerChar.Health / playerChar.MaxHealth;
		staminaImage.fillAmount = playerChar.Stamina / playerChar.MaxStamina;
		if(playerChar.MaxSlots > 0) skillImage.fillAmount = Mathf.Lerp(skillImage.fillAmount, playerChar.Slots / playerChar.MaxSlots, Time.deltaTime * 3f);
		else skillImage.fillAmount = 0f;

		if(playerChar.Slots >= playerChar.MaxSlots && playerChar.MaxSlots > 0)
		{
			if(!skillSystem.isPlaying) skillSystem.Play();
		}
		else
		{
			if(skillSystem.isPlaying) skillSystem.Stop();
		}

		// Update previous player slots
		previousSlots = currentSlots;
		currentSlots = playerChar.Slots;

		// Play skill action particle system if needed
		if(currentSlots < previousSlots) skillActionSystem.Play();


		// Update combo UI behaviour
		comboUI.UpdateBehaviour();

		// Update all input UI behaviours
		if(updateInputs) for(int i = 0; i < inputs.Count; i++) inputs[i].UpdateBehaviour();
	}

	private void UpdateBoss()
	{
		// Check if boss state is enabled
		if(bossUI)
		{
			// Update boss based on current boss health
			bossHealth.fillAmount = boss.Health / boss.MaxHealth;

			// Disable boss interface if boss died
			if(boss.Health <= 0f) DisableBossUI();
		}
	}

	private void UpdateStamina()
	{
		if(staminaState)
		{
			// Update stamina image color based on animation curve
			staminaImage.color = Color.Lerp(staminaColorInit, staminaColor, staminaCurve.Evaluate(staminaCounter / staminaDuration));

			// Update time counter
			staminaCounter += Time.deltaTime;

			if(staminaCounter >= staminaDuration)
			{
				// Reset stamina time counter
				staminaCounter = 0f;

				// Reset stamina state
				staminaState = false;
			}
		}
	}

	private void UpdateDamage()
	{
		// Update damage image alpha based on player health
		damageImage.color = Color.Lerp(damageColor, damageColorInit, playerChar.Health / playerChar.MaxHealth);
	}

	private void UpdateFaces()
	{
		switch(facesState)
		{
			case 1:
			{
				// Update faces materials dissolve amount based on animation curve
				bulletMat.SetFloat("_DissolveAmount", Mathf.Lerp(facesLimits.x, facesLimits.y, facesCurve.Evaluate(facesCounter / facesDuration)));
				tankMat.SetFloat("_DissolveAmount", facesLimits.y - Mathf.Lerp(facesLimits.x, facesLimits.y, facesCurve.Evaluate(facesCounter / facesDuration)) - 0.2f);

				// Update faces time counter
				facesCounter += Time.deltaTime;

				if(facesCounter >= facesDuration)
				{
					// Fix faces materials dissolve amount value
					bulletMat.SetFloat("_DissolveAmount", facesLimits.y);
					tankMat.SetFloat("_DissolveAmount", facesLimits.x);

					// Reset faces time counter
					facesCounter = 0f;

					// Update faces state
					facesState = 2;
				}
			} break;
			case 3:
			{
				// Update faces materials dissolve amount based on animation curve
				bulletMat.SetFloat("_DissolveAmount", facesLimits.y - Mathf.Lerp(facesLimits.x, facesLimits.y, facesCurve.Evaluate(facesCounter / facesDuration)));
				tankMat.SetFloat("_DissolveAmount", Mathf.Lerp(facesLimits.x, facesLimits.y, facesCurve.Evaluate(facesCounter / facesDuration)) - 0.2f);

				// Update faces time counter
				facesCounter += Time.deltaTime;

				if(facesCounter >= facesDuration)
				{
					// Fix faces materials dissolve amount value
					bulletMat.SetFloat("_DissolveAmount", facesLimits.x);
					tankMat.SetFloat("_DissolveAmount", facesLimits.y);

					// Reset faces time counter
					facesCounter = 0f;

					// Update faces state
					facesState = 0;
				}
			} break;
			default: break;
		}
	}
	#endregion

	#region Input Methods
	public void SetGameplay()
	{
		// Play inversed pause transition
		for(int i = 0; i < pauseTransition.Length; i++) pauseTransition[i].Play(true);

		// Cancel previous transitions if needed
		if(IsInvoking("GoGameplay")) CancelInvoke("GoGameplay");
		if(IsInvoking("GoPause")) CancelInvoke("GoPause");
		if(IsInvoking("GoSettings")) CancelInvoke("GoSettings");

		// Disable main camera blur
		for(int i = 0; i < cameraLogics.Length; i++) cameraLogics[i].PauseCamera(false);

		// Play positive audio source
		if(positiveSource.enabled) positiveSource.Play();

		// Enable and lock cursor
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		// Update current selected button
		EventSystem.current.SetSelectedGameObject(null);

		// Invoke settings events after transition duration
		Invoke("GoGameplay", pauseTransition[0].Duration + pauseTransition[0].MaxDelay + 0.5f);
	}

	private void GoGameplay()
	{
		pauseUI.SetActive(false);
		gameplayUI.SetActive(true);
	}

	public void SetPause()
	{
		// Disable gameplay interface
		gameplayUI.SetActive(false);

		// Cancel previous transitions if needed
		if(IsInvoking("GoGameplay")) CancelInvoke("GoGameplay");
		if(IsInvoking("GoPause")) CancelInvoke("GoPause");
		if(IsInvoking("GoSettings")) CancelInvoke("GoSettings");

		// Update current selected button
		EventSystem.current.SetSelectedGameObject(continueButton);

		// Enable main camera blur
		for(int i = 0; i < cameraLogics.Length; i++) cameraLogics[i].PauseCamera(true);

		// Disable and lock cursor
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		// Play negative audio source
		if(negativeSource.enabled) negativeSource.Play();

		// Check if we are in settings
		if(settingsUI.activeSelf)
		{
			// Invoke settings events after transition duration
			Invoke("GoPause", settingsTransition.Duration + settingsTransition.MaxDelay);
			settingsTransition.Play(true);
			alphaTransition.Play(true);
		}
		else
		{
			pauseUI.SetActive(true);
			for(int i = 0; i < pauseTransition.Length; i++) pauseTransition[i].Play(false);
		}
	}

	private void GoPause()
	{
		settingsUI.SetActive(false);
		pauseUI.SetActive(true);
	}

	public void SetSettings()
	{
		// Play inversed pause transition
		for(int i = 0; i < pauseTransition.Length; i++) pauseTransition[i].Play(true);

		// Cancel previous transitions if needed
		if(IsInvoking("GoGameplay")) CancelInvoke("GoGameplay");
		if(IsInvoking("GoPause")) CancelInvoke("GoPause");
		if(IsInvoking("GoSettings")) CancelInvoke("GoSettings");

		// Invoke settings events after transition duration
		Invoke("GoSettings", pauseTransition[0].Duration + pauseTransition[0].MaxDelay + 0.5f);

		// Update current selected button
		EventSystem.current.SetSelectedGameObject(resolution.gameObject);
	}

	private void GoSettings()
	{
		settingsUI.SetActive(true);
		pauseUI.SetActive(false);
	}

	public void SetVictory()
	{
		gameplayFade.SetFadeOut();

		// Play win sound
		winSource.Play();

		// Update current selected button
		EventSystem.current.SetSelectedGameObject(winButton);

		// Cancel previous transitions if needed
		if(IsInvoking("GoGameplay")) CancelInvoke("GoGameplay");
		if(IsInvoking("GoPause")) CancelInvoke("GoPause");
		if(IsInvoking("GoSettings")) CancelInvoke("GoSettings");

		Invoke("GoVictory", 1f / gameplayFade.Speed);
	}

	private void GoVictory()
	{
		gameplayUI.SetActive(false);
		victoryUI.SetActive(true);
		victoryEvent.Invoke();

		// Play victory sound
		winMenuSource.Play();

		// Enable and lock cursor
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void SetDefeat()
	{
		gameplayFade.SetFadeOut();

		// Play game over sound
		gameOverSource.Play();

		// Update current selected button
		EventSystem.current.SetSelectedGameObject(tryAgainButton);

		// Cancel previous transitions if needed
		if(IsInvoking("GoGameplay")) CancelInvoke("GoGameplay");
		if(IsInvoking("GoPause")) CancelInvoke("GoPause");
		if(IsInvoking("GoSettings")) CancelInvoke("GoSettings");

		Invoke("GoDefeat", 1f / gameplayFade.Speed);
	}

	private void GoDefeat()
	{
		// Update defeat dissolve animation can work
		defeatWork = true;

		gameplayUI.SetActive(false);
		defeatUI.SetActive(true);
		defeatEvent.Invoke();

		// Play defeat over sound
		defeatMenuSource.Play();

		// Enable and lock cursor
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void SetRestart()
	{
		// Cancel previous transitions if needed
		if(IsInvoking("GoGameplay")) CancelInvoke("GoGameplay");
		if(IsInvoking("GoPause")) CancelInvoke("GoPause");
		if(IsInvoking("GoSettings")) CancelInvoke("GoSettings");

		// Enable main fade fade-out
		mainFade.SetFadeOut();

		Invoke("GoRestart", 1f/mainFade.Speed);

		// Make cursor invisible and lock it
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		// Update current selected button
		EventSystem.current.SetSelectedGameObject(null);
	}

	private void GoRestart()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void SetExit(bool nextLevel)
	{
		// Enable main fade fade-out
		mainFade.SetFadeOut();

		// Make cursor invisible and lock it
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		// Check if next level index is out of bounds
		if(gameManager.GameLevel >= 3)
		{
			// Fix next level value if game level is out of bounds
			gameManager.GameLevel = 2;

			Invoke("GoCredits", 1f/mainFade.Speed);
		}
		else Invoke((nextLevel ? "GoNextLevel" : "GoExit"), 1f/mainFade.Speed);

		// Update current selected button
		EventSystem.current.SetSelectedGameObject(null);
	}

	private void GoExit()
	{
		// Load main menu scene
		SceneManager.LoadScene("menu");
	}

	private void GoCredits()
	{
		// Load credits menu scene
		SceneManager.LoadScene("credits");
	}

	private void GoNextLevel()
	{
		// Get current scene
		Scene currentScene = SceneManager.GetActiveScene();

		// Check if next scene build index and current game manager game level index values match
		if(gameManager.GameLevel + 1 == currentScene.buildIndex + 1) SceneManager.LoadScene(currentScene.buildIndex + 1);
	#if DEBUG_BUILD
		else
		{
			// Trace warning message
			Debug.LogWarning("InterfaceManage: trying to loading scene (index: " + (currentScene.buildIndex + 1).ToString() + ") but (GameLevel + 1) is " + (gameManager.GameLevel + 1).ToString());
		}
	#endif
	}

	public void WarnStamina()
	{
		// Enable stamina state
		staminaState = true;

		// Reset stamina counter
		staminaCounter = 0f;
	}
	#endregion

	#region Audio Methods
	public void PlayAudioUI(bool positive)
	{
		if(positive && positiveSource.enabled) positiveSource.Play();
		else if(negativeSource.enabled) negativeSource.Play();
	}
	#endregion

	#region Cinematic Methods
	public void ShowGameplayUI(bool state)
	{
		// Update gameplay UI state based on cinematic state
		for(int i = 0; i < gameplayUI.transform.childCount; i++) gameplayUI.transform.GetChild(i).gameObject.SetActive(state);
	}
	#endregion

	#region Boss Methods
	public void EnableBossUI()
	{
		if(boss)
		{
			// Update boss interface state
			bossUI = true;

			// Play boss interface alpha transition animation
			bossAlpha.Play(false);

			// Enable all boss interface particle systems after delay
			Invoke("EnableBossParticles", 1f);
		}
	}

	public void DisableBossUI()
	{
		// Update boss interface state
		bossUI = false;

		// Play boss interface alpha transition animation
		bossAlpha.Play(true);

		// Disable all boss interface particle systems
		for(int i = 0; i < bossParticles.Length; i++) bossParticles[i].Stop();
	}

	private void EnableBossParticles()
	{
		// Enable all boss interface particle systems
		for(int i = 0; i < bossParticles.Length; i++) bossParticles[i].Play();
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
		Debug.Log("InterfaceManager: screen resolution switched to " + Screen.width + " x " + Screen.height);
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
		Debug.Log("InterfaceManager: window full screen mode set to " + state.ToString());
	#endif
	}

	public void SetQuality(int quality)
	{
		// Update quality settings level
		QualitySettings.SetQualityLevel(quality);

		// Update game manager quality
		gameManager.Quality = quality;

		// Apply quality to gameplay manager lights optimization
		gameplayManager.SetLights(quality);

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("InterfaceManager: quality settings switched to " + QualitySettings.names[quality] + " (" + quality + ")");
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
			} break;
			case 1:	// Normal settings
			{
				for(int i = 0; i < aas.Length; i++)	aas[i].enabled = true;
				for(int i = 0; i < blooms.Length; i++) blooms[i].enabled = true;
				for(int i = 0; i < distortions.Length; i++)	distortions[i].enabled = true;

				for(int i = 0; i < depthOfField.Length; i++) depthOfField[i].enabled = false;
				for(int i = 0; i < lens.Length; i++) lens[i].enabled = false;
				for(int i = 0; i < tones.Length; i++) tones[i].enabled = false;
			} break;
			case 2:	// High settings
			{
				for(int i = 0; i < aas.Length; i++)	aas[i].enabled = true;
				for(int i = 0; i < blooms.Length; i++) blooms[i].enabled = true;
				for(int i = 0; i < distortions.Length; i++)	distortions[i].enabled = true;
				for(int i = 0; i < depthOfField.Length; i++) depthOfField[i].enabled = true;

				for(int i = 0; i < lens.Length; i++) lens[i].enabled = false;
				for(int i = 0; i < tones.Length; i++) tones[i].enabled = false;
			} break;
			case 3: // Ultra settings
			{
				for(int i = 0; i < aas.Length; i++)	aas[i].enabled = true;
				for(int i = 0; i < blooms.Length; i++) blooms[i].enabled = true;
				for(int i = 0; i < distortions.Length; i++)	distortions[i].enabled = true;
				for(int i = 0; i < depthOfField.Length; i++) depthOfField[i].enabled = true;
				for(int i = 0; i < lens.Length; i++) lens[i].enabled = true;
				for(int i = 0; i < tones.Length; i++) tones[i].enabled = true;
			} break;
			default: break;
		}

		// Update game manager quality
		gameManager.Effects = value;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("InterfaceManager: effects settings switched to " + value);
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

		// Update game manager quality
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
		gameManager.Sensitivity = value;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("MenuUI: sensitivity settings switched to " + value);
	#endif
	}
	#endregion

	#region Properties
	public int FacesState
	{
		set { facesState = value; }
	}

	public List<InputUI> Inputs
	{
		get { return inputs; }
	}
	#endregion
}
