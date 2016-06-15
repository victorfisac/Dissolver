using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class Boss : MonoBehaviour 
{
	#region Enums
	public enum BossStates { NONE, START, PUNCH, SPIKES, DAMAGE, ENEMIES, LASER, FINAL, DIE };
	#endregion

	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private float maxHealth;
	[SerializeField] private bool awakeOnStart;
	[SerializeField] private BossStates startState;

	[Header("Motion")]
	[SerializeField] private Vector2 speed;

	[Header("Start")]
	[SerializeField] private float startDuration;
	[SerializeField] private float startDissolveDuration;
	[SerializeField] private BossEvent startEvent;

	[Header("Punch Attack")]
	[SerializeField] private Vector2 rangeLimits;
	[SerializeField] private float punchDuration;
	[SerializeField] private float punchDelay;
	[SerializeField] private SphereCollider punchColl;
	[SerializeField] private ParticleSystem punchParticles;
	[SerializeField] private AudioSource punchSource;
	[SerializeField] private Renderer punchRenderer;
	[SerializeField] private float feedbackDelay;
	[SerializeField] private Vector2 burnOffsetSpeed;
	[SerializeField] private float punchMinHealth;
	[SerializeField] private BoxCollider punchDamageColl;
	[SerializeField] private ParticleSystem punchExplosion;

	[Header("Spike")]
	[SerializeField] private BossSpike spike;
	[SerializeField] private float spikeDuration;
	[SerializeField] private float spikeDelay;
	[SerializeField] private int totalSpikes;
	[SerializeField] private ParticleSystem spikeParticles;
	[SerializeField] private AudioSource spikeSource;
	[SerializeField] private Renderer spikeRenderer;
	[SerializeField] private ParticleSystem spikeHandParticles;
	[SerializeField] private Collider spikeHandColl;
	[SerializeField] private ParticleSystem spikeExplosion;

	[Header("Damage")]
	[SerializeField] private float spikeMinHealth;
	[SerializeField] private float damageDistance;

	[Header("Enemies")]
	[SerializeField] private SpawnManager spawnManager;
	[SerializeField] private Transform spawnRoot;
	[SerializeField] private float laserOffset;
	[SerializeField] private AnimationCurve laserCurve;
	[SerializeField] private float laserDuration;

	[Header("Laser")]
	[SerializeField] private float laserStartDelay;
	[SerializeField] private float laserDurationDelay;
	[SerializeField] private float laserEndDelay;
	[SerializeField] private LineRenderer lineRenderer;
	[SerializeField] private Collider laserColl;
	[SerializeField] private float laserSpeed;
	[SerializeField] private float laserDistance;
	[SerializeField] private LayerMask laserMask;
	[SerializeField] private AudioSource laserSource;

	[Header("Final")]
	[SerializeField] private GameObject finalDamage;
	[SerializeField] private ParticleSystem finalParticles;

	[Header("Die")]
	[SerializeField] private AnimationCurve dissolveCurve;
	[SerializeField] private float dieDuration;
	[SerializeField] private BossEvent dieEvent;
	[SerializeField] private AudioSource dieSource;

	[Header("Ragdoll")]
	[SerializeField] private bool useRagdoll;
	[SerializeField] private float explosionForce;
	[SerializeField] private Transform explosionTrans;
	[SerializeField] private float explosionRadius;

	[Header("Feedback")]
	[SerializeField] private AnimationCurve damageCurve;
	[SerializeField] private Color damageColor;
	[SerializeField] private float damageDelay;
	[SerializeField] private AudioSource explosionSource;

	[Header("References")]
	[SerializeField] private Transform trans;
	[SerializeField] private Transform visualRoot;
	[SerializeField] private Transform laserRoot;
	[SerializeField] private Transform laserParticles;
	[SerializeField] private Transform laserHitParticles;
	[SerializeField] private Animator animator;
	#endregion

	#region Private Attributes
	// Settings
	private BossStates state;			// Current boss state
	private float timeCounter;			// Boss state time counter
	private float health;				// Current boss health
	private Quaternion initRotation;	// Boss transform rotation at start

	// Attack
	private bool attackDone;			// Current attack done state

	// Punch
	private Material punchMat;			// Boss punch material reference

	// Spikes
	private int spikesCounter;			// Boss spike counter
	private Material spikeMat;			// Boss spike hand material reference

	// Enemies
	private int laserState;				// Current laser state
	private RaycastHit laserHit;		// Laser raycast hit info reference
	private Quaternion initBallRot;		// Laser transform rotation at start
	private Vector3[] spawnPositions;	// Enemies spawn positions references
	private int enemiesCounter;			// Spawned enemies counter

	// Die
	private Rigidbody[] rigidbodies;	// Ragdoll rigidbodies references
	private BoxCollider[] colliders;	// Ragdoll colliders references

	// Feedback
	private float feedbackCounter;		// Feedback animation time counter
	private float damageCounter;		// Damage animation time counter
	private bool isDamage;				// Boss damage state
	private List<Material> damages;		// Current damage materials list

	// References
	private List<Material> mats;		// Boss renderers materials list
	private Character playerChar;		// Player character reference
	private CameraLogic cameraLogic;	// Camera logic reference
	#endregion

	#region Main Methods
	public void AwakeBehaviour(Character character, CameraLogic logic)
	{
		// Get references
		playerChar = character;
		cameraLogic = logic;
		rigidbodies = GetComponentsInChildren<Rigidbody>();
		colliders = new BoxCollider[rigidbodies.Length];
		for(int i = 0; i < colliders.Length; i++) colliders[i] = rigidbodies[i].GetComponent<BoxCollider>();

		// Initialize settings
		health = maxHealth;
		state = BossStates.NONE;
		initRotation = trans.rotation;
		initBallRot = laserRoot.rotation;

		// Initialize feedback
		mats = new List<Material>();
		SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		for(int i = 0; i < renderers.Length; i++)
		{
			for(int k = 0; k < renderers[i].materials.Length; k++) mats.Add(renderers[i].materials[k]);
		}
		punchMat = punchRenderer.material;
		spikeMat = spikeRenderer.material;
		damages = new List<Material>();

		// Initialize ragdolls
		for(int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;

		// Initialize spike
		spike.AwakeBehaviour();

		// Initialize enemies
		spawnPositions = new Vector3[spawnRoot.childCount];
		for(int i = 0; i < spawnPositions.Length; i++)
		{
			// Get spawn position
			spawnPositions[i] = spawnRoot.GetChild(i).position;

			// Calculate direction between ball and spawn positions
			Vector3 rayDirection = (spawnPositions[i] - laserRoot.position).normalized;
			spawnPositions[i] += rayDirection * laserOffset;
		}

		// Reset line renderer vertex count
		lineRenderer.SetVertexCount(0);
		lineRenderer.useWorldSpace = true;

		// Disable boss game object by default
		if(awakeOnStart) EnableBoss();
		else gameObject.SetActive(false);
	}

	public void UpdateBehaviour()
	{
		// Update spike behaviour
		spike.UpdateBehaviour();

		switch(state)
		{
			case BossStates.START: StartBehaviour(); break;
			case BossStates.PUNCH: PunchBehaviour(); break;
			case BossStates.SPIKES: SpikesBehaviour(); break;
			case BossStates.DAMAGE: DamageBehaviour(); break;
			case BossStates.ENEMIES: EnemiesBehaviour(); break;
			case BossStates.LASER: LaserBehaviour(); break;
			case BossStates.FINAL: FinalBehaviour(); break;
			case BossStates.DIE: DieBehaviour(); break;
			default: break;
		}

		if(isDamage)
		{
			// Update all material targets emission color based on animation curve
			for(int i = 0; i < damages.Count; i++) damages[i].SetColor("_EmissionColor", Color.Lerp(Color.black, damageColor, damageCurve.Evaluate(damageCounter / damageDelay)));

			// Update damage time counter
			damageCounter += Time.deltaTime;

			if(damageCounter >= damageDelay)
			{
				// Reset is damage state
				isDamage = false;

				// Reset damage time counter
				damageCounter = 0f;
			}
		}
	}
	#endregion

	#region Input Methods
	public void EnableBoss()
	{
		// Enable boss game object
		gameObject.SetActive(true);

		// Set boss start behaviour
		SetStart();
	}

	public void MakeDamage(float damage)
	{
		// Update current boss health
		health -= damage;

		// Update boss damage state
		isDamage = true;

		// Play damage source
		punchSource.Play();

		switch(state)
		{
			case BossStates.PUNCH: punchParticles.Play(); break;
			case BossStates.DAMAGE: spikeHandParticles.Play(); break;
			case BossStates.FINAL: finalParticles.Play(); break;
			default: break;
		}

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("Boss: boss damaged, current health: " + health);
	#endif
	}
	#endregion

	#region Set Methods
	private void SetStart()
	{
		// Update current boss state
		state = BossStates.START;

		// Reset time counter
		timeCounter = 0f;

		// Invoke all start event methods after delay
		Invoke("StartEvent", startEvent.Delay);

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("Boss: state set to START");
	#endif
	}

	private void SetPunch()
	{
		// Update current boss state
		state = BossStates.PUNCH;

		// Reset time counter
		timeCounter = 0f;

		// Add punch material to current damage target materials list
		damages.Add(punchMat);

		// Enable punch collider
		punchColl.enabled = true;

		// Apply animator punch trigger
		animator.SetTrigger("Punch");

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("Boss: state set to PUNCH");
	#endif
	}

	private void SetSpikes()
	{
		// Update current boss state
		state = BossStates.SPIKES;

		// Reset time counter
		timeCounter = 0f;

		// Remove punch material from current damage target materials list
		damages.Remove(punchMat);

		// Disable punch collider
		punchColl.enabled = false;

		// Remove all tank bosses references
		TankCombat tank = (TankCombat)playerChar.Combats[1] as TankCombat;
		for(int i = 0; i < tank.Bosses.Count; i++) tank.Bosses.RemoveAt(i);

		// Apply animator punch trigger
		animator.SetTrigger("Spikes");

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("Boss: state set to SPIKES");
	#endif
	}

	private void SetDamage()
	{
		// Update current boss state
		state = BossStates.DAMAGE;

		// Reset time counter
		timeCounter = 0f;

		// Reset feedback time counter
		feedbackCounter = 0f;

		// Add spikes material to current damage target materials list
		damages.Add(spikeMat);

		// Remove all tank bosses references
		TankCombat tank = (TankCombat)playerChar.Combats[1] as TankCombat;
		for(int i = 0; i < tank.Bosses.Count; i++) tank.Bosses.RemoveAt(i);

		// Enable punch collider
		spikeHandColl.enabled = true;

		// Apply animator punch trigger
		animator.SetTrigger("Damage");

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("Boss: state set to DAMAGE");
	#endif
	}

	private void SetEnemies()
	{
		// Update current boss state
		state = BossStates.ENEMIES;

		// Reset time counter
		timeCounter = 0f;

		// Remove spikes material from current damage target materials list
		damages.Add(spikeMat);

		// Disable punch collider
		spikeHandColl.enabled = false;

		// Remove all tank bosses references
		TankCombat tank = (TankCombat)playerChar.Combats[1] as TankCombat;
		for(int i = 0; i < tank.Bosses.Count; i++) tank.Bosses.RemoveAt(i);

		// Apply animator punch trigger
		animator.SetTrigger("Enemies");

		// Reset line renderer vertex count
		lineRenderer.SetVertexCount(2);
		for(int i = 0; i < 2; i++) lineRenderer.SetPosition(i, laserRoot.position);

		// Enable spawn manager
		spawnManager.StartSpawner();

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("Boss: state set to ENEMIES");
	#endif
	}

	private void SetLaser()
	{
		// Update current boss state
		state = BossStates.LASER;

		// Reset time counter
		timeCounter = 0f;

		// Remove all tank bosses references
		TankCombat tank = (TankCombat)playerChar.Combats[1] as TankCombat;
		for(int i = 0; i < tank.Bosses.Count; i++) tank.Bosses.RemoveAt(i);

		// Apply animator punch trigger
		animator.SetTrigger("LaserStart");

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("Boss: state set to LASER");
	#endif
	}

	private void SetFinal()
	{
		// Update current boss state
		state = BossStates.FINAL;

		// Reset time counter
		timeCounter = 0f;

		// Enable final damage game object
		finalDamage.SetActive(true);

		// Add final materials to current damage target materials list
		for(int i = 0; i < mats.Count; i++)
		{
			if(!damages.Contains(mats[i])) damages.Add(mats[i]);
		}

		// Apply animator punch trigger
		animator.SetTrigger("Damage");

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("Boss: state set to FINAL");
	#endif
	}

	private void SetDie()
	{
		// Update current boss state
		state = BossStates.DIE;

		// Reset time counter
		timeCounter = 0f;

		// Play die source
		dieSource.Play();

		if(useRagdoll)
		{
			// Disable animator logic
			animator.enabled = false;

			// Enable all ragdoll colliders
			for(int i = 0; i < colliders.Length; i++) colliders[i].enabled = true;

			for(int i = 0; i < rigidbodies.Length; i++)
			{
				// Disable all rigidbodies kinematic state
				rigidbodies[i].isKinematic = false;

				// Apply explosion force to rigidbodies
				rigidbodies[i].AddExplosionForce(explosionForce, explosionTrans.position, explosionRadius);
			}
		}

		// Invoke die event methods
		dieEvent.Methods.Invoke();

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("Boss: state set to DIE");
	#endif
	}
	#endregion

	#region Behaviour Methods
	private void StartBehaviour()
	{
		// Calculate player-boss direction locked in Y axis;
		Vector3 playerDirection = playerChar.Trans.position - trans.position;
		playerDirection.y = trans.position.y;

		// Update rotation to look at player with smooth delay
		trans.rotation = Quaternion.Lerp(trans.rotation, Quaternion.LookRotation(playerDirection.normalized), Time.deltaTime * speed.y);

		// Fix bos X axis rotation value
		playerDirection = trans.rotation.eulerAngles;
		playerDirection.x = 0f;
		trans.rotation = Quaternion.Euler(playerDirection);

		// Update materials dissolve amount based on time counter
		for(int i = 0; i < mats.Count; i++) mats[i].SetFloat("_DissolveAmount", Mathf.Lerp(1.1f, -0.1f, (timeCounter / startDissolveDuration)));

		// Update time counter
		timeCounter += Time.deltaTime;

		if(timeCounter >= startDuration)
		{
			// Set current state based on start value
			switch(startState)
			{
				case BossStates.PUNCH: SetPunch(); break;
				case BossStates.SPIKES: SetSpikes(); break;
				case BossStates.DAMAGE: SetDamage(); break;
				case BossStates.ENEMIES: SetEnemies(); break;
				case BossStates.LASER: SetLaser(); break;
				case BossStates.FINAL: SetFinal(); break;
				case BossStates.DIE: SetDie(); break;
				default: break;
			}
		}
	}

	private void PunchBehaviour()
	{
		// Update time counter
		timeCounter += Time.deltaTime;

		if(!attackDone)
		{
			// Calculate player-boss direction locked in Y axis;
			Vector3 playerDirection = Vector3.Scale(playerChar.Trans.position - trans.position, new Vector3(1f, 0f, 1f));

			if(playerDirection.magnitude > rangeLimits.y) trans.position += playerDirection.normalized * Time.deltaTime * speed.x;
			else if(playerDirection.magnitude < rangeLimits.x) trans.position -= playerDirection.normalized * Time.deltaTime * speed.x;

			// Calculate player boss direction locked in Y axis
			playerDirection = playerChar.Trans.position - trans.position;
			playerDirection.y = trans.position.y;

			// Update rotation to look at player with smooth delay
			trans.rotation = Quaternion.Lerp(trans.rotation, Quaternion.LookRotation(playerDirection.normalized), Time.deltaTime * speed.y);

			// Fix boss X axis rotation value
			playerDirection = trans.rotation.eulerAngles;
			playerDirection.x = 0f;
			trans.rotation = Quaternion.Euler(playerDirection);

			if(timeCounter >= punchDelay)
			{
				// Update attack done state
				attackDone = true;

				// Disable punch collider after a little delay
				Invoke("DisablePunch", 0.4f);

				// Apply shake to camera logic
				cameraLogic.ApplyShake();

				// Play punch particle system
				punchParticles.Play();

				// Play punch audio source
				punchSource.Play();
			}
		}
		else
		{
			// Update punch material burn amount
			punchMat.SetFloat("_BurnAmount", Mathf.Lerp(punchMat.GetFloat("_BurnAmount"), 0.5f, Time.deltaTime * 3f));

			// Update punch material emission color
			punchMat.SetColor("_EmissionColor", Color.Lerp(punchMat.GetColor("_EmissionColor"), new Color(0.5f, 0f, 0f, 0f), Time.deltaTime * 3f));

			// Update punch material burn texture offset
			Vector2 currentOffset = punchMat.GetTextureOffset("_BurnTex");
			currentOffset += burnOffsetSpeed * Time.deltaTime;
			punchMat.SetTextureOffset("_BurnTex", currentOffset);

			// Update feedback time counter
			feedbackCounter += Time.deltaTime;

			if(feedbackCounter >= feedbackDelay)
			{
				// Reset punch material burn amount
				punchMat.SetFloat("_BurnAmount", Mathf.Lerp(punchMat.GetFloat("_BurnAmount"), -0.1f, Time.deltaTime * 3f));

				// Reset punch material emission color
				punchMat.SetColor("_EmissionColor", Color.Lerp(punchMat.GetColor("_EmissionColor"), new Color(0f, 0f, 0f, 0f), Time.deltaTime * 3f));
			}
		}

		if(timeCounter >= punchDuration)
		{
			// Reset time counter
			timeCounter = 0f;

			// Reset feedback time counter
			feedbackCounter = 0f;

			// Reset attack done state
			attackDone = false;

			// Enable punch collider
			punchColl.enabled = true;
		}

		if(health <= punchMinHealth)
		{
			// Reset feedback time counter
			feedbackCounter = 0f;

			// Reset attack done state
			attackDone = false;

			// Disable punch collider
			punchColl.enabled = false;

			// Disable punch damage collider
			punchDamageColl.enabled = false;

			// Play damaged audio source
			explosionSource.Play();

			// Play damaged particle system
			punchExplosion.Play();

			SetSpikes();
		}
	}

	private void SpikesBehaviour()
	{
		// Reset punch material dissolve amount
		punchMat.SetFloat("_DissolveAmount", Mathf.Lerp(punchMat.GetFloat("_DissolveAmount"), 1.1f, Time.deltaTime));

		// Reset punch material emission color
		punchMat.SetColor("_EmissionColor", Color.Lerp(punchMat.GetColor("_EmissionColor"), new Color(0f, 0f, 0f, 0f), Time.deltaTime * 3f));

		if(!attackDone)
		{
			// Calculate player-boss direction locked in Y axis;
			Vector3 playerDirection = Vector3.Scale(playerChar.Trans.position - trans.position, new Vector3(1f, 0f, 1f));

			if(playerDirection.magnitude > rangeLimits.y) trans.position += playerDirection.normalized * Time.deltaTime * speed.x;
			else if(playerDirection.magnitude < rangeLimits.x) trans.position -= playerDirection.normalized * Time.deltaTime * speed.x;

			// Calculate player boss direction locked in Y axis
			playerDirection = playerChar.Trans.position - trans.position;
			playerDirection.y = trans.position.y;

			// Update rotation to look at player with smooth delay
			trans.rotation = Quaternion.Lerp(trans.rotation, Quaternion.LookRotation(playerDirection.normalized), Time.deltaTime * speed.y);

			// Fix boss X axis rotation value
			playerDirection = trans.rotation.eulerAngles;
			playerDirection.x = 0f;
			trans.rotation = Quaternion.Euler(playerDirection);
		}

		// Update time counter
		timeCounter += Time.deltaTime;

		if(!attackDone && timeCounter >= spikeDelay)
		{
			// Update attack done state
			attackDone = true;

			// Enable spike logic
			spike.EnableSpike(playerChar.Trans.position);

			// Apply shake to camera logic
			cameraLogic.ApplyShake();

			// Play spike particle system
			spikeParticles.Play();

			// Play spike audio source
			spikeSource.Play();
		}

		if(timeCounter >= spikeDuration)
		{
			// Reset time counter
			timeCounter = 0f;

			// Reset attack done state
			attackDone = false;

			// Update current spike counter
			spikesCounter++;

			// Stop spike particles
			spikeParticles.Stop();

			if(spikesCounter >= totalSpikes)
			{
				// Reset spike counter
				spikesCounter = 0;

				// Reset camera logic zoom value
				cameraLogic.ResetZoom();

				SetDamage();
			}
		}
	}

	private void DamageBehaviour()
	{
		// Calculate player-boss direction locked in Y axis;
		trans.rotation = Quaternion.Lerp(trans.rotation, initRotation, Time.deltaTime * 2f);

		// Update boss visual transform to fit damage position
		visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, Vector3.down * damageDistance, Time.deltaTime * 2f);

		// Update punch material burn amount
		spikeMat.SetFloat("_BurnAmount", Mathf.Lerp(spikeMat.GetFloat("_BurnAmount"), 0.5f, Time.deltaTime * 3f));

		// Update punch material emission color
		spikeMat.SetColor("_EmissionColor", Color.Lerp(spikeMat.GetColor("_EmissionColor"), new Color(0.5f, 0f, 0f, 0f), Time.deltaTime * 3f));

		// Update punch material burn texture offset
		Vector2 currentOffset = spikeMat.GetTextureOffset("_BurnTex");
		currentOffset += burnOffsetSpeed * Time.deltaTime;
		spikeMat.SetTextureOffset("_BurnTex", currentOffset);

		if(health <= spikeMinHealth)
		{
			// Reset feedback time counter
			feedbackCounter = 0f;

			// Disable punch damage collider
			spikeHandColl.enabled = false;

			// Play damaged audio source
			explosionSource.Play();

			// Play damaged particle system
			spikeExplosion.Play();

			SetEnemies();
		}
	}

	private void EnemiesBehaviour()
	{
		// Reset punch material dissolve amount
		spikeMat.SetFloat("_DissolveAmount", Mathf.Lerp(spikeMat.GetFloat("_DissolveAmount"), 1.1f, Time.deltaTime));

		// Reset punch material emission color
		spikeMat.SetColor("_EmissionColor", Color.Lerp(spikeMat.GetColor("_EmissionColor"), new Color(0f, 0f, 0f, 0f), Time.deltaTime * 3f));

		// Check if enemies are currently spawning
		if(spawnManager.EnemiesCount < spawnManager.TotalEnemies)
		{
			// Update boss rotation to look forward as in its start
			trans.rotation = Quaternion.Lerp(trans.rotation, initRotation, Time.deltaTime * speed.y);

			// Update ball position to look at current spawning enemy position
			laserRoot.rotation = Quaternion.Lerp(laserRoot.rotation, Quaternion.LookRotation((spawnPositions[spawnManager.EnemiesCount] - laserRoot.position).normalized), Time.deltaTime * laserSpeed);

			// Update end line position based on animation curve
			lineRenderer.SetPosition(0, laserRoot.position);
			lineRenderer.SetPosition(1, Vector3.Lerp(laserRoot.position, spawnPositions[spawnManager.EnemiesCount], laserCurve.Evaluate(timeCounter / laserDuration)));

			// Get current curve animation value
			float curveValue = laserCurve.Evaluate(timeCounter / laserDuration);

			// Update laser collider based on curve value
			laserColl.enabled = (curveValue == 1f);

			// Update laser particles enabled state based on curve value if needed
			if(laserParticles.gameObject.activeSelf != (curveValue == 1f)) laserParticles.gameObject.SetActive((curveValue == 1f));

			// Check if laser is currently enabled
			if(curveValue == 1f)
			{
				// Update laser hit particles position and enable its game object
				laserHitParticles.position = spawnPositions[spawnManager.EnemiesCount] + Vector3.up;
				if(!laserHitParticles.gameObject.activeSelf) laserHitParticles.gameObject.SetActive(true);
			}
			else if(laserHitParticles.gameObject.activeSelf) laserHitParticles.gameObject.SetActive(false);

			// Update time counter
			timeCounter += Time.deltaTime;

			// Reset time counter if its value is out of bounds to make loop
			if(timeCounter >= laserDuration) timeCounter = 0f;
		}
		else
		{
			// Update ball position to look forward
			laserRoot.rotation = Quaternion.Lerp(laserRoot.rotation, initBallRot, Time.deltaTime * laserSpeed * 2f);

			// Disable laser particles system if needed
			if(laserHitParticles.gameObject.activeSelf) laserHitParticles.gameObject.SetActive(false);
			if(laserParticles.gameObject.activeSelf) laserParticles.gameObject.SetActive(false);

			// Reset line renderer vertex count
			lineRenderer.SetVertexCount(0);
		}
	}

	private void LaserBehaviour()
	{
		switch(laserState)
		{
			case 0:
			{
				// Update time counter
				timeCounter += Time.deltaTime;

				// Update laser start particle system based on lerp interpolation
				laserParticles.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, (timeCounter / laserStartDelay));

				if(timeCounter >= laserStartDelay)
				{
					// Reset time counter
					timeCounter = 0f;

					// Update current laser state
					laserState = 1;

					// Set line renderer vertexs, positions and disable world space mode
					lineRenderer.SetVertexCount(2);
					lineRenderer.SetPosition(0, Vector3.zero);
					lineRenderer.SetPosition(1, Vector3.forward * 50f);
					lineRenderer.useWorldSpace = false;

					// Play laser audio source
					laserSource.Play();

					// Enable laser particles system if needed
					if(!laserHitParticles.gameObject.activeSelf) laserHitParticles.gameObject.SetActive(true);
					if(!laserParticles.gameObject.activeSelf) laserParticles.gameObject.SetActive(true);

					// Enable laser collider
					laserColl.enabled = true;
				}
			} break;
			case 1:
			{
				// Update laser rotation to look player with smooth delay
				laserRoot.rotation = Quaternion.Lerp(laserRoot.rotation, Quaternion.LookRotation((playerChar.Trans.position + Vector3.up - laserRoot.position).normalized), Time.deltaTime * laserSpeed);

				// Update laser hit particles position based on raycast hit
				if(Physics.Raycast(laserParticles.position, laserParticles.TransformDirection(Vector3.forward), out laserHit, laserDistance, laserMask)) laserHitParticles.position = laserHit.point;

				// Update laser line renderer forward position
				lineRenderer.SetPosition(1, Vector3.forward * (Vector3.Distance(laserParticles.position, laserHitParticles.position) + 3f));

				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= laserDurationDelay)
				{
					// Reset time counter
					timeCounter = 0f;

					// Update current laser state
					laserState = 2;

					// Disable line renderer vertexs and enable world space mode
					lineRenderer.SetVertexCount(0);
					lineRenderer.useWorldSpace = true;

					// Disable laser particles system if needed
					laserHitParticles.gameObject.SetActive(false);
					laserParticles.gameObject.SetActive(false);

					// Apply animator laser end trigger
					animator.SetTrigger("LaserEnd");

					// Stop laser audio source
					laserSource.Stop();

					// Disable laser collider
					laserColl.enabled = false;
				}
			} break;
			case 2:
			{
				// Update time counter
				timeCounter += Time.deltaTime;

				// Update laser start particle system based on lerp interpolation
				laserParticles.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, (timeCounter / laserStartDelay));

				if(timeCounter >= laserEndDelay)
				{
					// Reset current laser state
					laserState = 0;

					SetFinal();
				}
			} break;
			default: break;
		}
	}

	private void FinalBehaviour()
	{
		// Calculate player-boss direction locked in Y axis;
		trans.rotation = Quaternion.Lerp(trans.rotation, initRotation, Time.deltaTime * 2f);

		// Update boss visual transform to fit damage position
		visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, Vector3.down * damageDistance, Time.deltaTime * 2f);

		// Update punch material burn amount
		for(int i = 0; i < mats.Count; i++)
		{
			mats[i].SetFloat("_BurnAmount", Mathf.Lerp(mats[i].GetFloat("_BurnAmount"), 0.5f, Time.deltaTime * 3f));
			mats[i].SetColor("_EmissionColor", Color.Lerp(mats[i].GetColor("_EmissionColor"), new Color(0.5f, 0f, 0f, 0f), Time.deltaTime * 3f));
		}

		// Update punch material burn texture offset
		Vector2 currentOffset = spikeMat.GetTextureOffset("_BurnTex");
		currentOffset += burnOffsetSpeed * Time.deltaTime;
		spikeMat.SetTextureOffset("_BurnTex", currentOffset);

		// Update feedback time counter
		feedbackCounter += Time.deltaTime;

		if(feedbackCounter >= feedbackDelay)
		{
			for(int i = 0; i < mats.Count; i++)
			{
				mats[i].SetFloat("_BurnAmount", Mathf.Lerp(mats[i].GetFloat("_BurnAmount"), -0.1f, Time.deltaTime * 3f));
				mats[i].SetColor("_EmissionColor", Color.Lerp(mats[i].GetColor("_EmissionColor"), new Color(0f, 0f, 0f, 0f), Time.deltaTime * 3f));
			}
		}

		if(health <= 0)
		{
			// Reset feedback time counter
			feedbackCounter = 0f;

			SetDie();
		}
	}

	private void DieBehaviour()
	{
		// Update boss materials dissolve amount based on animation curve
		for(int i = 0; i < mats.Count; i++) mats[i].SetFloat("_DissolveAmount", Mathf.Lerp(-0.1f, 1.1f, dissolveCurve.Evaluate(timeCounter / dieDuration)));

		// Update time counter
		timeCounter += Time.deltaTime;

		if(timeCounter >= dieDuration)
		{
			// Set boss exit state
			state++;

			// Reset time counter
			timeCounter = 0f;

			// Disable boss game object
			gameObject.SetActive(false);
		}
	}
	#endregion

	#region Invoked Methods
	private void StartEvent()
	{
		// Call all start event methods
		startEvent.Methods.Invoke();
	}

	private void DisablePunch()
	{
		// Disable punch collider
		punchColl.enabled = false;
	}
	#endregion

	#region Debug Methods
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		// Draw range outter limit circle
		Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(trans.position, rangeLimits.y);

		// Draw range inner limit circle
		Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(trans.position, rangeLimits.x);
	}
#endif
	#endregion

	#region Properties
	public float MaxHealth
	{
		get { return maxHealth; }
	}

	public float Health
	{
		get { return health; }
	}
	#endregion

	#region Serializable
	[System.Serializable]
	public class BossEvent
	{
		#region Inspector Attributes
		[SerializeField] private UnityEvent methods;
		[SerializeField] private float delay;
		#endregion

		#region Properties
		public UnityEvent Methods
		{
			get { return methods; }
		}

		public float Delay
		{
			get { return delay; }
		}
		#endregion
	}
	#endregion
}
