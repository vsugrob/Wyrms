using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// This detonator is a fallback for CollisionDetonator which is buggy sometimes.
/// Under some random occasions unity 4.5.4f1 doesn't report OnCollisionEnter2D between rigidbodies
/// when at least one of them have its collision detection set to continuous.
/// </summary>
public class AccelerationDetonator : MonoBehaviour {
	public float CriticalAcceleration = 250;
	public bool DetonateOnlyOnSlowdown = true;
	[TooltipAttribute (
		"This value will be responded whenever LifetimeRequest received by this object." +
		" Think of it as an average period of time from instantiation to destruction for this kind of object." +
		" It doesn't need to be exact."
	)]
	public float LifetimeResponseValue = 3;

	private float prevVelocityMag;
	public bool IsDetonated { get; private set; }

	void Start () {
		if ( rigidbody2D == null )
			return;

		prevVelocityMag = rigidbody2D.velocity.magnitude;
	}
	
	void FixedUpdate () {
		if ( !enabled || IsDetonated || rigidbody2D == null )
			return;

		float curVelocityMag = rigidbody2D.velocity.magnitude;
		float velChange = curVelocityMag - prevVelocityMag;

		if ( DetonateOnlyOnSlowdown && velChange >= 0 )
			return;

		float accel = Mathf.Abs ( velChange ) / Time.fixedDeltaTime;

		if ( accel > CriticalAcceleration ) {
			var messageData = new DetonateMessageData ( transform.position, this );
			BroadcastMessage ( DetonateMessageData.MessageName, messageData, SendMessageOptions.DontRequireReceiver );
			IsDetonated = true;
		}

		prevVelocityMag = curVelocityMag;
	}

	void OnLifetimeRequest ( LifetimeRequest request ) {
		if ( !enabled )
			return;

		request.MinLifetime = IsDetonated ? 0 : LifetimeResponseValue;
	}
}
