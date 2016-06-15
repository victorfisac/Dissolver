using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using XInputDotNetPure;

public class GameManager : MonoBehaviour 
{
	#region Static Attributes
	private static GameManager instance = null;
	#endregion

	#region Private Attributes
	// Flow
	private bool isDebug;				// Game manager is debug state
	private bool firstTime;				// First time check result

	// Settings
	private int resolution;				// Screen resolution
	private bool fullscreen;			// Screen windowed state
	private int quality;				// Current quality settings
	private int effects;				// Current effects settings
	private float gamma;				// Current gamma settings
	private float volume;				// Current global volume settings
	private float music;				// Current music volume settings
	private float sensitivity;			// Current sensitivity settings
	private bool help;					// Current help setting

	// Stats
	private int gameLevel;				// Last played game level
	private int gameProgress;			// Player status progress
	private int[] achievements;			// Player achievements status
	private Vector3 playerPosition;		// Player last position
	private Vector3 playerRotation;		// Player last rotation
	private Vector2 cameraRotation;		// Camera last rotation

	// Inputs
	private PlayerIndex gamepadIndex;	// Current gamepad index
	#endregion

	#region Main Methods
	private void Awake()
	{
		// Initialize values
		isDebug = false;
		achievements = new int[2];
		gamepadIndex = (PlayerIndex)(-1);

		CheckGamepad();

		CheckFirstTime();

		if(firstTime) ResetData();
		else LoadData();

		ApplyLoadedData();

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("GameManager: game manager initialized");
	#endif
	}

	public void UpdateBehaviour()
	{
		// Check if detected gamepad has been disconnected
		if(gamepadIndex != (PlayerIndex)(-1) && !GamePad.GetState(gamepadIndex).IsConnected) gamepadIndex = (PlayerIndex)(-1);

		if(gamepadIndex == (PlayerIndex)(-1)) CheckGamepad();

		// GamePad.SetVibration(gamepadIndex, 1f, 1f);
	}
	#endregion

	#region Initialization Methods
	private void CheckGamepad()
	{
		for(int i = 0; i < 4; i++)
		{
			GamePadState auxState = GamePad.GetState((PlayerIndex)i);

			// Check if current slot is connected
			if(auxState.IsConnected)
			{
				// Update current gamepad index
				gamepadIndex = (PlayerIndex)i;

			#if DEBUG_BUILD
				// Trace debug message
				Debug.Log("GameManager: detected gamepad index " + i);
			#endif
				break;
			}
		}
	}

	private void CheckFirstTime()
	{
		// Check if player prefs has first time value
		firstTime = !(PlayerPrefs.GetInt("firstTime") == 1);

		// If it is, create new first time value
		if(firstTime) PlayerPrefs.SetInt("firstTime", 1);
	}
	#endregion

	#region Data Methods
	private void ResetData()
	{
		// Reset settings values to current internal values
		resolution = 2;
		fullscreen = true;
		quality = 5;
		effects = 3;
		gamma = 1f;
		volume = 0f;
		music = 0f;
		sensitivity = 1f;
		achievements = new int[2];
		help = true;

		ApplyLoadedData();

		ResetProgress();

		// Save data after reset
		SaveData();
	}

	public void ResetProgress()
	{
		// Initialize stats values
		gameLevel = 1;
		gameProgress = 0;
		playerPosition = Vector3.zero + Vector3.up * -8.071174f;
		playerRotation = Vector3.zero;
		cameraRotation = new Vector2(-8f, 180f);

	#if DEBUG_BUILD
		// Trace debug log
		Debug.Log("GameManager: game progress reset");
	#endif
	}

