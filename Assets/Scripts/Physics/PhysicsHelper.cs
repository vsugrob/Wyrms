using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class PhysicsHelper {
	public static float CalculateTotalMass ( GameObject root ) {
		var bodies = root.GetComponentsInChildren <Rigidbody2D> ();

		return	bodies.Sum ( rigidbody => rigidbody.mass );
	}

	public static void IgnoreCollision ( GameObject gameObject1, GameObject gameObject2, bool ignore = true ) {
		var colliders1 = gameObject1.GetComponentsInChildren <Collider2D> ();
		var colliders2 = gameObject2.GetComponentsInChildren <Collider2D> ();
		
		foreach ( var collider1 in colliders1 ) {
			foreach ( var collider2 in colliders2 ) {
				Physics2D.IgnoreCollision ( collider1, collider2, ignore );
			}
		}
	}

	public static float ConvertToNewtons ( float amount, ForceMode mode, float bodyMass ) {
		if ( mode == ForceMode.Force )
			return	amount;
		else if ( mode == ForceMode.Impulse )
			return	amount / Time.fixedDeltaTime;
		else if ( mode == ForceMode.Acceleration )
			return	amount * bodyMass;
		else if ( mode == ForceMode.VelocityChange )
			return	amount * bodyMass / Time.fixedDeltaTime;
		else
			throw new System.ArgumentOutOfRangeException ( "mode" );
	}

	public static Vector2 ConvertToNewtons ( Vector2 dirAndAmount, ForceMode mode, float bodyMass ) {
		if ( mode == ForceMode.Force )
			return	dirAndAmount;
		else if ( mode == ForceMode.Impulse )
			return	dirAndAmount / Time.fixedDeltaTime;
		else if ( mode == ForceMode.Acceleration )
			return	dirAndAmount * bodyMass;
		else if ( mode == ForceMode.VelocityChange )
			return	dirAndAmount * bodyMass / Time.fixedDeltaTime;
		else
			throw new System.ArgumentOutOfRangeException ( "mode" );
	}

	public static float ConvertToImpulse ( float amount, ForceMode mode, float bodyMass ) {
		if ( mode == ForceMode.Force )
			return	amount * Time.fixedDeltaTime;
		else if ( mode == ForceMode.Impulse )
			return	amount;
		else if ( mode == ForceMode.Acceleration )
			return	amount * bodyMass * Time.fixedDeltaTime;
		else if ( mode == ForceMode.VelocityChange )
			return	amount * bodyMass;
		else
			throw new System.ArgumentOutOfRangeException ( "mode" );
	}

	public static float ConvertToVelocityChange ( float amount, ForceMode mode, float bodyMass ) {
		if ( mode == ForceMode.Force )
			return	amount / bodyMass * Time.fixedDeltaTime;
		else if ( mode == ForceMode.Impulse )
			return	amount / bodyMass;
		else if ( mode == ForceMode.Acceleration )
			return	amount * Time.fixedDeltaTime;
		else if ( mode == ForceMode.VelocityChange )
			return	amount;
		else
			throw new System.ArgumentOutOfRangeException ( "mode" );
	}

	public static float ConvertToNewtons ( float amount, ForceMode mode, GameObject gameObject ) {
		if ( mode == ForceMode.Force )
			return	amount;
		else if ( mode == ForceMode.Impulse )
			return	amount / Time.fixedDeltaTime;
		else if ( mode == ForceMode.Acceleration )
			return	amount * PhysicsHelper.CalculateTotalMass ( gameObject );
		else if ( mode == ForceMode.VelocityChange )
			return	amount * PhysicsHelper.CalculateTotalMass ( gameObject ) / Time.fixedDeltaTime;
		else
			throw new System.ArgumentOutOfRangeException ( "mode" );
	}

	public static void AddForce ( this Rigidbody2D body, Vector2 amountAndDirection, ForceMode mode ) {
		float amount = amountAndDirection.magnitude;
		var dir = amountAndDirection.normalized;
		amount = ConvertToNewtons ( amount, mode, body.mass );
		var force = dir * amount;

		body.AddForce ( force, ForceMode2D.Force );
	}

	public static void AddTorque ( this Rigidbody2D body, float amount, ForceMode mode ) {
		amount = ConvertToNewtons ( amount, mode, body.mass );

		body.AddTorque ( amount, ForceMode2D.Force );
	}

	public static float CombineFriction ( float friction1, float friction2 ) {
		float friction = Mathf.Sqrt ( friction1 * friction2 );

		return	friction;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="newVelocity1"></param>
	/// <param name="relVelocity">oldVelocity2 - oldVelocity1</param>
	/// <param name="mass1"></param>
	/// <param name="mass2"></param>
	/// <param name="oldVelocity1"></param>
	/// <param name="oldVelocity2"></param>
	public static void CalculatePreCollisionVelocity (
		Vector2 newVelocity1, Vector2 relVelocity,
		float mass1, float mass2,
		out Vector2 oldVelocity1, out Vector2 oldVelocity2
	) {
		float mk = mass2 / ( mass1 + mass2 );
		oldVelocity1 = newVelocity1 - relVelocity * mk;
		oldVelocity2 = ( newVelocity1 + oldVelocity1 * ( mk - 1 ) ) / mk;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="newVelocity1"></param>
	/// <param name="relVelocity">oldVelocity2 - oldVelocity1</param>
	/// <param name="mass1"></param>
	/// <param name="mass2"></param>
	/// <param name="bounciness1"></param>
	/// <param name="oldVelocity1"></param>
	/// <param name="oldVelocity2"></param>
	public static void CalculatePreCollisionVelocity (
		Vector2 newVelocity1, Vector2 relVelocity,
		float mass1, float mass2,
		float bounciness1,
		out Vector2 oldVelocity1, out Vector2 oldVelocity2
	) {
		float mbk;

		if ( float.IsInfinity ( mass1 ) )
			mbk = 0;
		else if ( float.IsInfinity ( mass2 ) )
			mbk = 1;
		else
			mbk = mass2 * ( 1 + bounciness1 ) / ( mass1 + mass2 );

		oldVelocity1 = newVelocity1 - relVelocity * mbk;
		oldVelocity2 = ( newVelocity1 + oldVelocity1 * ( mbk - 1 ) ) / mbk;
	}

	public static void CalculatePreCollisionVelocity (
		Collision2D collision,
		out Vector2 oldVelocity1, out Vector2 oldVelocity2,
		PhysicsMaterial2D defaultMaterial = null
	) {
		var contact = collision.contacts [0];
		var otherCollider = collision.collider;
		var thisCollider = contact.collider != otherCollider ? contact.collider : contact.otherCollider;
		var body1 = thisCollider.rigidbody2D;
		var body2 = otherCollider.rigidbody2D;
		float mass1, mass2;
		Vector2 newVelocity1;

		if ( body1 != null ) {
			mass1 = body1.mass;
			newVelocity1 = body1.velocity;
		} else {
			mass1 = float.PositiveInfinity;
			newVelocity1 = Vector2.zero;
		}
		
		if ( body2 != null )
			mass2 = body2.mass;
		else
			mass2 = float.PositiveInfinity;

		float maxBounciness = Mathf.Max (
			GetBounciness ( thisCollider, defaultMaterial ),
			GetBounciness ( otherCollider, defaultMaterial )
		);

		if ( float.IsInfinity ( mass1 ) ) {
			var newVelocity2 = body2 != null ? body2.velocity : Vector2.zero;
			PhysicsHelper.CalculatePreCollisionVelocity (
				newVelocity2, collision.relativeVelocity,
				mass2, mass1,
				maxBounciness,
				out oldVelocity2, out oldVelocity1
			);
		} else {
			PhysicsHelper.CalculatePreCollisionVelocity (
				newVelocity1, -collision.relativeVelocity,
				mass1, mass2,
				maxBounciness,
				out oldVelocity1, out oldVelocity2
			);
		}
	}

	private static PhysicsMaterial2D defaultMaterialInstance = null;

	private static float GetBounciness ( Collider2D collider, PhysicsMaterial2D defaultMaterial ) {
		var material = collider.sharedMaterial;

		if ( material == null ) {
			if ( defaultMaterial == null ) {
				if ( defaultMaterialInstance == null )
					defaultMaterialInstance = new PhysicsMaterial2D ();

				material = defaultMaterialInstance;
			} else
				material = defaultMaterial;
		}

		return	material.bounciness;
	}

	public static int GetLayerCollisionMask ( int layer ) {
		int mask = 0;

		for ( int i = 0 ; i < 32 ; i++ ) {
			bool ignored = Physics2D.GetIgnoreLayerCollision ( layer, i );

			if ( !ignored )
				mask |= 1 << i;
		}

		return	mask;
	}
}
