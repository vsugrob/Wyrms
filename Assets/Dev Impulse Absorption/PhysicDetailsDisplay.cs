using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PhysicDetailsDisplay : MonoBehaviour {
	void FixedUpdate () {
		DisplayDetails ( "FixedUpdate", Vector2.zero );
	}

	void OnCollisionEnter2D ( Collision2D collision ) {
		//DisplayDetails ( "OnCollisionEnter2D", collision.relativeVelocity );
		
		DisplayPreCollisionDetails ( collision );
	}

	void OnCollisionStay2D ( Collision2D collision ) {
		DisplayDetails ( "OnCollisionStay2D", collision.relativeVelocity );
	}

	private void DisplayDetails ( string prefix, Vector2 relativeVelocity ) {
		var body = rigidbody2D;

		if ( body != null ) {
			if ( !string.IsNullOrEmpty ( prefix ) )
				prefix += " ";

			float impulse = body.velocity.magnitude * body.mass;

			SceneViewLabel.SetText (
				gameObject,
				prefix +
				"velocity: " + body.velocity.ToString ( "F2" ) +
				", impulse: " + impulse.ToString ( "F2" ) +
				", relativeVelocity: " + relativeVelocity.ToString ( "F2" )
			);
		}
	}

	private void DisplayPreCollisionDetails ( Collision2D collision ) {
		Vector2 oldVelocity1, oldVelocity2;

		PhysicsHelper.CalculatePreCollisionVelocity (
			collision,
			out oldVelocity1, out oldVelocity2
		);

		SceneViewLabel.SetText (
			gameObject,
			"oldVelocity1: " + oldVelocity1.ToString ( "F2" ) +
			", oldVelocity2: " + oldVelocity2.ToString ( "F2" )
		);
	}
}