	private void LoadData()
	{
		// Load settings values
		resolution = PlayerPrefs.GetInt("resolution");
		fullscreen = ((PlayerPrefs.GetInt("fullscreen") == 1) ? true : false);
		quality = PlayerPrefs.GetInt("quality");
		effects = PlayerPrefs.GetInt("effects");
		gamma = PlayerPrefs.GetFloat("gamma");
		volume = PlayerPrefs.GetFloat("volume");
		music = PlayerPrefs.GetFloat("music");
		sensitivity = PlayerPrefs.GetFloat("sensitivity");
		help = (PlayerPrefs.GetInt("help") == 1);
		gameLevel = PlayerPrefs.GetInt("gameLevel");
		gameProgress = PlayerPrefs.GetInt("gameProgress");
		for(int i = 0; i < achievements.Length; i++) achievements[i] = PlayerPrefs.GetInt("achievements_" + i);

		playerPosition = new Vector3(PlayerPrefs.GetFloat("playerPositionX"), PlayerPrefs.GetFloat("playerPositionY"), PlayerPrefs.GetFloat("playerPositionZ"));
		playerRotation = new Vector3(PlayerPrefs.GetFloat("playerRotationX"), PlayerPrefs.GetFloat("playerRotationY"), PlayerPrefs.GetFloat("playerRotationZ"));
		cameraRotation = new Vector2(PlayerPrefs.GetFloat("cameraRotationX"), PlayerPrefs.GetFloat("cameraRotationY"));
	}

	private void SaveData()
	{
		// Save settings values
		PlayerPrefs.SetInt("resolution", resolution);
		PlayerPrefs.SetInt("fullscreen", (fullscreen ? 1 : 0));
		PlayerPrefs.SetInt("quality", quality);
		PlayerPrefs.SetInt("effects", effects);
		PlayerPrefs.SetFloat("gamma", gamma);
		PlayerPrefs.SetFloat("volume", volume);
		PlayerPrefs.SetFloat("music", music);
		PlayerPrefs.SetFloat("sensitivity", sensitivity);
		PlayerPrefs.SetInt("help", (help ? 1 : 0));

		PlayerPrefs.SetInt("gameLevel", gameLevel);
		PlayerPrefs.SetInt("gameProgress", gameProgress);
		for(int i = 0; i < achievements.Length; i++) PlayerPrefs.SetInt("achievements_" + i, achievements[i]);

		PlayerPrefs.SetFloat("playerPositionX", playerPosition.x);
		PlayerPrefs.SetFloat("playerPositionY", playerPosition.y);
		PlayerPrefs.SetFloat("playerPositionZ", playerPosition.z);

		PlayerPrefs.SetFloat("playerRotationX", playerRotation.x);
		PlayerPrefs.SetFloat("playerRotationY", playerRotation.y);
		PlayerPrefs.SetFloat("playerRotationZ", playerRotation.z);

		PlayerPrefs.SetFloat("camearRotationX", cameraRotation.x);
		PlayerPrefs.SetFloat("cameraRotationY", cameraRotation.y);
	}

	private void ApplyLoadedData()
	{
		// Apply settings changes
		switch(resolution)
		{
			case 0: Screen.SetResolution(800, 600, fullscreen); break;
			case 1: Screen.SetResolution(1024, 768, fullscreen); break;
			case 2: Screen.SetResolution(1280, 720, fullscreen); break;
			case 3: Screen.SetResolution(1280, 1024, fullscreen); break;
			case 4: Screen.SetResolution(1366, 768, fullscreen); break;
			case 5: Screen.SetResolution(1440, 900, fullscreen); break;
			case 6: Screen.SetResolution(1680, 1050, fullscreen); break;
			case 7: Screen.SetResolution(1920, 1080, fullscreen); break;
			default: break;
		}

		QualitySettings.SetQualityLevel(quality);

		// NOTE: effects are applied from gameplay manager and menu UI
	}
	#endregion

	#region Manager Methods
	public void CallResetData()
	{
		ResetData();

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("GameManager: data reset started");
	#endif
	}

	public void CallSaveData()
	{
		SaveData();

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("GameMaster: save data process started");
	#endif
	}

	public void SaveCheckPoint(Vector3 playerPos, Vector3 playerRot, Vector2 cameraOrb)
	{
		// Update status values
		playerPosition = playerPos;
		playerRotation = playerRot;
		cameraRotation = cameraOrb;

		// Save data
		CallSaveData();
	}

	public void SetAchievement(int index)
	{
		// Update achievement value
		achievements[index] = 1;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("GameMaster: unlocked achievement " + index);
	#endif

		// Save data
		CallSaveData();
	}

