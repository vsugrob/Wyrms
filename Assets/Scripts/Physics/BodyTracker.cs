using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BodyTracker : MonoBehaviour {
	private static float lastTrackersUpdateTimestamp = float.NegativeInfinity;
	private static HashSet <Rigidbody2D> trackedBodies = new HashSet <Rigidbody2D> ();

	public bool IsBootstrapInstance = false;

	private Vector2 prevLinearVelocity;
	private float prevAngularVelocity;

	private float lastFetchTimestamp = float.NegativeInfinity;
	private Vector2 curLinearVelocity;
	private float curAngularVelocity;

	public Vector2 PrevLinearVelocity {
		get {
			if ( lastFetchTimestamp == Time.fixedTime )
				return	prevLinearVelocity;
			else
				return	curLinearVelocity;
		}
	}

	public float PrevAngularVelocity {
		get {
			if ( lastFetchTimestamp == Time.fixedTime )
				return	prevAngularVelocity;
			else
				return	curAngularVelocity;
		}
	}

	void FixedUpdate () {
		UpdateTrackerComponents ();

		if ( !IsBootstrapInstance )
			FetchData ();
	}

	private void FetchData () {
		if ( rigidbody2D != null ) {
			prevLinearVelocity = curLinearVelocity;
			prevAngularVelocity = curAngularVelocity;
			curLinearVelocity = rigidbody2D.velocity;
			curAngularVelocity = rigidbody2D.angularVelocity;
			lastFetchTimestamp = Time.fixedTime;
		}
	}

	public static BodyTracker GetTracker ( GameObject gameObject ) {
		var tracker = gameObject.GetComponent <BodyTracker> ();

		if ( tracker == null && gameObject.rigidbody2D != null )
			tracker = AddTrackerComponent ( gameObject );

		return	tracker;
	}

	public Vector2 GetVelocityChange ( Vector2 worldPoint ) {
		var prevVelocity = GetPrevPointVelocity ( worldPoint );
		var curVelocity = rigidbody2D.GetPointVelocity ( worldPoint );

		return	curVelocity - prevVelocity;
	}

	// TODO: unused now. Remove?
	public void GetContactVelocityChanges (
		Vector2 worldPoint, BodyTracker otherTracker,
		out Vector2 velocityChange1, out Vector2 velocityChange2
	) {
		var prevVelocity1 = GetPrevPointVelocity ( worldPoint );
		var curVelocity1 = rigidbody2D.GetPointVelocity ( worldPoint );
		velocityChange1 = curVelocity1 - prevVelocity1;

		var prevVelocity2 = otherTracker.GetPrevPointVelocity ( worldPoint );
		var curVelocity2 = otherTracker.rigidbody2D.GetPointVelocity ( worldPoint );
		velocityChange2 = curVelocity2 - prevVelocity2;
	}

	public Vector2 GetPrevPointVelocity ( Vector2 worldPoint ) {
		var vToPoint = worldPoint - rigidbody2D.worldCenterOfMass;
		float angVelRad = PrevAngularVelocity * Mathf.Deg2Rad;
		
		var tangent = Common.LeftOrthogonal ( vToPoint );
		var angularVelocity = tangent * angVelRad;
		var totalVelocity = angularVelocity + PrevLinearVelocity;

		return	totalVelocity;
	}

	private static void UpdateTrackerComponents () {
		if ( lastTrackersUpdateTimestamp != Time.fixedTime ) {
			// TODO: is there any faster way of querying rigidbodies?
			var bodies = GameObject.FindObjectsOfType <Rigidbody2D> ();

			foreach ( var body in bodies ) {
				if ( !trackedBodies.Contains ( body ) ) {
					AddTrackerComponent ( body.gameObject );
					trackedBodies.Add ( body );
				}

				// TODO: remove element from the trackedBodies list when body == null (object was destroyed from the Unity point of view).
			}

			lastTrackersUpdateTimestamp = Time.fixedTime;
		}
	}

	private static BodyTracker AddTrackerComponent ( GameObject gameObject ) {
		var tracker = gameObject.AddComponent <BodyTracker> ();

		/* Don't wait for the next update,
		 * make the data available as soon as possible. */
		tracker.FetchData ();

		return	tracker;
	}
}
