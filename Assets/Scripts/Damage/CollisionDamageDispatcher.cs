using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
[RequireComponent ( typeof ( CollisionImpulseDispatcher ) )]
public class CollisionDamageDispatcher : MonoBehaviour {
	public float MinHarmfulCollisionImpulse = 6;
	public float DamagePerImpulseUnit = 2.5f;
	public float MinImpulseDamage = 5;
	public float MaxImpulseDamage = 25;
	public DamageKind DamageKind = DamageKind.Collision;

	private CollisionImpulseDispatcher collisionImpulseDispatcher;

	void Start () {
		collisionImpulseDispatcher = GetComponent <CollisionImpulseDispatcher> ();
	}

	void FixedUpdate () {
		collisionImpulseDispatcher.ForceMessageDispatch ();
	}

	/// <summary>
	/// Use this when your behaviour is updated before
	/// collision dispatcher and you want some specific order.
	/// </summary>
	public void ForceMessageDispatch () {
		collisionImpulseDispatcher.ForceMessageDispatch ();
	}

	void OnCollisionImpulse ( CollisionMessageData collisionData ) {
		if ( !enabled )
			return;

		float impulse = collisionData.TotalImpulse;

		if ( impulse > MinHarmfulCollisionImpulse ) {
			float impulseDamage = ( impulse - MinHarmfulCollisionImpulse ) * DamagePerImpulseUnit;

			if ( impulseDamage < MinImpulseDamage )
				return;
			else if ( impulseDamage > MaxImpulseDamage )
				impulseDamage = MaxImpulseDamage;

			DamageReceiver.InflictDamage (
				gameObject,
				collisionData.PrimaryContact.Collider,
				impulseDamage,
				DamageKind,
				collisionData.Contacts.ToList ()
			);
		}
	}
}
