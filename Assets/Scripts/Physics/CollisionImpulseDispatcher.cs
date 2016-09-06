using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
[RequireComponent ( typeof ( CollisionContactsTracker ) )]
public class CollisionImpulseDispatcher : MonoBehaviour {
	public ImpulseNormalizationType SelfImpulseNormalization;
	public float SelfImpulseMultiplier = 1;
	public ImpulseNormalizationType IncomingImpulseNormalization;
	public float IncomingImpulseMultiplier = 1;

	private CollisionContactsTracker collisionTracker;
	private float lastDispatchTimestamp = float.NegativeInfinity;

	void Start () {
		collisionTracker = GetComponent <CollisionContactsTracker> ();
	}

	void FixedUpdate () {
		ProcessCollisions ();
	}

	/// <summary>
	/// Use this when your behaviour is updated before
	/// collision dispatcher and you want some specific order.
	/// </summary>
	public void ForceMessageDispatch () {
		ProcessCollisions ();
	}

	private void ProcessCollisions () {
		if ( Time.fixedTime == lastDispatchTimestamp )
			return;

		lastDispatchTimestamp = Time.fixedTime;
		var contacts = collisionTracker.Contacts;

		if ( contacts.Count != 0 ) {
			var contactsByBodies = contacts
				.Where ( contact => contact.Collider != null )	// Avoid destroyed contacts.
				.GroupBy ( contact => contact.Collider.attachedRigidbody );
			// TODO: implement case when this.rigidbody2D is null.
			var thisTracker = BodyTracker.GetTracker ( this.gameObject );

			foreach ( var contactsOfBody in contactsByBodies ) {
				var body = contactsOfBody.Key;
				SurfaceContact collisionContact;
				float otherImpulse;

				if ( body != null ) {	// Dynamic obstacle.
					var otherTracker = BodyTracker.GetTracker ( body.gameObject );
					var otherVelChanges = contactsOfBody.Select ( contact => new {
						VelocityChange = otherTracker.GetVelocityChange ( contact.Point ),
						Contact = contact
					} );
					var otherMaxVelChange = otherVelChanges.WithMax ( velChange => velChange.VelocityChange.sqrMagnitude );
					var otherVelChange = otherMaxVelChange.VelocityChange;
					otherImpulse = otherVelChange.magnitude;

					if ( IncomingImpulseNormalization == ImpulseNormalizationType.MultiplyByCoefficient )
						otherImpulse *= body.mass * IncomingImpulseMultiplier;

					collisionContact = otherMaxVelChange.Contact;
				} else {				// Static obstacle.
					otherImpulse = 0;
					collisionContact = contactsOfBody.First ();
				}

				var thisVelChange = thisTracker.GetVelocityChange ( collisionContact.Point );
				float thisImpulse = thisVelChange.magnitude;

				if ( SelfImpulseNormalization == ImpulseNormalizationType.MultiplyByCoefficient )
					thisImpulse *= rigidbody2D.mass * IncomingImpulseMultiplier;

				var messageData = new CollisionMessageData (
					thisImpulse, otherImpulse,
					collisionContact,
					contactsOfBody.ToList ()
				);

				SendMessage ( CollisionMessageData.MessageName, messageData, SendMessageOptions.DontRequireReceiver );
			}
		}
	}
}

public enum ImpulseNormalizationType {
	MultiplyByCoefficient, DivideByMass
}

class CollisionMessageData {
	public const string MessageName = "OnCollisionImpulse";
	public float ThisImpulse;
	public float OtherImpulse;
	public float TotalImpulse { get { return	ThisImpulse + OtherImpulse; } }
	public SurfaceContact PrimaryContact;
	public List <SurfaceContact> Contacts;

	public CollisionMessageData (
		float thisImpulse, float otherImpulse,
		SurfaceContact primaryContact,
		List <SurfaceContact> contacts
	) {
		this.ThisImpulse = thisImpulse;
		this.OtherImpulse = otherImpulse;
		this.PrimaryContact = primaryContact;
		this.Contacts = contacts;
	}
}