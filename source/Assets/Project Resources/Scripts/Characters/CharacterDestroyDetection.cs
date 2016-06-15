using UnityEngine;
using System.Collections;

public class CharacterDestroyDetection : MonoBehaviour 
{
	#region Inspector Methods
	[Header("Settings")]
	[SerializeField] private string destroyableTag;

	[Header("References")]
	[SerializeField] private Character character;
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == destroyableTag) character.AddDestroyable(other.transform);
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == destroyableTag) character.RemoveDestroyable(other.transform);
	}
	#endregion
}
