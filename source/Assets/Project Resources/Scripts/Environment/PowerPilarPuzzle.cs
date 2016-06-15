using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PowerPilarPuzzle : MonoBehaviour 
{
	#region Enums
	public enum PilarStates { NONE = -2, INTRO, DEFAULT, PILAR0, PILAR1, PILAR2 };
	#endregion

	#region Inspector Attributes
	[Header("Line")]
	[SerializeField] private LineRenderer lineRenderer;
	[SerializeField] private Color lineColor;
	[SerializeField] private float offsetSpeed;
	[SerializeField] private float lineDuration;

	[Header("Pilars")]
	[SerializeField] private PuzzleVisual[] visuals;

	[Header("Skip")]
	[SerializeField] private UnityEvent skipEvent;

	[Header("Particles")]
	[SerializeField] private ParticleSystem laserParticle;
	[SerializeField] private ParticleSystem[] particles;
	#endregion

	#region Private Attributes
	private PilarStates puzzleState;		// Power puzzle current state

	// Intro
	private int pilarCounter;				// Pilars animation current counter

	// Visual
	private float timeCounter;				// Power pilar time counter
	private Material lineMat;				// Power pilar line renderer material reference
	private Color transparentColor;			// Power pilar material transparent color

	// Line renderer
	private Vector3[] linePositions;		// Line renderer positions array
	private float lineCounter;				// Line animation time counter

	// Particles
	private Material[] materials;			// Pilars materials references
	private Color shadowColor;				// Pilars materials default shadow color
	private Color rimColor;					// Pilars materials default rim color
	private int visualCounter;				// Visual object time counter
	#endregion

	#region Main Methods
	public void AwakeBehaviour()
	{
		// Initialize values
		puzzleState = PilarStates.NONE;
		lineMat = lineRenderer.material;
		transparentColor = lineColor;
		transparentColor.a = 0f;

		linePositions = new Vector3[transform.childCount];
		for(int i = 0; i < linePositions.Length; i++) linePositions[i] = transform.GetChild(i).position;

		// Initialize visuals values
		materials = new Material[visuals.Length];
		for(int i = 0; i < materials.Length; i++) materials[i] = visuals[i].VisualRenderer.materials[visuals[i].MaterialIndex];
		shadowColor = materials[0].GetColor("_SColor");
		rimColor = materials[0].GetColor("_RimColor");

		// Initialize start materials color
		for(int i = 0; i < materials.Length; i++)
		{
			materials[i].SetColor("_SColor", Color.black);
			materials[i].SetColor("_RimColor", Color.black);
		}
	}

	public void UpdateBehaviour()
	{
		// Update current line material offset 
		Vector2 currentOffset = lineMat.GetTextureOffset("_MainTex");
		currentOffset.x += offsetSpeed * Time.deltaTime;
		lineMat.SetTextureOffset("_MainTex", currentOffset);

		switch(puzzleState)
		{
			case PilarStates.INTRO:
			{
				if(pilarCounter < linePositions.Length)
				{
					// Update current position based on previous position lerp
					lineRenderer.SetPosition(pilarCounter, Vector3.Lerp(linePositions[pilarCounter - 1], linePositions[pilarCounter], lineCounter / lineDuration));

					// Update material shadow and rim color based on animation curve
					materials[pilarCounter - 1].SetColor("_SColor", Color.Lerp(Color.black, shadowColor, lineCounter / lineDuration));
					materials[pilarCounter - 1].SetColor("_RimColor", Color.Lerp(Color.black, rimColor, lineCounter / lineDuration));

					// Update line time counter
					lineCounter += Time.deltaTime / 2;

					if(lineCounter >= lineDuration)
					{
						// Update pilar counter value
						pilarCounter++;

						// Add new index in line renderer
						if(pilarCounter < linePositions.Length) lineRenderer.SetVertexCount(pilarCounter + 1);

						// Enable current line renderer particle system
						particles[pilarCounter - 2].Play();

						// Enable all current pilar particles
						for(int i = 0; i < visuals[visualCounter].Particles.Length; i++) visuals[visualCounter].Particles[i].Play();

						// Update visual counter if needed
						if(visualCounter < visuals.Length - 1) visualCounter++;

						// Reset line time counter
						lineCounter = 0f;
					}
				}
				else
				{
					// Update puzzle state
					puzzleState = PilarStates.DEFAULT;

					// Reset line time counter
					lineCounter = 0f;
				}

				// Check skip pilar input
				if(Input.GetButtonDown("Submit")) SkipPilar();
			} break;
			case PilarStates.PILAR0:
			case PilarStates.PILAR1:
			case PilarStates.PILAR2:
			{
				if(lineCounter < lineDuration)
				{
					// Update current position based on previous position lerp
					lineRenderer.SetPosition(linePositions.Length - (int)puzzleState, Vector3.Lerp(linePositions[linePositions.Length - (int)puzzleState], linePositions[linePositions.Length - (int)puzzleState - 1], lineCounter / lineDuration));

					// Update line time counter
					lineCounter += Time.deltaTime;
				}
				else if(puzzleState == PilarStates.PILAR2) lineRenderer.enabled = false;
			} break;
			default: break;
		}
	}
	#endregion

	#region Pilar Methods
	public void DisablePilar()
	{
		// Update puzzle state
		puzzleState++;

		// Remove previous line renderer positiosn index
		lineRenderer.SetVertexCount(linePositions.Length - (int)puzzleState + 1);

		// Disable current particle system
		particles[particles.Length - (int)puzzleState].Stop();

		// Invoke laser particle system disable method after cinematic camera delay
		if(puzzleState == PilarStates.PILAR2) Invoke("DisableLaser", lineDuration);

		// Reset line time counter
		lineCounter = 0f;
	}

	public void SkipPilar()
	{
		// Enable all particle systems
		for(int i = 0; i < visuals.Length; i++)
		{
			for(int j = 0; j < visuals[i].Particles.Length; j++)
			{
				visuals[visualCounter].Particles[i].Play();
			}
		}

		// Emable all pilars laser particle system
		for(int i = 0; i < particles.Length; i++) particles[i].Play();

		// Enable all pilars material bright and rim colors
		for(int i = 0; i < materials.Length; i++)
		{
			materials[i].SetColor("_SColor", shadowColor);
			materials[i].SetColor("_RimColor", rimColor);
		}

		// Enable laser particle system
		laserParticle.Play();

		// Invoke all skil pilar event methods
		skipEvent.Invoke();

		// Reset line time counter
		lineCounter = 0f;

		// Update puzzle state
		puzzleState = PilarStates.DEFAULT;

		// Reset line renderer vertex count and apply final positions
		lineRenderer.SetVertexCount(linePositions.Length);
		lineRenderer.SetPositions(linePositions);
	}

	private void DisableLaser()
	{
		// Disable laser start particle system
		laserParticle.Stop();
	}

	public void IntroPilar()
	{
		// Update pauzzle state
		puzzleState = PilarStates.INTRO;

		// Set calculated positions to line renderer positions
		lineRenderer.SetVertexCount(2);
		lineRenderer.SetPosition(pilarCounter, linePositions[pilarCounter]);

		// Play laser start particle effect
		laserParticle.Play();

		// Update pilar counter
		pilarCounter++;

		// Reset line time counter
		lineCounter = 0f;
	}
	#endregion

	#region Serializable
	[System.Serializable]
	public class PuzzleVisual
	{
		#region Inspector Attributes
		[Header("Settings")]
		[SerializeField] private int materialIndex;

		[Header("References")]
		[SerializeField] private MeshRenderer visualRenderer;
		[SerializeField] private ParticleSystem[] particles;
		#endregion

		#region Properties
		public int MaterialIndex
		{
			get { return materialIndex; }
		}

		public MeshRenderer VisualRenderer
		{
			get { return visualRenderer; }
		}

		public ParticleSystem[] Particles
		{
			get { return particles; }
		}
		#endregion
	}
	#endregion
}