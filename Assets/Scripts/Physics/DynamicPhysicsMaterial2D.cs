using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DynamicPhysicsMaterial2D : MonoBehaviour {
	public float Friction = 0.5f;
	public float Bounciness = 0;

	void Start () {
		var m = new PhysicsMaterial2D ( name + "'s PhysicsMaterial2D" );
		m.friction = Friction;
		m.bounciness = Bounciness;
		// TODO: employ material instancing via collider2D.material instead of collider2D.sharedMaterial + new material technique.
		collider2D.sharedMaterial = m;
		collider2D.enabled = false;
		collider2D.enabled = true;
	}
	
	void FixedUpdate () {
		var m = collider2D.sharedMaterial;

		if ( m.friction != Friction ||
			m.bounciness != Bounciness
		) {
			m.friction = Friction;
			m.bounciness = Bounciness;
			collider2D.enabled = false;
			collider2D.enabled = true;
		}
	}
}
