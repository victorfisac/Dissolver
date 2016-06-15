using UnityEngine;
using System.Collections;

public class BarrierTrap : MonoBehaviour 
{
	#region Enums
	public enum BarrierType { SCALE, DISSOLVE, BOTH };
	public enum AxisType { X, Y, Z };
	#endregion

	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private BarrierType type;
	[SerializeField] private bool playOnAwake;
	[SerializeField] private float damage;
	[SerializeField] private float damageForce;
	[SerializeField] private Collider physicColl;

	[Header("Scale")]
	[SerializeField] private AnimationCurve scaleCurve;
	[SerializeField] private float scaleDuration;
	[SerializeField] private Transform trans;

	[Header("Dissolve")]
	[SerializeField] private AnimationCurve dissolveCurve;
	[SerializeField] private Vector2 dissolveLimits;
	[SerializeField] private Renderer[] renderers; 

	[Header("Eye")]
	[SerializeField] private AxisType axis;
	[SerializeField] private AnimationCurve positionCurve;
	[SerializeField] private float positionDuration;
	[SerializeField] private Transform eyeTrans;
	[SerializeField] private Animator eyeAnimator;
	[SerializeField] private Transform pupil;
	#endregion

	#region Private Attributes
	// State
	private int state;						// Barrier trap state
	private Vector3 endScale;				// End animation scale
	private float scaleCounter;				// Scale animation time counter

	// Dissolver
	private float dissolverCounter;			// Dissolver time counter

	// Eye
	private float eyeCounter;				// Eye animation time counter
	private Vector3 eyePositionInit;		// Eye position at start
	private Transform playerTrans;			// Player transform reference
	private bool isCollision;				// Trap collision state
	#endregion

	#region Main Methods
	public void AwakeBehaviour(Transform newTrans)
	{
		// Get references
		if(!physicColl) physicColl = GetComponent<Collider>();

		// Initialize values
		state = -1;
		endScale = trans.localScale;
		if(type == BarrierType.SCALE || type == BarrierType.BOTH) trans.localScale = Vector3.zero;
		playerTrans = newTrans;

		if(eyeTrans)
		{
			eyePositionInit = eyeTrans.localPosition;
			eyeTrans.gameObject.SetActive(false);
		}

		if(playOnAwake) FadeIn();
	}

