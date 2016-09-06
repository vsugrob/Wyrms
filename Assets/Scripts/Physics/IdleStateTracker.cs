using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class IdleStateTracker : MonoBehaviour {
	public const string OnBecameIdleMessageName = "OnBecameIdle";
	public float MaxVelocity = 0.2f;
	public float IdleDuration = 1;

	private float idleStartTimestamp;
	private bool velocityWasLow = false;
	public bool IsIdle { get; private set; }

	void FixedUpdate () {
		var body = rigidbody2D;

		if ( body.velocity.magnitude <= MaxVelocity ) {
			if ( !IsIdle ) {
				if ( !velocityWasLow ) {
					idleStartTimestamp = Time.fixedTime;
					velocityWasLow = true;
				}

				float idleTimeElapsed = Time.fixedTime - idleStartTimestamp;

				if ( idleTimeElapsed >= IdleDuration ) {
					IsIdle = true;
					SendMessage ( OnBecameIdleMessageName, SendMessageOptions.DontRequireReceiver );
				}
			}
		} else {
			velocityWasLow = false;
			IsIdle = false;
		}
	}
}
