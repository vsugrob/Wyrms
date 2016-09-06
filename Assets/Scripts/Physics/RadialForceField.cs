using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RadialForceField : ForceFieldBase {
	public Vector2 Offset;
	public bool DecreaseWithDistance = true;
	public float Radius = 3;
	public float Duration = float.Epsilon;
	public bool DecreaseOverTime = true;
	public int NumSweepDirections = 4;
	public LayerMask LayerMask = Physics2D.DefaultRaycastLayers;
	/// <summary>
	/// Convenience property for animation.
	/// </summary>
	public float AnimatedMultiplier = 1;

	public Vector2 Origin { get { return	( Vector2 ) transform.position + Offset; } }
	public float TimeElapsed { get { return	Time.fixedTime - startTimestamp; } }

#if UNITY_EDITOR
	/// <summary>
	/// Contacts from last FixedUpdate () run, stored for gizmo rendering.
	/// </summary>
	private Dictionary <Rigidbody2D, List <SurfaceContact>> lastFrameContacts =
		new Dictionary <Rigidbody2D, List <SurfaceContact>> ();
#endif

	private float startTimestamp;
	bool isFixedStarted;

	void FixedStart () {
		startTimestamp = Time.fixedTime;
	}

	void FixedUpdate () {
		if ( !isFixedStarted ) {
			FixedStart ();
			isFixedStarted = true;
		}

		float timeElapsed = this.TimeElapsed;

		if ( Amount == 0 || Radius == 0 || AnimatedMultiplier == 0 || timeElapsed >= Duration ) {
#if UNITY_EDITOR
			lastFrameContacts.Clear ();
#endif

			return;
		}

		float timeFactor = DecreaseOverTime ? 1 - timeElapsed / Duration : 1;

		ApplyToBodies ( timeFactor );
		ApplyToParticles ( timeFactor );
	}

	private void ApplyToBodies ( float timeFactor ) {
		var ignoreSelfCollisionPredicate = Spatial.IgnoreSelfCollisionPredicate ( rigidbody2D );
		// TODO: consider using CircleCollider2D trigger. I don't know how much faster trigger will be.

		var contactsByBody = Spatial.GetCircleContactsByBody (
			Origin, Radius, 0, Radius,
			hit => hit.collider.attachedRigidbody == null || ignoreSelfCollisionPredicate ( hit ),
			NumSweepDirections,
			LayerMask
		);

#if UNITY_EDITOR
		lastFrameContacts = contactsByBody;
#endif

		foreach ( var bodyAndContacts in contactsByBody ) {
			var body = bodyAndContacts.Key;
			var bodyContacts = bodyAndContacts.Value;

			float influence = ForceReceiver.GetInfluence ( body.gameObject, Kind );

			if ( influence == 0 )
				continue;

			float forceAmount = PhysicsHelper.ConvertToNewtons ( Amount, Mode, body.gameObject );
			forceAmount *= influence * timeFactor * AnimatedMultiplier;

			if ( DecreaseWithDistance ) {
				var contactForceFactors = new ContactForceFactor [bodyContacts.Count];
				float maxDistanceFactor = float.NegativeInfinity;
				float distanceFactorSum = 0;
				
				for ( int i = 0 ; i < bodyContacts.Count ; i++ ) {
					var contact = bodyContacts [i];
					float distanceFactor = 1 - contact.Distance / Radius;

					if ( distanceFactor > maxDistanceFactor )
						maxDistanceFactor = distanceFactor;

					distanceFactorSum += distanceFactor;
					var cff = new ContactForceFactor ( contact, distanceFactor );
					contactForceFactors [i] = cff;
				}

				forceAmount *= maxDistanceFactor;

				/* Sum of all contact factors multiplied
				 * by following factorScale will be 1. */
				float factorScale = 1 / distanceFactorSum;

				foreach ( var cff in contactForceFactors ) {
					var contact = cff.Contact;
					float distanceFactor = cff.Factor * factorScale;
					float contactForceAmount = forceAmount * distanceFactor;
					var forceVector = contact.Direction * contactForceAmount;

					if ( SendForceMessage ( body.gameObject, forceVector, contact.Point ) )
						body.AddForceAtPosition ( forceVector, contact.Point );
				}
			} else {
				forceAmount /= bodyContacts.Count;

				foreach ( var contact in bodyContacts ) {
					var forceVector = contact.Direction * forceAmount;

					if ( SendForceMessage ( body.gameObject, forceVector, contact.Point ) )
						body.AddForceAtPosition ( forceVector, contact.Point );
				}
			}
		}
	}

	/* TODO: make ParticleSystemForceReceiver update itself.
	 * Query all force fields in FixedUpdate method of ParticleSystemForceReceiver,
	 * apply forces and call FlushParticles (). This way we will flush particles only once. */
	private void ApplyToParticles ( float timeFactor ) {
		var origin = Origin;
		float radiusSq = Radius * Radius;
		var susceptibleReceivers = ParticleSystemForceReceiver.GetSusceptibleReceivers ( Kind );

		foreach ( var receiverInfo in susceptibleReceivers ) {
			var receiver = receiverInfo.ForceReceiver;
			var bounds = receiver.Bounds;

			if ( bounds.SqrDistance ( origin ) <= radiusSq ) {
				float influence = receiverInfo.Influence;
				float velocityChangeAmount = PhysicsHelper.ConvertToVelocityChange ( Amount, Mode, receiver.ParticleMass );
				velocityChangeAmount *= influence * timeFactor * AnimatedMultiplier;
				int count;
				var particles = receiver.ReadParticles ( out count );
				bool changed = false;
				
				for ( int i = 0 ; i < count ; i++ ) {
					var particle = particles [i];
					var distanceVector = ( Vector2 ) particle.position - origin;
					float distance = distanceVector.magnitude;

					if ( distance < Radius ) {
						float distanceFactor = DecreaseWithDistance ? 1 - distance / Radius : 1;
						float particleVelocityChangeAmount = velocityChangeAmount * distanceFactor;
						var direction = distanceVector.normalized;
						var velocityChange = direction * particleVelocityChangeAmount;
						particle.velocity += ( Vector3 ) velocityChange;

						particles [i] = particle;
						changed = true;
					}
				}

				if ( changed )
					receiver.FlushParticles ();
			}
		}
	}

	void OnLifetimeRequest ( LifetimeRequest request ) {
		request.MinLifetime = Mathf.Max ( 1, Duration ) - TimeElapsed;
	}

	private class ContactForceFactor {
		public SurfaceContact Contact;
		public float Factor;

		public ContactForceFactor ( SurfaceContact contact, float factor ) {
			this.Contact = contact;
			this.Factor = factor;
		}
	}

	private static readonly Color CircleColor = new Color ( 1, 0.5f, 0.25f );
	private static readonly Color CenterPointColor = new Color ( 1, 0.5f, 0.25f );

	void OnDrawGizmos () {
		if ( TimeElapsed >= Duration )
			return;

#if UNITY_EDITOR
		// This class have associated editor script. Don't interfere with it.
		if ( UnityEditor.Selection.activeObject == gameObject )
			return;
#endif
		
		DebugHelper.UseGizmos = true;
		DebugHelper.DrawCircle ( Origin, Radius, CircleColor );
		Gizmos.color = CenterPointColor;
		Gizmos.DrawSphere ( Origin, 0.05f );

#if UNITY_EDITOR
		var contacts = lastFrameContacts.SelectMany ( bodyAndContacts => bodyAndContacts.Value );

		foreach ( var contact in contacts ) {
			contact.DebugDraw ();
		}
#endif

		DebugHelper.UseGizmos = false;
	}
}
