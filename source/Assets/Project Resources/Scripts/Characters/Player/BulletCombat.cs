using UnityEngine;
using System.Collections;

public class BulletCombat : CharacterCombat 
{
	#region Inspector Attributes
	[Header("Dash")]
	[SerializeField] private float dashForce;
	[SerializeField] private float dashDuration;
	[SerializeField] private float dashStamina;
	[SerializeField] private AnimationCurve dashCurve;
	[SerializeField] private float dashFov;

#if BULLET_DOUBLEJUMP
	[Header("Double Jump")]
	[SerializeField] private float doubleJumpForce;
	[SerializeField] private float doubleJumpTimer;
	[SerializeField] private float doubleJumpStamina;
#endif

	[Header("Invoke")]
	[SerializeField] private GameObject allyPrefab;
	[SerializeField] private Transform invokeRoot;
	[SerializeField] private float checkOffset;
	[SerializeField] private float checkDistance;
	[SerializeField] private LayerMask checkMask;

	[Header("Projectile")]
	[SerializeField] private float projectileTimer;
	[SerializeField] private GameObject projectile;
#if BULLET_TIME
	[SerializeField] private GameObject freezeProjectile;
#endif
	[SerializeField] private float projectileStamina;

	[Header("Aim")]
	[SerializeField] private AnimationCurve aimCurve;
	[SerializeField] private float aimDuration;
	[SerializeField] private Transform aimDamage;
#if BULLET_TIME
	[SerializeField] private Transform aimTime;
#endif
	[SerializeField] private LayerMask aimMask;

	[Header("Effects")]
	[SerializeField] private TrailRenderer[] dashTrails;
	[SerializeField] private TrailRenderer[] dashDistortions;
	[SerializeField] private float dashTrailSpeed;

	[Header("Sounds")]
	[SerializeField] private AudioSource dashSource;
	[SerializeField] private AudioSource invokeSource;

#if BULLET_DOUBLEJUMP
	[SerializeField] private AudioSource doubleJumpSource;
#endif
	#endregion

	#region Private Attributes
	// Dash
	private int dashState;						// Current dash state	(0 = none, 1 = dashing)
	private float dashCounter;					// Dash input counter
	private Vector3 dashDirection;				// Dash direction vector
	private float startFov;						// Main camera fov at start

#if BULLET_DOUBLEJUMP
	// Double Jump
	private bool doubleJumpState;				// Double jump state
#endif

	// Attack
	private int nearestEnemy;					// Current nearest enemy reference
	private float currentDistance;				// Current nearest enemy distance

	// Projectile
	private int projectileState;				// Current can projectile state
	private float projectileCounter;			// Current projectile time counter

	// Aim
	private float aimCounter;					// Aim animation time counter
	private Vector3 aimInitScale;				// Aim transform init scale
	private RaycastHit aimHit;					// Aim raycast hit reference

	// Invoke
	private int invokeSpawned;					// Invoke power current invoked amount

	// Effects
	private float dashAlpha;					// Dash effect alpha value
	private float dashDistortion;				// Dash distortion effect value
	private Material[] dashTrailsMats;			// Dash trail renderer materials
	private Material[] dashDistortionsMats;		// Dash distortion trail renderer materials
	#endregion

	#region Main Methods
	public override void AwakeBehaviour(CameraLogic logic)
	{
		// Call base class Awake method
		base.AwakeBehaviour(logic);

		// Initialize values
		startFov = logic.Cam.fieldOfView;
		dashTrailsMats = new Material[dashTrails.Length];
		dashDistortionsMats = new Material[dashDistortions.Length];

		for(int i = 0; i < dashTrailsMats.Length; i++) dashTrailsMats[i] = dashTrails[i].material;
		for(int i = 0; i < dashDistortionsMats.Length; i++) dashDistortionsMats[i] = dashDistortions[i].material;

		// Initialize materials values
		for(int i = 0; i < dashTrailsMats.Length; i++) dashTrailsMats[i].SetColor("_TintColor", new Color(1f, 1f, 0f, dashAlpha));
		for(int i = 0; i < dashDistortionsMats.Length; i++)
		{
			dashDistortionsMats[i].SetFloat("_BumpAmt", dashDistortion);
			dashDistortions[i].enabled = false;
		}

		// Initialize aim values
		if(character.IsPlayer)
		{
			aimInitScale = aimDamage.localScale;
			aimDamage.localScale = Vector3.one * 0.001f;
		#if BULLET_TIME
			aimTime.localScale = Vector3.one * 0.001f;
		#endif
			aimCounter = aimDuration + 1f;
		}
	}

