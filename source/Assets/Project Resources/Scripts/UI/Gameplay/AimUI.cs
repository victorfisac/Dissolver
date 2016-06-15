using UnityEngine;
using System.Collections;

public class AimUI : MonoBehaviour 
{
	#region Enums
	public enum AimStates { NONE, AIMUP, AIMDOWN, AIMING };
	#endregion

	#region Inspector Attributes
	[Header("Aim")]
	[SerializeField] private AnimationCurve aimCurve;
	[SerializeField] private float aimDuration;

	[Header("Shot")]
	[SerializeField] private AnimationCurve shotCurve;
	[SerializeField] private float shotDuration;

	[Header("References")]
	[SerializeField] private Transform baseImage;
	[SerializeField] private Transform pointsImage;
	#endregion

	#region Private Attributes
	// States
	private AimStates state;					// Aiming interface state

	// Aiming
	private Vector3 aimEndPos;					// Aiming end position
	private float aimCounter;					// Aim time counter

	// Shot
	private bool isShooting;					// Is shooting state
	private float shotCounter;					// Shooting time counter

	// References
	private BulletCombat playerBulletCombat;	// Player bullet combat reference
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Get references
		playerBulletCombat = GameObject.FindWithTag("Player").GetComponent<BulletCombat>();

		// Initialize values
		aimEndPos = baseImage.localScale;
		baseImage.localScale = Vector3.zero;
	}

	public void UpdateBehaviour()
	{
		// Update aiming state
		Aim(playerBulletCombat.Aiming);

		switch(state)
		{
			case AimStates.AIMUP:
			{
				// Update base image scale based on animation curve
				baseImage.localScale = Vector3.Lerp(Vector3.zero, aimEndPos, aimCurve.Evaluate(aimCounter / aimDuration));

				// Update time counter
				aimCounter += Time.deltaTime;

				if(aimCounter >= aimDuration) state = AimStates.AIMING;
			} break;
			case AimStates.AIMDOWN:
			{
				// Update base image scale based on animation curve
				baseImage.localScale = Vector3.Lerp(aimEndPos, Vector3.zero, aimCurve.Evaluate(aimCounter / aimDuration));

				// Update time counter
				aimCounter += Time.deltaTime;

				if(aimCounter >= aimDuration)
				{
					// Reset state value
					state = AimStates.NONE;

					// Disable aiming game object
					baseImage.gameObject.SetActive(false);
				}
			} break;
			default: break;
		}

		if(isShooting)
		{
			// Update base image scale based on animation curve
			pointsImage.localScale = Vector3.one * shotCurve.Evaluate(shotCounter / shotDuration);

			// Update time counter
			shotCounter += Time.deltaTime;

			// Reset is shooting state if animation has finished
			if(shotCounter >= shotDuration) isShooting = false;
		}
	}
	#endregion

	#region Input Methods
	public void Aim(bool aiming)
	{
		if(state == AimStates.NONE)
		{
			if(aiming)
			{
				// Update aim state
				state = AimStates.AIMUP;

				// Reset aim counter
				aimCounter = 0f;

				// Disable aiming game object
				baseImage.gameObject.SetActive(true);
			}
		}
		else if(state == AimStates.AIMING || state == AimStates.AIMUP)
		{
			if(!aiming)
			{
				// Update aim state
				state = AimStates.AIMDOWN;

				// Reset aim counter
				aimCounter = 0f;
			}
			else if(Input.GetButtonDown("Fire1")) Shot();
		}
	}

	public void Shot()
	{
		// Update is shooting state
		isShooting = true;

		// Reset shooting counter
		shotCounter = 0f;
	}
	#endregion
}
