using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Fade : MonoBehaviour 
{
	#region Enums
	public enum FadeState { FADEIN, IN, FADEOUT, OUT };
	public enum EndAction { NONE, RESET, QUIT };
	#endregion

	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private FadeState state;
	[Range(0, 3)] [SerializeField] private float fadeSpeed;
	[Range(0, 1)] [SerializeField] private float minValue;
	[Range(0, 1)] [SerializeField] private float maxValue;
	[SerializeField] private EndAction endAction;

	[Header("Audio")]
	[SerializeField] private AudioSource[] sources;
	[SerializeField] private bool isMain;

	[Header("References")]
	[SerializeField] private Image image;
	#endregion

	#region Private Attributes
	private Color fadeColor;		// Fade animation color
	#endregion

	#region Main Methods
	public void UpdateBehaviour () 
	{
		switch(state)
		{
			case FadeState.FADEIN: FadeIn(); break;
			case FadeState.FADEOUT: FadeOut(); break;
			default: break;
		}
	}
	#endregion

	#region Fade Methods
	private void FadeIn()
	{
		fadeColor = image.color;
		fadeColor.a -= fadeSpeed * Time.deltaTime;
		image.color = fadeColor;

		if(isMain)
		{
			if(AudioListener.volume < 1.0f) AudioListener.volume += fadeSpeed*Time.deltaTime;
			else AudioListener.volume = 1.0f;
		}

		if(image.color.a <= minValue) SetIn();
	}

	private void FadeOut()
	{
		fadeColor = image.color;
		fadeColor.a += fadeSpeed * Time.deltaTime;
		image.color = fadeColor;

		if(isMain)
		{
			if(AudioListener.volume > 0f) AudioListener.volume -= fadeSpeed * Time.deltaTime;
			else AudioListener.volume = 0f;
		}

		for(int i = 0; i < sources.Length; i++)
		{
			if(sources[i])
			{
				if(sources[i].volume > 0f) sources[i].volume -= fadeSpeed * Time.deltaTime;
				else sources[i].volume = 0f;
			}
		}

		if(image.color.a >= maxValue) SetOut();
	}

	public void SetFadeIn()
	{
		gameObject.SetActive(true);

		fadeColor = image.color;
		fadeColor.a = maxValue;
		image.color = fadeColor;

		state = FadeState.FADEIN;
	}

	public void SetIn()
	{
		fadeColor = image.color;
		fadeColor.a = minValue;
		image.color = fadeColor;

		state = FadeState.IN;

		gameObject.SetActive(false);
	}

	public void SetFadeOut()
	{
		// Enable object
		gameObject.SetActive(true);

		fadeColor = image.color;
		fadeColor.a = minValue;
		image.color = fadeColor;

		state = FadeState.FADEOUT;
	}

	public void SetOut()
	{
		fadeColor = image.color;
		fadeColor.a = maxValue;
		image.color = fadeColor;

		switch(endAction)
		{
			case EndAction.RESET: SceneManager.LoadScene(SceneManager.GetActiveScene().name); break;
			case EndAction.QUIT: Application.Quit(); break;
			default: break;
		}

		state = FadeState.OUT;
	}

	public void SetEndAction(int value)
	{
		endAction = (EndAction)value;
	}
	#endregion

	#region Properties
	public float Speed
	{
		get { return fadeSpeed; }
	}
	#endregion
}