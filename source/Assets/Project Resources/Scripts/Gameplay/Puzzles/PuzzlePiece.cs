using UnityEngine;
using System.Collections;

public class PuzzlePiece : MonoBehaviour
{
	#region Inspector Attributes
	[Header("Puzzle Target")]
	[SerializeField] protected GameObject target;

	[Header("Visual")]
	[SerializeField] private string colorName;
	[SerializeField] private Color pieceColor;
	[SerializeField] private MeshRenderer[] renderers;

	[Header("Time")]
	[SerializeField] protected AnimationCurve timeLerpCurve;
	[SerializeField] protected float timeLerpDuration;
	#endregion

	#region Private Attributes
	// Conditions
	protected bool done;				// Current conditions done state

	// Time
	protected float timeScale;			// Current time scale
	protected float desiredTimeScale;	// Current desired time scale for interpolation
	protected float lastTimeScale;		// Last stored time scale for interpolation
	protected float timeDuration;		// Time scale state duration
	protected float timeCounter;		// Time scale state time counter
	protected float timeLerpCounter;	// Time interpolation counter
	#endregion

	#region Main Methods
	public virtual void AwakeBehaviour()
	{
		// Initialize values
		timeScale = 1f;
		desiredTimeScale = timeScale;
		lastTimeScale = timeScale;
		for(int i = 0; i < renderers.Length; i++) renderers[i].material.SetColor(colorName, pieceColor);
	}

	public virtual void UpdatePiece()
	{
		if(timeDuration > 0f)
		{
			// Update time scale state counter
			timeCounter += Time.deltaTime;

			if(timeCounter >= timeDuration) ResetTimeScale();
		}

		if(timeScale != desiredTimeScale)
		{
			// Update time scale interpolation
			timeScale = Mathf.Lerp(lastTimeScale, desiredTimeScale, timeLerpCurve.Evaluate(timeLerpCounter / timeLerpDuration));

			// Update time scale counter
			timeLerpCounter += Time.deltaTime;
		}
		else timeLerpCounter = 0f;
	}
	#endregion

	#region Time Methods
	public void SetTimeScale(float time, float scale)
	{
		// Update time scale timer duration
		timeDuration += time;

		// Store previous time scale
		lastTimeScale = timeScale;

		// Update time scale
		desiredTimeScale = scale;
	}

	protected void ResetTimeScale()
	{
		// Store previous time scale
		lastTimeScale = timeScale;

		// Reset time lerp counter
		timeLerpCounter = 0f;

		// Reset current time scale
		desiredTimeScale = 1f;

		// Reset time scale time counter
		timeCounter = 0f;

		// Reset time scale duration
		timeDuration = 0f;
	}
	#endregion

	#region Detection Methods
	private void OnCollisionEnter(Collision other)
	{
		if(other.gameObject == target) done = true;
	}

	private void OnCollisionExit(Collision other)
	{
		if(other.gameObject == target) done = false;
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

			// Add data to string value
			string text = "";
			text += "PuzzlePiece.timeScale: " + timeScale + "\n";
			text += "PuzzlePiece.timeCounter: " + timeCounter + "\n";
			text += "PuzzlePiece.timeDuration: " + timeDuration;

			// Draw label based on calculated position with some important data values
			GUI.Label(rect, text, style);
		}
	}
#endif
	#endregion

	#region Properties
	public bool Done
	{
		get { return done; }
	}

	public float DeltaTime
	{
		get { return Time.deltaTime * timeScale; }
	}
	#endregion
}
