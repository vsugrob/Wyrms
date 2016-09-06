using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DevManualImpulseCalculation : MonoBehaviour {
	void Start () {
		rigidbody2D.angularVelocity = 90;
		rigidbody2D.velocity = Vector2.one;
	}

	void FixedUpdate () {
		var p = ( Vector2 ) transform.position + 1 * Vector2.right;
		DebugHelper.DrawCircle ( p, 0.05f, Color.yellow );
		//var pVel = rigidbody2D.GetPointVelocity ( p );
		var pVel = GetPointVelocity ( rigidbody2D, p );

		print (
			"pVel: " + pVel.ToString ( "G" ) +
			", inertia: " + rigidbody2D.inertia
		);
	}

	private static Vector2 GetPointVelocity ( Rigidbody2D body, Vector2 worldPoint ) {
		var vToPoint = worldPoint - body.worldCenterOfMass;
		float angVelRad = body.angularVelocity * Mathf.Deg2Rad;
		
		var tangent = Common.LeftOrthogonal ( vToPoint );
		var angularVelocity = tangent * angVelRad;
		var totalVelocity = angularVelocity + body.velocity;

		return	totalVelocity;
	}
}
