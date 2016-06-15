using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class CharacterFeedback : MonoBehaviour 
{
	#region Inspector Attributes
	[Header("Feedback Events")]
	[SerializeField] private FeedbackEvent[] events;

	[Header("Burn")]
	[SerializeField] private bool playOnAwake;
	[SerializeField] private AnimationCurve burnCurve;
	[SerializeField] private float burnDuration;
	[SerializeField] private Vector2 burnLimits;
	[SerializeField] private Renderer[] burnRenderers;
	[SerializeField] private Vector2 offsetSpeed;

	[Header("References")]
	[SerializeField] private Character character;
	#endregion

	#region Private Attributes
	// Burn
	private float burnCounter;		// Burn effect time counter
	private bool useBurn;			// Burn can work state
	private float burnStart;		// Burn fade out start value
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Initialize values
		useBurn = playOnAwake;
	}

	public void UpdateBehaviour()
	{
		// Update events logic
		for(int i = 0; i < events.Length; i++) events[i].UpdateFeedback(character.DeltaTime);

		UpdateBurn();
	}
	#endregion

	#region Feedback Methods
	public void EnableEvent(int index)
	{
		bool canStart = true;

		for(int i = 0; i < events.Length; i++)
		{
			// Check if there are any other feedback event currently working
			if(events[i].Enabled)
			{
				canStart = false;
				break;
			}
		}

		// Initilize damage event
		if(canStart) events[index].StartFeedback();
	}
	#endregion

	#region Burn Methods
	private void UpdateBurn()
	{
		// Check if burn effect is enabled
		if(useBurn)
		{
			for(int i = 0; i < burnRenderers.Length; i++)
			{
				// Update burn effect property to all renderers' materials based on animation curve
				burnRenderers[i].material.SetFloat("_BurnAmount", Mathf.Lerp(burnLimits.x, burnLimits.y, burnCurve.Evaluate(burnCounter / burnDuration)));

				// Update materials offset based on offset speed
				Vector2 currentOffset = burnRenderers[i].material.GetTextureOffset("_BurnTex");
				currentOffset += offsetSpeed * character.DeltaTime;
				burnRenderers[i].material.SetTextureOffset("_BurnTex", currentOffset);
			}


			// Update burn effect time counter
			burnCounter += character.DeltaTime;

			// Reset burn time counter when finished to loop animation
			if(burnCounter >= burnDuration) burnCounter = 0f;
		}
		else
		{
			if(burnCounter < burnDuration)
			{
				// Update burn effect property to all renderers' materials based on animation curve
				for(int i = 0; i < burnRenderers.Length; i++) burnRenderers[i].material.SetFloat("_BurnAmount", Mathf.Lerp(burnStart, -0.1f, burnCounter / burnDuration));

				// Update burn effect time counter
				burnCounter += character.DeltaTime;
			}
		}
	}

	public void SetBurn(bool state)
	{
		if(useBurn != state)
		{
			// Update burn effect state
			useBurn = state;

			if(!useBurn)
			{
				// Reset time counter
				burnCounter = 0f;

				// Get current burn value to make fade out animation
				burnStart = Mathf.Lerp(burnLimits.x, burnLimits.y, burnCurve.Evaluate(burnCounter / burnDuration));
			}
		}
	}
	#endregion

	#region Serializable
	[System.Serializable]
	public class FeedbackEvent
	{
		#region Enums
		public enum FeedbackType { FLOAT, COLOR };
		#endregion

		#region Public Attributes
		[Header("State")]
		[SerializeField] private bool enabled;

		[Header("Events")]
		[SerializeField] private UnityEvent startEvent;

		[Header("Settings")]
		[SerializeField] private Renderer[] meshRenderer;
		[SerializeField] private FeedbackType type;
		[SerializeField] private AnimationCurve curve;
		[SerializeField] private float duration;

		[Header("Values")]
		[SerializeField] private string attribute;
		[SerializeField] private Color targetColor;
		[SerializeField] private float targetValue;
		#endregion

		#region Private Attributes
		private float counter;					// Feedback event time counter
		private Color[] startColor;				// Feedback event start color value
		private float[] startValue;				// Feedback event start value
		#endregion

		#region Main Methods
		public void StartFeedback()
		{
			if(!enabled)
			{
				// Initialize values
				enabled = true;
				counter = 0;

				// Invoke start feedback event
				startEvent.Invoke();

				switch(type)
				{
					case FeedbackType.FLOAT:
					{
						startValue = new float[meshRenderer.Length];
						for(int i = 0; i < meshRenderer.Length; i++) startValue[i] = meshRenderer[i].material.GetFloat(attribute); 
					} break;
					case FeedbackType.COLOR: 
					{
						startColor = new Color[meshRenderer.Length];
						for(int i = 0; i < meshRenderer.Length; i++) startColor[i] = meshRenderer[i].material.GetColor(attribute); 
					} break;
				}
			}
		}

		public void UpdateFeedback(float delta)
		{
			if(enabled)
			{
				// Update shader values
				switch(type)
				{
					case FeedbackType.FLOAT: 
					{
						for(int i = 0; i < meshRenderer.Length; i++) meshRenderer[i].material.SetFloat(attribute, Mathf.Lerp(startValue[i], targetValue, curve.Evaluate(counter / duration)));
					} break;
					case FeedbackType.COLOR: 
					{
						for(int i = 0; i < meshRenderer.Length; i++) meshRenderer[i].material.SetColor(attribute, Color.Lerp(startColor[i], targetColor, curve.Evaluate(counter / duration)));
					} break;
				}

				// Update event timer
				counter += delta;

				if(counter > duration)
				{
					// Reset state value
					enabled = false;
					counter = 0;

					// Fix shader values
					switch(type)
					{
						case FeedbackType.FLOAT: 
						{
							for(int i = 0; i < meshRenderer.Length; i++) meshRenderer[i].material.SetFloat(attribute, Mathf.Lerp(startValue[i], targetValue, curve.Evaluate(1f))); 
						} break;
						case FeedbackType.COLOR: 
						{
							for(int i = 0; i < meshRenderer.Length; i++) meshRenderer[i].material.SetColor(attribute, Color.Lerp(startColor[i], targetColor, curve.Evaluate(1f))); 
						} break;
					}
				}
			}
		}
		#endregion

		#region Properties
		public bool Enabled
		{
			get { return enabled; }
		}
		#endregion
	}
	#endregion
}
