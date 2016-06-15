using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Puzzle : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Puzzle")]
	[SerializeField] private PuzzlePiece[] pieces;

	[Header("Events")]
	[SerializeField] private UnityEvent exitEvent;
	#endregion

	#region Private Attributes
	private bool piecesDone;		// All pieces done state
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Initialize values
		for(int i = 0; i < pieces.Length; i++) pieces[i].AwakeBehaviour();
	}

	public void UpdateBehaviour()
	{
		// Update pieces behaviour
		for(int i = 0; i < pieces.Length; i++) pieces[i].UpdatePiece();

		// Reset pieces done state
		piecesDone = true;

		// Check if all conditions are met
		for(int i = 0; i < pieces.Length; i++)
		{
			if(!pieces[i].Done) piecesDone = false;
		}

		// Complete puzzle if conditions are met
		if(piecesDone)
		{
			// Invoke puzzle complete event
			exitEvent.Invoke();

			// Disable all puzzle pieces
			for(int i = 0; i < pieces.Length; i++) pieces[i].enabled = false;

			// Disable puzzle behaviour
			this.enabled = false;
		}
	}
	#endregion

	#region Debug Methods
#if DEBUG_BUILD
	private void OnGUI()
	{
		// Calculate enemy transform screen position
		Vector3 position = Camera.main.WorldToScreenPoint(transform.position);
		Rect rect = new Rect(position.x, Screen.height - position.y - 50, 200, 100);

		if(position.x > 0f && position.x < Screen.width && position.y > 0f && position.y < Screen.height && position.z > 0f)
		{
			// Create a default GUI style
			GUIStyle style = new GUIStyle();
			style.alignment = TextAnchor.UpperLeft;
			style.fontSize = 10;
			style.normal.textColor = new Color (1.0f, 0.0f, 0.0f, ((Vector3.Distance(Camera.main.transform.position, transform.position) < 50f) ? (1 - Vector3.Distance(Camera.main.transform.position, transform.position) / 50f) : 0f));

			int conditions = 0;
			for(int i = 0; i < pieces.Length; i++)
			{
				if(pieces[i].Done) conditions++;
			}

			// Add data to string value
			string text = "";
			text += "Puzzle.conditions: " + conditions + " / " + pieces.Length;

			// Draw label based on calculated position with some important data values
			GUI.Label(rect, text, style);
		}
	}
#endif
	#endregion
}
