using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent ( typeof ( MutableSpriteCollider ) )]
public class ChompDestructible : MonoBehaviour {
	// Just to add Enable/Disable check button.
	void Start () {}

	void OnChomp ( ChompMessageData chompData ) {
		if ( !enabled )
			return;

		var mutableCollider = GetComponent <MutableSpriteCollider> ();
		
		if ( mutableCollider.enabled )
			mutableCollider.CutCircle ( chompData.Center, chompData.Radius, chompData.Colliders );
	}
}
