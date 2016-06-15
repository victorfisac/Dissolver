using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComboUI : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Position")]
	[SerializeField] private float distance;
	[SerializeField] private AnimationCurve positionCurve;
	[SerializeField] private float positionDuration;

	[Header("Rotation")]
	[SerializeField] private int minCombo;
	[SerializeField] private AnimationCurve rotationCurve;
	[SerializeField] private float rotationDuration;

	[Header("Scale")]
	[SerializeField] private AnimationCurve scaleCurve;
	[SerializeField] private float scaleDuration;

	[Header("References")]
	[SerializeField] private Transform trans;
	[SerializeField] private int comboIndex;
	[SerializeField] private Text[] texts;
	#endregion

	#region Private Attributes
	private float comboDuration;		// Combo max duration
	private float timeCounter;			// Animation time counter
	private float rotationCounter;		// Rotation time counter
	private Vector3 initPos;			// Local position at start
	private Vector3 initScale;			// Local scale at start
	private Vector3 initRotation;		// Euler rotation at start
	private int currentState;			// Animation current state
	private int currentCombo;			// Current player character combo
	private int lastCombo;				// Previous player character combo
	private Character playerChar;		// Player character reference
	#endregion

	#region Main Methods
	public void AwakeBehaviour(Character player) 
	{
		// Get references
		playerChar = player;

		// Initialize values
		comboDuration = player.ComboDuration;
		initPos = trans.localPosition;
		initRotation = trans.rotation.eulerAngles;
		initScale = trans.localScale;

		// Reset all texts alpha values
		for(int i = 0; i < texts.Length; i++)
		{
			Color auxColor = texts[i].color;
			auxColor.a = 0f;
			texts[i].color = auxColor;
			texts[i].enabled = false;
		}
	}

	public void UpdateBehaviour() 
	{
		// Store previous character combo
		lastCombo = currentCombo;
		currentCombo = playerChar.CurrentCombo;

		// Check if player character made damage
		if(currentCombo > lastCombo) SetCombo(currentCombo);

		// Play rotation animation if combo is cool
		if(currentCombo > minCombo)
		{
			// Update rotation based on animation curve
			trans.rotation = Quaternion.Euler(initRotation.x, initRotation.y, initRotation.z + rotationCurve.Evaluate(rotationCounter / rotationDuration));

			// Update rotation counter
			rotationCounter += Time.deltaTime;

			if(rotationCounter >= rotationDuration) rotationCounter = 0f;
		}

		switch(currentState)
		{
			case 1:
			{
				// Update local scale based on animation curve
				trans.localScale = Vector3.Lerp(Vector3.zero, initScale, scaleCurve.Evaluate(timeCounter / scaleDuration));

				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= scaleDuration)
				{
					// Reset time counter
					timeCounter = 0f;

					// Fix local scale to end value
					trans.localScale = initScale;

					// Update current state
					currentState = 2;
				}
			} break;
			case 2:
			{
				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= comboDuration)
				{
					// Reset time counter
					timeCounter = 0f;

					// Update current state
					currentState = 3;
				}
			} break;
			case 3:
			{
				// Update local position based on animation curve
				trans.localPosition = Vector3.Lerp(initPos, initPos + Vector3.up * distance, positionCurve.Evaluate(timeCounter / positionDuration));

				// Update texts alpha based on animation curve
				for(int i = 0; i < texts.Length; i++) texts[i].color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), positionCurve.Evaluate(timeCounter / positionDuration));

				// Update time counter
				timeCounter += Time.deltaTime;

				if(timeCounter >= comboDuration)
				{
					// Reset time counter
					timeCounter = 0f;

					// Reset all texts alpha values
					for(int i = 0; i < texts.Length; i++)
					{
						Color auxColor = texts[i].color;
						auxColor.a = 0f;
						texts[i].color = auxColor;
						texts[i].enabled = false;
					}

					// Update current state
					currentState = 0;
				}
			} break;
			default: break;
		}
	}
	#endregion

	#region Combo Methods
	public void SetCombo(int amount)
	{
		if(amount > 0)
		{
			// Update visual combo value
			texts[comboIndex].text = amount.ToString();

			// Reset local position, rotation and scale to cached at start
			trans.localPosition = initPos;
			trans.rotation = Quaternion.Euler(initRotation);
			trans.localScale = Vector3.zero;

			// Reset time counter
			timeCounter = 0f;

			// Reset all texts alpha values
			for(int i = 0; i < texts.Length; i++)
			{
				Color auxColor = texts[i].color;
				auxColor.a = 1f;
				texts[i].color = auxColor;
				texts[i].enabled = true;
			}

			// Update current state
			currentState = 1;
		}
	}
	#endregion
}
