using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Enemy : Character 
{
	#region Enums
	public enum EnemyStates { START, DETECTION, FOLLOWING, GUARD, ATTACK, DAMAGE, DIE };
	#endregion

	#region Inspector Attributes
	[Header("Enemy")]
	[SerializeField] protected EnemyStates state;
	[SerializeField] protected float startDuration;
	[SerializeField] protected bool backToPosition;

	[Header("Regions")]
	[SerializeField] protected float attackDistance;

	[Header("Attack")]
	[SerializeField] protected float attackTimer;

	[Header("Effects")]
	[SerializeField] protected ParticleSystem startEndParticles;

	[Header("Temporal")]
	[SerializeField] private bool temporalEnemy;
	[SerializeField] private float temporalDuration;
	#endregion

	#region Private Attributes
	// Start
	protected float startCounter;				// Start state time counter
	protected Vector3 initPosition;				// Transform position at start

	// Targets
	private List<Character> targets;			// Current available targets character reference
	protected Character nearTarget;				// Near target character reference
	protected float currentDistance;			// Current target distance

	// Attack
	protected float attackCounter;				// Attack time counter

	// Damage
	protected float damageCounter;				// Damage time counter

	// Temporal
	private float temporalCounter;				// Temporal time counter
	#endregion

	#region Main Methods
	public override void AwakeBehaviour(CameraLogic cameraLogic)
	{
		// Call base class Awake method
		base.AwakeBehaviour(cameraLogic);

		// Initialize values
		targets = new List<Character>();
		initPosition = trans.position;
	}

	public override void UpdateBehaviour()
	{
		FixTargetsList();

		UpdateTargets();

		switch(state)
		{
			case EnemyStates.START: StartBehaviour(); break;
			case EnemyStates.DETECTION: DetectionBehaviour(); break;
			case EnemyStates.FOLLOWING: FollowingBehaviour(); break;
			case EnemyStates.GUARD: GuardBehaviour(); break;
			case EnemyStates.ATTACK: AttackBehaviour(); break;
			case EnemyStates.DAMAGE: DamageBehaviour(); break;
			case EnemyStates.DIE: DieBehaviour(); break;
		}

		if(temporalEnemy && state != EnemyStates.DIE)
		{
			// Update temporal time counter
			temporalCounter += DeltaTime;

			if(temporalCounter >= temporalDuration)
			{
				SetDie();

			#if DEBUG_BUILD
				// Trace debug message
				Debug.Log("Enemy: died temporal enemy by duration");
			#endif
			}
		}

		// Call base class Update method
		base.UpdateBehaviour();
	}
	#endregion

	#region Target Methods
	private void FixTargetsList()
	{
		for(int i = 0; i < targets.Count; i++)
		{
			if(!targets[i]) targets.RemoveAt(i);
		}
	}

	private void UpdateTargets()
	{
		// Check if current near target exists
		bool exists = false;
		for(int i = 0; i < targets.Count; i++)
		{
			if(targets[i] == nearTarget)
			{
				exists = true;
				break;
			}
		}

		// Remove near target reference if it doesn't exist
		if(!exists) nearTarget = null;

		// Calculate current near target distance or set it to positive inifinity
		if(nearTarget && trans) currentDistance = Vector3.Distance(nearTarget.Trans.position, trans.position);
		else currentDistance = Mathf.Infinity;

		// Update near target reference from all available targets
		for(int i = 0; i < targets.Count; i++)
		{
			// Check if current target reference exist
			if(targets[i])
			{
				if(currentDistance > Vector3.Distance(targets[i].Trans.position, trans.position))
				{
					nearTarget = targets[i];
					currentDistance = Vector3.Distance(targets[i].Trans.position, trans.position);
				}
			}
		}
	}
	#endregion

	#region Behaviour Methods
	public virtual void StartBehaviour() 
	{ 
		// Update start counter
		startCounter += DeltaTime;

		if(startCounter >= startDuration)
		{
			// Reset start counter 
			startCounter = 0f;

			SetDetection();
		}
	}

	public virtual void DetectionBehaviour() { }

	public virtual void FollowingBehaviour() { }

	public virtual void GuardBehaviour() { }

	public virtual void AttackBehaviour() { }

	public virtual void DamageBehaviour() { }

	public virtual void DieBehaviour() { }
	#endregion

	#region Set Methods
	public virtual void SetDetection()
	{
		// Update state
		state = EnemyStates.DETECTION;
	}

	public virtual void SetFollowing()
	{
		// Update state
		state = EnemyStates.FOLLOWING;
	}

	public virtual void SetGuard()
	{
		// Update state
		state = EnemyStates.GUARD;
	}

	public virtual void SetAttack()
	{
		// Update state
		state = EnemyStates.ATTACK;
	}


	public override void SetDamage(float amount, Vector3 direction, float force, Character killer)
	{
		// Call base class SetDamage method
		base.SetDamage(amount, direction, force, killer);

		// Update state
		state = EnemyStates.DAMAGE;

		// Reset damage counter
		damageCounter = 0f;

		// Check if character is dead
		if(health <= 0f)
		{
			SetDie();

			// Check if killer exists
			if(killer)
			{
				// Apply die force (multiplied by high value to get a good physics behaviour)
				if(useRagdoll && killer.AnimIndex == 1) DieForce(direction * force * dieForce.z + Vector3.up * dieForce.y);
			}
		}
	}

	public override void SetDie()
	{
		// Call base class SetDie method
		base.SetDie();

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		// Play die particle effect
		if(startEndParticles) startEndParticles.Play();

		// Update state
		state = EnemyStates.DIE;
	}

	public override void DieForce(Vector3 direction)
	{
		// Apply force to all ragdoll rigidbodies
		for(int i = 0; i < ragdollRbs.Count; i++) ragdollRbs[i].AddForce(direction);
	}
	#endregion

	#region Debug Methods
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(trans.position, attackDistance);
	}
#endif
	#endregion

	#region Properties
	public List<Character> Targets
	{
		get { return targets; }
		set { targets = value; }
	}

	public Character NearTarget
	{
		get { return nearTarget; }
		set { nearTarget = value; }
	}
	#endregion
}