	public void UpdateBehaviour()
	{
		switch(state)
		{
			case 0:
			{
				switch(type)
				{
					case BarrierType.SCALE:
					{
						// Update transform scale based on animation curve
						trans.localScale = Vector3.Lerp(Vector3.zero, endScale, scaleCurve.Evaluate(scaleCounter / scaleDuration));
						if(eyeTrans) eyeTrans.localScale = Vector3.Lerp(Vector3.zero, endScale, scaleCurve.Evaluate(scaleCounter / scaleDuration));
					} break;
					case BarrierType.DISSOLVE:
					{
						// Update dissolve amount based on animation curve
						for(int i = 0; i < renderers.Length; i++) renderers[i].material.SetFloat("_DissolveAmount", Mathf.Lerp(dissolveLimits.x, dissolveLimits.y, dissolveCurve.Evaluate(scaleCounter / scaleDuration)));
					} break;
					case BarrierType.BOTH:
					{
						// Update transform scale based on animation curve
						trans.localScale = Vector3.Lerp(Vector3.zero, endScale, scaleCurve.Evaluate(scaleCounter / scaleDuration));
						if(eyeTrans) eyeTrans.localScale = Vector3.Lerp(Vector3.zero, endScale, scaleCurve.Evaluate(scaleCounter / scaleDuration));

						// Update dissolve amount based on animation curve
						for(int i = 0; i < renderers.Length; i++) renderers[i].material.SetFloat("_DissolveAmount", Mathf.Lerp(dissolveLimits.x, dissolveLimits.y, dissolveCurve.Evaluate(scaleCounter / scaleDuration)));
					} break;
					default: break;
				}

				// Update dissolver counter
				scaleCounter += Time.deltaTime;

				if(scaleCounter >= scaleDuration)
				{
					// Update trap state
					state = 1;

					// Reset scale counter
					scaleCounter = 0f;
				}
			} break;
			case 2:
			{
				switch(type)
				{
					case BarrierType.SCALE:
					{
						// Update transform scale based on animation curve
						trans.localScale = Vector3.Lerp(endScale, Vector3.zero, scaleCurve.Evaluate(scaleCounter / scaleDuration));
						if(eyeTrans) eyeTrans.localScale = Vector3.Lerp(endScale, Vector3.zero, scaleCurve.Evaluate(scaleCounter / scaleDuration));
					} break;
					case BarrierType.DISSOLVE:
					{
						// Update dissolve amount based on animation curve
						for(int i = 0; i < renderers.Length; i++) renderers[i].material.SetFloat("_DissolveAmount", Mathf.Lerp(dissolveLimits.y, dissolveLimits.x, dissolveCurve.Evaluate(scaleCounter / scaleDuration)));
					} break;
					case BarrierType.BOTH:
					{
						// Update transform scale based on animation curve
						trans.localScale = Vector3.Lerp(endScale, Vector3.zero, scaleCurve.Evaluate(scaleCounter / scaleDuration));
						if(eyeTrans) eyeTrans.localScale = Vector3.Lerp(endScale, Vector3.zero, scaleCurve.Evaluate(scaleCounter / scaleDuration));

						// Update dissolve amount based on animation curve
						for(int i = 0; i < renderers.Length; i++) renderers[i].material.SetFloat("_DissolveAmount", Mathf.Lerp(dissolveLimits.y, dissolveLimits.x, dissolveCurve.Evaluate(scaleCounter / scaleDuration)));
					} break;
					default: break;
				}

				// Update dissolver counter
				scaleCounter += Time.deltaTime;

				if(scaleCounter >= scaleDuration)
				{
					// Reset state value
					state = -1;

					// Disable physic collision collider
					physicColl.enabled = false;

					// Reset scale counter
					scaleCounter = 0f;
				}
			} break;
			default: break;
		}

		if(eyeTrans)
		{
			// Update eye position based on animation curve
			switch(axis)
			{
				case AxisType.X: eyeTrans.localPosition = eyePositionInit + Vector3.right * positionCurve.Evaluate(eyeCounter / positionDuration); break;
				case AxisType.Y: eyeTrans.localPosition = eyePositionInit + Vector3.up * positionCurve.Evaluate(eyeCounter / positionDuration); break;
				case AxisType.Z: eyeTrans.localPosition = eyePositionInit + Vector3.forward * positionCurve.Evaluate(eyeCounter / positionDuration); break;
				default: break;
			}

			// Update eye time counter
			eyeCounter += Time.deltaTime;

			if(eyeCounter >= positionDuration) eyeCounter = 0f;
		}

		if(pupil)
		{
			// Update pupil transform rotation to look player position
			pupil.LookAt(playerTrans.position + Vector3.up);
		}
	}
	#endregion

	#region Input Methods
	public void FadeIn()
	{
		// Update state value
		state = 0;

		// Enable physic collision collider
		physicColl.enabled = true;

		// Enable eye game object
		if(eyeTrans) eyeTrans.gameObject.SetActive(true);

		// Restore eye animator state
		if(eyeAnimator) eyeAnimator.SetTrigger("Restore");
	}

	public void FadeOut()
	{
		// Update state value
		state = 2;

		// Set eye animator dead state
		if(eyeAnimator) eyeAnimator.SetTrigger("Dead");
	}
	#endregion

	#region Detection Methods
	public void OnCollisionEnter(Collision other)
	{
		if(!isCollision)
		{
			// Check if detected collision object is from a character
			if(other.gameObject.layer == LayerMask.NameToLayer("Shatters"))
			{
				// Check if main game object is player
				if(other.transform.root.tag == "Player")
				{
					// Make damage to character and move it backwards
					other.transform.root.gameObject.GetComponent<Character>().SetDamage(damage, Vector3.Scale((other.transform.root.position - trans.position).normalized, new Vector3(1, 0, 1)), damageForce, null);

					// Update collision state
					isCollision = true;
				}
			}
		}
	}

	public void OnCollisionExit(Collision other)
	{
		// Reset is collision state
		if(isCollision) isCollision = false;
	}
	#endregion
}
