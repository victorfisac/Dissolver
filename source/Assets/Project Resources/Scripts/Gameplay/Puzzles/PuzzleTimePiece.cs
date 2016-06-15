using UnityEngine;
using System.Collections;

public class PuzzleTimePiece : PuzzlePiece
{
	#region Inspector Attributes
	[Header("Destroy")]
	[SerializeField] private float destroyTimer;
	#endregion

	#region Private Attributes
	// Destroy
	private Vector3 initPosition;		
	private float destroyCounter;		// Destroy time counter
	#endregion

	#region Main Methods
	public override void AwakeBehaviour()
	{
		// Call base class Awake method
		base.AwakeBehaviour();

		// Initialize values
		initPosition = transform.position;
	}

	public override void UpdatePiece()
	{
		// Call base class Update method
		base.UpdatePiece();

		if(done)
		{
			// Update destroy counter
			destroyCounter += DeltaTime;

			if(destroyCounter >= destroyTimer)
			{
				// Reset position
				transform.position = initPosition;

				// Reset destroy counter
				destroyCounter = 0f;

				// Reset done value
				done = false;
			}
		}
	}
	#endregion
}
