using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class Spatial {
	// TODO: make predicate instead of ignorePredicate (accept "true" instead of "false").
	public static IEnumerable <RaycastHit2D> CircleCastAllFiltered (
		Vector2 origin, float radius, Vector2 direction, float distance,
		System.Func <RaycastHit2D, bool> ignorePredicate = null,
		int layerMask = Physics2D.DefaultRaycastLayers
	) {
		var hits = Physics2D.CircleCastAll ( origin, radius, direction, distance, layerMask );

		foreach ( var hit in hits ) {
			if ( ignorePredicate != null && ignorePredicate ( hit ) )
				continue;

			yield return hit;
		}
	}

	// TODO: make predicate instead of ignorePredicate (accept "true" instead of "false").
	public static RaycastHit2D CircleCastFiltered (
		Vector2 origin, float radius, Vector2 direction, float distance,
		System.Func <RaycastHit2D, bool> ignorePredicate = null,
		int layerMask = Physics2D.DefaultRaycastLayers
	) {
		var hits = CircleCastAllFiltered ( origin, radius, direction, distance, ignorePredicate, layerMask );

		return	hits.FirstOrDefault ();
	}

	// TODO: make predicate instead of ignorePredicate (accept "true" instead of "false").
	public static List <SurfaceContact> GetCircleContacts (
		Vector2 center, float radius, float distanceThreshold = 0.01f,
		System.Func <RaycastHit2D, bool> ignorePredicate = null,
		int numSweepDirections = 4,
		int layerMask = Physics2D.DefaultRaycastLayers
	) {
		float minDistance = radius - distanceThreshold;
		float maxDistance = radius + distanceThreshold;

		return	GetCircleContacts (
			center, radius, minDistance, maxDistance,
			ignorePredicate,
			numSweepDirections,
			layerMask
		);
	}

	private const float CastOffset = 0.01f;

	// TODO: make predicate instead of ignorePredicate (accept "true" instead of "false").
	public static List <SurfaceContact> GetCircleContacts (
		Vector2 center, float radius, float minDistance, float maxDistance,
		System.Func <RaycastHit2D, bool> ignorePredicate = null,
		int numSweepDirections = 4,
		int layerMask = Physics2D.DefaultRaycastLayers
	) {
		var contacts = new List <SurfaceContact> ();
		float angle = 0;
		float dAngle = Mathf.PI * 2 / numSweepDirections;
		float offset = radius - minDistance + CastOffset;
		float radiusSq = radius * radius;
		float sweepDistance = maxDistance - minDistance + CastOffset * 2;

		// DEBUG
		//DebugHelper.DrawCircle ( center, minDistance, new Color ( 1, 0.5f, 0.25f ) );
		//DebugHelper.DrawCircle ( center, maxDistance, new Color ( 1, 0.5f, 0.25f ) );
		
		for ( int i = 0 ; i < numSweepDirections ; i++, angle += dAngle ) {
			var dir = new Vector2 (
				Mathf.Cos ( angle ),
				Mathf.Sin ( angle )
			);
			
			var origin = center - dir * offset;
			var hits = Physics2D.CircleCastAll ( origin, radius, dir, sweepDistance, layerMask );
			//Debug.DrawRay ( hits [1].point, hits [1].normal, Color.red );
			//DebugHelper.DrawCircle ( hits [1].point, 0.1f, Color.yellow );
			//Debug.Log ( hits [1].collider );
			//DebugHelper.DrawCircle ( origin, radius, Color.yellow );

			for ( int j = 0 ; j < hits.Length ; j++ ) {
				var hit = hits [j];

				// TODO: make argument "acceptInnerCollisions"
				/* TODO: make option to discard inner collisions when outer collisions for the same collider were found.
				 * UPD: or perform this by default? */
				// Filter inner collisions and fix their normals.
				if ( hit.distance <= 0 ) {
					// Some points can be out of original circle.
					float distFromCenterSq = Common.DistanceSq ( hit.point, center );
					
					if ( distFromCenterSq > radiusSq )
						continue;

					/* Some inner collisions are reported outside of bounding box
					 * of other collider. Filter them out as they're quite inaccurate. */
					var otherBounds = hit.collider.bounds;
					var hitPointInZPlane = ( Vector3 ) hit.point;
					hitPointInZPlane.z = otherBounds.center.z;
					bool hitIsInsideBb = otherBounds.Contains ( hitPointInZPlane );
					
					if ( !hitIsInsideBb )
						continue;

					/* Now we definitely sure that this is appropriate inner collision
					 * and we can fix its normal. */
					var vToCenter = center - hit.point;
					hit.normal = vToCenter.normalized;
				}

				if ( ignorePredicate != null && ignorePredicate ( hit ) )
					continue;

				var vToPoint = hit.point - center;
				float distance = vToPoint.magnitude;

				if ( distance < minDistance || distance > maxDistance )
					continue;

				var dirToPoint = vToPoint.normalized;
				
				var contact = new SurfaceContact (
					hit.point, hit.normal,
					hit.collider,
					dirToPoint, distance
				);
				contacts.Add ( contact );
			}
		}

		const float sameVertexDistanceThreshold = 0.001f;	// TODO: promote to arguments.
		const float sameVertexDistanceThresholdSq = sameVertexDistanceThreshold * sameVertexDistanceThreshold;

		for ( int i = 0 ; i < contacts.Count ; i++ ) {
			var c1 = contacts [i];
			bool foundCloseVerices = false;

			for ( int j = contacts.Count - 1 ; j > i ; j-- ) {
				var c2 = contacts [j];

				if ( c1.Collider == c2.Collider &&
					Common.DistanceSq ( c1.Point, c2.Point ) < sameVertexDistanceThresholdSq
				) {
					contacts.RemoveAt ( j );
					foundCloseVerices = true;
				}
			}

			if ( foundCloseVerices ) {
				// Most probable it's a corner collision. Calculate precise normal.
				c1.Normal = ( center - c1.Point ).normalized;
			}
		}

		return	contacts;
	}

	public static Dictionary <Rigidbody2D, List <SurfaceContact>> GetCircleContactsByBody (
		Vector2 center, float radius, float minDistance, float maxDistance,
		System.Func <RaycastHit2D, bool> ignorePredicate = null,
		int numSweepDirections = 4,
		int layerMask = Physics2D.DefaultRaycastLayers
	) {
		var contacts = Spatial.GetCircleContacts (
			center, radius, 0, radius,
			ignorePredicate,
			numSweepDirections,
			layerMask
		);

		var contactsByBody = new Dictionary <Rigidbody2D, List <SurfaceContact>> ();

		foreach ( var contact in contacts ) {
			List <SurfaceContact> bodyContacts;
			var body = contact.Collider.attachedRigidbody;

			if ( body == null )
				continue;

			if ( !contactsByBody.TryGetValue ( body, out bodyContacts ) ) {
				bodyContacts = new List <SurfaceContact> ();
				contactsByBody [body] = bodyContacts;
			}

			bodyContacts.Add ( contact );
		}

		return	contactsByBody;
	}

	public static Dictionary <TComponent, List <SurfaceContact>> GetCircleContactsByAttachedComponent <TComponent> (
		Vector2 center, float radius, float minDistance, float maxDistance,
		System.Func <RaycastHit2D, bool> ignorePredicate = null,
		int numSweepDirections = 4,
		int layerMask = Physics2D.DefaultRaycastLayers
	) where TComponent : Component {
		var contacts = Spatial.GetCircleContacts (
			center, radius, 0, radius,
			ignorePredicate,
			numSweepDirections,
			layerMask
		);

		var contactsByComponent = new Dictionary <TComponent, List <SurfaceContact>> ();

		foreach ( var contact in contacts ) {
			List <SurfaceContact> componentContacts;
			var component = contact.Collider.GetComponentInParent <TComponent> ();

			if ( component == null )
				continue;

			if ( !contactsByComponent.TryGetValue ( component, out componentContacts ) ) {
				componentContacts = new List <SurfaceContact> ();
				contactsByComponent [component] = componentContacts;
			}

			componentContacts.Add ( contact );
		}

		return	contactsByComponent;
	}

	public static System.Func <RaycastHit2D, bool> IgnoreSelfCollisionPredicate ( Collider2D collider ) {
		if ( collider == null )
			return	hit => false;
		else {
			return	hit => {
				var hitCollider = hit.collider;

				return	hitCollider == collider ||
					hitCollider.attachedRigidbody == collider.attachedRigidbody ||
					Physics2D.GetIgnoreCollision ( hitCollider, collider );
			};
		}
	}

	public static System.Func <RaycastHit2D, bool> IgnoreSelfCollisionPredicate ( Rigidbody2D rigidbody ) {
		if ( rigidbody == null )
			return	hit => false;
		else
			return	hit => hit.collider.attachedRigidbody == rigidbody;
	}
}

// TODO: move into separate file.
public class SurfaceContact {
	public Vector2 Point;
	public Vector2 Normal;
	public Collider2D Collider;
	public Vector2 Direction;
	public float Distance;

	public SurfaceContact (
		Vector2 point, Vector2 normal,
		Collider2D collider,
		Vector2 direction,
		float distance
	) {
		this.Point = point;
		this.Normal = normal;
		this.Collider = collider;
		this.Direction = direction;
		this.Distance = distance;
	}

	#region Debug
	private const float NormalLength = 0.1f;
	private static readonly Color NormalColor = Color.green;

	public void DebugDraw ( Color color ) {
		DebugHelper.DrawRay ( Point, Normal * NormalLength, color, 0, false );
		DebugHelper.DrawCircle ( Point, 0.01f, Color.yellow );
	}

	public void DebugDraw () {
		DebugDraw ( NormalColor );
	}
	#endregion Debug
}