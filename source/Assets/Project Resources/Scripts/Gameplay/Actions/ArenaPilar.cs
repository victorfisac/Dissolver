using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ArenaPilar : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Scale")]
	[SerializeField] private AnimationCurve scaleCurve;
	[SerializeField] private float scaleDuration;

	[Header("Visual")]
	[SerializeField] private AnimationCurve colorCurve;
	[SerializeField] private float colorDuration;
	[SerializeField] private MeshRenderer pilarRenderer;
	[SerializeField] private Color endColor;

	[Header("Line")]
	[SerializeField] private LineRenderer lineRenderer;
	[SerializeField] private Color lineColor;
	[SerializeField] private float offsetSpeed;

	[Header("Combat")]
	[SerializeField] private SpawnManager spawner;

	[Header("Events")]
	[SerializeField] private UnityEvent finalEvent;

	[Header("References")]
	[SerializeField] private Transform scaleTrans;
	#endregion

	#region Private Attributes
	private int pilarState;				// Arena pilar behaviour state
	private float timeCounter;			// Arena animation time counter

	// Scale
	private Vector3 finalScale;			// Arena local scale end target

	// Material
	private Material brilliantMat;		// Pilar renderer material reference
	private Color brilliantColor;		// Pilar material brilliant color
	private Material lineMat;			// Line renderer material reference
	private Color transparentColor;		// Line material transparent color
	#endregion

	#region Main Methods
	public void AwakeBehaviour () 
	{
		// Get references
		brilliantMat = pilarRenderer.material;
		brilliantColor = brilliantMat.GetColor("_Color");
		lineMat = lineRenderer.material;
		transparentColor = lineColor;
		transparentColor.a = 0f;
		lineMat.SetColor("_TintColor", transparentColor);

		// Reset transform scale to zero
		finalScale = scaleTrans.localScale;
		scaleTrans.localScale = Vector3.zero;
	}

	public void UpdateBehaviour () 
	{
		switch(pilarState)
		{
			case 1:
			{
				// Update pilar scale based on animation curve
				scaleTrans.localScale = Vector3.Lerp(Vector3.zero, finalScale, scaleCurve.Evaluate(timeCounter / scaleDuration));

				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= scaleDuration)
				{
					// Update pilar state
					pilarState = 2;

					// Enable spawn manager
					spawner.StartSpawner();

					// Reset time counter
					timeCounter = 0f;
				}
			} break;
			case 2:
			{
				if(timeCounter < colorDuration)
				{
					// Update line renderer color based on animation curve
					lineMat.SetColor("_TintColor", Color.Lerp(transparentColor, lineColor, colorCurve.Evaluate(timeCounter / colorDuration)));

					// Update time counter
					timeCounter += Time.deltaTime;
				}

				// Update current line material offset 
				Vector2 currentOffset = lineMat.GetTextureOffset("_MainTex");
				currentOffset.x += offsetSpeed * Time.deltaTime;
				lineMat.SetTextureOffset("_MainTex", currentOffset);
			} break;
			case 3:
			{
				// Update pilar material based on animation curve
				brilliantMat.SetColor("_Color", Color.Lerp(brilliantColor, endColor, colorCurve.Evaluate(timeCounter / colorDuration)));

				// Update line renderer color based on animation curve
				lineMat.SetColor("_TintColor", Color.Lerp(lineColor, transparentColor, colorCurve.Evaluate(timeCounter / colorDuration)));

				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= colorDuration)
				{
					// Reset time counter
					timeCounter = 0f;

					// Invoke final event methods
					finalEvent.Invoke();

					// Reset pilar state
					pilarState = 0;

				#if DEBUG_BUILD
					// Trace debug message
					Debug.Log("ArenaPilar: arena pilar disabled");
				#endif
				}
			} break;
			default: break;
		}
	}
	#endregion

	#region Pilar Methods
	public void EnablePilar()
	{
		// Update pilar state
		pilarState = 1;

		// Reset time counter
		timeCounter = 0f;
	}

	public void DisablePilar()
	{
		// Update pilar state
		pilarState = 3;

		// Reset time counter
		timeCounter = 0f;
	}
	#endregion
}
