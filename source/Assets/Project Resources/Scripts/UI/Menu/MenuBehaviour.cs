using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class MenuBehaviour : MonoBehaviour 
{
	#region Inspector Methods
	[Header("Transition")]
	[SerializeField] private float duration;

	[Header("References")]
	[SerializeField] private GameObject[] transitionMenus;
	[SerializeField] private UnityEvent enableEvent;
	#endregion

	#region Private Attributes
	private bool inTransition;			// Menu transition state
	private float counter;				// Transition time counter
	private int nextMenu;				// Current next menu after transition
	#endregion

	#region Main Methods
	private void OnEnable()
	{
		// Invoke on enable event
		enableEvent.Invoke();
	}

	public void UpdateBehaviour () 
	{
		if(inTransition)
		{
			// Update time counter
			counter += Time.deltaTime;

			if(counter >= duration)
			{
				// Reset time counter
				counter = 0f;

				// Reset transition state
				inTransition = false;

				// Change menu
				transitionMenus[nextMenu].SetActive(true);

				// Disable current menu
				gameObject.SetActive(false);
			}
		}
	}
	#endregion

	#region Menu Methods
	public void SetTransition(int menu)
	{
		// Update next menu value
		nextMenu = menu;

		// Update transition state
		inTransition = true;
	}
	#endregion
}
