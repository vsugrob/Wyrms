using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RadialDamageField : DamageFieldBase {
	public Vector2 Offset;
	public bool DecreaseWithDistance = true;
	public float Radius = 3;
	public Collider2D ColliderToIgnore = null;
	public int NumSweepDirections = 4;
	public LayerMask LayerMask = Physics2D.DefaultRaycastLayers;

	public Vector2 Origin { get { return	( Vector2 ) transform.position + Offset; } }

	bool isFixedStarted;

	void FixedStart () {
		// TODO: invent Delay field!
		Apply ();
	}

	void FixedUpdate () {
		if ( !isFixedStarted ) {
			FixedStart ();
			isFixedStarted = true;
		}
	}

	private void Apply () {
		var ignoreSelfCollisionPredicate = Spatial.IgnoreSelfCollisionPredicate ( ColliderToIgnore );
		var contactsByComponent = Spatial.GetCircleContactsByAttachedComponent <DamageReceiver> (
			Origin, Radius, 0, Radius,
			ignoreSelfCollisionPredicate,
			NumSweepDirections,
			LayerMask
		);

		foreach ( var componentAndContacts in contactsByComponent ) {
			var component = componentAndContacts.Key;
			var componentContacts = componentAndContacts.Value;
			float influence = DamageReceiver.GetInfluence ( component.gameObject, Kind );

			if ( influence == 0 )
				continue;

			float damageAmount = Amount * influence;
			var closestContact = componentContacts.WithMin ( c => c.Distance );
			float distanceFactor;

			if ( DecreaseWithDistance )
				distanceFactor = 1 - closestContact.Distance / Radius;
			else
				distanceFactor = 1;

			float damage = damageAmount * distanceFactor;
			
			if ( SendDamageMessage ( component.gameObject, damage, componentContacts ) ) {
				/* TODO: apply some effect.
				 * E.g: fire damage can spawn little burning torches in componentContacts.
				 * Acid damage can spawn some greenish steamy particles.
				 * Electricity will produce lightning bursts in points of contact.
				 * Healing (Love) can have some pleasant effect too. */
			}
		}
	}
}
