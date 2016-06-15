using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private bool alwaysEnabled;
	[SerializeField] private float[] totalEnemies;
	[SerializeField] private float floorDistance;
	[SerializeField] private bool infinite;

	[Header("Enemies")]
	[SerializeField] private GameObject[] spawner;
	[SerializeField] private bool randomSpawn;
	[SerializeField] private int specificSpawn;

	[Header("Events")]
	[SerializeField] private UnityEvent startEvent;
	[SerializeField] private UnityEvent endEvent;

	[Header("References")]
	[SerializeField] private Transform trans;
	#endregion

	#region Private Attributes
	private float timeCounter;					// Spawn time counter
	private int enemiesCount;					// Total spawned enemies
	private int spawnCount;						// Spawn positions current index
	private int maxSpawnCount;					// Total spawn positions
	private GameplayManager gameplayManager;	// Gameplay manager reference
	private bool canWork;						// Spawner can work state
	private List<Character> spawnedCharac;		// Spawned characters references list
	private List<SpawnEnemy> spawners;			// Spawners references list
	#endregion

	#region Main Methods
	public void AwakeBehaviour(GameplayManager gameplay)
	{
		// Get references
		spawnedCharac = new List<Character>();
		spawners = new List<SpawnEnemy>();
		gameplayManager = gameplay;
		maxSpawnCount = trans.childCount;
	}

	public void UpdateBehaviour()
	{
		if(canWork)
		{
			CheckSpawnedList();

			// Check if all enemies was already spawned
			if(enemiesCount < totalEnemies.Length)
			{
				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= totalEnemies[enemiesCount])
				{
					// Find current spawner transform reference
					Transform spawnTrans = trans.FindChild("SpawnManager_Position" + spawnCount.ToString("00"));

					if(spawnTrans)
					{
						// Spawn new enemy
						GameObject newSpawner = (GameObject)Instantiate((randomSpawn ? spawner[Random.Range((int)0, (int)spawner.Length)] : spawner[specificSpawn]), spawnTrans.position, spawnTrans.rotation);

						// Get new spawn enemy reference
						SpawnEnemy newSpawnEnemy = newSpawner.GetComponent<SpawnEnemy>();

						// Add new spawner to spawners list
						spawners.Add(newSpawnEnemy);

						// Initialize new enemy spawner
						newSpawnEnemy.Initialize(gameplayManager, this);

						// Add new enemy spawner to gameplay manager list
						gameplayManager.SpawnEnemies.Add(newSpawnEnemy);

						// Parent new spawn enemy to spawn manager transform
						newSpawner.transform.parent = trans;
					}
				#if DEBUG_BUILD
					else Debug.LogWarning("SpawnManager: cannot find spawn position: " + enemiesCount.ToString("00"));
				#endif

					// Update enemies count
					enemiesCount++;

					// Update current spawn position index
					spawnCount++;

					// Reset spawn count index if it is out of bounds
					if(spawnCount > maxSpawnCount) spawnCount = 0;

					// Reset time counter
					timeCounter = 0f;
				}
			}
			else
			{
				if(infinite) enemiesCount = 0;
				else
				{
					// Check if all enemies was defeat
					if(spawnedCharac.Count == 0 && spawners.Count == 0)
					{
					#if DEBUG_BUILD
						// Trace debug message
						Debug.Log("SpawnManager: total enemies from " + gameObject.name + " have been spawned");
					#endif

						// Remove spawn manager from gameplay manager
						if(!alwaysEnabled) gameplayManager.SpawnManagers.Remove(this);

						// Invoke spawn ending event
						endEvent.Invoke();

						// Update can work state
						canWork = false;
					}
				}
			}
		}
	}
	#endregion

	#region Spawner Methods
	private void CheckSpawnedList()
	{
		for(int i = 0; i < spawnedCharac.Count; i++)
		{
			if(!spawnedCharac[i]) spawnedCharac.RemoveAt(i);
		}

		for(int i = 0; i < spawners.Count; i++)
		{
			if(!spawners[i]) spawners.RemoveAt(i);
		}
	}

	public void StartSpawner()
	{
		// Update can work state
		canWork = true;

		// Invoke spawn starting event
		startEvent.Invoke();
	}

	public void StopSpawner()
	{
		// Update can work state
		canWork = false;

		// Reset spawner values
		enemiesCount = 0;
		spawnCount = 0;

		// Invoke spawn starting event
		endEvent.Invoke();
	}
	#endregion

	#region Editor Methods
	public void AlignSpawners()
	{
		for(int i = 0; i < trans.childCount; i++)
		{
			RaycastHit hit;

			if(Physics.Raycast(trans.GetChild(i).position, Vector3.down, out hit, 100f))
			{
				// Update child position to get aligned
				trans.GetChild(i).position = new Vector3(trans.GetChild(i).position.x, hit.point.y + floorDistance, trans.GetChild(i).position.z);
			}
		}
	}
	#endregion

	#region Properties
	public int EnemiesCount
	{
		get { return enemiesCount; }
	}

	public int TotalEnemies
	{
		get { return totalEnemies.Length; }
	}

	public List<Character> SpawnedCharac
	{
		get { return spawnedCharac; }
	}

	public List<SpawnEnemy> Spawners
	{
		get { return spawners; }
	}
	#endregion
}
