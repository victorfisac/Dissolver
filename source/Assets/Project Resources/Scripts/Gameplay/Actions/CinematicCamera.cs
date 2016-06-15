using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class CinematicCamera : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private bool blockPlayer;
	[SerializeField] private float duration;

	[Header("Events")]
	[SerializeField] private UnityEvent endEvent;

	[Header("References")]
	[SerializeField] private GameplayManager gameplayManager;
	#endregion

	#region Private Attributes
	private bool enabledCamera;				// Camera enabled state
	private float timeCounter;				// Camera disable time counter
	private Camera gameplayCamera;			// Gameplay camera reference
	private PlayerCharacter playerChar;		// Player character reference
	#endregion

	#region Main Methods
	public void AwakeBehaviour(Camera camera)
	{
		// Get references
		gameplayCamera = camera;
		playerChar = gameplayManager.Characters[0] as PlayerCharacter;
	}

	public void UpdateBehaviour()
	{
		if(enabledCamera)
		{
			// Update time counter
			timeCounter += Time.deltaTime;

			if(timeCounter >= duration)
			{
				// Reset time counter
				timeCounter = 0f;

				DisableCinematic();
			}
		}
	}
	#endregion

	#region Input Methods
	public void EnableCinematic()
	{
		// Update enabled camera state
		enabledCamera = true;

		// Disable main camera for performance
		gameplayCamera.enabled = false;

		// Disable player inputs if needed
		if(blockPlayer) playerChar.SetPlay(false);

		// Disable interface manager gameplay UI
		gameplayManager.UI.ShowGameplayUI(false);

		// Enable camera game object
		gameObject.SetActive(true);
	}

	public void DisableCinematic()
	{
		// Reset enabled camera state
		enabledCamera = false;

		// Restore gameplay behaviour
		gameplayManager.RestoreGameplay();

		// Enable player inputs if needed
		if(blockPlayer) playerChar.SetPlay(true);

		// Enable main camera to continue gameplay
		gameplayCamera.enabled = true;

		// Enable interface manager gameplay UI
		gameplayManager.UI.ShowGameplayUI(true);

		// Invoke all end event methods
		endEvent.Invoke();

		// Disable camera game object
		gameObject.SetActive(false);
	}
	#endregion
}
