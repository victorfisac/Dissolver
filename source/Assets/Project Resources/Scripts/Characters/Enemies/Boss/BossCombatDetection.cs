using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossCombatDetection : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private float damageDuration;
	[SerializeField] private float damage;
	[SerializeField] private Vector3 forceDirection;
	[SerializeField] private float forceAmount;
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag == "Player" || other.gameObject.tag == "Ally")
		{
			// Find triggered collider character component
			Character otherChar = other.GetComponent<Character>();

			// Make damage to triggered character if found
			if(otherChar) otherChar.SetDamage(damage, Vector3.Scale(transform.root.TransformDirection(forceDirection), new Vector3(1f, 0f, 1f)), forceAmount, null);
		}
	}
	#endregion
}
