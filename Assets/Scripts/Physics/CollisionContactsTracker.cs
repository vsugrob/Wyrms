using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

[DisallowMultipleComponent]
public class CollisionContactsTracker : MonoBehaviour {
	public bool ProcessOnCollisionStay = false;
	/// <summary>
	/// This check is a guard against divergent collisions which oftenly happen
	/// when active state of collider is changed.
	/// </summary>
	public bool CheckRelativeVelocity = true;

	private List <SurfaceContact> prevContacts = new List <SurfaceContact> ();
	private List <SurfaceContact> contacts = new List <SurfaceContact> ();
	public List <SurfaceContact> Contacts {
		get {
			if ( prevContactsTimestamp == Time.fixedTime )
				return	prevContacts;
			else
				return	contacts;
		}
	}

	private float prevContactsTimestamp = float.NegativeInfinity;

	void OnCollisionEnter2D ( Collision2D collision ) {
		ProcessCollision ( collision );
	}

	void OnCollisionStay2D ( Collision2D collision ) {
		if ( ProcessOnCollisionStay )
			ProcessCollision ( collision );
	}

	void FixedUpdate () {
		/* TODO: use prevContacts strategy when ProcessOnCollisionStay is enabled.
		 * Take it from GrabbingDolls contact tracking.
		 * We need it becase when object is going to sleep, contacts are cleared,
		 * however body is still in contact with surrounding objects. */

		prevContacts = contacts;
		prevContactsTimestamp = Time.fixedTime;
		contacts = new List <SurfaceContact> ();
	}

	private void ProcessCollision ( Collision2D collision ) {
		foreach ( var contact in collision.contacts ) {
			var normal = contact.normal;
			var v = ( Vector2 ) collider2D.bounds.center - contact.point;

			/* Fix normal, as Unity (or Box2D) messing it up by randomly swapping
			 * contact.collider and contact.otherCollider
			 * as well as providing opposite normal direction. */
			if ( Vector2.Dot ( v, contact.normal ) < 0 )
				normal = -normal;

			if ( CheckRelativeVelocity && Vector2.Dot ( collision.relativeVelocity, normal ) >= 0 )
				continue;

			var surfaceContact = new SurfaceContact ( contact.point, normal, collision.collider, Vector2.zero, 0 );
			contacts.Add ( surfaceContact );
		}
	}
}
