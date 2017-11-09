using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour 
{
	#region Enums
	public enum GameplayStates { START, GAMEPLAY, PAUSE, SETTINGS, VICTORY, DEFEAT, EXIT };
	public enum UpdateType { DEFAULT, FIXED };
	#endregion

	#region Inspector Attributes
	[Header("Main Settings")]
	[SerializeField] private GameplayStates state;
	[SerializeField] private int targetFrameRate;
	[SerializeField] private int initProgress;
	[SerializeField] private Vector3 initPosition;
	[SerializeField] private Vector3 initRotation;

	[Header("Behaviours Settings")]
	[SerializeField] private UpdateType updateType;
	[SerializeField] private bool updateInputs;
	[SerializeField] private bool updateCamera;
	[SerializeField] private bool updateUI;
	[SerializeField] private bool updateLightingManager;
	[SerializeField] private bool updateCharacters;
	[SerializeField] private bool updateRotateObjects;
	[SerializeField] private bool updatePickups;
	[SerializeField] private bool updateSpikeTraps;
	[SerializeField] private bool updateBarrierTraps;
	[SerializeField] private bool updateSpawnManagers;
	[SerializeField] private bool updateSpawnEnemies;
	[SerializeField] private bool updateDynamicFloors;
	[SerializeField] private bool updateCinematicCameras;
	[SerializeField] private bool updatePlatformWeaks;
	[SerializeField] private bool updatePuzzles;
	[SerializeField] private bool updateLightnings;
	[SerializeField] private bool updateLightLogics;
	[SerializeField] private bool updateDestroyables;
	[SerializeField] private bool updateDissolveLogics;
	[SerializeField] private bool updatePowerPilars;
	[SerializeField] private bool updateTriggerTraps;
	[SerializeField] private bool updateAchievements;
	[SerializeField] private bool updateArenaPilars;
	[SerializeField] private bool updateTriggerEnds;
	[SerializeField] private bool updateHelper;
	[SerializeField] private bool updatePilarPuzzles;
	[SerializeField] private bool updateDoors;
	[SerializeField] private bool updateInvokePilars;
	[SerializeField] private bool updateModuleManager;
	[SerializeField] private bool updateBosses;

	[Header("Gameplay Behaviours")]
	[SerializeField] private CameraLogic cameraLogic;
	[SerializeField] private Helper helper;
	[SerializeField] private InterfaceManager ui;
	[SerializeField] private LightingManager lightingManager;
	[SerializeField] private ModuleManager moduleManager;
	[SerializeField] private List<CinematicCamera> cinematicCameras;

	[Header("Optimization Behaviours Settings")]
	[SerializeField] private bool updateMeshesMergers;

	[Header("Lights Optimization")]
	[SerializeField] private Light[] lowLights;
	[SerializeField] private Light[] mediumLights;
	[SerializeField] private Light[] highLights;

	[Header("Status Events")]
	[SerializeField] private bool savedPosition;
	[SerializeField] private bool applyStatus;
	[SerializeField] private ProgressEvent[] statusEvents;
	#endregion

	#region Private Attributes
	private GameManager gameManager;					// Game Manager reference

	// Level default values
	private Vector3 playerPosition;						// Player position before progress transformations
	private Vector3 playerRotation;						// Player rotation before progress transformations
	private Vector2 cameraRotation;						// Camera rotation before progress transformations

	// Internal references
	private PlayerCharacter playerChar;					// Player character reference
	private List<Character> characters;					// Characters searched references
	private List<RotateObject> rotateObjects;			// Rotate objects searched references
	private List<SpawnEnemy> spawnEnemies;				// Spawn enemies searched references
	private List<PlatformWeak> platformWeaks;			// Platform weak searched references
	private List<LightningLogic> lightnings;			// Lightning logic searched references
	private List<Destroyable> destroyables;				// Destroyables searched references
	private List<MeshesMerger> meshesMergers;			// Meshes mergers searched references
	private List<LightLogic> lights;					// Light logics searched references
	private List<DissolveLogic> dissolveLogics;			// Dissolve logics searched references
	private List<PowerPilar> powerPilars;				// Power pilars logics searched references
	private List<DynamicFloor> dynamicFloors;			// Dynamic floor logics searched references
	private List<TriggerTrap> triggerTraps;				// Trigger trap searched references
	private List<SpikeTrap> spikeTraps;					// Spike trap searched references
	private List<Pickup> pickups;						// Pickups searched references
	private List<BarrierTrap> barrierTraps;				// Barrier traps searched references
	private List<AchievementLogic> achievements;		// Achievements searched references
	private List<ArenaPilar> arenaPilars;				// Arena pilars searched references
	private List<TriggerEnd> triggerEnds;				// Trigger ends searched references
	private List<PowerPilarPuzzle> pilarPuzzles;		// Power pilar puzzles searched references
	private List<SpawnManager> spawnManagers;			// Spawn manager searched references
	private List<Puzzle> puzzles;						// Puzzles searched references
	private List<DoorLogic> doors;						// Doors searched references
	private List<InvokePilar> invokePilars;				// Invoke pilars searched references
	private List<Boss> bosses;							// Boss logics searched references
	#endregion

	#region Main Methods
	private void Awake()
	{
		// Get references
		gameManager = GameManager.Instance;

		SearchReferences();

		// Store default player and camera transformations
		playerPosition = characters[0].Trans.position;
		playerRotation = characters[0].Trans.rotation.eulerAngles;
		cameraRotation = cameraLogic.CameraOrbit;

		if(gameManager.GameProgress == 0 && savedPosition)
		{
			gameManager.PlayerPosition = initPosition;
			gameManager.PlayerRotation = initRotation;
		}

		// Initialize lights optimization
		SetLights(QualitySettings.GetQualityLevel());

		AwakeBehaviours();

		// Initialize gameplay state
		state = GameplayStates.GAMEPLAY;

		// Disable and lock cursor
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		// Set frames per second target
		Application.targetFrameRate = targetFrameRate;
	}

	private void Start()
	{
		StartBehaviours();
	}

	private void Update()
	{
		FixListsReferences();

		CheckInputs();

		UpdateBehaviours();
	}

	private void FixedUpdate()
	{
		UpdateFixedBehaviours();
	}
	#endregion

	#region Initialization Methods
	private void SearchReferences()
	{
		if(updateCharacters)
		{
			// Find all characters references
			Enemy[] enemies = FindObjectsOfType<Enemy>();
			characters = new List<Character>();

			// Get player character reference
			playerChar = GameObject.FindWithTag("Player").GetComponent<PlayerCharacter>();

			// Add player as first character reference
			characters.Add(playerChar);

			// Add founded character references
			for(int i = 0; i < enemies.Length; i++) characters.Add((Character)enemies[i]);
		}

		if(updateRotateObjects)
		{
			// Find all rotate objects references
			RotateObject[] tempRotate = FindObjectsOfType<RotateObject>();
			rotateObjects = new List<RotateObject>();
			for(int i = 0; i < tempRotate.Length; i++) rotateObjects.Add(tempRotate[i]);
		}

		if(updateSpawnEnemies) spawnEnemies = new List<SpawnEnemy>();

		if(updatePlatformWeaks)
		{
			// Find all platform weak references
			PlatformWeak[] tempPlatforms = FindObjectsOfType<PlatformWeak>();
			platformWeaks = new List<PlatformWeak>();
			for(int i = 0; i < tempPlatforms.Length; i++) platformWeaks.Add(tempPlatforms[i]);
		}

		if(updateLightnings)
		{
			// Find all lightnings logic references
			LightningLogic[] tempLightnings = FindObjectsOfType<LightningLogic>();
			lightnings = new List<LightningLogic>();
			for(int i = 0; i < tempLightnings.Length; i++) lightnings.Add(tempLightnings[i]);
		}

		if(updateDestroyables)
		{
			// Find all destroyables references
			Destroyable[] tempDestroyables = FindObjectsOfType<Destroyable>();
			destroyables = new List<Destroyable>();
			for(int i = 0; i < tempDestroyables.Length; i++) destroyables.Add(tempDestroyables[i]);
		}

		if(updateLightLogics)
		{
			// Find all meshes mergers references
			LightLogic[] tempLights = FindObjectsOfType<LightLogic>();
			lights = new List<LightLogic>();
			for(int i = 0; i < tempLights.Length; i++) lights.Add(tempLights[i]);
		}

		if(updateMeshesMergers)
		{
			// Find all meshes mergers references
			MeshesMerger[] tempMergers = FindObjectsOfType<MeshesMerger>();
			meshesMergers = new List<MeshesMerger>();
			for(int i = 0; i < tempMergers.Length; i++) meshesMergers.Add(tempMergers[i]);
		}

		if(updateDissolveLogics)
		{
			// Find all dissolve logics references
			DissolveLogic[] tempDissolve = FindObjectsOfType<DissolveLogic>();
			dissolveLogics = new List<DissolveLogic>();
			for(int i = 0; i < tempDissolve.Length; i++) dissolveLogics.Add(tempDissolve[i]);
		}

		if(updatePowerPilars)
		{
			// Find all power pilars logics references
			PowerPilar[] tempPilars = FindObjectsOfType<PowerPilar>();
			powerPilars = new List<PowerPilar>();
			for(int i = 0; i < tempPilars.Length; i++) powerPilars.Add(tempPilars[i]);
		}

		if(updateDynamicFloors)
		{
			// Find all dynamic floor logics references
			DynamicFloor[] tempFloors = FindObjectsOfType<DynamicFloor>();
			dynamicFloors = new List<DynamicFloor>();
			for(int i = 0; i < tempFloors.Length; i++) dynamicFloors.Add(tempFloors[i]);
		}

		if(updateSpikeTraps)
		{
			// Find all spike traps logics references
			SpikeTrap[] tempSpikes = FindObjectsOfType<SpikeTrap>();
			spikeTraps = new List<SpikeTrap>();
			for(int i = 0; i < tempSpikes.Length; i++) spikeTraps.Add(tempSpikes[i]);
		}

		if(updateBarrierTraps)
		{
			// Find all spike traps logics references
			BarrierTrap[] tempBarrier = FindObjectsOfType<BarrierTrap>();
			barrierTraps = new List<BarrierTrap>();
			for(int i = 0; i < tempBarrier.Length; i++) barrierTraps.Add(tempBarrier[i]);
		}

		if(updateTriggerTraps)
		{
			// Find all spike traps logics references
			TriggerTrap[] tempTrigger = FindObjectsOfType<TriggerTrap>();
			triggerTraps = new List<TriggerTrap>();
			for(int i = 0; i < tempTrigger.Length; i++) triggerTraps.Add(tempTrigger[i]);
		}

		if(updatePickups)
		{
			// Find all pickups traps logics references
			Pickup[] tempPickup = FindObjectsOfType<Pickup>();
			pickups = new List<Pickup>();
			for(int i = 0; i < tempPickup.Length; i++) pickups.Add(tempPickup[i]);
		}

		if(updateAchievements)
		{
			// Find all pickups traps logics references
			AchievementLogic[] tempAchievement = FindObjectsOfType<AchievementLogic>();
			achievements = new List<AchievementLogic>();
			for(int i = 0; i < tempAchievement.Length; i++) achievements.Add(tempAchievement[i]);
		}

		if(updateArenaPilars)
		{
			// Find all arena pilars logics references
			ArenaPilar[] tempArena = FindObjectsOfType<ArenaPilar>();
			arenaPilars = new List<ArenaPilar>();
			for(int i = 0; i < tempArena.Length; i++) arenaPilars.Add(tempArena[i]);
		}

		if(updateTriggerEnds)
		{
			// Find all trigger ends logics references
			TriggerEnd[] tempTrigger = FindObjectsOfType<TriggerEnd>();
			triggerEnds = new List<TriggerEnd>();
			for(int i = 0; i < tempTrigger.Length; i++) triggerEnds.Add(tempTrigger[i]);
		}

		if(updatePilarPuzzles)
		{
			// Find all pilar puzzles logics references
			PowerPilarPuzzle[] tempPilarPuzzles = FindObjectsOfType<PowerPilarPuzzle>();
			pilarPuzzles = new List<PowerPilarPuzzle>();
			for(int i = 0; i < tempPilarPuzzles.Length; i++) pilarPuzzles.Add(tempPilarPuzzles[i]);
		}

		if(updateSpawnManagers)
		{
			// Find all spawn managers references
			SpawnManager[] tempSpawn = FindObjectsOfType<SpawnManager>();
			spawnManagers = new List<SpawnManager>();
			for(int i = 0; i < tempSpawn.Length; i++) spawnManagers.Add(tempSpawn[i]);
		}

		if(updatePuzzles)
		{
			// Find all puzzles logics references
			Puzzle[] tempPuzzle = FindObjectsOfType<Puzzle>();
			puzzles = new List<Puzzle>();
			for(int i = 0; i < tempPuzzle.Length; i++) puzzles.Add(tempPuzzle[i]);
		}

		if(updateDoors)
		{
			// Find all doors logics references
			DoorLogic[] tempDoor = FindObjectsOfType<DoorLogic>();
			doors = new List<DoorLogic>();
			for(int i = 0; i < tempDoor.Length; i++) doors.Add(tempDoor[i]);
		}

		if(updateInvokePilars)
		{
			// Find all doors logics references
			InvokePilar[] tempInvoke = FindObjectsOfType<InvokePilar>();
			invokePilars = new List<InvokePilar>();
			for(int i = 0; i < tempInvoke.Length; i++) invokePilars.Add(tempInvoke[i]);
		}

		if(updateBosses)
		{
			// Find all boss logics references
			Boss[] tempBoss = FindObjectsOfType<Boss>();
			bosses = new List<Boss>();
			for(int i = 0; i < tempBoss.Length; i++) bosses.Add(tempBoss[i]);
		}
	}
	#endregion

	#region Behaviours Methods
	private void AwakeBehaviours()
	{
		// Awake interface logic behaviour
		if(updateUI) ui.AwakeBehaviour(characters[0]);

		// Awake camera logic behaviour
		if(updateCamera) cameraLogic.AwakeBehaviour(characters[0].transform.Find("Player_CameraTarget"), this);

		// Awake helper logic behaviour
		if(updateHelper) helper.AwakeBehaviour(characters[0].Trans);

		// Awake lighting manager behaviour
		if(updateLightingManager) lightingManager.AwakeBehaviour();

		// Awake all characters behaviours
		if(updateCharacters) for(int i = 0; i < characters.Count; i++) characters[i].AwakeBehaviour(cameraLogic);

		// Awake all rotating objects behaviours
		if(updateRotateObjects) for(int i = 0; i < rotateObjects.Count; i++) rotateObjects[i].AwakeBehaviour();

		// Awake all spike traps behaviours
		if(updateSpikeTraps) for(int i = 0; i < spikeTraps.Count; i++) spikeTraps[i].AwakeBehaviour();

		// Awake all barrier traps behaviours
		if(updateBarrierTraps) for(int i = 0; i < barrierTraps.Count; i++) barrierTraps[i].AwakeBehaviour(characters[0].Trans);

		// Awake all spawn managers behaviours
		if(updateSpawnManagers) for(int i = 0; i < spawnManagers.Count; i++) spawnManagers[i].AwakeBehaviour(this);

		// Awake all dynamic floors behaviours
		if(updateDynamicFloors) for(int i = 0; i < dynamicFloors.Count; i++) dynamicFloors[i].AwakeBehaviour();

		// Awake all puzzles behaviours
		if(updatePuzzles) for(int i = 0; i < puzzles.Count; i++) puzzles[i].AwakeBehaviour();

		// Awake all lightnings logic behaviours
		if(updateLightnings) for(int i = 0; i < lightnings.Count; i++) lightnings[i].AwakeBehaviour();

		// Awake all destroyables behaviours
		if(updateDestroyables) for(int i = 0; i < destroyables.Count; i++) destroyables[i].AwakeBehaviour();

		// Awake all meshes mergers behaviours
		if(updateMeshesMergers) for(int i = 0; i < meshesMergers.Count; i++) meshesMergers[i].AwakeBehaviour();

		// Awake all cinematic cameras behaviours
		if(updatePlatformWeaks) for(int i = 0; i < platformWeaks.Count; i++) platformWeaks[i].AwakeBehaviour();

		// Awake all dissolve logics behaviours
		if(updateDissolveLogics) for(int i = 0; i < dissolveLogics.Count; i++) dissolveLogics[i].AwakeBehaviour();

		// Awake all trigger traps behaviours
		if(updateTriggerTraps) for(int i = 0; i < triggerTraps.Count; i++) triggerTraps[i].AwakeBehaviour();

		// Awake all achievements logics behaviours
		if(updateTriggerTraps) for(int i = 0; i < triggerTraps.Count; i++) triggerTraps[i].AwakeBehaviour();

		// Awake all power pilars logics behaviours
		if(updatePowerPilars) for(int i = 0; i < powerPilars.Count; i++) powerPilars[i].AwakeBehaviour(characters[0]);

		// Awake all achievements logics behaviours
		if(updateAchievements) for(int i = 0; i < achievements.Count; i++) achievements[i].AwakeBehaviour(this);

		// Awake all arena pilars logics behaviours
		if(updateArenaPilars) for(int i = 0; i < arenaPilars.Count; i++) arenaPilars[i].AwakeBehaviour();

		// Awake all trigger ends logics behaviours
		if(updateTriggerEnds) for(int i = 0; i < triggerEnds.Count; i++) triggerEnds[i].AwakeBehaviour();

		// Awake all pilar puzzles logics behaviours
		if(updatePilarPuzzles) for(int i = 0; i < pilarPuzzles.Count; i++) pilarPuzzles[i].AwakeBehaviour();

		// Awake all doors logics behaviours
		if(updateDoors) for(int i = 0; i < doors.Count; i++) doors[i].AwakeBehaviour(moduleManager);

		// Awake all invoke pilars logics behaviours
		if(updateInvokePilars) for(int i = 0; i < invokePilars.Count; i++) invokePilars[i].AwakeBehaviour(characters[0]);

		// Awake all boss logics behaviours
		if(updateBosses) for(int i = 0; i < bosses.Count; i++) bosses[i].AwakeBehaviour(characters[0], cameraLogic);

		// Awake module manager behaviour
		if(updateModuleManager) moduleManager.AwakeBehaviour();

		// Awake all cinematic cameras behaviours
		if(updateCinematicCameras)
		{
			Camera gameplayCam = cameraLogic.GetComponent<Camera>();
			for(int i = 0; i < cinematicCameras.Count; i++) cinematicCameras[i].AwakeBehaviour(gameplayCam);
		}
	}

	private void StartBehaviours()
	{
		// Awake all pickup objects behaviour
		if(updatePickups) for(int i = 0; i < pickups.Count; i++) pickups[i].StartBehaviour();

		if(applyStatus)
		{
			// Call status events to get the last saved scene state
			for(int i = 0; i < statusEvents.Length; i++)
			{
				if(gameManager.GameProgress >= statusEvents[i].Progress) statusEvents[i].EventProgress.Invoke();
			}
		}
	}

	private void UpdateBehaviours()
	{
		// Update game manager behaviour
		gameManager.UpdateBehaviour();

		// Update interface logic behaviour
		if(updateUI) ui.UpdateBehaviour();

		// Update player inputs behaviour once per frame
		if(updateInputs && updateType == UpdateType.DEFAULT) playerChar.GetPlayerInputs();

		switch(state)
		{
			case GameplayStates.GAMEPLAY:
			{
				// Update all characters behaviour
				if(updateCharacters && updateType == UpdateType.DEFAULT) for(int i = 0; i < characters.Count; i++) characters[i].UpdateBehaviour();

				// Update lighting manager behaviour
				if(updateLightingManager) lightingManager.UpdateBehaviour();

				// Update all rotating objects behaviours
				if(updateRotateObjects) for(int i = 0; i < rotateObjects.Count; i++) rotateObjects[i].UpdateBehaviour();

				// Update all pickup objects behaviours
				if(updatePickups) for(int i = 0; i < pickups.Count; i++) pickups[i].UpdateBehaviour();

				// Awake all spike traps behaviours
				if(updateSpikeTraps) for(int i = 0; i < spikeTraps.Count; i++) spikeTraps[i].UpdateBehaviour();

				// Update all barrier traps behaviours
				if(updateBarrierTraps) for(int i = 0; i < barrierTraps.Count; i++) barrierTraps[i].UpdateBehaviour();

				// Update all spawn managers behaviours
				if(updateSpawnManagers) for(int i = 0; i < spawnManagers.Count; i++) spawnManagers[i].UpdateBehaviour();

				// Update all spawn managers behaviours
				if(updateSpawnEnemies) for(int i = 0; i < spawnEnemies.Count; i++) spawnEnemies[i].UpdateBehaviour();

				// Update all spawn managers behaviours
				if(updateDynamicFloors) for(int i = 0; i < dynamicFloors.Count; i++) dynamicFloors[i].UpdateBehaviour();

				// Update all cinematic cameras behaviours
				if(updateCinematicCameras) for(int i = 0; i < cinematicCameras.Count; i++) cinematicCameras[i].UpdateBehaviour();

				// Update all cinematic cameras behaviours
				if(updatePlatformWeaks) for(int i = 0; i < platformWeaks.Count; i++) platformWeaks[i].UpdateBehaviour();

				// Update all puzzles behaviours
				if(updatePuzzles) for(int i = 0; i < puzzles.Count; i++) puzzles[i].UpdateBehaviour();

				// Update all lights logic behaviours
				if(updateLightLogics) for(int i = 0; i < lights.Count; i++) lights[i].UpdateBehaviour();

				// Update all destroyables behaviours
				if(updateDestroyables) for(int i = 0; i < destroyables.Count; i++) destroyables[i].UpdateBehaviour();

				// Update all dissolve logics behaviours
				if(updateDissolveLogics) for(int i = 0; i < dissolveLogics.Count; i++) dissolveLogics[i].UpdateBehaviour();

				// Update all power pilars logics behaviours
				if(updatePowerPilars) for(int i = 0; i < powerPilars.Count; i++) powerPilars[i].UpdateBehaviour();

				// Update all trigger traps behaviours
				if(updateTriggerTraps) for(int i = 0; i < triggerTraps.Count; i++) triggerTraps[i].UpdateBehaviour();

				// Update all achievement logics behaviours
				if(updateAchievements) for(int i = 0; i < achievements.Count; i++) achievements[i].UpdateBehaviour();

				// Update all arena pilars logics behaviours
				if(updateArenaPilars) for(int i = 0; i < arenaPilars.Count; i++) arenaPilars[i].UpdateBehaviour();

				// Update all trigger ends logics behaviours
				if(updateTriggerEnds) for(int i = 0; i < triggerEnds.Count; i++) triggerEnds[i].UpdateBehaviour();

				// Update all pilar puzzles logics behaviours
				if(updatePilarPuzzles) for(int i = 0; i < pilarPuzzles.Count; i++) pilarPuzzles[i].UpdateBehaviour();

				// Update all doors logics behaviours
				if(updateDoors) for(int i = 0; i < doors.Count; i++) doors[i].UpdateBehaviour();

				// Update all invoke pilars logics behaviours
				if(updateInvokePilars) for(int i = 0; i < invokePilars.Count; i++) invokePilars[i].UpdateBehaviour();

				// Update all characters foot detections behaviours
				if(updateCharacters) for(int i = 0; i < characters.Count; i++) characters[i].Movement.UpdateFootDetections();

				// Update helper logic behaviour
				if(updateHelper) helper.UpdateBehaviour();

				// Update all lightnings logic behaviours
				if(updateLightnings) for(int i = 0; i < lightnings.Count; i++) lightnings[i].UpdateBehaviour();

				// Update all boss logics behaviours
				if(updateBosses) for(int i = 0; i < bosses.Count; i++) bosses[i].UpdateBehaviour();

				// Update camera logic behaviour
				if(updateCamera) cameraLogic.UpdateBehaviour(false);

				// Update module manager behaviour
				// if(updateModuleManager) moduleManager.UpdateBehaviour();

			#if DEBUG_BUILD
				// Update debug behaviour if needed
				DebugBehaviour();
			#endif
			} break;
			case GameplayStates.VICTORY:
			{
				// Update all characters behaviour
				if(updateCharacters && updateType == UpdateType.DEFAULT) for(int i = 0; i < characters.Count; i++) characters[i].UpdateBehaviour();
			} break;
			case GameplayStates.DEFEAT:
			{
				// Update all characters behaviour
				if(updateCharacters && updateType == UpdateType.DEFAULT) for(int i = 0; i < characters.Count; i++) characters[i].UpdateBehaviour();
			} break;
			default: break;
		}
	}

	private void UpdateFixedBehaviours()
	{
		// Update player inputs behaviour once per frame
		if(updateInputs && updateType == UpdateType.FIXED) playerChar.GetPlayerInputs();

		switch(state)
		{
			case GameplayStates.GAMEPLAY:
			{
				// Update all characters behaviour
				if(updateCharacters && updateType == UpdateType.FIXED) for(int i = 0; i < characters.Count; i++) characters[i].UpdateBehaviour();
			} break;
			case GameplayStates.DEFEAT:
			{
				// Update all characters behaviour
				if(updateCharacters && updateType == UpdateType.FIXED) for(int i = 0; i < characters.Count; i++) characters[i].UpdateBehaviour();
			} break;
			case GameplayStates.VICTORY:
			{
				// Update all characters behaviour
				if(updateCharacters && updateType == UpdateType.FIXED) for(int i = 0; i < characters.Count; i++) characters[i].UpdateBehaviour();
			} break;
			default: break;
		}
	}
	#endregion

	#region Gameplay Methods
	public void SetCheckPoint()
	{
		// Save check point in game manager
		gameManager.SaveCheckPoint(characters[0].Trans.position, characters[0].Trans.rotation.eulerAngles, cameraLogic.CameraOrbit);

		// Update game manager progress
		gameManager.GameProgress++;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("GameplayManager: new check point set to game progress: " + gameManager.GameProgress);
	#endif
	}

	public void DisableGameplay(int camera)
	{
		// Disable characters and camera update
		cinematicCameras[camera].EnableCinematic();
		updateCharacters = false;
		updateCamera = false;
	}

	public void RestoreGameplay()
	{
		// Enable characters and camera update
		updateCharacters = true;
		updateCamera = true;
	}

	public void UnlockAchievement(int index)
	{
		// Call game manager achievement unlock
		gameManager.SetAchievement(index);
	}
	#endregion

	#region Combat Methods
	public Transform GetClosestFocusedEnemy()
	{
		// Calculate all enemies positions in screen view space (all characters elements ignoring player)
		EnemyElement[] enemyElements = new EnemyElement[characters.Count - 1];

		for(int i = 0; i < enemyElements.Length; i++)
		{
			enemyElements[i] = new EnemyElement();
			enemyElements[i].EnemyTrans = characters[i + 1].Trans;
			enemyElements[i].EnemyDistance = Mathf.Abs(Vector2.Distance((Vector2)cameraLogic.Cam.WorldToScreenPoint(enemyElements[i].EnemyTrans.position), new Vector2(Screen.width / 2, Screen.height / 2)));
		}

		// Sort positions by which is the nearest to the center of screen
		for (int i = 0; i < enemyElements.Length; i++)
        {
			for (int k = i + 1; k < enemyElements.Length; k++)
            {
				if(enemyElements[k].EnemyDistance < enemyElements[i].EnemyDistance)
				{
					EnemyElement temp = enemyElements[k];
					enemyElements[k] = enemyElements[i];
					enemyElements[i] = temp;
				}
            }
        }


        // Return nearest enemy transform component
		return ((enemyElements.Length > 0) ? enemyElements[0].EnemyTrans : null);
	}
	#endregion

	#region Input Methods
	private void CheckInputs()
	{
		// Check pause input
		if(Input.GetButtonDown("Escape"))
		{
			switch(state)
			{
				case GameplayStates.GAMEPLAY: SetPause(); break;
				case GameplayStates.PAUSE: SetGameplay(); break;
				case GameplayStates.SETTINGS: SetPause(); break;
				default: break;
			}
		}
		else if(Input.GetButtonDown("Cancel"))
		{
			switch(state)
			{
				case GameplayStates.PAUSE: SetGameplay(); break;
				case GameplayStates.SETTINGS: SetPause(); break;
				default: break;
			}
		}
	}

	public void SetGameplay()
	{
		if(state != GameplayStates.GAMEPLAY)
		{
			// Set gameplay interface
			ui.SetGameplay();

			// Pause all animators animation and characters delta time to zero
			Animator[] animators = FindObjectsOfType<Animator>();
			for(int i = 0; i < animators.Length; i++) animators[i].speed = 1f;

			// Update gameplay state
			state = GameplayStates.GAMEPLAY;

		#if DEBUG_BUILD
			// Trace debug message
			Debug.Log("GameplayManager: state changed to gameplay");
		#endif
		}
	}

	public void SetPause()
	{
		if(state != GameplayStates.PAUSE)
		{
			// Set pause interface
			ui.SetPause();

			// Pause all animators animation and characters delta time to zero
			Animator[] animators = FindObjectsOfType<Animator>();
			for(int i = 0; i < animators.Length; i++) animators[i].speed = 0f;

			// Update gameplay state
			state = GameplayStates.PAUSE;
		}
	}

	public void SetSettings()
	{
		if(state != GameplayStates.SETTINGS)
		{
			// Set settings interface
			ui.SetSettings();

			// Update gameplay state
			state = GameplayStates.SETTINGS;
		}
	}

	public void SetVictory()
	{
		if(state != GameplayStates.VICTORY)
		{
			// Update gameplay state
			state = GameplayStates.VICTORY;

			// Update current game level
			gameManager.GameLevel++;

			// Reset current game progress
			gameManager.GameProgress = 0;

			ui.SetVictory();

		#if DEBUG_BUILD
			// Trace debug message
			Debug.Log("GameplayManager: state changed to victory");
		#endif
		}
	}

	public void SetDefeat()
	{
		if(state != GameplayStates.DEFEAT)
		{
			// Update gameplay state
			state = GameplayStates.DEFEAT;

			ui.SetDefeat();

		#if DEBUG_BUILD
			// Trace debug message
			Debug.Log("GameplayManager: state changed to defeat");
		#endif
		}
	}

	public void SetSuicide()
	{
		ui.SetRestart();

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("GameplayManager: state changed to defeat");
	#endif
	}

	public void SetRestart()
	{
		// Reset game progress to level first time value
		gameManager.GameProgress = initProgress;
		gameManager.PlayerPosition = playerPosition;
		gameManager.PlayerRotation = playerRotation;
		gameManager.CameraRotation = cameraRotation;

		ui.SetRestart();

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("GameplayManager: state changed to defeat");
	#endif
	}

	public void SetNextLevel()
	{
		if(state != GameplayStates.EXIT)
		{
			// Set exit interface to next level
			ui.SetExit(true);

			// Update gameplay state
			state = GameplayStates.EXIT;

		#if DEBUG_BUILD
			// Trace debug message
			Debug.Log("GameplayManager: state changed to exit for next level");
		#endif
		}
	}

	public void SetExit()
	{
		if(state != GameplayStates.EXIT)
		{
			// Set exit interface to menu menu
			ui.SetExit(false);

			// Update gameplay state
			state = GameplayStates.EXIT;

		#if DEBUG_BUILD
			// Trace debug message
			Debug.Log("GameplayManager: state changed to exit for main menu");
		#endif
		}
	}
	#endregion

	#region List Methods
	private void FixListsReferences()
	{
		if(updateCharacters)
		{
			// Check all characters references
			for(int i = 0; i < characters.Count; i++)
			{
				if(!characters[i]) characters.RemoveAt(i);
			}
		}

		if(updateRotateObjects)
		{
			// Check all rotating objects references
			for(int i = 0; i < rotateObjects.Count; i++)
			{
				if(!rotateObjects[i]) rotateObjects.RemoveAt(i);
			}
		}

		if(updatePickups)
		{
			// Check all pickup objects references
			for(int i = 0; i < pickups.Count; i++)
			{
				if(!pickups[i]) pickups.RemoveAt(i);
			}
		}
	}

	private void DebugBehaviour()
	{
		// Check debug inputs
		if(Input.GetKeyDown(KeyCode.F1)) gameManager.IsDebug = !gameManager.IsDebug;
		if(Input.GetKeyDown(KeyCode.F2)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		if(Input.GetKeyDown(KeyCode.F3)) SceneManager.LoadScene("menu");
	}
	#endregion

	#region Lights Methods
	public void SetLights(int quality)
	{
		switch(quality)
		{
			case 0:		// Very low
			case 1:		// Low
			{
				for(int i = 0; i < lowLights.Length; i++) lowLights[i].enabled = true;
				for(int i = 0; i < mediumLights.Length; i++) mediumLights[i].enabled = false;
				for(int i = 0; i < highLights.Length; i++) highLights[i].enabled = false;
			} break;
			case 2:		// Normal
			case 3:		// High
			{
				for(int i = 0; i < lowLights.Length; i++) lowLights[i].enabled = true;
				for(int i = 0; i < mediumLights.Length; i++) mediumLights[i].enabled = true;
				for(int i = 0; i < highLights.Length; i++) highLights[i].enabled = false;
			} break;
			case 4:		// Very high
			case 5:		// Ultra
			{
				for(int i = 0; i < lowLights.Length; i++) lowLights[i].enabled = true;
				for(int i = 0; i < mediumLights.Length; i++) mediumLights[i].enabled = true;
				for(int i = 0; i < highLights.Length; i++) highLights[i].enabled = true;
			} break;
			default: break;
		}

		// Awake all lights logic behaviours
		if(updateLightLogics) for(int i = 0; i < lights.Count; i++) lights[i].AwakeBehaviour(Camera.main.transform);
	}
	#endregion
#if DEBUG_BUILD
	private void OnGUI()
	{
		if(gameManager.IsDebug)
		{
			// Calculate enemy transform screen position
			Rect rect = new Rect(Screen.width - 200, Screen.height - 12 * 22, 500, 500);

			// Create a default GUI style
			GUIStyle style = new GUIStyle();
			style.alignment = TextAnchor.UpperLeft;
			style.fontSize = 10;
			style.normal.textColor = Color.red;

			int lightsCount = 0;

			for(int i = 0; i < lowLights.Length; i++)
			{
				if(lowLights[i].enabled) lightsCount++;
			}

			for(int i = 0; i < mediumLights.Length; i++)
			{
				if(mediumLights[i].enabled) lightsCount++;
			}

			for(int i = 0; i < highLights.Length; i++)
			{
				if(highLights[i].enabled) lightsCount++;
			}

			// Add data to string value
			string text = "";
			if(updateCharacters) text += "GameplayManager.characters: " + characters.Count + "\n";
			if(updateRotateObjects) text += "GameplayManager.rotateObjects: " + rotateObjects.Count + "\n";
			if(updateSpawnEnemies) text += "GameplayManager.spawnEnemies: " + spawnEnemies.Count + "\n";
			if(updatePlatformWeaks) text += "GameplayManager.platformWeaks: " + platformWeaks.Count + "\n";
			if(updateLightnings) text += "GameplayManager.lightnings: " + lightnings.Count + "\n";
			if(updateDestroyables) text += "GameplayManager.destroyables: " + destroyables.Count + "\n";
			if(updateMeshesMergers) text += "GameplayManager.meshesMergers: " + meshesMergers.Count + "\n";
			if(updateLightLogics) text += "GameplayManager.lights: " + lights.Count + " (enabled: " + lightsCount + ")\n";
			if(updateDissolveLogics) text += "GameplayManager.dissolveLogics: " + dissolveLogics.Count + "\n";
			if(updatePowerPilars) text += "GameplayManager.powerPilars: " + powerPilars.Count + "\n";
			if(updateDynamicFloors) text += "GameplayManager.dynamicFloors: " + dynamicFloors.Count + "\n";
			if(updateTriggerTraps) text += "GameplayManager.triggerTraps: " + triggerTraps.Count + "\n";
			if(updateSpikeTraps) text += "GameplayManager.spikeTraps: " + spikeTraps.Count + "\n";
			if(updatePickups) text += "GameplayManager.pickups: " + pickups.Count + "\n";
			if(updateBarrierTraps) text += "GameplayManager.barrierTraps: " + barrierTraps.Count + "\n";
			if(updateAchievements) text += "GameplayManager.achievements: " + achievements.Count + "\n";
			if(updateArenaPilars) text += "GameplayManager.arenaPilars: " + arenaPilars.Count + "\n";
			if(updateTriggerEnds) text += "GameplayManager.triggerEnds: " + triggerEnds.Count + "\n";
			if(updatePilarPuzzles) text += "GameplayManager.pilarPuzzles: " + pilarPuzzles.Count + "\n";
			if(updateSpawnManagers) text += "GameplayManager.spawnManagers: " + spawnManagers.Count + "\n";
			if(updatePuzzles) text += "GameplayManager.puzzles: " + puzzles.Count + "\n";
			if(updateDoors) text += "GameplayManager.doors: " + doors.Count + "\n";

			// Draw label based on calculated position with some important data values
			GUI.Label(rect, text, style);
		}
	}
#endif
	#region Debug

	#endregion

	#region Properties
	public List<Character> Characters
	{
		get { return characters; }
		set { characters = value; }
	}

	public List<Pickup> Pickups
	{
		get { return pickups; }
		set { pickups = value; }
	}

	public List<SpikeTrap> SpikeTraps
	{
		get { return SpikeTraps; }
		set { SpikeTraps = value; }
	}

	public List<BarrierTrap> BarrierTraps
	{
		get { return barrierTraps; }
		set { barrierTraps = value; }
	}

	public List<SpawnManager> SpawnManagers
	{
		get { return spawnManagers; }
		set { spawnManagers = value; }
	}

	public List<SpawnEnemy> SpawnEnemies
	{
		get { return spawnEnemies; }
		set { spawnEnemies = value; }
	}

	public List<DynamicFloor> DynamicFloors
	{
		get { return dynamicFloors; }
		set { dynamicFloors = value; }
	}

	public List<CinematicCamera> CinematicCameras
	{
		get { return cinematicCameras; }
		set { cinematicCameras = value; }
	}

	public List<Destroyable> Destroyables
	{
		get { return destroyables; }
	}

	public List<PlatformWeak> PlatformWeaks
	{
		get { return platformWeaks; }
	}

	public List<AchievementLogic> Achievements
	{
		get { return achievements; }
	}

	public CameraLogic LogicCamera
	{
		get { return cameraLogic; }
	}

	public int State
	{
		get { return (int)state; }
	}

	public InterfaceManager UI
	{
		get { return ui; }
	}
	#endregion

	#region Serializable
	[System.Serializable]
	public class ProgressEvent
	{
		#region Inspector Attributes
		[SerializeField] private int progress;
		[SerializeField] private UnityEvent eventProgress;
		#endregion

		#region Properties
		public int Progress
		{
			get { return progress; }
		}

		public UnityEvent EventProgress
		{
			get { return eventProgress; }
		}
		#endregion
	}

	[System.Serializable]
	public class EnemyElement
	{
		[SerializeField] private Transform enemyTrans;
		[SerializeField] private float enemyDistance;

		public Transform EnemyTrans
		{
			get { return enemyTrans; }
			set { enemyTrans = value; }
		}

		public float EnemyDistance
		{
			get { return enemyDistance; }
			set { enemyDistance = value; }
		}
	}
	#endregion
}
