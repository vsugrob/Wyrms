using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayableArea : MonoBehaviour {
	public bool DestroyFugitives = true;

	void OnTriggerExit2D ( Collider2D collider ) {
		if ( DestroyFugitives ) {
			var objToDestroy = collider.gameObject;
			var body = collider.attachedRigidbody;

			if ( body != null )
				objToDestroy = body.gameObject;
			/* TODO: do not destroy characters, I want them to be resurrectable.
			 * It would be much easier to restore character health and make it visible again
			 * rather than recreate it. */
			Destroy ( objToDestroy );
		}
	}
}
