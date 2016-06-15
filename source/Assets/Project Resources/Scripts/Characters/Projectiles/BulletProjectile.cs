using UnityEngine;
using System.Collections;

public class BulletProjectile : MonoBehaviour 
{
	#region Enums
	public enum ProjectileType { DAMAGE, TIME };
	#endregion

	#region Inspector Attributes
	[Header("Settings")]
	[SerializeField] private ProjectileType type;
	[SerializeField] private string characterMask;
	[SerializeField] private string pickableMask;

	[Header("Time")]
	[SerializeField] private float scale;
	[SerializeField] private float duration;

	[Header("Damage")]
	[SerializeField] private float damage;

	[Header("Motion")]
	[SerializeField] private float force;

	[Header("Destroy")]
	[SerializeField] private float startDestroyDelay;
	[SerializeField] private float destroyDelay;

	[Header("References")]
	[SerializeField] private Transform trans;
	[SerializeField] private Rigidbody rb;
	[SerializeField] private Collider coll;
	[SerializeField] private GameObject projectile;
	[SerializeField] private GameObject explosion;
	#endregion

	#region Private Attributes
	private Character character;	// Shooter character reference
	private bool isDone;			// Detection done state
	#endregion

	#region Main Methods
	public void SetDirection(Vector3 direction, Character charac)
	{
		// Set projectile direction
		rb.velocity = direction * force;
		character = charac;

		// Destroy game object after a time
		Destroy(gameObject, startDestroyDelay);
	}
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		Debug.Log(other.gameObject.name);

		if(!isDone)
		{
			if(other.gameObject.layer == LayerMask.NameToLayer(characterMask))
			{
				// Get collided character reference
				Character otherChar = other.GetComponent<Character>();

				if(otherChar)
				{
					if((character.IsPlayer && otherChar.gameObject.tag == "Enemy") || (!character.IsPlayer && otherChar.gameObject.tag == "Player")) 
					{
						// Apply damage or time scale to character
						switch(type)
						{
							case ProjectileType.DAMAGE: otherChar.SetDamage(damage, Vector3.zero, 0f, character); break;
							case ProjectileType.TIME: otherChar.SetTimeScale(duration, scale); break;
							default: break;
						}
					}
				}
			}
			else if(other.gameObject.layer == LayerMask.NameToLayer(pickableMask))
			{
				// Apply time scale to pickable object
				if(type == ProjectileType.TIME) other.GetComponent<PuzzlePiece>().SetTimeScale(duration, scale);
			}
			else if(other.gameObject.tag == "Destroyable")
			{
				// Get destroyable reference from collided object
				Destroyable otherDestroy = other.GetComponent<Destroyable>();

				// If exist, shatter destroyable object
				if(otherDestroy) otherDestroy.Shatter(false);
			}

			if(other.gameObject.layer != LayerMask.NameToLayer("Actions") && other.gameObject.tag != "Trap")
			{
				// Disable physics and parent bullet to triggered transform component
				rb.isKinematic = true;
				trans.parent = other.transform;

				// Destroy object after a delay
				Invoke("DestroyObject", destroyDelay);

				// Disable projectile and enable explosion
				projectile.SetActive(false);
				explosion.SetActive(true);

				// Update done state
				isDone = true;

				// Disable collider
				coll.enabled = false;
			}
		}
	}
	#endregion

	#region Projectile Methods
	private void DestroyObject()
	{
		// Destroy game object
		Destroy(gameObject);
	}
	#endregion
}
