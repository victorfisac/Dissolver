using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class AchievementLogic : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Motion")]
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float duration;

	[Header("Events")]
	[SerializeField] private UnityEvent achievementEvent;

	[Header("References")]
	[SerializeField] private Transform trans;
	#endregion

	#region Private Attributes
	private float timeCounter;					// Scale animation time counter
	private Vector3 initScale;					// Transform local scale at start
	private int state;							// Achievement state
	private GameplayManager gameplayManager;	// Gameplay manager reference
	#endregion

	#region Main Methods
	public void AwakeBehaviour (GameplayManager manager) 
	{
		// Initialize values
		initScale = trans.localScale;
		gameplayManager = manager;
	}

	public void UpdateBehaviour () 
	{
		switch(state)
		{
			case 1:
			{
				// Update local scale based on animation curve
				trans.localScale = Vector3.Lerp(initScale, Vector3.zero, curve.Evaluate(timeCounter / duration));

				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= duration)
				{
					// Reset time counter
					timeCounter = 0f;

					// Remove reference from gameplay manager achievement list
					gameplayManager.Achievements.Remove(this);

					// Destroy game object
					Destroy(gameObject);
				}
			} break;
			default: break;
		}
	}
	#endregion

	#region Achievement Methods
	public void GetAchievement()
	{
		// Invoke all achievement event methods
		achievementEvent.Invoke();

		// Update achievement state
		state = 1;
	}
	#endregion
}
