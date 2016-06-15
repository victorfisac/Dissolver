using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroUI : MonoBehaviour
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private bool playOnAwake;

	[Header("Movie")]
	[SerializeField] private MovieTexture movie;
	[SerializeField] private AudioSource source;

	[Header("References")]
	[SerializeField] private Fade fade;
	[SerializeField] private RawImage rawImage;
	#endregion

	#region Main Methods
	private void Awake()
	{
		if(playOnAwake)
		{
			// Initialize values
			rawImage.texture = movie as MovieTexture;
			source.clip = movie.audioClip;

			// Play movie texture and its audio source
			movie.Play();
			source.Play();
		}
	}

	private void Update()
	{
		fade.UpdateBehaviour();

		if(!IsInvoking("ChangeLevel"))
		{
			if(Input.GetButtonDown("Submit") || !movie.isPlaying)
			{
				// Set fade out state
				fade.SetFadeOut();

				// Invoke change level method after fade out
				Invoke("ChangeLevel", 1f/fade.Speed);
			}
		}
	}
	#endregion

	#region Intro Methods
	private void ChangeLevel()
	{
		// Load new game scene
		SceneManager.LoadScene("level_01");
	}
	#endregion
}
