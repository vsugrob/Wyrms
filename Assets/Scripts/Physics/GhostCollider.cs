using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GhostCollider : MonoBehaviour {
	public int GhostLayer = 0;
	public bool CollideWithPrototype = false;
	public GameObject GhostGameObject { get; private set; }
	
	void FixedUpdate () {
		if ( GhostGameObject != null ) {
			if ( GhostGameObject.layer != GhostLayer )
				GhostGameObject.layer = GhostLayer;

			Common.SetTransform ( GhostGameObject.transform, transform );
		}
	}

	void OnEnable () {
		if ( GhostGameObject != null )
			GhostGameObject.SetActive ( true );
		else
			CloneColliders ();

		if ( GhostGameObject != null )
			PhysicsHelper.IgnoreCollision ( GhostGameObject, gameObject, !CollideWithPrototype );
	}

	void OnDisable () {
		if ( GhostGameObject != null )
			GhostGameObject.SetActive ( false );
	}

	private void CloneColliders () {
		var protoColliders = GetComponentsInChildren <Collider2D> ();

		if ( protoColliders.Length != 0 ) {
			if ( GhostGameObject == null ) {
				GhostGameObject = new GameObject ( gameObject.name + "'s Ghost" );

				foreach ( var protoCollider in protoColliders ) {
					if ( protoCollider.isTrigger )
						continue;

					var container = GhostGameObject;
					var protoGo = protoCollider.gameObject;

					if ( protoGo != this.gameObject ) {
						container = new GameObject ( protoGo.name + "'s Ghost" );
						var containerTf = container.transform;
						Common.SetTransform ( containerTf, protoGo.transform );
						containerTf.parent = GhostGameObject.transform;
					}

					Common.Clone ( protoCollider, container );
				}
			}
		}
	}
}
