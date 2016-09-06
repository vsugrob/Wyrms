using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent ( typeof ( ParticleSystem ) )]
public class ParticleSystemForceReceiver : MonoBehaviour {
	public ForceMaterial Material;
	public float ParticleMass = 0.01f;

	public static IEnumerable <SusceptibleParticleSystem> GetSusceptibleReceivers ( ForceKind kind ) {
		// TODO: cache 'em, cache em all!
		var receivers = GameObject.FindObjectsOfType <ParticleSystemForceReceiver> ();

		foreach ( var receiver in receivers ) {
			if ( receiver.ParticleMass == 0 )
				continue;

			var material = receiver.Material;

			if ( material == null )
				material = DefaultSettings.Singleton.ParticleSystemForceReceiverMaterialOrDefault;

			float influence = material.GetInfluence ( kind );

			if ( influence == 0 )
				continue;

			yield return	new SusceptibleParticleSystem ( receiver, influence, material );
		}
	}

	public class SusceptibleParticleSystem {
		public ParticleSystemForceReceiver ForceReceiver;
		public float Influence;
		public ForceMaterial Material;

		public SusceptibleParticleSystem (
			ParticleSystemForceReceiver forceReceiver, float influence, ForceMaterial material
		) {
			this.ForceReceiver = forceReceiver;
			this.Influence = influence;
			this.Material = material;
		}
	}

	#region Particle Tracking
	private float particlesUpdateTimestamp = float.NegativeInfinity;
	private ParticleSystem.Particle [] particles = new ParticleSystem.Particle [100];

	private float boundsUpdateTimestamp = float.NegativeInfinity;
	private Bounds bounds;

	public Bounds Bounds {
		get {
			if ( Time.fixedTime != boundsUpdateTimestamp ) {
				int count;
				var ps = ReadParticles ( out count );

				if ( count != 0 ) {
					var particle = ps [0];
					bounds = new Bounds ( particle.position, Vector3.zero );

					for ( int i = 1 ; i < count ; i++ ) {
						bounds.Encapsulate ( ps [i].position );
					}
				}

				boundsUpdateTimestamp = Time.fixedTime;
			}

			return	bounds;
		}
	}

	public ParticleSystem.Particle [] ReadParticles ( out int count ) {
		count = particleSystem.particleCount;

		if ( Time.fixedTime != particlesUpdateTimestamp ) {
			if ( count > particles.Length )
				System.Array.Resize ( ref particles, count );

			particleSystem.GetParticles ( particles );
			particlesUpdateTimestamp = Time.fixedTime;
		}

		return	particles;
	}

	public void FlushParticles ( int count = -1 ) {
		if ( count < 0 )
			count = particleSystem.particleCount;

		particleSystem.SetParticles ( particles, count );
	}

	void OnDrawGizmosSelected () {
		var bounds = Bounds;
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube ( bounds.center, bounds.size );
	}
	#endregion Particle Tracking
}
