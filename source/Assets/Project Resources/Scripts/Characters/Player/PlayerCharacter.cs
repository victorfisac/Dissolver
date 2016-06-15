using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerCharacter : Character 
{
	#region Inspector Attributes
	[Header("Status")]
	[SerializeField] private bool savedPosition;

    [Header("Transformation")]
    [SerializeField] private bool handleTransformation;
    [SerializeField] private Transformation transformation;

    [Header("Swords")]
	[SerializeField] private GameObject swordsObject;

	[Header("Skill")]
	[SerializeField] private bool skillFeedback;
	#endregion

	#region Private Attributes
	// Camera
	private Transform cameraTrans;		// Main camera transform reference
	private Vector3 cameraForward;		// Current main camera forward direction

    // Transformation
    private bool transformInput;        // Current transform input

    // Interact
    private bool canPlay;				// Player can play state
	#endregion

	#region Main Methods
	public override void AwakeBehaviour(CameraLogic cameraLogic)
	{
		// Call base class Awake method
		base.AwakeBehaviour(cameraLogic);

		// Awake player transformation
		transformation.AwakeBehaviour();

		// Get references
		cameraTrans = cameraLogic.transform;

		// Initialize values
		slots = maxSlots;
		isGrounded = true;
		canPlay = true;

		// Load player position and rotation from game manager if needed
		if(savedPosition && SceneManager.GetActiveScene().name != "demo")
		{
			trans.position = gameManager.PlayerPosition;
			trans.rotation = Quaternion.Euler(gameManager.PlayerRotation);
		}
	}

	public override void UpdateBehaviour()
	{
		if(canPlay)
		{
			// Calculate movement relative to camera
			cameraForward = Vector3.Scale(cameraTrans.forward, new Vector3(1f, 0f, 1f)).normalized;
			move = move.z * cameraForward + move.x * cameraTrans.right;

			if(!secondary) move *= (0.5f + run);
			else move *= 0.5f;

			if(freeMovement) transform.Translate(trans.TransformDirection(move) * Time.deltaTime * 10f);
		}
		else
		{
			// Reset movement input values
			move = Vector3.zero;
			jump = false;
			run = 0f;

			// Reset combat input values
			attack = false;
			secondary = false;
			skill = false;
			defend = false;
			action = false;
	        transformInput = false;
		}

		if(handleTransformation) transformation.UpdateTransformation(transformInput);

		if(handleFeedback && skillFeedback && maxSlots > 0) feedback.SetBurn(slots == maxSlots);

		// Call base class Update method
		base.UpdateBehaviour();
	}
	#endregion

	#region Player Methods
	public void GetPlayerInputs()
	{
		// Update movement input values
		move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		jump = Input.GetButtonDown("Jump");

		// Check run input when there is enough stamina
		if(CheckStamina(runStamina))
		{
			// Calculate run amount based on axis input
			run = Input.GetAxis("Run");

			// Subtract stamina from character
			if(move.magnitude > 0) SubtractStamina(run * runStamina);
		}
		else run = 0f;

		// Update combat input values
		attack = Input.GetButtonDown("Fire1");
		secondary = (Input.GetButton("Fire2") ||( Input.GetAxis("Fire2") > 0f));
		skill = Input.GetButtonDown("Skill");
		defend = Input.GetButton("Defend");
		action = Input.GetButtonDown("Action");
        transformInput = Input.GetButtonDown("Transform");

        GetDebugInputs();
	}

	private void GetDebugInputs()
	{
		// Update cheat input values
        if(Input.GetKeyDown(KeyCode.U)) slots = maxSlots;
		else if(Input.GetKeyDown(KeyCode.I)) maxSlots = 100;

		// Check for screenshot input
		if(Input.GetKeyDown(KeyCode.F12)) Application.CaptureScreenshot("screenshot.png", 2);

		// God mode and free movement input
		if(Input.GetKeyDown(KeyCode.G)) 
		{
			godMode = !godMode;
			if(godMode)
			{
				health = MaxHealth;
				stamina = MaxStamina;
				slots = maxSlots;
			}
		}

		if(Input.GetKeyDown(KeyCode.H)) 
		{
			freeMovement = !freeMovement;
			controller.enabled = !freeMovement;
		}

		// Apply free moment logic if enabled
		if(freeMovement)
		{
			if(Input.GetKey(KeyCode.LeftShift))
			{
				// Transform Z axis movement to Y axis
				move.y = move.z;
				move.z = 0;
			}

			// Calculate movement relative to camera
			cameraForward = Vector3.Scale(cameraTrans.forward, (Input.GetKey(KeyCode.LeftShift) ? new Vector3(1f, 1f, 0f) : new Vector3(1f, 0f, 1f))).normalized;
			move = (Input.GetKey(KeyCode.LeftShift) ? move.y : move.z) * cameraForward + move.x * cameraTrans.right;
		}
	}
	#endregion

	#region Character Methods
	public override void SetDie ()
	{
		// Switch to defeat gameplay state
		if(isPlayer) GameObject.FindWithTag("GameController").GetComponent<GameplayManager>().SetDefeat();

		// Call base class SetDie method
		base.SetDie ();
	}
	#endregion

	#region Player Methods
	public void SetSwords(bool state)
	{
		// Enable swords game object
		swordsObject.SetActive(state);
	}

	public void SetPlay(bool state)
	{
		// Update player can play state
		canPlay = state;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("PlayerCharacter: player can play state set to: " + canPlay.ToString());
	#endif
	}
	#endregion
}
