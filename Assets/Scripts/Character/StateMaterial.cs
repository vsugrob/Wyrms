using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class StateMaterial {
	public float Friction;
	public float Bounciness;

	public StateMaterial ( float friction, float bounciness ) {
		this.Friction = friction;
		this.Bounciness = bounciness;
	}

	public void SetupValues ( Collider2D collider ) {
		var m = collider.sharedMaterial;

		if ( m.friction != Friction || m.bounciness != Bounciness ) {
			m.friction = Friction;
			m.bounciness = Bounciness;
			collider.enabled = false;
			collider.enabled = true;
		}
	}

	public static StateMaterial Lerp ( StateMaterial from, StateMaterial to, float t ) {
		return	new StateMaterial (
			Mathf.Lerp ( from.Friction, to.Friction, t ),
			Mathf.Lerp ( from.Bounciness, to.Bounciness, t )
		);
	}

	public override string ToString () {
		return	string.Format (
			"Friction: {0}, Bounciness: {1}",
			Friction, Bounciness
		);
	}
}
