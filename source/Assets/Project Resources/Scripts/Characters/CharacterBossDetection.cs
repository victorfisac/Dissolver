using UnityEngine;
using System.Collections;

public class CharacterBossDetection : MonoBehaviour 
{
	#region Inspector Methods
	[Header("Settings")]
	[SerializeField] private string bossTag;

	[Header("References")]
	[SerializeField] private TankCombat tankCombat;
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == bossTag && other.isTrigger)
		{
			Boss otherBoss = other.transform.root.GetComponent<Boss>();
			if(otherBoss && !tankCombat.Bosses.Contains(otherBoss)) tankCombat.Bosses.Add(otherBoss);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == bossTag && other.isTrigger)
		{
			Boss otherBoss = other.transform.root.GetComponent<Boss>();
			if(otherBoss && tankCombat.Bosses.Contains(otherBoss)) tankCombat.Bosses.Remove(otherBoss);
		}
	}
	#endregion
}
