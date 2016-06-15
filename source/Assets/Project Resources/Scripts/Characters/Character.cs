using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Character : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] protected bool isPlayer;
	[SerializeField] protected bool godMode;
	[SerializeField] protected bool freeMovement;

	[Header("Settings")]
	[SerializeField] protected float[] maxHealth;
	[SerializeField] protected float[] maxStamina;
	[SerializeField] protected float maxSlots;
	[SerializeField] protected float healthRecover;
	[SerializeField] protected float staminaRecover;
	[SerializeField] protected float runStamina;
	[SerializeField] protected float comboDuration;
	[SerializeField] protected float comboSlot;

	[Header("Behaviour")]
	[SerializeField] protected int animIndex;
	[SerializeField] protected bool handleMovement;
	[SerializeField] protected bool handleCombat;
	[SerializeField] protected bool handleFeedback;
	[SerializeField] protected bool handleActions;

	[Header("Time")]
	[SerializeField] private AnimationCurve timeLerpCurve;
	[SerializeField] private float timeLerpDuration;
	[SerializeField] private int timeEvent;

	[Header("Damage")]
	[SerializeField] protected int damageEvent;
	[SerializeField] protected EllipsoidParticleEmitter[] emitters;

	[Header("Ragdolls")]
	[SerializeField] protected bool useRagdoll;
	[SerializeField] protected GameObject skeletonRoot;

	[Header("Die")]
	[SerializeField] protected Vector3 dieForce;
	[SerializeField] protected float dieTimer;
	[SerializeField] protected float destroyTimer;
	[SerializeField] protected int dieEvent;
	[SerializeField] protected bool randomPickup;
	[SerializeField] protected GameObject[] pickups;

	[Header("Audio")]
	[SerializeField] protected AudioSource dieSource;

	[Header("Ground")]
	[SerializeField] private bool checkGround;
	[SerializeField] private float checkOffset;
	[SerializeField] private float checkDistance;
	[SerializeField] private LayerMask checkMask;

	[Header("References")]
	[SerializeField] protected Transform trans;
	[SerializeField] protected CharacterController controller;
	[SerializeField] protected Animator[] animators;
	[SerializeField] protected CharacterMovement movement;
	[SerializeField] protected CharacterCombat[] combats;
	[SerializeField] protected CharacterFeedback feedback;
	[SerializeField] protected CharacterAction actions;
	#endregion

	#region Private Attributes
	// Character
	protected float health;				// Current character health
	protected float stamina;			// Current character stamina
	protected float slots;				// Current character skill slots
	protected bool canRecover;			// Current can recover stamina state
	protected int deadState;			// Current character dead state
	protected bool canInteract;			// Current character can interact state

	// Movement
	protected Vector3 move;				// Current move direction input
	protected bool jump;				// Current jump state input
	protected float run;				// Current run amount input
	protected bool isGrounded;			// Current character is grounded state

	// Combat
	protected bool attack;				// Current attack input
	protected bool secondary;			// Current secondary input
	protected bool skill;				// Current skill input
	protected bool defend;				// Current defend input
	protected bool action;				// Current action input
	protected int currentCombo;			// Current combo count
	protected float comboCounter;		// Combo reset time counter

	// Time
	protected float timeScale;			// Current time scale
	protected float desiredTimeScale;	// Desired time scale for interpolation
	protected float lastTimeScale;		// Last time scale for interpolation
	protected float timeCounter;		// Time scale counter
	protected float timeDuration;		// Time scale duration
	protected float timeLerpCounter;	// Time lerp counter

	// Ground
	protected RaycastHit groundHit;		// Ground detection raycast reference

	// Ragdoll
	protected List<Rigidbody> ragdollRbs;		// Enemy ragdoll rigidbodies references

	// Die
	protected float dieCounter;			// Die delay counter
	protected float destroyCounter;		// Destroy delay counter

	// References
	protected GameManager gameManager;	// Game manager reference
	#endregion

	#region Main Methods
	public virtual void AwakeBehaviour(CameraLogic cameraLogic)
	{ 
		// Get references
		gameManager = GameManager.Instance;

		// Initialize values
		health = MaxHealth;
		stamina = MaxStamina;
		timeScale = 1f;
		desiredTimeScale = timeScale;
		canRecover = true;
		canInteract = true;

		if(useRagdoll)
		{
			// Search all rigidbodies in skeleton game object childs
			Rigidbody[] tempRbs = skeletonRoot.GetComponentsInChildren<Rigidbody>();

			// Initialize ragdoll rigidbodies lists
			ragdollRbs = new List<Rigidbody>();

			for(int i = 0; i < tempRbs.Length; i++)
			{
				if(tempRbs[i].gameObject.layer == LayerMask.NameToLayer("Shatters")) ragdollRbs.Add(tempRbs[i]);
			}

			// Disable all ragdoll rigidbodies
			for(int i = 0; i < ragdollRbs.Count; i++) ragdollRbs[i].isKinematic = true;
		}

		// Awake character movement
		if(handleMovement) movement.AwakeBehaviour();

		if(handleFeedback) feedback.AwakeBehaviour();

		// Awake character actions
		if(actions) actions.AwakeBehaviour();

		// Awake all character combats
		for(int i = 0; i < combats.Length; i++) combats[i].AwakeBehaviour(cameraLogic);
	}

	public virtual void UpdateBehaviour()
	{
		UpdateCharacter();

		switch(deadState)
		{
			case 1:
			{
				// Update die counter
				dieCounter += DeltaTime;

				if(dieCounter >= dieTimer)
				{
					controller.enabled = false;
					deadState++;
				}
			} break;
			case 2:
			{
				// Update destroy counter
				destroyCounter += DeltaTime;

				if(destroyCounter >= destroyTimer)
				{
					// Remove character reference from gameplay manager list
					GameObject.FindWithTag("GameController").GetComponent<GameplayManager>().Characters.Remove(this);

					// Destroy character game object
					Destroy(gameObject);
				}
			} break;
			default: break;
		}
	}

	private void UpdateCharacter()
	{
		UpdateGroundState();

		UpdateStatus();	// Health and stamina recover

		UpdateTimeScale();

		UpdateCombo();

		if(!canInteract)
		{
			// Reset input values
			move = Vector3.zero;
			run = 0f;
			jump = false;
			attack = false;
			secondary = false;
			skill = false;
			defend = false;
			action = false;
		}

		if(deadState < 2)
		{
			// Update component behaviours
			if(handleMovement && !freeMovement) movement.Move(move, jump);
			if(handleCombat) combats[animIndex].Attack(attack, secondary, skill, defend, action);
		}

		if(handleFeedback) feedback.UpdateBehaviour();

		if(handleActions && animIndex == 0) actions.UpdateActions(action);
	}

	public void ResetInputs()
	{
		// Reset input values
		run = 0f;
		jump = false;
		attack = false;
		secondary = false;
		skill = false;
		defend = false;
		action = false;
	}

	private void UpdateCombo()
	{
		if(currentCombo > 0)
		{
			// Update combo counter
			comboCounter += Time.deltaTime;

			if(comboCounter >= comboDuration)
			{
				// Reset combo counter
				comboCounter = 0f;

				// Reset current combo value
				currentCombo = 0;
			}
		}
	}
	#endregion

	#region Character Methods
	public void AddHealth(float amount)
	{
		// Update current health amount
		health += amount;

		// Fix value if it's out of bounds
		if(health > MaxHealth) health = MaxHealth;
	}

	public void AddSkill(float amount)
	{
		// Update current slots amount
		slots += amount;

		// Fix value if it's out of bounds
		if(slots > maxSlots) slots = maxSlots;
	}

	public void SubtractSlot(int amount)
	{
		if(!godMode)
		{
			// Update current slots amount
			slots -= amount;

			// Fix value if it's out of bounds
			if(slots < 0) slots = 0;
		}
	}

	public virtual bool CheckStamina(float amount)
	{
		bool result = false;

		result = (stamina >= amount);
		if(!result)
		{
			GameObject interfaceManager = GameObject.FindWithTag("UIController");
			if(interfaceManager) interfaceManager.GetComponent<InterfaceManager>().WarnStamina();
		}

		return result;
	}

	public virtual bool CheckSkill(float amount)
	{
		return (slots >= amount);
	}

	public virtual void SubtractStamina(float amount)
	{
		if(!godMode)
		{
			// Update stamina amount
			stamina -= amount;

			// Fix stamina amount if needed
			if(stamina < 0f) stamina = 0f;
		}
	}

	public virtual void SubtractSkill(float amount)
	{
		if(!godMode)
		{
			// Update slots amount
			slots -= amount;

			// Fix stamina amount if needed
			if(slots < 0f) slots = 0f;
		}
	}

	public virtual void UpdateStatus()
	{
		if(health < MaxHealth) health += DeltaTime * healthRecover;
		else health = MaxHealth;

		// Avoid stamina recover when run button is pressed
		if(!Input.GetButton("Run"))
		{
			if(stamina < MaxStamina) stamina += DeltaTime * staminaRecover;
			else stamina = MaxStamina;
		}
	}

	public virtual void SetDamage(float amount, Vector3 direction, float force, Character killer)
	{
		if(!godMode && health > 0f)
		{
			// Check if character is blocking
			if(canRecover)
			{
				// Update current health amount
				health -= amount;

				// Apply damage logic to combat
				Combat.SetDamage(direction * force);

				// Play damage effects
				for(int i = 0; i < emitters.Length; i++) emitters[i].Emit();

				if(health <= 0f)
				{
					// Fix health value
					health = 0f;

					// Remove character reference from killer combat targets list
					GameplayManager gameplayManager = GameObject.FindWithTag("GameController").GetComponent<GameplayManager>();
					for(int i = 0; i < gameplayManager.Characters.Count; i++)
					{
						// Remove character reference from every character combat targets list
						for(int j = 0; j < gameplayManager.Characters[i].Combats.Length; j++) gameplayManager.Characters[i].Combats[j].Targets.Remove(this);

						// Check if current character is not player
						if(i != 0)
						{
							// Get enemy character reference as Enemy object
							Enemy enemy = gameplayManager.Characters[i] as Enemy;

							// Remove character reference from enemies targets list
							enemy.Targets.Remove(this);
						}
					}

					// Check if killer exists
					if(killer)
					{
						// Apply distortion effect to camera if player killed somebody
						if(killer.IsPlayer) Camera.main.GetComponent<ScreenDistortion>().StartDistortion((Vector2)Camera.main.ScreenToViewportPoint(trans.position), false);
					}

					SetDie();
				}
			}
			else SubtractStamina(amount);
		}

		// Apply feedback damage event
		feedback.EnableEvent(damageEvent);

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("Character: character " + gameObject.name + " received " + amount + " damage");
	#endif
	}

	public virtual void SetDie()
	{
		if(Combat.CanCombat)
		{
			if(deadState != 1)
			{
				// Update die state
				deadState = 1;

				// Disable character movement motion and turn
				movement.CanMove = false;
				movement.CanTurn = false;

				// Disable character attack
				Combat.CanCombat = false;

				// Apply animator die state
				if(useRagdoll) for(int i = 0; i < Anims.Length; i++) Anims[i].enabled = false;
				else Anim.SetBool("Die", true);

				if(useRagdoll)
				{
					// Enable all ragdoll rigidbodies
					for(int i = 0; i < ragdollRbs.Count; i++) ragdollRbs[i].isKinematic = false;
				}

				// Apply feedback damage event
				feedback.EnableEvent(dieEvent);

				// Play die sound effect
				dieSource.Play();

				// Instantiate character dead pickup
				if(pickups.Length > 0)
				{
					// Create new pickup game object reference
					GameObject newObject = null;

					if(randomPickup)
					{
						if(Random.Range((int)0, (int)2) > 0) newObject = (GameObject)Instantiate(pickups[Random.Range((int)0, pickups.Length)], trans.position + Vector3.up, Quaternion.identity);
					}
					else newObject = (GameObject)Instantiate(pickups[Random.Range((int)0, pickups.Length)], trans.position + Vector3.up, Quaternion.identity);

					// Add new pickup reference to gameplay manager list
					if(newObject) 
					{
						// Get new pickup reference
						Pickup newPickup = newObject.GetComponent<Pickup>();

						// Initialize new pickup
						newPickup.StartBehaviour();

						// Add new pickup to gameplay manager list
						GameObject.FindWithTag("GameController").GetComponent<GameplayManager>().Pickups.Add(newPickup);
					}
				}

			#if DEBUG_BUILD
				// Trace debug message
				Debug.Log("Character: character " + gameObject.name + " died");
			#endif
			}
		}
	}

	public virtual void DieForce(Vector3 direction) { }

	public virtual void DisableCharacter()
	{
		// Disable character controller component
		controller.enabled = false;
	}
	#endregion

	#region Detection Methods
	public virtual void UpdateGroundState()
	{
		if(controller) isGrounded = controller.isGrounded;
		else isGrounded = false;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.DrawRay(trans.position + Vector3.up * checkOffset, Vector3.down * checkDistance, Color.white);
	#endif

		// Check again using raycast to ensure detection accuracy
		if(!isGrounded) isGrounded = Physics.Raycast(trans.position + Vector3.up * checkOffset, Vector3.down, out groundHit, checkDistance, checkMask);
	}
	#endregion

	#region Time Methods
	public virtual void UpdateTimeScale()
	{
		if(timeDuration > 0f)
		{
			// Update time scale counter
			timeCounter += Time.deltaTime;

			if(timeCounter >= timeDuration)
			{
				// Reset time scale
				ResetTimeScale();
			}
		}

		if(timeScale != desiredTimeScale)
		{
			// Update time scale interpolation
			timeScale = Mathf.Lerp(lastTimeScale, desiredTimeScale, timeLerpCurve.Evaluate(timeLerpCounter / timeLerpDuration));

			// Update time scale counter
			timeLerpCounter += Time.deltaTime;
		}
		else
		{
			// Reset time lerp counter
			timeLerpCounter = 0f;
		}

		// Update animator components playback speed based on time scale
		for(int i = 0; i < animators.Length; i++) animators[i].speed = timeScale;

		// Update movement speed based on time scale
		if(handleMovement) movement.DesiredVelocity *= timeScale;
	}

	public virtual void SetTimeScale(float time, float scale)
	{
		// Check if input time is not infinity and update time scale duration
		if(time > 0f) timeDuration += time;

		// Store previous time scale
		lastTimeScale = timeScale;

		// Update time scale
		desiredTimeScale = scale;

		// Call feedback time end event
		if(scale != 1f) feedback.EnableEvent(timeEvent);
	}

	public virtual void ResetTimeScale()
	{
		// Store previous time scale
		lastTimeScale = timeScale;

		// Reset time lerp counter
		timeLerpCounter = 0f;

		// Reset current time scale
		desiredTimeScale = 1f;

		// Reset time counter
		timeCounter = 0f;

		// Reset time duration
		timeDuration = 0f;
	}

	public void SetMaxSkill(float value)
	{
		// Update max skill value
		maxSlots = value;
		slots = maxSlots;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("Character: max skill slots set to " + maxSlots);
	#endif
	}
	#endregion

	#region Destroyables Methods
	public void AddDestroyable(Transform trans)
	{
		// Get destroyable reference
		Destroyable tempDestroyable = trans.GetComponent<Destroyable>();

		// Add destroyable reference to all character combat destroyable list
		for(int i = 0; i < combats.Length; i++) combats[i].Destroyables.Add(tempDestroyable);	
	}

	public void RemoveDestroyable(Transform trans)
	{
		// Get destroyable reference
		Destroyable tempDestroyable = trans.GetComponent<Destroyable>();

		// Remove reference from all character combat destroyable list
		for(int i = 0; i < combats.Length; i++) combats[i].Destroyables.Remove(tempDestroyable);
	}
	#endregion

	#region Debug Methods
