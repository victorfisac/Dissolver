using UnityEngine;
using System.Collections;

public class DissolveLogic : MonoBehaviour 
{
	#region Enums
	public enum DissolveType { CHILDS, ALL };
	#endregion

	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private bool manual;
	[SerializeField] private bool once;
	[SerializeField] private DissolveType type;
	[SerializeField] private Vector2 dissolveLimits;
	[SerializeField] private string dissolveString;
	[SerializeField] private bool destroyOnEnd;

	[Header("Animation")]
	[SerializeField] private AnimationCurve dissolveCurve;
	[SerializeField] private float dissolveDuration;
	#endregion

	#region Private Attributes
	private float timeCounter;				// Dissolve animation time counter
	private Renderer[] renderers;			// Ragdoll models mesh renderers
	private Material[] materials;			// Renderers material references
	private bool canWork;					// Can work state
	#endregion

	#region Main Methods
	public void AwakeBehaviour() 
	{
		InitializeDissolve();
	}

	public void StartBehaviour() 
	{
		// Check if references are not initialized yet
		if(renderers.Length == 0) InitializeDissolve();
	}

	public void UpdateBehaviour() 
	{
		if(canWork)
		{
			// Update material dissolve amount based on animation curve
			for(int i = 0; i < materials.Length; i++) materials[i].SetFloat(dissolveString, Mathf.Lerp(dissolveLimits.x, dissolveLimits.y, dissolveCurve.Evaluate(timeCounter / dissolveDuration)));

			// Update time counter
			timeCounter += Time.deltaTime;

			// Reset time counter value to loop animation
			if(timeCounter >= dissolveDuration)
			{
				if(destroyOnEnd) Destroy(gameObject);
				else if(!once) timeCounter = 0f;
				else canWork = false;
			}
		}
	}
	#endregion

	#region Dissolve Methods
	private void InitializeDissolve()
	{
		// Initialize values
		if(!manual) canWork = true;

		switch(type)
		{
			case DissolveType.CHILDS:
			{
				// Initialize values
				renderers = new MeshRenderer[transform.childCount];
				materials = new Material[renderers.Length];

				// Get references
				for(int i = 0; i < renderers.Length; i++)
				{
					renderers[i] = transform.GetChild(i).GetComponent<Renderer>();
					materials[i] = renderers[i].material;
				}
			} break;
			case DissolveType.ALL:
			{
				// Initialize values
				renderers = GetComponentsInChildren<Renderer>();
				materials = new Material[renderers.Length];

				// Get references
				for(int i = 0; i < renderers.Length; i++) materials[i] = renderers[i].material;
			} break;
			default: break;
		}
	}

	public void EnableDissolve()
	{
		// Enable dissolver behaviour
		canWork = true;

		// Reset time counter
		timeCounter = 0f;
	}

	public void DisableDissolve()
	{
		// Disable dissolver behaviour
		canWork = false;

		// Reset time counter
		timeCounter = 0f;
	}
	#endregion
}
