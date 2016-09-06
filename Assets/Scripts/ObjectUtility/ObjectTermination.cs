using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ObjectTermination {
	public bool TerminationEnabled = true;
	public bool DisableColliders = true;
	public bool StopSimulatingRigidbody = true;
	public bool StopParticleEmission = true;
	public bool DisableCameraEvents = true;
	public bool StopSounds = true;
	public float DestroyDelay = 5;

	public void Terminate ( GameObject gameObject ) {
		if ( !TerminationEnabled )
			return;

		if ( DisableColliders ) {
			var colliders = gameObject.GetComponentsInChildren <Collider2D> ();

			foreach ( var collider in colliders ) {
				collider.enabled = false;
			}
		}

		if ( StopSimulatingRigidbody ) {
			var bodies = gameObject.GetComponentsInChildren <Rigidbody2D> ();

			foreach ( var body in bodies ) {
				body.simulated = false;
			}
		}

		if ( StopParticleEmission ) {
			var particleSystems = gameObject.GetComponentsInChildren <ParticleSystem> ();

			foreach ( var ps in particleSystems ) {
				ps.enableEmission = false;
			}
		}

		if ( DisableCameraEvents ) {
			var events = gameObject.GetComponentsInChildren <CameraEvent> ();

			foreach ( var ev in events ) {
				ev.enabled = false;
			}
		}

		if ( StopSounds ) {
			var audioSources = gameObject.GetComponentsInChildren <AudioSource> ();

			foreach ( var src in audioSources ) {
				src.Stop ();
			}
		}

		Object.Destroy ( gameObject, DestroyDelay );
	}
}
