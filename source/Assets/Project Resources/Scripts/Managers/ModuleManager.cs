using UnityEngine;
using System.Collections;

public class ModuleManager : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("References")]
	[SerializeField] private Transform trans;
	#endregion

	#region Private Attributes
	// Modules
	private GameObject[] modules;
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Get References
		modules = new GameObject[trans.childCount];
		for(int i = 0; i < modules.Length; i++) modules[i] = trans.GetChild(i).gameObject;

		// Initialize values
		for(int i = 0; i < modules.Length; i++) modules[i].SetActive(false);
		if(modules.Length > 0) modules[0].SetActive(true);
	}
	#endregion

	#region Module Methods
	public void SetModule(int index, bool state)
	{
		if(modules != null)
		{
			// Update specific module state
			if(modules[index].activeSelf != state) modules[index].SetActive(state);
		}
	}

	public void EnableModule(int index)
	{
		if(modules != null)
		{
			// Enable specific module state
			if(!modules[index].activeSelf) modules[index].SetActive(true);
		}
	}

	public void DisableModule(int index)
	{
		if(modules != null)
		{
			// Disable specific module state
			if(modules[index].activeSelf) modules[index].SetActive(false);
		}
	}
	#endregion
}
