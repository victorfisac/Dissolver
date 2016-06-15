using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Transformation : MonoBehaviour
{
    #region Enums
    public enum TransformationState { BULLET, TANK };
    #endregion

    #region Inspector Attributes
    [Header("Scale")]
    [SerializeField] private Vector3 tankScale;
	[SerializeField] private Vector3 bulletScale;

	[Header("Camera")]
	[SerializeField] private float tankZoom;

    [Header("Shader")]
	[SerializeField] private string attributeName;
	[SerializeField] private AnimationCurve curve;
    [SerializeField] private float duration;

	[Header("Renderers")]
	[SerializeField] private Renderer[] tankRenderer;
	[SerializeField] private Renderer[] bulletRenderer;

	[Header("Particles")]
	[SerializeField] private ParticleSystem[] particles;

	[Header("Audio")]
	[SerializeField] private AudioSource transSource;

	[Header("Colliders")]
	[SerializeField] private Collider[] bulletColls;
	[SerializeField] private Collider[] tankColls;

	[Header("Objects")]
	[SerializeField] private GameObject[] bulletObjs;
	[SerializeField] private GameObject[] tankObjs;

	[Header("Vibration")]
	[SerializeField] private float transformationVibration;

    [Header("References")]
	[SerializeField] private Character character;
    #endregion

    #region Private Attributes
    // Transformation
    private TransformationState state;				// Transformation state
    private bool inTransition;						// Transformation transition state
    private float counter;							// Transformation animation counter
    private bool canTransform;						// Player can transform state
    private int unlockState;						// Transformation unlock state (0 = TRUE, 1 = FALSE)

	// References
	private InterfaceManager interfaceManager;		// Interface manager reference
	private CameraLogic cameraLogic;				// Camera logic reference
	private ScreenDistortion screenDistortion;		// Main camera distortion reference
	private GameManager gameManager;				// Game manager reference
    #endregion

    #region Main Methods
    public void AwakeBehaviour()
    {
    	// Get references
    	gameManager = GameManager.Instance;
    	if(GameObject.Find("InterfaceManager")) interfaceManager = GameObject.Find("InterfaceManager").GetComponent<InterfaceManager>();
    	screenDistortion = Camera.main.GetComponent<ScreenDistortion>();
    	cameraLogic = Camera.main.GetComponent<CameraLogic>();

        // Initialize values
        canTransform = true;
		transform.localScale = Vector3.Lerp(tankScale, bulletScale, 1f);

		// Set start material values
		for(int i = 0; i < tankRenderer.Length; i++) tankRenderer[i].material.SetFloat(attributeName, 1.1f);
		for(int i = 0; i < bulletRenderer.Length; i++) bulletRenderer[i].material.SetFloat(attributeName, -0.1f);
		for(int i = 0; i < tankRenderer.Length; i++) tankRenderer[i].enabled = false;

		for(int i = 0; i < tankObjs.Length; i++) tankObjs[i].SetActive(false);
    }
    #endregion

    #region Transformation Methods
    public void UpdateTransformation(bool transformation)
    {
        if (inTransition)
        {
            if (counter <= duration)
            {
                // Update transition values
                switch (state)
                {
                    case TransformationState.BULLET:
                    {
                        // Update scale value
						transform.localScale = Vector3.Lerp(tankScale, bulletScale, curve.Evaluate(counter / duration));

                        // Update shader values
                        for(int i = 0; i < tankRenderer.Length; i++) tankRenderer[i].material.SetFloat(attributeName, curve.Evaluate(counter / duration));
                        for(int i = 0; i < bulletRenderer.Length; i++) bulletRenderer[i].material.SetFloat(attributeName, 1 - curve.Evaluate(counter / duration));
                        break;
                    }
                    case TransformationState.TANK:
                    {
                        // Update scale value
						transform.localScale = Vector3.Lerp(bulletScale, tankScale, curve.Evaluate(counter / duration));

                        // Update shader values
						for(int i = 0; i < bulletRenderer.Length; i++) bulletRenderer[i].material.SetFloat(attributeName, curve.Evaluate(counter / duration));
						for(int i = 0; i < tankRenderer.Length; i++) tankRenderer[i].material.SetFloat(attributeName, 1 - curve.Evaluate(counter / duration));
                        break;
                    }
                }

                // Apply gamepad vibration
                gameManager.SetVibration(transformationVibration);

                // Update transition counter
                counter += Time.deltaTime;
            }
            else
            {
                // Set final value
                switch (state)
                {
                    case TransformationState.BULLET:
                    {
						transform.localScale = Vector3.Lerp(tankScale, bulletScale, 1f);

						for(int i = 0; i < tankRenderer.Length; i++) tankRenderer[i].material.SetFloat(attributeName, 1.1f);
						for(int i = 0; i < bulletRenderer.Length; i++) bulletRenderer[i].material.SetFloat(attributeName, -0.1f);
						for(int i = 0; i < tankRenderer.Length; i++) tankRenderer[i].enabled = false;
						for(int i = 0; i < tankObjs.Length; i++) tankObjs[i].SetActive(false);
                        break;
                    }
                    case TransformationState.TANK:
                    {
						transform.localScale = Vector3.Lerp(bulletScale, tankScale, 1f);

						for(int i = 0; i < tankRenderer.Length; i++) tankRenderer[i].material.SetFloat(attributeName, -0.1f);
						for(int i = 0; i < bulletRenderer.Length; i++) bulletRenderer[i].material.SetFloat(attributeName, 1.1f);
						for(int i = 0; i < bulletRenderer.Length; i++) bulletRenderer[i].enabled = false;
						for(int i = 0; i < bulletObjs.Length; i++) bulletObjs[i].SetActive(false);
                        break;
                    }
                }

                // Reset values
                inTransition = false;
                counter = 0f;

				// Reset gamepad vibration
                gameManager.SetVibration(0f);

                // Store current health and stamina based proportionally
                float normalizedHealth = character.Health / character.MaxHealth;
                float normalizedStamina = character.Stamina / character.MaxStamina;

                // Update character movement animator index
                character.AnimIndex = (int)state;

                // Update new health and stamina values based on previous normalized values
                character.Health = character.MaxHealth * normalizedHealth;
                character.Stamina = character.MaxStamina * normalizedStamina;
            }
        }
        else
        {
            // Check transformation input
			if (transformation && !inTransition && character.Combat.PlayingAttack == 0 && canTransform && character.CanInteract && unlockState == 0) TransformPlayer();
        }
    }
    #endregion

    #region Transformation Methods
    public void TransformPlayer()
    {
		// Update state and counter
        inTransition = true;
        state = ((state == TransformationState.BULLET) ? TransformationState.TANK : TransformationState.BULLET);
        counter = 0f;

        // Play transformation sound
		if(transSource) transSource.Play();

        // Apply screen distortion effect
		Invoke("ApplyDistortion", 0f);

		// Update camera logic current zoom based on current transformation state
		cameraLogic.SetZoom((state == TransformationState.BULLET) ? cameraLogic.InitZoom : tankZoom);

		// Disable and enable current character colliders
		for(int i = 0; i < bulletColls.Length; i++) bulletColls[i].enabled = state == TransformationState.BULLET;
		for(int i = 0; i < tankColls.Length; i++) tankColls[i].enabled = state == TransformationState.TANK;

		// Update interface manager transformation state
		if(interfaceManager) interfaceManager.FacesState = ((state == TransformationState.BULLET) ? 3 : 1);

		// Play current state particle system
		particles[(int)state].Play();

        // Enable all renderers for transition
		for(int i = 0; i < tankRenderer.Length; i++) tankRenderer[i].enabled = true;
		for(int i = 0; i < bulletRenderer.Length; i++) bulletRenderer[i].enabled = true;

		// Enable all transformations game objects
		for(int i = 0; i < bulletObjs.Length; i++) bulletObjs[i].SetActive(true);
		for(int i = 0; i < tankObjs.Length; i++) tankObjs[i].SetActive(true);
    }

    public void PowerEffect()
    {
		// Apply screen distortion effect
		Invoke("ApplyDistortion", 0f);

		// Update camera logic current zoom based on current transformation state
		cameraLogic.SetZoom((state == TransformationState.BULLET) ? cameraLogic.InitZoom : tankZoom);

		// Play current state particle system
		particles[(int)state].Play();
    }

    private void ApplyDistortion()
    {
		// Apply distortion effect to camera
		screenDistortion.StartDistortion((Vector2)Camera.main.ScreenToViewportPoint(character.Trans.position + Vector3.up), true);
    }

    public void SetUnlock(int value)
    {
    	// Update unlock state
    	unlockState = value;
    }

    public void PowerAnimation()
    {
		// Disable player interactable state
		character.CanInteract = false;

		// Apply animator power trigger
		character.Anims[1].SetTrigger("Radial");

		Invoke("PlayerInteract", 2f);
    }

    private void PlayerInteract()
    {
    	// Enable player interactable state
    	character.CanInteract = true;
    }
    #endregion

    #region Properties
    public bool CanTransform
    {
        get { return canTransform; }
        set { canTransform = value; }
    }

    public int UnlockState
    {
    	get { return unlockState; }
    }
    #endregion
}
