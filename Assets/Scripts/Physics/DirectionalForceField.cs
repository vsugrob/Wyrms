using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DirectionalForceField : ForceFieldBase {
	public Vector2 Direction = Vector2.right;

	void FixedUpdate () {
		ApplyToBodies ();
		ApplyToParticles ();
	}

	private void ApplyToBodies () {
		var susceptibleBodies = GetSusceptibleBodies ( Kind );

		foreach ( var bodyInfo in susceptibleBodies ) {
			var body = bodyInfo.Rigidbody2D;
			float forceAmount = PhysicsHelper.ConvertToNewtons ( Amount, Mode, body.gameObject );
			var force = Direction * forceAmount * bodyInfo.Influence;
			
			if ( SendForceMessage ( body.gameObject, force, body.transform.position ) )
				body.AddForce ( force, ForceMode2D.Force );
		}
	}

	private static IEnumerable <SusceptibleRigidbody2D> GetSusceptibleBodies ( ForceKind kind ) {
		// TODO: cache 'em, cache em all!
		var bodies = GameObject.FindObjectsOfType <Rigidbody2D> ();

		foreach ( var body in bodies ) {
			var material = ForceReceiver.GetMaterial ( body.gameObject );
			float influence = material.GetInfluence ( kind );

			if ( influence == 0 )
				continue;

			yield return	new SusceptibleRigidbody2D ( body, influence, material );
		}
	}

	private class SusceptibleRigidbody2D {
		public Rigidbody2D Rigidbody2D;
		public float Influence;
		public ForceMaterial Material;

		public SusceptibleRigidbody2D ( Rigidbody2D rigidbody2D, float influence, ForceMaterial material ) {
			this.Rigidbody2D = rigidbody2D;
			this.Influence = influence;
			this.Material = material;
		}
	}

	/* TODO: make ParticleSystemForceReceiver update itself.
	 * Query all force fields in FixedUpdate method of ParticleSystemForceReceiver,
	 * apply forces and call FlushParticles (). This way we will flush particles only once. */
	private void ApplyToParticles () {
		var susceptibleReceivers = ParticleSystemForceReceiver.GetSusceptibleReceivers ( Kind );

		foreach ( var receiverInfo in susceptibleReceivers ) {
			var receiver = receiverInfo.ForceReceiver;
			float velocityChangeAmount = PhysicsHelper.ConvertToVelocityChange ( Amount, Mode, receiver.ParticleMass );
			var velocityChange = Direction * velocityChangeAmount * receiverInfo.Influence;
			int count;
			var particles = receiver.ReadParticles ( out count );
			
			for ( int i = 0 ; i < count ; i++ ) {
				particles [i].velocity += ( Vector3 ) velocityChange;
			}

			receiver.FlushParticles ();
		}
	}
}
