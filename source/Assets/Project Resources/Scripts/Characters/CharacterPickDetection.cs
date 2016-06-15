using UnityEngine;
using System.Collections;

public class CharacterPickDetection : MonoBehaviour 
{
	#region Inspector Methods
	[Header("Settings")]
	[SerializeField] private string characterLayer;

	[Header("References")]
	[SerializeField] private TankCombat tankCombat;
	#endregion

	#region Detection Methods
#if TANK_PICK
	private void OnTriggerEnter(Collider other)
	{
        if (other.gameObject.layer == LayerMask.NameToLayer(characterLayer)) tankCombat.PickObjects.Add(other.transform);
	}

	private void OnTriggerExit(Collider other)
	{
        if (other.gameObject.layer == LayerMask.NameToLayer(characterLayer)) tankCombat.PickObjects.Remove(other.transform);
	}
#endif
	#endregion
}
