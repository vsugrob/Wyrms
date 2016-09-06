using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DevAbsorbImpulse : MonoBehaviour {
	void OnCollisionEnter2D ( Collision2D collision ) {
		AbsorbIncomingImpulse ( collision );
	}

	void OnCollisionStay2D ( Collision2D collision ) {
		AbsorbIncomingImpulse ( collision );
	}

	void FixedUpdate () {
		print ( "FixedUpdate, rigidbody2D.velocity: " + rigidbody2D.velocity.ToString ( "F2" ) );
	}

	void AbsorbIncomingImpulse ( Collision2D collision ) {
		Vector2 oldVelocity1, oldVelocity2;
		PhysicsHelper.CalculatePreCollisionVelocity ( collision, out oldVelocity1, out oldVelocity2 );

		var velocityChange = rigidbody2D.velocity - oldVelocity1;
		print (
			"oldVelocity1: " + oldVelocity1.ToString ( "F2" ) +
			", rigidbody2D.velocity: " + rigidbody2D.velocity.ToString ( "F2" ) +
			", oldVelocity2: " + oldVelocity2.ToString ( "F2" ) +
			", collision.rigidbody.velocity: " + collision.rigidbody.velocity.ToString ( "F2" ) +
			", -velocityChange: " + ( -velocityChange ).ToString ( "F2" )
		);
		//rigidbody2D.AddForce ( -velocityChange, ForceMode.VelocityChange );

		if ( rigidbody2D.velocity.magnitude > oldVelocity1.magnitude )
			rigidbody2D.velocity += -velocityChange;
	}
}
