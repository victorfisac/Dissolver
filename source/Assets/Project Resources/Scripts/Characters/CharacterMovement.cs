using UnityEngine;
using System.Collections;

public class CharacterMovement : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private bool applyMotion;
	[SerializeField] private bool applyGravity;

	[Header("Grounded")]
	[SerializeField] private bool jumpAccuracy;
	[SerializeField] private float groundDistance;
	[SerializeField] private LayerMask groundMask;

	[Header("Movement")]
	[SerializeField] private float[] moveSpeed;
	[SerializeField] private float[] airSpeed;

	[Header("Turn")]
	[SerializeField] private bool cameraTurn;
	[SerializeField] private float cameraTurnSpeed;
	[SerializeField] private float staticTurnSpeed;
	[SerializeField] private float dynamicTurnSpeed;

	[Header("Jump")]
	[SerializeField] private float[] jumpSpeed;
	[SerializeField] private float jumpStamina;

	[Header("Audio")]
	[SerializeField] private AudioSource jumpSource;
	[SerializeField] private AudioSource landingSource;

	[Header("References")]
	[SerializeField] private Character character;
	[SerializeField] private CharacterFoot[] foot;
	#endregion

	#region Private Attributes
	// Input
	private Vector3 input;						// Untransformed direction value
	private bool useGravity;					// Current movement use gravity state

	// Velocity
	private Vector3 desiredVelocity;			// Current desired velocity to move
	private Vector3 externalVelocity;			// Current external velocity to add
	private float forwardAmount;				// Current movement forward amount
	private float rightAmount;					// Current movement right amount
	private Vector3 groundNormal;				// Current ground normal vector
	private Vector3 moveDirection;				// Current final move direction
	private bool canMove;						// Current can move state

	// Turn
	private float turnAmount;					// Curren turn amount
	private float turnSpeed;					// Current turn speed
	private bool canTurn;						// Current can turn state

	// Grounded
	private bool grounded;						// Is grounded state
	private bool lastGrounded;					// Last frame grounded state
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Initialize values
		useGravity = true;
		canMove = true;
		canTurn = true;
		grounded = character.IsGrounded;
		lastGrounded = grounded;
	}

	public void Move(Vector3 direction, bool jump)
	{
		// Store last recorded grounded state
		lastGrounded = grounded;
		grounded = character.IsGrounded;

		if(jumpAccuracy) CheckGroundedValue();

		// Normalize move value if needed
		if(direction.magnitude > 1f) direction.Normalize();

		// Store input value
		input = direction;

		// Transform to inversed local space
		direction = character.Trans.InverseTransformDirection(direction);

		// Calculate forward and turn value
		forwardAmount = direction.z;
		rightAmount = direction.x;

		turnAmount = ((forwardAmount != 0f) ? Mathf.Atan2(direction.x, direction.z) : 0f);

		TurnRotation();

		if(canMove)
		{
			if(grounded) GroundMovement((character.AnimIndex == 0 && jump));
			else AirMovement();
		}
		else desiredVelocity = Vector3.Scale(desiredVelocity, Vector3.up);

		ApplyPhysics();

		UpdateGrounded();

		UpdateAnimator(jump);
	}
	#endregion

	#region Movement Methods

	private void CheckGroundedValue()
	{
		if(!grounded)
		{
			// Check character grounded state by raycast to ensure accuracy
			if(Physics.Raycast(character.Trans.position + Vector3.up * 0.01f, Vector3.down, groundDistance, groundMask)) grounded = true;
		}
	}

	private void TurnRotation()
	{
		// Calculate turn speed based on direction
		turnSpeed = Mathf.Lerp(staticTurnSpeed, dynamicTurnSpeed, Mathf.Abs(forwardAmount));

		if(cameraTurn)
		{
			// Apply rotation to transform based on current camera forward direction
			if(applyMotion && canTurn) character.Trans.rotation = Quaternion.Lerp(character.Trans.rotation, Quaternion.Euler(new Vector3(0f, Camera.main.transform.rotation.eulerAngles.y, 0f)), character.DeltaTime * cameraTurnSpeed);
		}
		else
		{
			// Apply rotation to transform
			if(applyMotion && canTurn) character.Trans.Rotate(0f, turnAmount * turnSpeed * character.DeltaTime, 0f);
		}
	}

	private void GroundMovement(bool jump)
	{
		// Calculate final move direction based on ground normal projection
		moveDirection = Vector3.ProjectOnPlane(input, groundNormal);

		// Calculate desired velocity based on direction value
		if(applyMotion) desiredVelocity = new Vector3(moveDirection.x * moveSpeed[character.AnimIndex], ((groundNormal.y != 1) ? (-groundNormal.y * 2f) : 0f), moveDirection.z * moveSpeed[character.AnimIndex]);

		// Check if player want to jump
		if(jump && character.CheckStamina(jumpStamina))
		{
			// Update desired velocity y axis value
			desiredVelocity.y = jumpSpeed[character.AnimIndex];

			// Subtract character stamina
			character.SubtractStamina(jumpStamina);

			// Play jump sound
			jumpSource.Play();
		}
	}

	private void AirMovement()
	{
		// Calculate desired velocity based on direction value
		if(applyMotion) desiredVelocity = new Vector3(input.x * airSpeed[character.AnimIndex], character.Controller.velocity.y, input.z * airSpeed[character.AnimIndex]);
	}

	private void ApplyPhysics()
	{
		// Apply gravity speed
		if(applyGravity)
		{
			if(useGravity) desiredVelocity.y += Physics.gravity.y * character.DeltaTime;
			else desiredVelocity.y = Physics.gravity.y * character.DeltaTime;
		}

		if(character.Controller.enabled)
		{
			// Apply movement to character controller
			character.Controller.Move((desiredVelocity + externalVelocity) * character.DeltaTime);

		#if DEBUG_BUILD
			// Trace debug message (overhead)
			// Debug.Log(desiredVelocity.ToString() + " + " + externalVelocity.ToString() + " = " + (desiredVelocity + externalVelocity).ToString());
		#endif
		}

		// Fix character rotation
		transform.rotation = Quaternion.Euler(Vector3.Scale(transform.rotation.eulerAngles, new Vector3(0, 1, 0)));
	}

	private void UpdateGrounded()
	{
		// Play grounding sound if character just grounded
		if(grounded && !lastGrounded) landingSource.Play();
	}

	public void UpdateFootDetections()
	{
		if(foot.Length > 0)
		{
			// Update current foot detections based on character animation index
			for(int i = 0; i < foot[character.AnimIndex].FootDetections.Length; i++) foot[character.AnimIndex].FootDetections[i].UpdateBehaviour();
		}
	}
	#endregion

	#region Animator Methods
	private void UpdateAnimator(bool isJump)
	{
		// Update animator movement values
		for(int i = 0; i < character.Anims.Length; i++)
		{
			character.Anims[i].SetFloat("Forward", forwardAmount);
			character.Anims[i].SetFloat("Right", rightAmount);
			character.Anims[i].SetFloat("Turn", turnAmount);

			// Update animator jumping value
			if(applyGravity)
			{
				character.Anims[i].SetBool("Jump", isJump);
				character.Anims[i].SetBool("IsGrounded", grounded);
				character.Anims[i].SetFloat("Jumping", desiredVelocity.y);
			}
		}
	}
	#endregion

	#region Detection Methods
	private void OnControllerColliderHit(ControllerColliderHit hit) 
	{
		// Check if hit comes from ground
		if(hit.moveDirection.y < -0.3f) groundNormal = hit.normal;
        else groundNormal = Vector3.up;

        // Check weak objects
        if(hit.gameObject.tag == "Weak") hit.gameObject.GetComponent<PlatformWeak>().MakeWeak();
    }
    #endregion

	#region Properties
	public Vector3 DesiredVelocity
	{
		get { return desiredVelocity; }
		set { desiredVelocity = value; }
	}

	public Vector3 ExternalVelocity
	{
		get { return externalVelocity; }
		set { externalVelocity = value; }
	}

	public bool UseGravity
	{
		set { useGravity = value; }
	}

	public bool CameraTurn
	{
		set { cameraTurn = value; }
	}

	public bool CanMove
	{
		set { canMove = value; }
	}

	public bool CanTurn
	{
		set { canTurn = value; }
	}

	public float ForwardAmount
	{
		get { return forwardAmount; }
	}
	#endregion

	#region Serializable
	[System.Serializable]
	public class CharacterFoot
	{
		#region Inspector Attributes
		[Header("References")]
		[SerializeField] private CharacterFootDetection[] footDetections;
		#endregion

		#region Properties
		public CharacterFootDetection[] FootDetections
		{
			get { return footDetections; }
		}
		#endregion
	}
	#endregion
}
