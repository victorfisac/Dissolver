using UnityEngine;
#if UNITY_EDITOR
	using UnityEditor;
#endif
using System.Collections;

public class LightLogic : MonoBehaviour
{
	#region Enums
	public enum LightStates { FAR, RANGE, CLOSE };
	#endregion

    #region Inspector Attributes
    [Header("Attenuation")]
	[SerializeField] private float minValue;
	[SerializeField] private float maxValue;

	[Header("Animation")]
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private Vector2 durationLimits;

	[Header("Optimization")]
	[SerializeField] private bool disableOptimization;
	[SerializeField] private Vector2 distanceRange;   

    [Header("References")]
	[SerializeField] private Light pointLight;
    #endregion

    #region Private Attributes
    private LightStates state;			// Current light state
    private float timeCounter;			// Light animation time counter
    private float duration;				// Current animation duration
    private Transform cameraTrans;		// Main camera transform reference
    private bool canWork;				// Light logic can work state
    private Transform trans;			// Point light transform reference
    private float currentDistance;		// Camera and light distance
    private float lerpValue;			// Range state current lerp value
    #endregion

    #region Main Methods
    public void AwakeBehaviour(Transform cam)
    {
    	// Initialize values
    	trans = transform;
    	duration = Random.Range(durationLimits.x, durationLimits.y);
    	cameraTrans = cam;
		canWork = pointLight.enabled;
    }

	public void UpdateBehaviour ()
    {
    	if(canWork)
    	{
			if(disableOptimization)
			{
				// Update light intensity based on animation curve
				pointLight.intensity = Mathf.Lerp(minValue, maxValue, curve.Evaluate(timeCounter / duration));

				// Update time counter
		        timeCounter += Time.deltaTime;

		        // Reset time counter to loop animation
				if(timeCounter >= duration)
				{
					// Reset time counter
					timeCounter = 0f;

					// Calculate a new animation duration between duration limits
					duration = Random.Range(durationLimits.x, durationLimits.y);
				}
			}
			else
			{
		    	switch(state)
		    	{
		    		case LightStates.FAR:
		    		{
		    			if(Vector3.Distance(trans.position, cameraTrans.position) < distanceRange.y) SetRange();
					} break;
		    		case LightStates.RANGE:
		    		{
						if(Vector3.Distance(trans.position, cameraTrans.position) < distanceRange.x) SetClose();
						else if(Vector3.Distance(trans.position, cameraTrans.position) > distanceRange.y) SetFar();

						// Update current distance between camera and light value
						currentDistance = Vector3.Distance(trans.position, cameraTrans.position);

						// Rescale values to normalized
						lerpValue = 1f / (distanceRange.y - distanceRange.x) * (currentDistance - distanceRange.y) + 1f;

						// Apply value to point light intensity
						pointLight.intensity = Mathf.Lerp(maxValue, 0f, lerpValue);
					} break;
		    		case LightStates.CLOSE:
		    		{
						if(Vector3.Distance(trans.position, cameraTrans.position) > distanceRange.x) SetRange();

				    	// Update light intensity based on animation curve
						pointLight.intensity = Mathf.Lerp(minValue, maxValue, curve.Evaluate(timeCounter / duration));

						// Update time counter
				        timeCounter += Time.deltaTime;

				        // Reset time counter to loop animation
						if(timeCounter >= duration)
						{
							// Reset time counter
							timeCounter = 0f;

							// Calculate a new animation duration between duration limits
							duration = Random.Range(durationLimits.x, durationLimits.y);
						}
					} break;
		    		default: break;
		    	}
		    }
	    }
    }
    #endregion

    #region Light Methods
    private void SetFar()
    {
    	// Disable point light
    	pointLight.enabled = false;

    	// Update current light state
    	state = LightStates.FAR;
    }

    private void SetRange()
    {
		// Enable point light
    	pointLight.enabled = true;

		// Update current light state
    	state = LightStates.RANGE;
    }

    private void SetClose()
    {
		// Update current light state
    	state = LightStates.CLOSE;

    	// Reset time counter
    	timeCounter = 0f;
    }
    #endregion

    #region Debug Method
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if(state != LightStates.FAR || Selection.activeGameObject == this.gameObject)
		{
			Gizmos.color = Color.red;
	        Gizmos.DrawWireSphere(transform.position, distanceRange.x);

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, distanceRange.y);
		}
	}
#endif
    #endregion
}
