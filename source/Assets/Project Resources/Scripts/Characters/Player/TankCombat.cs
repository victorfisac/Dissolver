using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TankCombat : CharacterCombat 
{
	#region Inspector Attributes
	[Header("Radial")]
	[SerializeField] private CharacterAttack radial;
	[SerializeField] private GameObject radialObject;
	[SerializeField] private TankExplosion[] tankExplosion;

#if TANK_PICK
	[Header("Pick")]
	[SerializeField] private float pickDuration;
	[SerializeField] private Transform pickTransform;
#endif
	#endregion

	#region Private Attributes
	// Attack
	private int nearestEnemy;										// Current nearest enemy reference
	private float currentDistance;									// Current nearest enemy distance

	// Pick
	private bool isPicking;											// Is pick state
#if TANK_PICK
	private bool pickState;											// Pick mechanic state
	private float pickCounter;										// Pick time counter
	private List<Transform> pickObjects;							// Near available pick objects list
	private Rigidbody pickRb;										// Picked object rigidbody component reference
#endif

	// Boss
	private List<Boss> bosses;										// Boss target references list
	#endregion

	#region Main Methods
	public override void AwakeBehaviour(CameraLogic logic)
	{
		// Call base class Awake method
		base.AwakeBehaviour(logic);

	#if TANK_PICK
		pickObjects = new List<Transform>();
	#else
		isPicking = false;
	#endif

		// Initialize values
		bosses = new List<Boss>();

		// Awake tank explosion behaviour
		for(int i = 0; i < tankExplosion.Length; i++) tankExplosion[i].AwakeBehaviour();
	}

	public override void Attack(bool attack, bool secondary, bool skill, bool defend, bool action)
	{
		if(canCombat)
		{
			if(damageCounter == 0f && canAttack && !secondary && !isPicking)
			{
				if(attack && currentAttackInput < attackInputs.Length && character.CheckStamina(attacks[currentAttackInput].UsedStamina))
				{
					// Enable attack combo
					attackInputs[currentAttackInput] = true;

					// Reset attack counter
					attackCounter = -attackTimer;

					// Update current attack state value
					currentAttackInput++;
				}
			}

			// Update can attack state based on grounded state to limit attack amount during air state
			if(character.IsGrounded) canAttack = true;
			else if(attackInputs[0]) canAttack = false;

			if(!isPicking)
			{
				UpdateAttack();

				// Update radial attack if skill is unlocked
				if(unlockState == 1) UpdateRadial(skill);

				UpdateBlock(defend);
			}

		#if TANK_PICK
			UpdatePick(action);
		#endif
		}

		// Update tank explosion behaviour
		for(int i = 0; i < tankExplosion.Length; i++) tankExplosion[i].UpdateBehaviour();

		// Call base class Attack method
		base.Attack(attack, secondary, skill, defend, action);
	}
	#endregion

	#region Combat Methods
	private void UpdateAttack()
	{
		if(IsAttacking())
		{
			if(!attacks[playingAttack].IsEnabled)
			{
				// Enable attack state
				attacks[playingAttack].StartAttack();

				// Play current attack sound effect
				attackSource.clip = attacks[playingAttack].AttackSound;
				attackSource.Play();

				// Disable movement gravity velocity
				character.Movement.UseGravity = false;

				// Subtract character stamina
				character.SubtractStamina(attacks[playingAttack].UsedStamina);

				// Detect which is the nearest enemy
				nearestEnemy = 0;
				currentDistance = Mathf.Infinity;

				for(int i = 0; i < targets.Count; i++)
				{
					float distance = Mathf.Infinity;
					if(targets[i]) distance = Vector3.Distance(targets[i].Trans.position, character.Trans.position);

					if(distance < currentDistance)
					{
						// Update nearest enemy index and its distance to compare
						nearestEnemy = i;
						currentDistance = distance;
					}
				}

			#if DEBUG_BUILD
				// Trace debug message
				Debug.Log("TankCombat: attack started");
			#endif
			}
			else
			{
				// Update current attack counter
				attacks[playingAttack].AttackCounter += character.DeltaTime;

				if(!attacks[playingAttack].DamageDone)
				{
					if(attacks[playingAttack].AttackCounter >= attacks[playingAttack].DamageDelay)
					{
						bool hitDone = false;

						// Apply damage to all available targets
						for(int i = 0; i < targets.Count; i++) 
						{
							if(targets[i] && targets[i].Health > 0)
							{
								// Apply damage to all available targets
								targets[i].SetDamage(attacks[playingAttack].Damage, Vector3.Scale((targets[i].Trans.position - character.Trans.position), new Vector3(1f, 0f, 1f)).normalized, attacks[playingAttack].Force, character);

								ApplyHit();
								hitDone = true;

								// Update current combo amount
								character.CurrentCombo++;

								// Reset combo counter
								character.ComboCounter = 0f;
							}
						}

						// Apply damage to all available bosses
						for(int i = 0; i < bosses.Count; i++)
						{
							if(bosses[i] && bosses[i].Health > 0)
							{
								// Apply damage to all available bosses
								bosses[i].MakeDamage(attacks[playingAttack].Damage);

								ApplyHit();
								hitDone = true;

								// Update current combo amount
								character.CurrentCombo++;

								// Reset combo counter
								character.ComboCounter = 0f;
							}
						}

						// Apply damage to destroyable objects
						for(int i = 0; i < destroyables.Count; i++)
						{
							if(destroyables[i])
							{
								destroyables[i].Shatter(true);
								if(destroyables[i].Destroyed) destroyables.RemoveAt(i);
							}
						}

						// Update attack damage done state
						attacks[playingAttack].DamageDone = true;

						if(destroyables.Count > 0 && !hitDone) ApplyHit();

					#if DEBUG_BUILD
						// Trace debug message
						Debug.Log("TankCombat: attack damage " + attacks[playingAttack].Damage + " done at " + attacks[playingAttack].AttackCounter);
					#endif
					}
				}

				if(targets.Count > 0 && nearestEnemy < targets.Count)
				{
					if(character.IsGrounded)
					{
						// Rotate character to look at nearest enemy
						if(targets[nearestEnemy]) character.Trans.rotation = Quaternion.Lerp(character.Trans.rotation, Quaternion.LookRotation((targets[nearestEnemy].Trans.position - character.Trans.position).normalized), character.DeltaTime * orientationSpeed);

						// Update desired velocity to relative forward direction to enemy position
						desiredVelocity = character.Trans.TransformDirection(attacks[playingAttack].Movement * attacks[playingAttack].MovementCurve.Evaluate(attacks[playingAttack].AttackCounter / attacks[playingAttack].Duration));
					}
					else
					{
						// Update desired velocity to current character forward direction
						desiredVelocity = Vector3.Lerp(Vector3.zero, -character.Movement.DesiredVelocity, airTime);
					}
				}
				else
				{
					// Update desired velocity to current character forward direction
					if(character.IsGrounded) desiredVelocity = character.Trans.TransformDirection(attacks[playingAttack].Movement * attacks[playingAttack].MovementCurve.Evaluate(attacks[playingAttack].AttackCounter / attacks[playingAttack].Duration));
					else desiredVelocity = Vector3.Lerp(Vector3.zero, -character.Movement.DesiredVelocity, airTime);
				}

				if(attacks[playingAttack].AttackCounter >= attacks[playingAttack].Duration)
				{
					// Disable attack
					attacks[playingAttack].ResetAttack();

					// Reset attack input value
					attackInputs[playingAttack] = false;

					// Check if next attack can start
					if((playingAttack + 1) < attackInputs.Length)
					{
						if(attackInputs[playingAttack + 1]) playingAttack++;
						else
						{
							playingAttack = 0;
							currentAttackInput = 0;
						}
					}
					else
					{
						playingAttack = 0;
						currentAttackInput = 0;
					}

				#if DEBUG_BUILD
					// Trace debug message
					Debug.Log("TankCombat: attack " + playingAttack + "finished");
				#endif
				}
			}
		}
		else
		{
			// Disable movement gravity velocity
			character.Movement.UseGravity = true;
		}
	}

	private void UpdateRadial(bool skill)
	{
		// Check if character is grounded
		if(character.IsGrounded)
		{
			if(!IsAttacking() || (attacks[attacks.Length - 1].IsEnabled && attacks[attacks.Length - 1].AttackCounter > 0.1f))
			{
				if(!radial.IsEnabled)
				{
					if(skill && character.CheckSkill(100))
					{
						// Enable radial attack
						radial.StartAttack();

						// Call animator radial trigger
						character.Anim.SetTrigger("Radial");

						tankExplosion[0].InitializeExplosion();

						// Subtract character skill slot
						character.SubtractSkill(100);

						// Disable character interact state
						character.CanInteract = false;

						// Apply shake to camera
						if(cameraLogic) cameraLogic.ApplyShake(2f);

						// Check if character is player
						if(character.IsPlayer)
						{
							// Apply vibration to gamepad
							gameManager.SetVibration(skillVibration);

							// Reset vibration after a time
							Invoke("ResetVibration", vibrationDuration);
						}

					#if DEBUG_BUILD
						// Trace debug message
						Debug.Log("TankCombat: radial attack enabled");
					#endif
					}
				}
			}
		}

		if(radial.IsEnabled)
		{
			// Update radial attack counter
			radial.AttackCounter += character.DeltaTime;

			if(!radial.DamageDone)
			{
				if(radial.AttackCounter >= radial.DamageDelay)
				{
					// Update radial attack damage done state
					radial.DamageDone = true;

					// Apply shake to camera
					if(cameraLogic) cameraLogic.ApplyShake(3f);

					// Enable radial spikes game object
					radialObject.SetActive(true);

				#if DEBUG_BUILD
					// Trace debug message
					Debug.Log("TankCombat: radial attack damage " + radial.Damage + " done at " + radial.AttackCounter);
				#endif
				}
			}

			// Stop character based on attack motion
			desiredVelocity = Vector3.Scale(-character.Movement.DesiredVelocity, new Vector3(1, 0, 1));

			if(radial.AttackCounter >= radial.Duration)
			{
				// Reset radial attack
				radial.ResetAttack();

				// Reset character interact state
				character.CanInteract = true;

			#if DEBUG_BUILD
				// Trace debug message
				Debug.Log("TankCombat: radial attack finished");
			#endif
			}
		}
	}

	private void UpdateBlock(bool defend)
	{
		if(character.IsGrounded)
		{
			if(defend)
			{
				// Stop character based on attack motion
				desiredVelocity = Vector3.Scale(-character.Movement.DesiredVelocity, new Vector3(1f, 0f, 1f));

				// Update character can recover stamina and movement can turn states
				character.CanRecover = false;
				character.Movement.CanTurn = false;
			}
			else
			{
				// Update character can recover stamina and movement can turn states
				character.CanRecover = true;
				character.Movement.CanTurn = true;
			}
		}
	}

#if TANK_PICK
	private void UpdatePick(bool action)
	{
        if (action)
        {
            if (!isPicking && pickObjects.Count > 0)
            {
                // Update pick state
                if (!pickState) pickState = true;
            }
            else pickState = false;
        }

		if(pickState)
		{
			if(!isPicking)
			{
				// Update pick counter
				pickCounter += character.DeltaTime;
				
				if(pickCounter >= pickDuration)
				{
					// Update is pick state
					isPicking = true;
					
					// Reset pick counter
					pickCounter = 0f;
					
					// Calculate nearly pick object
					float distance = Mathf.Infinity;
					float directionMagnitude = 0f;
					int finalTarget = 0;

					for(int i = 0; i < pickObjects.Count; i++)
					{
						directionMagnitude = (pickObjects[i].position - transform.position).magnitude;
						if(directionMagnitude < distance)
						{	
							distance = directionMagnitude;
							finalTarget = i;
						}
					}

					// Parent nearly pick object
					pickObjects[finalTarget].parent = pickTransform;
					pickRb = pickObjects[finalTarget].GetComponent<Rigidbody>();

					// Fix pick object position
					pickObjects[finalTarget].position = transform.position + Vector3.up * 2f + transform.forward;

					// Disable physics and collision for picked object
					pickRb.isKinematic = true;
					pickRb.GetComponent<Collider>().enabled = false;
				}
			}
		}
		else
		{
			// Check if player is picking
			if(isPicking)
			{
				// Update pick counter
				pickCounter += character.DeltaTime;
				
				if(pickCounter >= pickDuration)
				{
					// Update is pick state
					isPicking = false;
					
					// Reset pick counter
					pickCounter = 0f;

					// Unparent nearly pick object
					pickRb.transform.parent = null;

					// Enable physics and collision for picked object
					pickRb.isKinematic = false;
					pickRb.GetComponent<Collider>().enabled = true;

                    // Remove from pickable objects list
                    pickObjects.Remove(pickRb.transform);

					// Reset pick rigidbody reference
					pickRb = null;
				}
			}
		}
	}
#endif
	#endregion

	#region Character Methods
	public override void SetDamage(Vector3 direction)
	{
		// Call base class SetDamage method
		base.SetDamage(direction);

		// Apply shake to camera
		if(cameraLogic) cameraLogic.ApplyShake();

		// Reset all attack values
		for(int i = 0; i < attacks.Length; i++) attacks[i].ResetAttack();
	}
	#endregion

	#region Animator Methods
	public override void UpdateAnimator(bool attack, bool sec, bool sk, bool def, bool act)
	{
		// Call base class UpdateAnimator method
		base.UpdateAnimator(attack, sec, sk, def, act);

		character.Anim.SetBool("Defend", def);
	}
	#endregion

	#region Properties
	public CharacterAttack Radial
	{
		get { return radial; }
	}

#if TANK_PICK
	public List<Transform> PickObjects
	{
		get { return pickObjects; }
		set { pickObjects = value; }
	}
#endif

	public List<Boss> Bosses
	{
		get { return bosses; }
	}
	#endregion
}
