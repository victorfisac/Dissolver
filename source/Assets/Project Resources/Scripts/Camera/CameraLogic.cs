using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class CameraLogic : MonoBehaviour 
{
	#region Enums
	public enum CameraStates { ORBIT, TOOBJECTIVE, OBJECTIVE, TOORBIT };
	#endregion

	#region Public Attributes
	[Header("Settings")]
	[SerializeField] private CameraStates state;
	[SerializeField] private bool handleMouse;
	[SerializeField] private bool handleGamepad;
	[SerializeField] private Vector2 initRotation;
	[SerializeField] private float initDistance;
	[SerializeField] private bool savedOrbit;

	[Header("Orbit Mode")]
	[SerializeField] private PositionSettings position = new PositionSettings();
	[SerializeField] private OrbitSettings orbit = new OrbitSettings();
	[SerializeField] private InputSettings input = new InputSettings();
	[SerializeField] private DebugSettings debug = new DebugSettings();
	[SerializeField] private CollisionHandler collision = new CollisionHandler();

	[Header("Interpolation")]
	[SerializeField] private string transitionInput;
	[SerializeField] private AnimationCurve lerpCurve;
	[SerializeField] private float lerpDuration;
	[SerializeField] private float offsetLerpScale;

	[Header("Switch")]
	[SerializeField] private AnimationCurve switchCurve;
	[SerializeField] private float switchDuration;

	[Header("Shake")]
	[SerializeField] private Vector3 shakeVector;
	[SerializeField] private AnimationCurve shakeCurve;
	[SerializeField] private float shakeDuration;

	[Header("Distortion")]
	[SerializeField] private ScreenDistortion screenDistortion;

	[Header("Blur")]
	[SerializeField] private AnimationCurve blurCurve;
	[SerializeField] private float blurDuration;
	[SerializeField] private BlurOptimized blurEffect;

	[Header("Zoom")]
	[SerializeField] private AnimationCurve zoomCurve;
	[SerializeField] private float zoomDuration;

	[Header("Offset")]
	[SerializeField] private Vector3 objectiveOffset;
	[SerializeField] private AnimationCurve offsetCurve;
	[SerializeField] private float offsetDuration;

	[Header("References")]
	[SerializeField] private Camera cam;
	#endregion

	#region Private Attributes
	// Orbit
	private Transform target;												// Camera target transform reference
	private Vector3 targetPos;												// Calculated target position with offset
	private Vector3 destination;											// Default destination position for interpolation
	private Vector3 adjustedDestination;									// Destination position after clipping correction
	private Vector3 camVel;													// Camera movement speed
	private float vOrbitInput, hOrbitInput, zoomInput, hOrbitSnapInput;		// Orbit input values
	private bool getInputs;													// Get inputs state

	// Shake
	private bool shakeState;												// Camera shake state
	private float shakeCounter;												// Camera shake animation counter
	private float shakeAmount;												// Camera shake amount (scale)

	// Blur
	private int blurState;													// Blur animation state
	private float blurCounter;												// Blur time counter
	private bool canWork;													// Camera logic can work state

	// Switch
	private int switchState;												// Camera change state
	private float switchCounter;											// Camera switch time counter
	private Vector3 switchInit;												// Camera offset start value

	// Zoom
	private float zoomCounter;												// Zoom animation time counter
	private float zoomStart;												// Zoom aniamtion start value
	private float zoomTarget;												// Zoom animation end value
	private bool zoomState;													// Zoom animation state

	// Interpolation
	private float lerpCounter;												// Lerp interpolation time counter
	private bool canLerp;													// Can lerp state

	// Aim
	private Transform objective;											// Objective mode target transform reference

	// Offset
	private bool isOffset;													// Offset transition state
	private float offsetCounter;											// Offset animation time counter
	private Vector3 startOffset;											// Offset animation start value
	private Vector3 endOffset;												// Offset animation end value
	private CameraStates endState;											// Offset animation end state

	// References
	private GameplayManager gameplayManager;								// Gameplay manager reference
	private GameManager gameManager;										// Game manager reference
	#endregion

	#region Main Methods
	public void AwakeBehaviour(Transform tar, GameplayManager gameplay)
	{
		// Get references
		gameplayManager = gameplay;
		gameManager = GameManager.Instance;

		// Initialize target
		canWork = true;
		getInputs = true;
		switchInit = position.offset;
		target = tar;
		SetCameraTarget(target);

		// Awake screen distortion effect behaviour
		screenDistortion.AwakeDistortion();

		// Initialize input
		vOrbitInput = hOrbitInput = zoomInput = hOrbitSnapInput = 0f;

		// Initialize camera orbit position
		MoveToTarget();

		// Initialize collisions
		collision.Initialize(Camera.main);
		collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
		collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desiredCameraClipPoints);

		// Initialize position from saved data or inspector
		if(savedOrbit)
		{
			orbit.xRotation = gameManager.CameraRotation.x;
			orbit.yRotation = gameManager.CameraRotation.y;
		}
		else
		{
			orbit.xRotation = initRotation.x;
			orbit.yRotation = initRotation.y;
		}

		// Initialize values based on inspector start values
		position.distanceFromTarget = initDistance;
		position.newDistance = initDistance;

		// Initialize zoom animation values
		zoomTarget = initDistance;
	}

	public void UpdateBehaviour(bool dead)
	{
		if(target && canWork)
		{
			if(dead) LookAtTarget();
			else
			{
				// Update camera logic behaviour
				GetInput();
				ZoomInOnTarget();
				UpdateSwitch();
				UpdateZoom();
				UpdateCamera();
				UpdateOffset();
			}

			// Update screen distortion effect behaviour
			screenDistortion.UpdateDistortion();
		}

		// NOTE: UpdateBlur is called from interface manager
	}

	private void UpdateSwitch()
	{
		switch(switchState)
		{
			case 0:
			{
				if(Input.GetButtonDown(input.SWITCH_OFFSET) && getInputs)
				{
					// Update switch state
					switchState = 1;

					// Reset time counter
					switchCounter = 0f;
				}
			} break;
			case 1:
			{
				// Update current camera offset based on animation curve
				position.offset = Vector3.Lerp(switchInit, -switchInit, switchCurve.Evaluate(switchCounter / switchDuration));

				// Update switch time counter
				switchCounter += Time.deltaTime;

				if(switchCounter >= switchDuration)
				{
					// Fix camera offset value
					position.offset = -switchInit;

					// Reset time counter
					switchCounter = 0f;

					// Update switch state
					switchState = 2;
				}
			} break;
			case 2:
			{
				if(Input.GetButtonDown(input.SWITCH_OFFSET) && getInputs)
				{
					// Update switch state
					switchState = 3;

					// Reset time counter
					switchCounter = 0f;
				}
			} break;
			case 3:
			{
				// Update current camera offset based on animation curve
				position.offset = Vector3.Lerp(-switchInit, switchInit, switchCurve.Evaluate(switchCounter / switchDuration));

				// Update switch time counter
				switchCounter += Time.deltaTime;

				if(switchCounter >= switchDuration)
				{
					// Fix camera offset value
					position.offset = switchInit;

					// Reset time counter
					switchCounter = 0f;

					// Update switch state
					switchState = 0;
				}
			} break;
			default: break;
		}
	}

	private void UpdateCamera()
	{
		switch(state)
		{
			case CameraStates.ORBIT:
			{
				MoveToTarget();
				LookAtTarget();

				if(handleGamepad) OrbitTarget();
				if(handleMouse) MouseOrbitTarget();
			} break;
			case CameraStates.OBJECTIVE:
			{
				if(!objective) break;

				MoveToObjective();
				LookAtObjective();
			} break;
			case CameraStates.TOORBIT:
			{
				ToOrbit();
				OrbitTarget();
			} break;
			case CameraStates.TOOBJECTIVE:
			{
				ToObjective();
			} break;
			default: break;
		}

		collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
		collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desiredCameraClipPoints);

		// Draw debug
		for(int i = 0; i < 5; i++)
		{
			if(debug.drawDesiredCollisionLines) Debug.DrawLine(targetPos, collision.desiredCameraClipPoints[i], Color.white);
			if(debug.drawAdjustedCollisionLines) Debug.DrawLine(targetPos, collision.adjustedCameraClipPoints[i],Color.green);
		}

		collision.CheckColliding(targetPos);
		position.adjustmentDistance = collision.GetAdjustedDistanceWithRayFrom(targetPos);

		if(shakeState) UpdateShake();
	}

	private void UpdateOffset()
	{
		if(isOffset)
		{
			// Update current offset based on animation curve
			position.offset = Vector3.Lerp(startOffset, endOffset, offsetCurve.Evaluate(offsetCounter / offsetDuration));

			// Update offset time counter
			offsetCounter += Time.deltaTime;

			if(offsetCounter >= offsetDuration)
			{
				// Reset offset time counter
				offsetCounter = 0f;

				// Disable offset transition state
				isOffset = false;

				// Fix current offset end value
				position.offset = endOffset;

				// Reset offset transition start and end values
				startOffset = Vector3.zero;
				endOffset = Vector3.zero;

				// Update current camera state
				state = endState;

				// Reset lerp counter
				lerpCounter = 0f;
			}
		}
	}
	#endregion

	#region Input Methods
	public void SetCameraInputs(bool newState)
	{
		// Update current get inputs state
		getInputs = newState;
	}

	private void SetCameraTarget(Transform t)
	{
		// Update target transform reference
		target = t;

	#if DEBUG_BUILD
		if(!target) Debug.LogError("CameraLogic: the camera needs a target");
	#endif
	}

	private void GetInput()
	{
		if(getInputs)
		{
			if(handleGamepad)
			{
				vOrbitInput = Input.GetAxisRaw(input.ORBIT_VERTICAL);
				hOrbitInput = Input.GetAxisRaw(input.ORBIT_HORIZONTAL);
			}

			zoomInput = Input.GetAxisRaw(input.ZOOM);
			hOrbitSnapInput = Input.GetAxisRaw(input.ORBIT_HORIZONTAL_SNAP);

			// GetCameraStateInput();
		}
		else
		{
			// Reset inputs values
			vOrbitInput = 0f;
			hOrbitInput = 0f;
			zoomInput = 0f;
		}
	}

	private void GetCameraStateInput()
	{
		// Update camera switch state
		canLerp = (state != CameraStates.TOOBJECTIVE && state != CameraStates.TOORBIT);

		// Check camera change input
		if(Input.GetButton(transitionInput) && canLerp && state == CameraStates.ORBIT)
		{
			// Calculate closest enemy transform reference
			 objective = gameplayManager.GetClosestFocusedEnemy();

		  	 // Get its camera target transform reference (first child)
			 if(objective) objective = objective.GetChild(0);
		}

		// Check for change 
		// TODO: check if there is any target
		if(Input.GetButtonUp(transitionInput) && canLerp && (gameplayManager.Characters.Count > 1))
		{
			// Update camera state based on previous state
			state = ((state == CameraStates.ORBIT) ? CameraStates.TOOBJECTIVE : CameraStates.TOORBIT);

			// Reset lerp counter
			lerpCounter = 0f;

			if(state == CameraStates.TOORBIT)
			{
				Vector3 currentEulers = Quaternion.LookRotation(objective.position - transform.position).eulerAngles;
				orbit.yRotation = currentEulers.y - 180;
			}
		}

		// Fix camera state if objective doesn't exist
		if(state == CameraStates.TOOBJECTIVE && !objective) state = CameraStates.ORBIT;
		else if(state == CameraStates.OBJECTIVE && !objective)
		{
			// Reset camera state to orbit
			state = CameraStates.TOORBIT;

			// Reset lerp counter
			lerpCounter = 0f;
		}
	}

	public void SetCameraObjective(Transform obj)
	{
		// Update camera objective reference
		objective = obj;
	}

	public void SetCameraOffset(bool objec)
	{
		// Update end value camera offset
		endOffset = (objec ? objectiveOffset : Vector3.right);

		// Get current camera offset
		startOffset = (objec ? position.offset : objectiveOffset);

		// Update offset animation end state
		endState = (objec ? CameraStates.TOOBJECTIVE : CameraStates.TOORBIT);

		// Enable offset transition state
		isOffset = true;

		// Reset offset animation time counter
		offsetCounter = 0f;
	}

	public void ResetCamera()
	{
		// Reset camera state to orbit with interpolation
		state = CameraStates.TOORBIT;
		lerpCounter = 0f;

		// Reset orbit camera state values
		if(objective)
		{
			Vector3 currentEulers = Quaternion.LookRotation(objective.position - transform.position).eulerAngles;
			orbit.yRotation = currentEulers.y - 180;

			// Reset objective reference
			objective = null;
		}
	}
	#endregion

	#region Camera Methods
	private void MoveToTarget()
	{
		targetPos = target.position + Vector3.up * position.offset.y + transform.TransformDirection(Vector3.forward * position.offset.z) + transform.TransformDirection(Vector3.right * position.offset.x);
		destination = targetPos + Quaternion.Euler(orbit.xRotation, orbit.yRotation, 0f) * Vector3.back * position.distanceFromTarget;

		if(collision.colliding)
		{
			adjustedDestination = Quaternion.Euler(orbit.xRotation, orbit.yRotation, 0f) * Vector3.forward * position.adjustmentDistance;
			adjustedDestination += targetPos;

			if(position.smoothFollow) transform.position = Vector3.SmoothDamp(transform.position, adjustedDestination, ref camVel, position.smooth);
			else transform.position = adjustedDestination;
		}
		else
		{
			if(position.smoothFollow) transform.position = Vector3.SmoothDamp(transform.position, destination, ref camVel, position.smooth);
			else transform.position = destination;
		}
	}

	private void LookAtTarget()
	{
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetPos - transform.position), 100f * Time.deltaTime);
	}

	private void OrbitTarget()
	{
		if(hOrbitSnapInput > 0) orbit.yRotation = target.eulerAngles.y - 180f;

		orbit.xRotation += -vOrbitInput * orbit.vOrbitSmooth * 1f/Time.timeScale * gameManager.Sensitivity;
		orbit.yRotation += hOrbitInput * orbit.hOrbitSmooth * 1f/Time.timeScale * gameManager.Sensitivity;

		CheckVerticalRotation();
	}

	private void MouseOrbitTarget()
	{
		if(hOrbitSnapInput > 0) orbit.yRotation = target.eulerAngles.y - 180f;

		orbit.yRotation += Input.GetAxis(input.ORBIT_MOUSE_HORIZONTAL) * (orbit.hOrbitSmooth / 5);
		orbit.xRotation += Input.GetAxis(input.ORBIT_MOUSE_VERTICAL) * (orbit.vOrbitSmooth / 5);

		CheckVerticalRotation();
	}

	private void CheckVerticalRotation()
	{
		if(orbit.xRotation > orbit.maxXRotation) orbit.xRotation = orbit.maxXRotation;
		if(orbit.xRotation < orbit.minXRotation) orbit.xRotation = orbit.minXRotation;
	}

	private void ZoomInOnTarget()
	{
		position.newDistance += position.zoomStep * zoomInput;

		position.distanceFromTarget = Mathf.Lerp(position.distanceFromTarget, position.newDistance, position.zoomSmooth * Time.deltaTime);

		if(position.distanceFromTarget > position.maxZoom)
		{
			position.distanceFromTarget = position.maxZoom;
			position.newDistance = position.maxZoom;
		}

		if(position.distanceFromTarget < position.minZoom)
		{
			position.distanceFromTarget = position.minZoom;
			position.newDistance = position.minZoom;
		}
	}
	#endregion

	#region Objective Methods
	private void MoveToObjective()
	{
		// Update target and destination position based on objective
		targetPos = target.position + Vector3.up * position.offset.y + target.TransformDirection(Vector3.forward * position.offset.z) + target.TransformDirection(Vector3.right * position.offset.x);

		destination = transform.rotation * new Vector3(0f, 0f, position.distanceFromTarget) + target.position + Vector3.up * position.offset.y + objective.TransformDirection(Vector3.back * position.offset.z) + target.TransformDirection(Vector3.right * position.offset.x);

		if(collision.colliding)
		{
			//adjustedDestination = targetPos + Quaternion.Euler(0f, 0f, position.distanceFromTarget) * Vector3.back * position.adjustmentDistance + Vector3.up * position.offset.y + target.TransformDirection(Vector3.forward * position.offset.z) + target.TransformDirection(Vector3.right * position.offset.x);
			adjustedDestination = transform.rotation * new Vector3(0f, 0f, -position.adjustmentDistance) + target.position + Vector3.up * position.offset.y + target.TransformDirection(Vector3.forward * position.offset.z) + target.TransformDirection(Vector3.right * position.offset.x);

			if(position.smoothFollow) transform.position = Vector3.SmoothDamp(transform.position, adjustedDestination, ref camVel, position.smooth);
			else transform.position = adjustedDestination;
		}
		else
		{
			if(position.smoothFollow) transform.position = Vector3.SmoothDamp(transform.position, destination, ref camVel, position.smooth);
			else transform.position = destination;
		}
	}

	private void LookAtObjective()
	{
		// Calculate camera rotation to look at objective
		Quaternion targetRotation = Quaternion.LookRotation(objective.position - transform.position);
		transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 100f * Time.deltaTime);
	}
	#endregion

	#region Interpolation Methods
	private void ToOrbit()
	{
		if(lerpCounter <= lerpDuration)
		{
			targetPos = target.position + Vector3.up * position.offset.y + Vector3.forward * position.offset.z + Vector3.right * position.offset.x;
			destination = targetPos + Quaternion.Euler(orbit.xRotation, orbit.yRotation, 0f) * Vector3.back * position.distanceFromTarget;

			if(collision.colliding)
			{
				adjustedDestination = Quaternion.Euler(orbit.xRotation, orbit.yRotation, 0f) * Vector3.forward * position.adjustmentDistance;
				adjustedDestination += targetPos;

				if(position.smoothFollow) transform.position = Vector3.Lerp(transform.position, adjustedDestination, lerpCurve.Evaluate(lerpCounter / lerpDuration));
				else transform.position = adjustedDestination;
			}
			else
			{
				if(position.smoothFollow) transform.position = Vector3.Lerp(transform.position, destination, lerpCurve.Evaluate(lerpCounter / lerpDuration));
				else transform.position = destination;
			}

			// Update camera rotation to interpolate between orbit and objective camera modes
			Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lerpCurve.Evaluate(lerpCounter / lerpDuration));

			// Update lerp counter
			lerpCounter += Time.deltaTime;
		}
		else
		{
			lerpCounter = 0f;
			state = CameraStates.ORBIT;
		}
	}

	private void ToObjective()
	{
		if(lerpCounter <= lerpDuration)
		{
			targetPos = target.position + Vector3.up * position.offset.y + target.TransformDirection(Vector3.forward * position.offset.z) + target.TransformDirection(Vector3.right * position.offset.x);
			destination = transform.rotation * new Vector3(0f, 0f, position.distanceFromTarget) + target.position + Vector3.up * position.offset.y + objective.TransformDirection(Vector3.back * position.offset.z) + target.TransformDirection(Vector3.right * position.offset.x);
			transform.position = Vector3.Lerp(transform.position, destination, lerpCurve.Evaluate(lerpCounter / lerpDuration));

			// Update camera rotation to interpolate between orbit and objective camera modes
			Quaternion targetRotation = Quaternion.LookRotation(objective.position - transform.position);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lerpCurve.Evaluate(lerpCounter / lerpDuration));

			// Update lerp counter
			lerpCounter += Time.deltaTime;
		}
		else
		{
			// Reset lerp counter
			lerpCounter = 0f;

			// Update camera state
			state = CameraStates.OBJECTIVE;
		}
	}
	#endregion

	#region Zoom Methods
	private void UpdateZoom()
	{
		if(zoomState)
		{
			// Update current zoom based on animation curve
			position.newDistance = Mathf.Lerp(zoomStart, zoomTarget, zoomCurve.Evaluate(zoomCounter / zoomDuration));

			position.distanceFromTarget = position.newDistance;

			// Update zoom time counter
			zoomCounter += Time.deltaTime;

			if(zoomCounter >= zoomDuration)
			{
				// Reset zoom time counter
				zoomCounter = 0f;

				// Fix current zoom value
				position.newDistance = zoomTarget;

				// Disable zoom animation state
				zoomState = false;
			}
		}
	}

	public void SetZoom(float target)
	{
		// Enable zoom state
		zoomState = true;

		// Reset zoom time counter
		zoomCounter = 0f;

		// Update zoom end value
		zoomTarget = target;

		// Update zoom start value
		zoomStart = position.distanceFromTarget;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("CameraLogic: camera zoom set to " + zoomTarget);
	#endif
	}

	public void ResetZoom()
	{
		// Enable zoom state
		zoomState = true;

		// Reset zoom time counter
		zoomCounter = 0f;

		// Update zoom end value
		zoomTarget = initDistance;

		// Update zoom start value
		zoomStart = position.distanceFromTarget;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("CameraLogic: camera zoom reset successful");
	#endif
	}
	#endregion

	#region Shake Methods
	public void ApplyShake(float amount)
	{
		// Update shake state
		shakeState = true;

		// Update shake amount
		shakeAmount = amount;

		// Reset shake counter
		shakeCounter = 0f;
	}

	public void ApplyShake()
	{
		// Update shake state
		shakeState = true;

		// Update shake amount
		shakeAmount = 1f;

		// Reset shake counter
		shakeCounter = 0f;
	}

	private void UpdateShake()
	{
		// Move camera based on shake vector and curve
		transform.position += shakeVector * shakeCurve.Evaluate(shakeCounter / shakeDuration) * shakeAmount;

		// Update shake counter
		shakeCounter += Time.deltaTime;

		// Reset shake state if counter finished
		if(shakeCounter >= shakeDuration)
		{
			// Reset shake state and amount
			shakeState = false;
			shakeAmount = 1f;
		}
	}
	#endregion

	#region Blur Methods
	public void PauseCamera(bool state)
	{
		// Update pause camera blur
		if(state)
		{
			// Enable blur effect
			blurEffect.enabled = true;

			// Update blur state
			blurState = 1;
		}
		else blurState = 3;

		// Update can move state
		canWork = !state;
	}

	public void UpdateBlur()
	{
		switch(blurState)
		{
			case 1:
			{
				// Update blur size based on animation curve
				blurEffect.blurSize = blurCurve.Evaluate(blurCounter / blurDuration); 

				// Update blur time counter
				blurCounter += Time.deltaTime;

				if(blurCounter >= blurDuration)
				{
					// Reset blur counter
					blurCounter = 0f;

					// Update blur state
					blurState = 2;
				}
				break;
			}
			case 3:
			{
				// Update blur size based on animation curve
				blurEffect.blurSize = blurCurve.Evaluate((blurDuration - blurCounter) / blurDuration); 

				// Update blur time counter
				blurCounter += Time.deltaTime;

				if(blurCounter >= blurDuration)
				{
					// Reset blur counter
					blurCounter = 0f;

					// Update blur state
					blurState = 0;

					// Disable blur effect
					blurEffect.enabled = false;
				}
				break;
			}
		}
	}
	#endregion

	#region Properties
	public Vector2 CameraOrbit
	{
		get { return new Vector2(orbit.xRotation, orbit.yRotation); }
		set 
		{
			orbit.xRotation = value.x;
			orbit.yRotation = value.y;
		}
	}

	public float InitZoom
	{
		get { return initDistance; }
	}

	public Camera Cam
	{
		get { return cam; }
	}

	public ScreenDistortion Distortion
	{
		get { return screenDistortion; }
	}
	#endregion

	#region Serializable
	[System.Serializable]
	public class PositionSettings
	{
		public Vector3 offset;
		public float distanceFromTarget = -4f;
		public float zoomSmooth = 100;
		public float zoomStep = 2f;
		public float maxZoom = -2f;
		public float minZoom = -15f;
		public bool smoothFollow = true;
		public float smooth = 0.05f;

		[HideInInspector] public float newDistance = -4f;
		[HideInInspector] public float adjustmentDistance = -4f;
	}

	[System.Serializable]
	public class OrbitSettings
	{
		public float xRotation = -20f;
		public float yRotation = -180f;
		public float maxXRotation = 25f;
		public float minXRotation = -50f;
		public float vOrbitSmooth = 0.5f;
		public float hOrbitSmooth = 0.5f;
	}

	[System.Serializable]
	public class InputSettings
	{
		public string ORBIT_MOUSE_HORIZONTAL = "Mouse X";
		public string ORBIT_MOUSE_VERTICAL = "Mouse Y";
		public string ORBIT_HORIZONTAL_SNAP = "Center Camera";
		public string ORBIT_HORIZONTAL = "Mouse X";
		public string ORBIT_VERTICAL = "Mouse Y";
		public string ZOOM = "Mouse ScrollWheel";
		public string SWITCH_OFFSET = "Change Camera";
	}

	[System.Serializable]
	public class DebugSettings
	{
		public bool drawDesiredCollisionLines = true;
		public bool drawAdjustedCollisionLines = true;
	}

	[System.Serializable]
	public class CollisionHandler
	{
		public LayerMask collisionLayer;
		
		[HideInInspector] public bool colliding = false;
		[HideInInspector] public Vector3[] adjustedCameraClipPoints;
		[HideInInspector] public Vector3[] desiredCameraClipPoints;

		private Camera camera;
		private Vector3 vector = new Vector3();
		private RaycastHit hit;
		float distance;

		public void Initialize(Camera cam)
		{
			camera = cam;
			adjustedCameraClipPoints = new Vector3[5];
			desiredCameraClipPoints = new Vector3[5];
		}

		public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
		{
			if(!camera) return;

			// Clear the contens of intoArray
			intoArray = new Vector3[5];

			vector.z = camera.nearClipPlane;
			vector.x = Mathf.Tan(camera.fieldOfView / 3.41f) * vector.z;
			vector.y = vector.x / camera.aspect;

			// Top left
			intoArray[0] = (atRotation * new Vector3(-vector.x, vector.y, vector.z)) + cameraPosition;

			// Top right
			intoArray[1] = (atRotation * new Vector3(vector.x, vector.y, vector.z)) + cameraPosition;

			// Bottom left
			intoArray[2] = (atRotation * new Vector3(-vector.x, -vector.y, vector.z)) + cameraPosition;

			// Bottom right
			intoArray[3] = (atRotation * new Vector3(vector.x, -vector.y, vector.z)) + cameraPosition;

			// Camera position
			intoArray[4] = cameraPosition - camera.transform.forward;
		}

		bool CollisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition)
		{
			for(int i = 0; i < clipPoints.Length; i++)
			{
				if(Physics.Raycast(new Ray(fromPosition, clipPoints[i] - fromPosition), Vector3.Distance(clipPoints[i], fromPosition), collisionLayer)) return true;
			}

			return false;
		}

		public float GetAdjustedDistanceWithRayFrom(Vector3 from)
		{
			distance = -1;

			for(int i = 0; i < desiredCameraClipPoints.Length; i++)
			{
				if(Physics.Raycast(new Ray(from, desiredCameraClipPoints[i] - from), out hit, Vector3.Distance(desiredCameraClipPoints[i], from), collisionLayer))
				{
					if(distance == -1) distance = hit.distance;
					else if(hit.distance < distance) distance = hit.distance;
				}
			}

			if(distance == -1) return 0;
			else return distance;
		}

		public void CheckColliding(Vector3 targetPosition)
		{
			colliding = CollisionDetectedAtClipPoints(desiredCameraClipPoints, targetPosition);
		}
	}
	#endregion
}