	public override void Attack(bool attack, bool secondary, bool skill, bool defend, bool action)
	{
		if(canCombat)
		{
			if(unlockState < 2) 
			{
				if(damageCounter == 0f && canAttack && !secondary)
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
				else if(attackInputs[attackInputs.Length - 1]) canAttack = false;

				UpdateAttack();
			}

			if(unlockState < 3) UpdateDash(defend);

		#if BULLET_DOUBLEJUMP
			UpdateDoubleJump(character.Jump);
		#endif

			if(unlockState < 0) UpdateInvoke(skill);

			if(unlockState < 1 && character.IsPlayer) UpdateBullet(attack, secondary, skill);
			else UpdateBullet(false, false, false);
		}

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
				Debug.Log("BulletCombat: attack " + playingAttack + " started");
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
							if(targets[i])
							{
								// Apply damage to current target and apply force backwards
								targets[i].SetDamage(attacks[playingAttack].Damage, Vector3.Scale((targets[i].Trans.position - character.Trans.position), new Vector3(1f, 0f, 1f)).normalized, attacks[playingAttack].Force, character);

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
								destroyables[i].Shatter(false);
								if(destroyables[i].Destroyed) destroyables.RemoveAt(i);
							}
						}

						// Update attack damage done state
						attacks[playingAttack].DamageDone = true;

						if(destroyables.Count > 0 && !hitDone) ApplyHit();

					#if DEBUG_BUILD
						// Trace debug message
						Debug.Log("BulletCombat: attack damage " + attacks[playingAttack].Damage + " done at " + attacks[playingAttack].AttackCounter);
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
					if(playingAttack + 1 < attackInputs.Length)
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
					Debug.Log("BulletCombat: attack " + playingAttack + " finished");
				#endif
				}
			}
		}
		else character.Movement.UseGravity = true;		// Disable movement gravity velocity
	}

	private void UpdateDash(bool defend)
	{
		// Handle dash
		switch(dashState)
		{
			case 0:
			{
				// Reset alpha and distortion values
				for(int i = 0; i < dashTrailsMats.Length; i++) dashTrailsMats[i].SetColor("_TintColor", new Color(1f, 1f, 0f, dashAlpha));
				for(int i = 0; i < dashDistortionsMats.Length; i++) dashDistortionsMats[i].SetFloat("_BumpAmt", dashDistortion);

				if(dashDistortion > 0f) dashDistortion -= Time.deltaTime * dashTrailSpeed;
				else
				{
					// Disable distortion trails to avoid strange render
					for(int i = 0; i < dashDistortions.Length; i++) dashDistortions[i].enabled = false;
					dashDistortion = 0f;
				}

				if(dashAlpha > 0f) dashAlpha -= Time.deltaTime * dashTrailSpeed;
				else dashAlpha = 0f;

				if(defend && character.CheckStamina(dashStamina) && character.Movement.ForwardAmount > 0)
				{
					// Update dash state to moving
					dashState = 1;

					// Store current movement direction to scale it
					dashDirection = Vector3.Scale(character.Movement.DesiredVelocity, new Vector3(1f, 0f, 1f));

					// Subtract character stamina
					character.SubtractStamina(dashStamina);

					// Play dash sound
					dashSource.Play();

					// Enable distortion trails
					for(int i = 0; i < dashDistortions.Length; i++) dashDistortions[i].enabled = true;

					// Apply distortion effect to camera
					if(character.IsPlayer) cameraLogic.Distortion.StartDistortion((Vector2)Camera.main.ScreenToViewportPoint(character.Trans.position), true);
				}
			} break;
			case 1:
			{
				// Update main camera fov based on animation curve
				cameraLogic.Cam.fieldOfView = Mathf.Lerp(startFov, dashFov, dashCurve.Evaluate(dashCounter / dashDuration));

				// Apply dash velocity to final desired velocity
				desiredVelocity += dashDirection * dashForce;

				// Update alpha and distortion values
				for(int i = 0; i < dashTrailsMats.Length; i++) dashTrailsMats[i].SetColor("_TintColor", new Color(1f, 1f, 0f, dashAlpha));
				for(int i = 0; i < dashDistortionsMats.Length; i++) dashDistortionsMats[i].SetFloat("_BumpAmt", dashDistortion);

				if(dashDistortion < 8f) dashDistortion += Time.deltaTime * dashTrailSpeed;
				else dashDistortion = 8f;

				if(dashAlpha < 0.6f) dashAlpha += Time.deltaTime * dashTrailSpeed;
				else dashAlpha = 0.6f;

				// Update dash counter to finish cover
				dashCounter += character.DeltaTime;

				// Check if dash finished
				if(dashCounter >= dashDuration)
				{
					dashState = 0;
					dashCounter = 0f;
				}
			} break;
			default: break;
		}
	}

