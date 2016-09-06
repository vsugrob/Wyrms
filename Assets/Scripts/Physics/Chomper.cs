using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Chomper : MonoBehaviour {
	public float Radius = 1;
	public Vector2 Offset;
	public LayerMask LayerMask = Physics2D.DefaultRaycastLayers;

	public Vector2 Origin { get { return	( Vector2 ) transform.position + Offset; } }

	// TODO: make base class FixedMonoBehaviour : MonoBehaviour.
	bool isFixedStarted;

	void FixedStart () {
		Chomp ();
	}
	
	void FixedUpdate () {
		if ( !isFixedStarted ) {
			FixedStart ();
			isFixedStarted = true;
		}
	}

	void OnEnable () {
		isFixedStarted = false;
	}

	private void Chomp () {
		var origin = Origin;
		var colliders = Physics2D.OverlapCircleAll ( origin, Radius, LayerMask );
		
		if ( colliders.Length > 0 ) {
			/* Send message once per game object even when circle overlapped
			 * several colliders attached to the same object. */
			var collidersByGameObjects = new Dictionary <GameObject, List <Collider2D>> ();

			foreach ( var overlappedCollider in colliders ) {
				// Ignore self-overlapping.
				if ( collider2D != null && (
					overlappedCollider == collider2D ||
					overlappedCollider.attachedRigidbody == collider2D.attachedRigidbody
					)
				) {
					continue;
				}

				List <Collider2D> colliderList;

				if ( !collidersByGameObjects.TryGetValue ( overlappedCollider.gameObject, out colliderList ) ) {
					colliderList = new List <Collider2D> ();
					collidersByGameObjects [overlappedCollider.gameObject] = colliderList;
				}

				colliderList.Add ( overlappedCollider );
			}

			foreach ( var gameObjectColliders in collidersByGameObjects ) {
				var go = gameObjectColliders.Key;
				var colliderList = gameObjectColliders.Value;
				var data = new ChompMessageData ( transform.position, Radius, colliderList, this );
				go.SendMessage ( ChompMessageData.MessageName, data, SendMessageOptions.DontRequireReceiver );
			}
		}
	}
}

public class ChompMessageData {
	public const string MessageName = "OnChomp";
	public Vector2 Center { get; private set; }
	public float Radius { get; private set; }
	public List <Collider2D> Colliders { get; private set; }
	public Chomper Source { get; private set; }

	public ChompMessageData ( Vector2 center, float radius, List <Collider2D> colliders, Chomper source = null ) {
		this.Center = center;
		this.Radius = radius;
		this.Colliders = colliders;
		this.Source = source;
	}
}
