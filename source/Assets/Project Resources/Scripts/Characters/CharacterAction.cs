using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterAction : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private float fieldOfView;

	[Header("Audio")]
	[SerializeField] private AudioSource actionSource;

	[Header("References")]
	[SerializeField] private Character character;
	#endregion

	#region Private Attributes
	private bool canAction;					// Character action animation state
	private List<PowerPilar> pilars;		// Power pilars references list
	private List<InvokePilar> invokes;		// Invoke pilars references list
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Initialize values
		pilars = new List<PowerPilar>();
		invokes = new List<InvokePilar>();
	}

	public void UpdateActions (bool action) 
	{
		// Check if there are any pilar to do action
		if(pilars.Count > 0)
		{
			if(character.Controller.isGrounded)
			{
				if(action) 
				{
					for(int i = 0; i < pilars.Count; i++)
					{
						// Check if character is looking at pilar
						if(Vector3.Angle(pilars[i].transform.position - character.Trans.position, character.Trans.forward) <= fieldOfView && pilars[i].CanDisable)
						{
							// Disable pilar animation
							pilars[i].DisablePilar();

							// Remove power reference from list
							pilars.RemoveAt(i);

							// Update action animation state if needed
							canAction = true;
						}
					}
				}
			}
		}

		if(invokes.Count > 0)
		{
			if(character.Controller.isGrounded)
			{
				if(action) 
				{
					for(int i = 0; i < invokes.Count; i++)
					{
						// Disable pilar animation
						invokes[i].GetPower();

						// Remove invoke reference from list
						invokes.RemoveAt(i);

						// Update action animation state if needed
						canAction = true;
					}
				}
			}
		}

		// Play action source if any action was enabled
		if(canAction) actionSource.Play();

		// Update animator values
		UpdateAnimator();
	}
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		switch(other.tag)
		{
			case "PowerPilar": pilars.Add(other.GetComponent<PowerPilar>()); break;
			case "InvokePilar": invokes.Add(other.GetComponent<InvokePilar>()); break;
			default: break;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		switch(other.tag)
		{
			case "PowerPilar": pilars.Remove(other.GetComponent<PowerPilar>()); break;
			case "InvokePilar": invokes.Remove(other.GetComponent<InvokePilar>()); break;
			default: break;
		}
	}
	#endregion

	#region Animator Methods
	private void UpdateAnimator()
	{
		character.Anims[0].SetBool("Action", canAction);

		// Reset can action state
		canAction = false;
	}
	#endregion

	#region Debug Methods
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		// Draw fieldOfView
		Gizmos.color = Color.yellow;
		Gizmos.DrawRay(transform.position + Vector3.up * 1f, Quaternion.AngleAxis(fieldOfView, transform.up) * transform.forward * 5f);
		Gizmos.DrawRay(transform.position + Vector3.up * 1f, Quaternion.AngleAxis(-fieldOfView, transform.up) * transform.forward * 5f);
	}
#endif
	#endregion
}
