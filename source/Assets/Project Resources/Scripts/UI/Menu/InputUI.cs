using UnityEngine;
using System.Collections;

public class InputUI : MonoBehaviour 
{
	#region Private Attributes
	private GameObject[] childs;

	// References
	private GameManager gameManager;
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Get references
		gameManager = GameManager.Instance;

		// Initialize values
		childs = new GameObject[transform.childCount];
		for(int i = 0; i < childs.Length; i++) childs[i] = transform.GetChild(i).gameObject;
	}

	public void UpdateBehaviour()
	{
		if(childs != null)
		{
			for(int i = 0; i < childs.Length; i++)
			{
				if(childs[i].activeSelf && i != (gameManager.HasGamepad ? 1 : 0)) childs[i].SetActive(false);
			}

			if(!childs[(gameManager.HasGamepad ? 1 : 0)].activeSelf) childs[(gameManager.HasGamepad ? 1 : 0)].SetActive(true);
		}
	}
	#endregion
}
