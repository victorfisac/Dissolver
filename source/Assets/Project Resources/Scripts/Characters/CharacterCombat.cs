using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class CharacterCombat : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] protected bool applyMotion;
	[SerializeField] protected float attackTimer;
	[SerializeField] protected int maxAttacks;
	[SerializeField] protected Transform hitTransform;
	[SerializeField] protected GameObject hitPrefab;
	[SerializeField] protected float hitDestroyDelay;

	[Header("Attack")]
	[SerializeField] protected CharacterAttack[] attacks;
	[SerializeField] protected float orientationSpeed;

	[Header("Air")]
	[SerializeField] protected float airFriction;

	[Header("Damage")]
	[SerializeField] private float damageTimer;
	[SerializeField] private AnimationCurve damageCurve;

	[Header("Audio")]
	[SerializeField] protected AudioSource attackSource;
	[SerializeField] protected AudioSource damageSource;

	[Header("Vibration")]
	[SerializeField] private float damageVibration;
	[SerializeField] protected float skillVibration;
	[SerializeField] protected float vibrationDuration;

	[Header("References")]
	[SerializeField] protected Character character;
	#endregion

	#region Private Attributes
	// Inputs
	protected bool[] attackInputs;				// Current attack inputs states
	protected int currentAttackInput;			// Current attack input value
	protected float attackCounter;				// Current attack time counter
	protected bool canAttack;					// Current can attack state
	protected bool canCombat;					// Current can combat state

	// Attack
	protected int playingAttack;				// Current playing attack value

	// Movement
	protected Vector3 desiredVelocity;			// Current attack desired velocity
	protected float airTime;					// Current time in air counter

	// Damage
	protected float damageCounter;				// Current damage counter
	protected Vector3 damageDirection;			// Current damage direction

	// Targets
	protected List<Character> targets;			// Current character targets to attack
	protected List<Destroyable> destroyables;	// Current character destroyable targets

	// Camera
	protected CameraLogic cameraLogic;			// Main camera logic reference

	// Unlocks
	protected int unlockState;					// Unlock state (0 = SECONDARY, 1 = ATTACK, 2 = DASH, 3 = NONE)

	// Managers
	protected GameManager gameManager;			// Game manager reference
	#endregion

	#region Main Methods
	public virtual void AwakeBehaviour(CameraLogic logic)
	{
		// Get references
		gameManager = GameManager.Instance;

		// Initialize values
		cameraLogic = logic;
		attackInputs = new bool[maxAttacks];
		canAttack = true;
		canCombat = true;
		targets = new List<Character>();
		destroyables = new List<Destroyable>();

		// Check for dangerous values
		if(damageTimer == 0f) Debug.LogError("CharacterCombat: damageTimer value is " + damageTimer);
	}

	public virtual void Attack(bool attack, bool secondary, bool skill, bool defend, bool action) 
	{
		FixTargetsList();

		UpdateAirTime();

		UpdateDamage();

		ApplyPhysics();

		UpdateAnimator(attack, secondary, skill, defend, action);
	}
	#endregion

	#region Combat Methods
	private void FixTargetsList()
	{
		for(int i = 0; i < targets.Count; i++)
		{
			if(!targets[i]) targets.RemoveAt(i);
		}

		for(int i = 0; i < destroyables.Count; i++)
		{
			if(!destroyables[i]) destroyables.RemoveAt(i);
		}
	}

	private void UpdateAirTime()
	{
		if(character.IsGrounded) airTime = 0f;
		else if(airTime < 1f) airTime += airFriction * character.DeltaTime;
	}

	private void UpdateDamage()
	{
		if(damageDirection.magnitude > 0f)
		{
			// Disable movement gravity velocity
			character.Movement.UseGravity = false;

			// Check if character is player to apply damage gamepad vibration
			if(character.IsPlayer) gameManager.SetVibration(damageVibration);

			// Update damage counter
			damageCounter += character.DeltaTime;

			// Update external direction to movement
			desiredVelocity = damageDirection * damageCurve.Evaluate(damageCounter / damageTimer);

			if(damageCounter >= damageTimer)
			{
				// Restore damage counter and can attack state
				damageCounter = 0f;
				damageDirection = Vector3.zero;

				// Enable movement gravity velocity
				character.Movement.UseGravity = true;

				// Check if character is player to reset gamepad vibration
				if(character.IsPlayer) gameManager.SetVibration(0f);
			}
		}
	}

	private void ApplyPhysics()
	{
		if(applyMotion)
		{
			// Apply external velocity to movement
			character.Movement.ExternalVelocity = desiredVelocity;

			// Reset desired velocity
			desiredVelocity = Vector3.zero;
		}
	}

	public bool IsAttacking()
	{
		for(int i = 0; i < attackInputs.Length; i++)
		{
			if(attackInputs[i]) return true;
		}

		return false;
	}

	protected void ApplyHit()
	{
		// Instantiate hit effect
		GameObject newHit = (GameObject)Instantiate(hitPrefab, hitTransform.position + Vector3.forward * 0.25f, Quaternion.identity);

		// Destroy hit effect after delay
		Destroy(newHit, hitDestroyDelay);

		// Apply shake to camera if there is a target
		if(cameraLogic) cameraLogic.ApplyShake();

		// Play current attack damage sound effect
		damageSource.clip = attacks[playingAttack].DamageSound;
		damageSource.Play();
	}
	#endregion

	#region Character Methods
	public virtual void SetDamage(Vector3 direction)
	{
		// Update can attack and damage values
		if(direction.magnitude > 0)
		{
			damageDirection = Vector3.Scale(direction, new Vector3(1f, 0f, 1f));
			damageCounter = 0f;
		}

		// Reset combat inputs
		attackInputs = new bool[maxAttacks];
		currentAttackInput = 0;

		// Reset all attack values
		for(int i = 0; i < attacks.Length; i++) attacks[i].ResetAttack();

		// Reset character current combo and combo time counter
		character.CurrentCombo = 0;
		character.ComboCounter = 0f;
	}

	public void SetUnlock(int value)
	{
		// Update unlock state
		unlockState = value;
	}
	#endregion

	#region Animator Methods
	public virtual void UpdateAnimator(bool attack, bool sec, bool sk, bool def, bool act)
	{
		// Update attack values
		for(int i = 0; i < attackInputs.Length; i++) character.Anim.SetBool("Attack" + (i+1), attackInputs[i]);

		character.Anim.SetBool("Die", (character.Health <= 0f));
	}
	#endregion

	#region Invoked Methods
	protected void ResetVibration()
	{
		// Reset gamepad vibration
		gameManager.SetVibration(0f);
	}
	#endregion

	#region Properties
	public List<Character> Targets
	{
		get { return targets; }
		set { targets = value; }
	}

	public bool CanAttack
	{
		get { return canAttack; }
		set { canAttack = value; }
	}

    public int PlayingAttack
    {
        get { return playingAttack; }
    }

	public float DamageTimer
	{
		get { return damageTimer; }
	}

	public bool CanCombat
	{
		get { return canCombat; }
		set { canCombat = value; }
	}

	public List<Destroyable> Destroyables
	{
		get { return destroyables; }
	}
	#endregion

	#region Serializable
	[System.Serializable]
	public class CharacterAttack
	{
		#region Inspector Attributes
		[Header("Settings")]
		[SerializeField] private float damage;
		[SerializeField] private float force;
		[SerializeField] private float duration;
		[SerializeField] private float damageDelay;
		[SerializeField] private float usedStamina;

		[Header("Animation")]
		[SerializeField] private string name;

		[Header("Motion")]
		[SerializeField] private Vector3 movement;
		[SerializeField] private AnimationCurve movementCurve;

		[Header("Effects")]
		[SerializeField] private TrailRenderer[] trails;

		[Header("Audio")]
		[SerializeField] private AudioClip attackSound;
		[SerializeField] private AudioClip damageSound;
		#endregion

		#region Private Attributes
		private bool isEnabled;				// Attack enabled state
		private bool damageDone;			// Damage done state
		private float attackCounter;		// Current damage delay counter
		#endregion

		#region Main Methods
		public void StartAttack()
		{
			isEnabled = true;

			for(int i = 0; i < trails.Length; i++) trails[i].enabled = true;
		}

		public void ResetAttack()
		{
			isEnabled = false;
			damageDone = false;
			attackCounter = 0f;

			for(int i = 0; i < trails.Length; i++) trails[i].enabled = false;
		}
		#endregion

		#region Properties
		public bool IsEnabled
		{
			get { return isEnabled; }
			set { isEnabled = value; }
		}

		public float Damage
		{
			get { return damage; }
		}

		public float Force
		{
			get { return force; }
		}

		public float Duration
		{
			get { return duration; }
		}

		public float DamageDelay
		{
			get { return damageDelay; }
		}

		public string Name
		{
			get { return name; }
		}

		public Vector3 Movement
		{
			get { return movement; }
		}

		public AnimationCurve MovementCurve
		{
			get { return movementCurve; }
		}

		public bool DamageDone
		{
			get { return damageDone; }
			set { damageDone = value; }
		}

		public float AttackCounter
		{
			get { return attackCounter; }
			set { attackCounter = value; }
		}

		public float UsedStamina
		{
			get { return usedStamina; }
		}

		public AudioClip AttackSound
		{
			get { return attackSound; }
		}

		public AudioClip DamageSound
		{
			get { return damageSound; }
		}
		#endregion
	}
	#endregion
}