	public void SetVibration(float amount)
	{
		// Check if any gamepad is connected and apply vibration to gamepad
		if(gamepadIndex != (PlayerIndex)(-1)) GamePad.SetVibration(gamepadIndex, amount, amount);
	}
	#endregion

	#region Unitialization Methods
	private void OnApplicationQuit() 
	{
		// Save data
		SaveData();

		// Reset gamepad vibration if needed
		if(gamepadIndex != (PlayerIndex)(-1)) GamePad.SetVibration(gamepadIndex, 0f, 0f);

	#if DEBUG_BUILD
		// Trace debug message
        Debug.Log("GameManager: game ending after " + Time.time + " seconds");
   #endif
    }
	#endregion

	#region Debug Methods
#if DEBUG_BUILD
	private void OnGUI()
	{
		if(isDebug)
		{
			// Calculate enemy transform screen position
			Rect rect = new Rect(25, Screen.height - 75, 500, 500);

			// Create a default GUI style
			GUIStyle style = new GUIStyle();
			style.alignment = TextAnchor.UpperLeft;
			style.fontSize = 10;
			style.normal.textColor = Color.red;

			// Add data to string value
			string text = "";
			text += "Frame rate: " + (1f/Time.deltaTime).ToString("00") + " FPS\n";
			text += "GameManager.resolution: " + resolution + "\n";
			text += "GameManager.quality: " + quality + "\n";
			text += "GameManager.effects: " + effects + "\n";
			text += "GameManager.help: " + help.ToString() + "\n";
			text += "GameManager.gameProgress: " + gameProgress + "\n";
			text += "GameManager.gameLevel: " + gameLevel + "\n";

			// Draw label based on calculated position with some important data values
			GUI.Label(rect, text, style);
		}
	}
#endif
	#endregion

	#region Properties
	public static GameManager Instance
	{
		get 
		{ 
			if(!instance) 
			{
				instance = (GameManager)FindObjectOfType(typeof(GameManager));
				
				if(!instance)
				{
					instance = (new GameObject("GameManager")).AddComponent<GameManager>();
					DontDestroyOnLoad (instance.gameObject);
				}
			}
			
			return instance;		
		}
	}

	public bool IsDebug
	{
		get { return isDebug; }
		set { isDebug = value; }
	}

	public int Resolution
	{
		get { return resolution; }
		set { resolution = value; }
	}

	public bool FullScreen
	{
		get { return fullscreen; }
		set { fullscreen = value; }
	}

	public int Quality
	{
		get { return quality; }
		set { quality = value; }
	}

	public int Effects
	{
		get { return effects; }
		set { effects = value; }
	}

	public float Gamma
	{
		get { return gamma; }
		set { gamma = value; }
	}

	public float Volume
	{
		get { return volume; }
		set { volume = value; }
	}

	public float Music
	{
		get { return music; }
		set { music = value; }
	}

	public float Sensitivity
	{
		get { return sensitivity; }
		set { sensitivity = value; }
	}

	public bool Help
	{
		get { return help; }
		set
		{ 
			help = value;
			CallSaveData();
		}
	}

	public int GameLevel
	{
		get { return gameLevel; }
		set
		{ 
			gameLevel = value;
			CallSaveData();
		}
	}

	public int GameProgress
	{
		get { return gameProgress; }
		set 
		{
			gameProgress = value; 
			CallSaveData();
		}
	}

	public int[] Achievements
	{
		get { return achievements; }
		set 
		{ 
			achievements = value; 
			CallSaveData();
		}
	}

	public Vector3 PlayerPosition
	{
		get { return playerPosition; }
		set 
		{
			playerPosition = value; 
			CallSaveData();
		}
	}

	public Vector3 PlayerRotation
	{
		get { return playerRotation; }
		set 
		{
			playerRotation = value; 
			CallSaveData();
		}
	}

	public Vector2 CameraRotation
	{
		get { return cameraRotation; }
		set 
		{
			cameraRotation = value; 
			CallSaveData();
		}
	}

	public bool HasGamepad
	{
		get { return (gamepadIndex != (PlayerIndex)(-1)); }
	}
	#endregion
}