#if DEBUG_BUILD
	private void OnGUI()
	{
		if(gameManager.IsDebug && Camera.main.enabled && health > 0f)
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
				text += "Character.health: " + health + "\n";
				text += "Character.stamina: " + stamina + "\n";
				text += "Character.slots: " + slots + " / " + maxSlots + "\n";
				text += "Character.timeScale: " + timeScale + "\n";

				// Draw label based on calculated position with some important data values
				GUI.Label(rect, text, style);
			}
		}

		if(isPlayer && godMode)
		{
			// Create a default GUI style
			GUIStyle style = new GUIStyle();
			style.alignment = TextAnchor.UpperLeft;
			style.fontSize = 15;
			style.normal.textColor = new Color (1.0f, 0.0f, 0.0f, ((Vector3.Distance(Camera.main.transform.position, transform.position) < 50f) ? (1 - Vector3.Distance(Camera.main.transform.position, transform.position) / 50f) : 0f));

			// Draw label based on calculated position with some important data values
			GUI.Label(new Rect(20, 20, 200, 100), "GOD MODE ON", style);
		}
	}
#endif
	#endregion

	#region Properties
	public bool IsPlayer
	{
		get { return isPlayer; }
	}

	public Transform Trans
	{
		get { return trans; }
	}

	public CharacterController Controller
	{
		get { return controller; }
	}

	public int AnimIndex
	{
		get { return animIndex; }
        set { animIndex = value; }
	}

	public Animator Anim
	{
		get { return animators[animIndex]; }
	}

	public Animator[] Anims
	{
		get { return animators; }
	}

	public CharacterMovement Movement
	{
		get { return movement; }
	}

	public CharacterCombat Combat
	{
		get { return combats[animIndex]; }
	}

	public CharacterCombat[] Combats
	{
		get { return combats; }
	}

	public bool Jump
	{
		get { return jump; }
	}

	public float MaxHealth
	{
		get { return maxHealth[animIndex]; }
	}

    public float Health
    {
        get { return health; }
        set { health = value; }
    }

	public float MaxStamina
	{
		get { return maxStamina[animIndex]; }
	}

    public float Stamina
    {
        get { return stamina; }
        set { stamina = value; }
    }

	public bool CanRecover
	{
		get { return canRecover; }
		set { canRecover = value; }
	}

	public float DeltaTime
	{
		get { return Time.deltaTime * timeScale; }
	}

	public float Slots
	{
		get { return slots; }
		set { slots = value; }
	}

	public float MaxSlots
	{
		get { return maxSlots; }
	}

	public bool FreeMovement
	{
		get { return freeMovement; }
	}

	public bool IsGrounded
	{
		get { return isGrounded; }
	}

	public CharacterFeedback Feedback
	{
		get { return feedback; }
	}

	public bool CanInteract
	{
		get { return canInteract; }
		set { canInteract = value; }
	}

	public float ComboCounter
	{
		set { comboCounter = value; }
	}

	public float ComboDuration
	{
		get { return comboDuration; }
	}

	public int CurrentCombo
	{
		get { return currentCombo; }
		set 
		{
			currentCombo = value; 

			if(currentCombo != 0) AddSkill(currentCombo * comboSlot);
		}
	}
	#endregion
}
