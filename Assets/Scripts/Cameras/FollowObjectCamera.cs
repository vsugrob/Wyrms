using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FollowObjectCamera : MonoBehaviour {
	public Transform Target;
	public float PredictionTime = 0.5f;
	public float MaxPredictionDistance = 1;
	public bool FixedToTarget = false;

	void LateUpdate () {
		if ( Target != null ) {
			var camTf = camera.transform;

			if ( FixedToTarget ) {
				var pos = Target.position;
				pos.z = camTf.position.z;
				camTf.position = pos;
			} else {
				Vector2 targetPos;
				var rb = Target.rigidbody2D;

				if ( rb != null ) {
					// Attempt to predict target object position.
					var realTargetPos = ( Vector2 ) Target.position;
					targetPos = realTargetPos + rb.velocity * PredictionTime;
					var posDiff = targetPos - realTargetPos;

					if ( posDiff.magnitude > MaxPredictionDistance )
						targetPos = realTargetPos + posDiff.normalized * MaxPredictionDistance;
				} else
					targetPos = Target.position;

				var pos = Vector2.Lerp (
					camTf.position,
					targetPos,
					0.05f
				);

				camTf.position = new Vector3 ( pos.x, pos.y, camTf.position.z );
			}
		}
	}
}
