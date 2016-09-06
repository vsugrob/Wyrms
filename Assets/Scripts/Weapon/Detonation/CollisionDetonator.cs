using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CollisionDetonator : MonoBehaviour {
	public DetonationPosition DetonateAt;
	[TooltipAttribute (
		"This value will be responded whenever LifetimeRequest received by this object." +
		" Think of it as an average period of time from instantiation to destruction for this kind of object." +
		" It doesn't need to be exact."
	)]
	public float LifetimeResponseValue = 3;

	public bool IsDetonated { get; private set; }

	void Start () {}
	
	void OnCollisionEnter2D ( Collision2D collision ) {
		ProcessCollision ( collision );
	}

	void OnCollisionStay2D ( Collision2D collision ) {
		ProcessCollision ( collision );
	}

	private void ProcessCollision ( Collision2D collision ) {
		if ( !enabled || IsDetonated )
			return;

		Vector2 detonationPos;

		if ( DetonateAt == DetonationPosition.CollisionPoint ) {
			var farthestContact = collision.contacts
				.WithMax ( c => Common.DistanceSq ( c.point, transform.position ) );

			detonationPos = farthestContact.point;
		} else
			detonationPos = transform.position;

		var messageData = new DetonateMessageData ( detonationPos, this );
		BroadcastMessage ( DetonateMessageData.MessageName, messageData, SendMessageOptions.DontRequireReceiver );
		IsDetonated = true;
	}

	void OnLifetimeRequest ( LifetimeRequest request ) {
		if ( !enabled )
			return;

		request.MinLifetime = IsDetonated ? 0 : LifetimeResponseValue;
	}

	public enum DetonationPosition {
		CollisionPoint,
		TransformPosition
	}
}
