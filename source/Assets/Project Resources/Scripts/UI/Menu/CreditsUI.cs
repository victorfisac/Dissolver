using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsUI : MonoBehaviour
{
	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private bool playOnAwake;

	[Header("Credits")]
	[SerializeField] private float creditsDelay;
	[SerializeField] private float creditsSpeed;
	[SerializeField] private RectTransform trans;

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
		else Invoke("StartChange", creditsDelay);
	}

	private void Update()
	{
		fade.UpdateBehaviour();

		// Update credits y axis position
		trans.localPosition += Vector3.up * creditsSpeed * Time.deltaTime;

		if(!IsInvoking("ChangeLevel") && (Input.GetButtonDown("Submit") || (!movie.isPlaying && playOnAwake))) StartChange();
	}
	#endregion

	#region Intro Methods
	private void StartChange()
	{
		// Set fade out state
		fade.SetFadeOut();

		// Invoke change level method after fade out
		Invoke("ChangeLevel", 1f/fade.Speed);
	}

	private void ChangeLevel()
	{
		// Load new game scene
		SceneManager.LoadScene("menu");
	}
	#endregion
}
