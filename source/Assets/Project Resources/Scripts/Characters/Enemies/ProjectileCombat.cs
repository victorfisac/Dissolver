using UnityEngine;
using System.Collections;

public class ProjectileCombat : CharacterCombat 
{
	#region Inspector Attributes
	[Header("Projectile")]
	[SerializeField] private Transform sourceBullet;
	[SerializeField] private GameObject projectile;
	#endregion

	#region Private Attributes
	// Attack
	private int nearestEnemy;					// Current nearest enemy reference
	private float currentDistance;				// Current nearest enemy distance
	#endregion

	#region Main Methods
	public override void AwakeBehaviour(CameraLogic logic)
	{
		// Call base class Awake method
		base.AwakeBehaviour(cameraLogic);
	}

	public override void Attack(bool attack, bool secondary, bool skill, bool defend, bool action)
	{
		if(attack)
		{
			if(targets.Count > 0)
			{
				// Detect which is the nearest enemy
				nearestEnemy = 0;
				currentDistance = Mathf.Infinity;

				for(int i = 0; i < targets.Count; i++)
				{
					float distance = Vector3.Distance(targets[i].Trans.position, character.Trans.position);

					if(distance < currentDistance)
					{
						// Update nearest enemy index and its distance to compare
						nearestEnemy = i;
						currentDistance = distance;
					}
				}

				// Instantiate projectile
				GameObject newBullet = (GameObject)Instantiate(projectile, sourceBullet.position, Quaternion.identity);
				newBullet.GetComponent<BulletProjectile>().SetDirection((targets[nearestEnemy].Trans.position + Vector3.up - sourceBullet.position).normalized, character);

				// Disable collision detection between bullet and shooter character collider
				Physics.IgnoreCollision(newBullet.GetComponent<Collider>(), GetComponent<CharacterController>());

			#if DEBUG_BUILD
				// Trace debug message
				Debug.Log("ProjectileCombat: character " + gameObject.name + " shot a projectile to " + targets[nearestEnemy].gameObject.name);
			#endif
			}
		}

		// Call base class Attack method
		base.Attack(attack, secondary, skill, defend, action);
	}
	#endregion

	#region Character Methods
	public override void SetDamage(Vector3 direction)
	{
		// Call base class SetDamage method
		base.SetDamage(direction);

		// Apply shake to camera
		if(cameraLogic) cameraLogic.ApplyShake();
	}
	#endregion

	#region Animator Methods
	public override void UpdateAnimator(bool attack, bool sec, bool sk, bool def, bool act)
	{
		// Call base class UpdateAnimator method
		base.UpdateAnimator(attack, sec, sk, def, act);
	}
	#endregion
}
