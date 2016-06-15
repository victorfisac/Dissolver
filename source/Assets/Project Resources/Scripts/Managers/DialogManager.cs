using UnityEngine;
using System.Collections;

public class DialogManager : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private float endDelay;

	[Header("References")]
	[SerializeField] private Transform trans;
	#endregion

	#region Private Attributes
	private int currentDialog;		// Current dialog state
	#endregion

	#region Dialog Methods
	public void SetState(int state)
	{
		// Update current dialog value
		currentDialog = state;

	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("DialogManager: state set to " + currentDialog);
	#endif
	}

	public void NextDialog()
	{
	#if DEBUG_BUILD
		// Trace debug message
		Debug.Log("DialogManager: dialog number " + currentDialog + " started");
	#endif

		if(currentDialog != -1)
		{
			// Disable last used dialog object
			if(currentDialog - 1 >= 0) trans.GetChild(currentDialog - 1).gameObject.SetActive(false);

			// Enable current dialog
			EnableDialog();
		}
		else
		{
			for(int i = 0; i < trans.childCount; i++)
			{
				trans.GetChild(i).gameObject.SetActive(false);
				trans.GetChild(i).gameObject.GetComponent<AlphaTransition>().Play(true);
			}
		}
	}

	private void EnableDialog()
	{
		// Enable current dialog
		trans.GetChild(currentDialog).gameObject.SetActive(true);
		trans.GetChild(currentDialog).gameObject.GetComponent<AlphaTransition>().Play(false);

		// Invoke dialog disable event after a delay
		Invoke("DisableDialog", endDelay);
	}

	private void DisableDialog()
	{
		// Play reversed dialog transitions
		if(currentDialog >= 0 && currentDialog < trans.childCount) trans.GetChild(currentDialog).gameObject.GetComponent<AlphaTransition>().Play(true);

		// Update current dialog state
		currentDialog++;
	}
	#endregion
}
