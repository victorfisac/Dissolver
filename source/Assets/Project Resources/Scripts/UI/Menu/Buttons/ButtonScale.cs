using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonScale : MonoBehaviour 
{
	#region Enums
	public enum InteractableType { BUTTON, TOGGLE, DROPDOWN, SLIDER };
	#endregion

	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private InteractableType type;

	[Header("Scale")]
	[SerializeField] private Vector3 normalScale;
	[SerializeField] private Vector3 overScale;
	[SerializeField] private Color overColor;

	[Header("Motion")]
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float duration;

	[Header("References")]
	[SerializeField] private Transform trans;
	[SerializeField] private Image image;
	#endregion

	#region Private Attributes
	private bool isOver;					// Button over state
	private float counter;					// Interpolation counter
	private AudioSource buttonSource;		// Button over audio source reference
	private Button button;					// Button component reference
	private Toggle toggle;					// Toggle component reference
	private Dropdown dropdown;				// Dropdown component reference
	private Slider slider;					// Slider component reference
	private bool gamepadSelected;			// Button gamepad event system selected state
	private Color initColor;				// Button image color at start
	private bool canWork;					// Can work state
	private Color auxColor;					// Image current color
	#endregion

	#region Main Methods
	public void AwakeBehaviour(AudioSource source)
	{
		// Get references
		buttonSource = source;

		switch(type)
		{
			case InteractableType.BUTTON: button = GetComponent<Button>(); break;
			case InteractableType.TOGGLE: toggle = GetComponent<Toggle>(); break;
			case InteractableType.DROPDOWN: dropdown = GetComponent<Dropdown>(); break;
			case InteractableType.SLIDER: slider = GetComponent<Slider>(); break;
			default: break;
		}

		// Get image reference for color tint
		if(!image) image = GetComponent<Image>();

		// Initialize values
		if(image) initColor = image.color;

		// Initialize button state
		SetOver((EventSystem.current.currentSelectedGameObject == gameObject));
	}

	public void UpdateBehaviour()
	{
		// Update can work state based on interactable state
		switch(type)
		{
			case InteractableType.BUTTON: canWork = button.interactable; break;
			case InteractableType.TOGGLE: canWork = toggle.interactable; break;
			case InteractableType.DROPDOWN: canWork = dropdown.interactable; break;
			case InteractableType.SLIDER: canWork = slider.interactable; break;
			default: break;
		}

		if(canWork)
		{
			// Button scale interpolation based on over state
			if(isOver || gamepadSelected)
			{
				trans.localScale = Vector3.Lerp(normalScale, overScale, curve.Evaluate(counter / duration));
				if(counter < duration) counter += Time.deltaTime;
				else counter = duration;

				// Update button image color to over color
				if(image)
				{
					auxColor = image.color;
					auxColor = new Color(overColor.r, overColor.g, overColor.b, auxColor.a);
					image.color = auxColor;
				}

				if(EventSystem.current.currentSelectedGameObject != gameObject) gamepadSelected = false;
			}
			else 
			{
				trans.localScale = Vector3.Lerp(overScale, normalScale, curve.Evaluate((duration - counter) / duration));
				if(counter > 0f) counter -= Time.deltaTime;
				else counter = 0f;

				// Update button image color to default color
				if(image)
				{
					auxColor = image.color;
					auxColor = new Color(initColor.r, initColor.g, initColor.b, auxColor.a);
					image.color = auxColor;
				}

				if(EventSystem.current.currentSelectedGameObject == gameObject)
				{
					// Update gamepad event system button over state
					gamepadSelected = true;

					// Play audio sound if state is over
					buttonSource.Play();
				}
			}
		}
	}
	#endregion

	#region Button Methods
	public void SetOver(bool state)
	{
		if(canWork)
		{
			// Update button over state
			isOver = state;

			// Reset gamepad event system state
			gamepadSelected = false;

			// Reset current selected game object from event system
			EventSystem.current.SetSelectedGameObject(null);

			// Play audio sound if state is over
			if(state && buttonSource.enabled) buttonSource.Play();
		}
	}
	#endregion
}
