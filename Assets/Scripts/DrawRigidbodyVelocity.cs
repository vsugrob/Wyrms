using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DrawRigidbodyVelocity : MonoBehaviour {
	void FixedUpdate () {
		if ( rigidbody2D != null )
			Debug.DrawRay ( transform.position, rigidbody2D.velocity, Color.cyan, 0, false );
	}
}
