using UnityEngine;
using System.Collections;

public class EnemyFly : Enemy 
{
	#region Inspector Attributes
	[Header("Align")]
	[SerializeField] private float alignDuration;

	[Header("Spawn")]
	[SerializeField] private float groundDistance;
	[SerializeField] private LayerMask groundMask;
	#endregion

	#region Private Attributes
	private float alignCounter;		// Align behaviour time counter
	private RaycastHit hit;			// Align raycast hit reference
	#endregion

	#region Main Methods
	public override void AwakeBehaviour(CameraLogic cameraLogic)
	{
		// Call base class Awake method
		base.AwakeBehaviour(cameraLogic);

		// Enable start dissolver event
		feedback.EnableEvent(2);
	}
	#endregion

	#region Behaviour Methods
	public override void DetectionBehaviour()
	{
		AlignGround();

		// Check state transitions
		if(nearTarget) SetFollowing();
		else if(backToPosition)
		{
			// Calculate move direction and make it run
			move = (initPosition - trans.position).normalized;
			move *= 0.5f;
		}

		// Call base class DetectionBehaviour method
		base.DetectionBehaviour();
	}

	public override void FollowingBehaviour()
	{
		AlignGround();

		// Check state transitions
		if(!nearTarget) SetDetection();
		else
		{
			if(currentDistance < attackDistance) SetGuard();
			else
			{
				// Calculate move direction and make it run
				move = (nearTarget.Trans.position - trans.position).normalized;
				move *= 0.5f;
			}
		}

		// Call base class FollowingBehaviour method
		base.FollowingBehaviour();
	}

	public override void  GuardBehaviour()
	{
		// Reset attack state
		attack = false;

		if(nearTarget) transform.LookAt(nearTarget.Trans.position + Vector3.up);

		// Check state transitions
		if(currentDistance > attackDistance * 1.25f) SetFollowing();
		else
		{
			// Update attack counter
			attackCounter += DeltaTime;
			if(attackCounter >= attackTimer) SetAttack();
		}

		// Call base class GuardBehaviour method
		base.GuardBehaviour();
	}

	public override void  AttackBehaviour()
	{
		// Return to guard state
		SetGuard();

		// Call base class AttackBehaviour method
		base.AttackBehaviour();
	}

	public override void  DamageBehaviour()
	{
		// Update damage counter
		damageCounter += DeltaTime;
		if(damageCounter >= Combat.DamageTimer) SetFollowing();

		// Call base class DamageBehaviour method
		base.DamageBehaviour();
	}
	public override void  DieBehaviour()
	{
		// Call base class DieBehaviour method
		base.DieBehaviour();
	}
	#endregion

	#region Set Methods
	public override void SetDetection()
	{
		// Reset move direction vector
		if(!backToPosition) move = Vector3.zero;

		// Call base class SetDetection method
		base.SetDetection ();
	}

	public override void SetFollowing()
	{
		// Reset character movement can move state
		movement.CanMove = true;

		// Reset attack counter
		attackCounter = 0f;

		// Call base class SetFollowing method
		base.SetFollowing ();
	}

	public override void SetGuard()
	{
		// Disable character movement can move state
		movement.CanMove = false;
		move = Vector3.zero;

		// Reset attack state
		attack = false;

		// Call base class SetGuard method
		base.SetGuard ();
	}

	public override void SetAttack()
	{
		// Reset attack counter
		attackCounter = 0f;

		// Attack the nearest target
		attack = true;

		// Call base class SetAttack method
		base.SetAttack ();
	}
	#endregion

	#region Fly Methods
	private void AlignGround()
	{
		// Update align time counter
		alignCounter += DeltaTime;

		if(alignCounter >= alignDuration)
		{
			// Reset align time counter
			alignCounter = 0f;

			// Update child position to get aligned
			if(Physics.Raycast(new Ray(trans.position, Vector3.down), out hit, 100f, groundMask)) trans.position = new Vector3(trans.position.x, hit.point.y + groundDistance, trans.position.z);
		}
	}
	#endregion
}
