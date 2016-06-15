using UnityEngine;
using System.Collections;

public class SpawnEnemy : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private string detectionLayer;
	[SerializeField] private float distance;

	[Header("Motion")]
	[SerializeField] private AnimationCurve motionCurve;
	[SerializeField] private float motionDuration;

	[Header("Spawn")]
	[SerializeField] private GameObject spawn;

	[Header("References")]
	[SerializeField] private Transform trans;
	[SerializeField] private Collider coll;
	[SerializeField] private GameObject visualObject;
	[SerializeField] private GameObject explosionObject;
	[SerializeField] private AudioSource spawnSource;
	#endregion

	#region Private Attributes
	private GameplayManager game;			// Gameplay manager reference
	private float motionCounter;			// Motion time counter
	private SpawnManager spawnManager;		// Spawn manager reference
	#endregion

	#region Main Methods
	public void Initialize(GameplayManager gameplayManager, SpawnManager spawner)
	{
		// Get references
		spawnManager = spawner;

		// Initialize values
		game = gameplayManager;
		trans.position += Vector3.up * distance;

		// Enable work behaviour
		coll.enabled = true;
	}

	public void UpdateBehaviour()
	{
		transform.position += Vector3.down * motionCurve.Evaluate(motionCounter / motionDuration);

		// Update motion counter
		motionCounter += Time.deltaTime;
	}
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.layer == LayerMask.NameToLayer(detectionLayer))
		{
			// Disable collider to avoid enemies strange collision response
			coll.enabled = false;

			// Spawn enemy game object
			GameObject newEnemy = (GameObject)Instantiate(spawn, transform.position, transform.rotation);

			// Get new enemy character reference
			Character newEnemyCharacter = newEnemy.GetComponent<Character>();

			if(newEnemyCharacter)
			{
				// Add new character to gameplay manager list
				game.Characters.Add(newEnemyCharacter);

				// Add new character to spawner manager list
				spawnManager.SpawnedCharac.Add(newEnemyCharacter);

				// Initialize new enemy character
				newEnemyCharacter.AwakeBehaviour(game.LogicCamera);
			}

			// Remove spawner from spawn manager spawners list
			spawnManager.Spawners.Remove(this);

			// Remove spawn enemy from gameplay manager list
			game.SpawnEnemies.Remove(this);

			// Parent new enemy to spawn manager transform
			newEnemy.transform.parent = spawnManager.transform;

			// Disable spawner visual and enable spawn explosion
			visualObject.SetActive(false);
			explosionObject.SetActive(true);

			// Play spawn sound
			spawnSource.Play();

			// Destroy enemy spawner
			Destroy(gameObject, 2f);
		}
	}
	#endregion
}
