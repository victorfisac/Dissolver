using UnityEngine;
using System.Collections;

public class DoorLogic : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private bool openOnAwake;
	[SerializeField] private bool automatic;
	[SerializeField] private float colliderDelay;

	[Header("Visual")]
	[SerializeField] private AnimationCurve visualCurve;
	[SerializeField] private float visualDuration;
	[SerializeField] private Color closedColor;

	[Header("Module")]
	[SerializeField] private bool updateModules;
	[SerializeField] private int[] modules;
	[SerializeField] private GameObject moduleTriggers;

	[Header("References")]
	[SerializeField] private Animator animator;
	[SerializeField] private Collider coll;
	#endregion

	#region Private Attributes
	private bool opening;					// Current door opening state
	private bool canOpen;					// Can open door state
	private bool staying;					// Current player staying state
	private float timeCounter;				// Door logic time counter

	// Visual
	private Material[] materials;			// Door mesh renderers materials references
	private Color openColor;				// Open color reference
	private int visualState;				// Current visual state (0 = open, 1 = to close, 2 = close, 3 = to open)
	private float visualCounter;			// Materials animation time counter

	// References
	private ModuleManager moduleManager;	// Module manager reference
	#endregion

	#region Main Methods
	public void AwakeBehaviour(ModuleManager manager)
	{
		// Get references
		moduleManager = manager;
		MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

		// Initialize values
		canOpen = openOnAwake;

		materials = new Material[renderers.Length];
		for(int i = 0; i < materials.Length; i++) materials[i] = renderers[i].material;
		if(renderers.Length > 0) openColor = renderers[0].material.GetColor("_RimColor");

		if(!canOpen)
		{
			// Fix final materials color value
			for(int i = 0; i < materials.Length; i++) materials[i].SetColor("_RimColor", closedColor);

			// Update visual state
			visualState = 2;
		}
	}

	public void UpdateBehaviour()
	{
		if(opening && animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
		{
			// Update time counter
			timeCounter += Time.deltaTime;

			if(timeCounter >= colliderDelay)
			{
				// Update collider enabled state
				coll.enabled = false;

				// Reset time counter
				timeCounter = 0f;

				// Reset door opening state
				opening = false;
			}
		}

		switch(visualState)
		{
			case 1:
			{
				// Update materials rim color based on animation curve
				for(int i = 0; i < materials.Length; i++) materials[i].SetColor("_RimColor", Color.Lerp(openColor, closedColor, visualCurve.Evaluate(visualCounter / visualDuration)));

				// Update visual time counter
				visualCounter += Time.deltaTime;

				if(visualCounter >= visualDuration)
				{
					// Reset visual time counter
					visualCounter = 0f;

					// Fix final materials color value
					for(int i = 0; i < materials.Length; i++) materials[i].SetColor("_RimColor", closedColor);

					// Update visual state
					visualState = 2;
				}
			} break;
			case 3:
			{
				// Update materials rim color based on animation curve
				for(int i = 0; i < materials.Length; i++) materials[i].SetColor("_RimColor", Color.Lerp(closedColor, openColor, visualCurve.Evaluate(visualCounter / visualDuration)));

				// Update visual time counter
				visualCounter += Time.deltaTime;

				if(visualCounter >= visualDuration)
				{
					// Reset visual time counter
					visualCounter = 0f;

					// Fix final materials color value
					for(int i = 0; i < materials.Length; i++) materials[i].SetColor("_RimColor", openColor);

					// Update visual state
					visualState = 0;
				}
			} break;
			default: break;
		}
	}
	#endregion

	#region Detection Methods
	private void OnTriggerStay(Collider other)
	{
		// Check if player is not staying yet
		if(automatic && !staying && canOpen && other.gameObject.tag == "Player")
		{
			// Update staying state
			staying = true;

			// Apply animator close door trigger
			animator.SetTrigger("Open");

			// Update door opening state
			opening = true;

			// Disable module manager triggers
			moduleTriggers.SetActive(false);

			// Enable door modules game objects
			for(int i = 0; i < modules.Length; i++) moduleManager.SetModule(modules[i], true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if(automatic && staying && canOpen && other.gameObject.tag == "Player")
		{
			// Update staying state
			staying = false;

			// Apply animator close door trigger
			animator.SetTrigger("Close");

			// Update collider enabled state
			coll.enabled = true;

			// Reset door opening state
			opening = false;

			// Disable module manager triggers
			moduleTriggers.SetActive(true);
		}
	}
	#endregion

	#region Door Methods
	public void SetOpen(bool open)
	{
		// Update door state state
		canOpen = open;

		// Update visual state if needed
		if(open && visualState != 0) visualState = 3;
		else if(!open && visualState != 2) visualState = 1;
	}

	public void SetAutomatic(bool auto)
	{
		// Update door automatic state
		automatic = auto;
	}

	public void SetDoor(bool open)
	{
		if(!automatic)
		{
			// Apply animator close door trigger
			animator.SetTrigger((open ? "Open" : "Close"));

			// Update door opening state
			opening = open;

			// Enable door collider if needed
			if(!open) coll.enabled = true;

			// Update visual state if needed
			if(open && visualState != 0) visualState = 3;
			else if(!open && visualState != 2) visualState = 1;
		}
		else
		{
		#if DEBUG_BUILD
			// Trace warning message
			Debug.LogWarning("DoorLogic: attempting to change door state while it is an automatic door");
		#endif
		}
	}
	#endregion
}
