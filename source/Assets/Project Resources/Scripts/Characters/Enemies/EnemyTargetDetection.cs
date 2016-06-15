using UnityEngine;
using System.Collections;

public class EnemyTargetDetection : MonoBehaviour 
{
	#region Inspector Methods
	[Header("Settings")]
	[SerializeField] private string characterTag;
	[SerializeField] private string secondaryTag;

	[Header("References")]
	[SerializeField] private Enemy enemy;
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == characterTag || other.tag == secondaryTag) enemy.Targets.Add(other.GetComponent<Character>());
	}

	private void OnTriggerExit(Collider other)
	{
		if(other.tag == characterTag || other.tag == secondaryTag) enemy.Targets.Remove(other.GetComponent<Character>());
	}
	#endregion
}