#if BULLET_DOUBLEJUMP
	private void UpdateDoubleJump(bool jump)
	{
		// Check if player is on air
		if(!character.IsGrounded)
		{
			// Check if player is air during a time to be able to make double jump
			if(airTime >= doubleJumpTimer && !doubleJumpState)
			{
				if(jump && character.CheckStamina(doubleJumpStamina))
				{
					// Update double jump state
					doubleJumpState = true;

					// Calculate desired jump velocity
					desiredVelocity += Vector3.up * doubleJumpForce;

					// Subtract character stamina
					character.SubtractStamina(doubleJumpStamina);

					// Play double jump sound
					doubleJumpSource.Play();
				}
			}
		}
		else doubleJumpState = false;
	}
#endif

	private void UpdateInvoke(bool skill)
	{
		// Check if character is grounded
		if(character.IsGrounded && character.CheckSkill(100) && skill)
		{
			// Find gameplay manager reference
			GameplayManager manager = GameObject.FindWithTag("GameController").GetComponent<GameplayManager>();

			// Reset invoke spawned characters amount
			invokeSpawned = 0;

		#if DEBUG_BUILD
			bool spawned = false;
		#endif
			for(int i = 0; i < invokeRoot.childCount; i++)
			{
				if(Physics.Raycast(invokeRoot.GetChild(i).position + Vector3.up * checkOffset, Vector3.down, checkDistance, checkMask))
				{
					// Instantiate new ally game object
					GameObject newAlly = (GameObject)Instantiate(allyPrefab, invokeRoot.GetChild(i).position, character.Trans.rotation);

					// Get new ally character reference
					Character newAllyCharacter = newAlly.GetComponent<Character>();

					// Add new ally character to gameplay manager characters list
					manager.Characters.Add(newAllyCharacter);

					// Initialize new ally character
					newAllyCharacter.AwakeBehaviour(cameraLogic);

					// Update invoke spawned characters amount
					invokeSpawned++;

					// Check if there are enought invoked allies
					if(invokeSpawned >= 2)
					{
						// Reset invoke spawned characters amount
						invokeSpawned = 0;

						// Play invoke audio source
						invokeSource.Play();

						// Check if character is player
						if(character.IsPlayer)
						{
							// Apply vibration to gamepad
							gameManager.SetVibration(skillVibration);

							// Reset gamepad vibration after vibration duration
							Invoke("ResetVibration", vibrationDuration);
						}
					#if DEBUG_BUILD
						spawned = true;
					#endif
						break;
					}
				}
			}

		#if DEBUG_BUILD
			// Trace warning message
			if(!spawned) Debug.LogWarning("BulletCombat: trying to invoke allies failed because any spawn position is available");
		#endif

			// Subtract character skill
			character.SubtractSkill(100);
		}
	}

	private void UpdateBullet(bool attack, bool secondary, bool skill)
	{
		// Set character movement based on secondary input
		character.Movement.CameraTurn = secondary;

		switch(projectileState)
		{
			case 0:
			{
				if(aimDamage)
				{
					if(aimCounter <= aimDuration)
					{
						// Update aim transform based on animation curve
						if(aimDamage.localScale != Vector3.one * 0.01f) aimDamage.localScale = Vector3.Lerp(aimInitScale, Vector3.one * 0.01f, aimCurve.Evaluate(aimCounter / aimDuration));

					#if BULLET_TIME
						// Check if player unlocked time projectile
						if(unlockState == -1)
						{
							if(aimTime.localScale != Vector3.one * 0.01f) aimTime.localScale = Vector3.Lerp(aimInitScale, Vector3.one * 0.01f, aimCurve.Evaluate(aimCounter / aimDuration));
						}
					#endif

						// Update time counter
						aimCounter += Time.deltaTime;
					}
				}

				// Check secondary input
				if(secondary)
				{
					// Update projectile state
					projectileState++;

					// Reset aim counter
					aimCounter = 0f;
				}
			} break;
			case 1:
			{
				// Set bullet time
				if(!secondary)
				{
					// Reset projectile state
					projectileState = 0;

					// Reset aim counter
					aimCounter = 0f;
				}

				if(aimCounter <= aimDuration)
				{
					// Update aim transform based on animation curve
					if(aimDamage.localScale != aimInitScale) aimDamage.localScale = Vector3.Lerp(Vector3.one * 0.01f, aimInitScale, aimCurve.Evaluate(aimCounter / aimDuration));

				#if BULLET_TIME
					// Check if player unlocked time projectile
					if(unlockState == -1)
					{
						if(aimTime.localScale != aimInitScale) aimTime.localScale = Vector3.Lerp(Vector3.one * 0.01f, aimInitScale, aimCurve.Evaluate(aimCounter / aimDuration));
					}
				#endif

					// Update time counter
					aimCounter += Time.deltaTime;
				}

				if(attack)
				{
					// Check attack input
					if(character.CheckStamina(projectileStamina))
					{
						// Instantiate damage projectile
						GameObject auxProjectile = (GameObject)Instantiate(projectile, aimDamage.position, aimDamage.rotation);

						// Define default projectile direction
						Vector3 finalDirection = Camera.main.transform.forward;

						if(Physics.Raycast(Camera.main.transform.position, finalDirection, out aimHit, 1000f, aimMask)) finalDirection = (aimHit.point - aimDamage.position).normalized;

						// Apply direction to new projectile
						auxProjectile.GetComponent<BulletProjectile>().SetDirection(finalDirection, character);

						// Update projectile state
						projectileState++;

						// Reset aim transform local scale
						aimDamage.localScale = Vector3.one * 0.01f;

						// Subtract character stamina
						character.SubtractStamina(projectileStamina);
					}
				}
			#if BULLET_TIME
				else if(skill)
				{
					// Check attack input
					if(character.CheckStamina(projectileStamina))
					{
						// Instantiate freeze projectile
						GameObject auxProjectile = (GameObject)Instantiate(freezeProjectile, aimTime.position, aimTime.rotation);

						// Define default projectile direction
						Vector3 finalDirection = Camera.main.transform.forward;

						if(Physics.Raycast(Camera.main.transform.position, finalDirection, out aimHit, 1000f, aimMask)) finalDirection = (aimHit.point - aimTime.position).normalized;

						// Apply direction to new projectile
						auxProjectile.GetComponent<BulletProjectile>().SetDirection(finalDirection, character);

						// Update projectile state
						projectileState++;

						// Reset aim counter
						aimCounter = 0f;

						// Check if player unlocked time projectile
						if(unlockState == -1) aimTime.localScale = Vector3.one * 0.01f;		// Reset aim transform local scale

						// Subtract character stamina
						character.SubtractStamina(projectileStamina);
					}
				}
			#endif
			} break;
			case 2:
			{

				// Update projectile time counter
				projectileCounter += character.DeltaTime;

				if(aimCounter <= aimDuration)
				{
					// Update aim transform based on animation curve
					if(aimDamage.localScale != Vector3.one * 0.01f) aimDamage.localScale = Vector3.Lerp(aimInitScale, Vector3.one * 0.01f, aimCurve.Evaluate(aimCounter / aimDuration));

				#if BULLET_TIME
					// Check if player unlocked time projectile
					if(unlockState == -1)
					{
						if(aimTime.localScale != Vector3.one * 0.01f) aimTime.localScale = Vector3.Lerp(aimInitScale, Vector3.one * 0.01f, aimCurve.Evaluate(aimCounter / aimDuration));
					}
				#endif

					// Update aim counter
					aimCounter += Time.deltaTime;
				}

				if(projectileCounter >= projectileTimer)
				{
					// Reset projectile counter
					projectileCounter = 0f;

					// Reset projectile state
					projectileState = 0;
				}
			} break;
			default: break;
		}
	}
	#endregion

	#region Character Methods
	public override void SetDamage(Vector3 direction)
	{
		// Call base class SetDamage method
		base.SetDamage(direction);

		// Apply shake to camera
		if(cameraLogic && gameObject.tag != "Ally") cameraLogic.ApplyShake();
	}
	#endregion

	#region Animator Methods
	public override void UpdateAnimator(bool attack, bool sec, bool sk, bool def, bool act)
	{
		// Call base class UpdateAnimator method
		base.UpdateAnimator(attack, sec, sk, def, act);

		character.Anim.SetBool("Defend", (dashState == 1));

		character.Anim.SetBool("Shoot", (projectileState == 2 && attack));

		character.Anim.SetBool("Secondary", sec);
	}
	#endregion

	#region Properties
	public bool Aiming
	{
		get { return (projectileState == 1); }
	}
	#endregion
}
