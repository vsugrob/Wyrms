using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectFadeout : MonoBehaviour {
	public float FadeoutDuration = 1.5f;
	public bool FadeWhenIdle = true;
	public bool FadeOnDetonation = false;
	public bool FadeWhenTimerExpired = true;
	public float Timer = 10;
	public bool FadeOpacity = true;
	public RendererOpacityFadeout OpacityFadeout = new RendererOpacityFadeout ();
	public bool FadeParticleSystemEmissionRate = true;
	public bool FadeAudioSourceVolume = true;
	public ObjectTermination TerminationSettings = new ObjectTermination ();

	private float startTimestamp;
	public bool IsFadingOut { get; private set; }
	public bool IsFadedOut { get; private set; }
	private float fadeoutStartTimestamp;
	private Dictionary <Renderer, float> rendererAlphas = new Dictionary <Renderer, float> ();
	private Dictionary <ParticleSystem, float> particleSystemRates = new Dictionary <ParticleSystem, float> ();
	private Dictionary <AudioSource, float> audioSourceVolumes = new Dictionary <AudioSource, float> ();

	void Start () {
		startTimestamp = Time.fixedTime;
	}

	void FixedUpdate () {
		if ( IsFadedOut )
			return;

		if ( IsFadingOut ) {
			float fadeoutTimeElapsed = Time.fixedTime - fadeoutStartTimestamp;

			if ( fadeoutTimeElapsed > FadeoutDuration )
				EndFadeout ();
			else
				ProcessFadeout ( fadeoutTimeElapsed );
		} else if ( FadeWhenTimerExpired ) {
			float timeElapsed = Time.fixedTime - startTimestamp;

			if ( timeElapsed >= Timer )
				BeginFadeout ();
		}
	}

	void OnBecameIdle () {
		if ( enabled && FadeWhenIdle )
			BeginFadeout ();
	}

	void OnDetonate ( DetonateMessageData messageData ) {
		if ( enabled && FadeOnDetonation )
			BeginFadeout ();
	}

	private void BeginFadeout () {
		if ( IsFadingOut || IsFadedOut )
			return;

		if ( FadeOpacity ) {
			var renderers = GetComponentsInChildren <Renderer> ();

			foreach ( var renderer in renderers ) {
				if ( OpacityFadeout.IsAffected ( renderer ) ) {
					rendererAlphas [renderer] = renderer.sharedMaterial.color.a;
				}
			}
		}

		if ( FadeParticleSystemEmissionRate ) {
			var particleSystems = GetComponentsInChildren <ParticleSystem> ();

			foreach ( var particleSystem in particleSystems ) {
				particleSystemRates [particleSystem] = particleSystem.emissionRate;
			}
		}

		if ( FadeAudioSourceVolume ) {
			var audioSources = GetComponentsInChildren <AudioSource> ();

			foreach ( var audioSource in audioSources ) {
				audioSourceVolumes [audioSource] = audioSource.volume;
			}
		}

		IsFadingOut = true;
		fadeoutStartTimestamp = Time.fixedTime;
	}

	private void ProcessFadeout ( float fadeoutTimeElapsed ) {
		float t = fadeoutTimeElapsed / FadeoutDuration;

		if ( FadeOpacity ) {
			foreach ( var kv in rendererAlphas ) {
				float startAlpha = kv.Value;
				float curAlpha = Mathf.SmoothStep ( startAlpha, 0, t );
				var renderer = kv.Key;
				var material = renderer.material;
				var color = material.color;
				material.color = new Color (
					color.r, color.g, color.b,
					curAlpha
				);
				renderer.material = material;
			}
		}

		if ( FadeParticleSystemEmissionRate ) {
			foreach ( var kv in particleSystemRates ) {
				float startRate = kv.Value;
				float curRate = Mathf.SmoothStep ( startRate, 0, t );
				var particleSystem = kv.Key;
				particleSystem.emissionRate = curRate;
			}
		}

		if ( FadeAudioSourceVolume ) {
			foreach ( var kv in audioSourceVolumes ) {
				float startVolume = kv.Value;
				float curVolume = Mathf.SmoothStep ( startVolume, 0, t );
				var audioSource = kv.Key;
				audioSource.volume = curVolume;
			}
		}
	}

	private void EndFadeout () {
		TerminationSettings.Terminate ( gameObject );
		IsFadingOut = false;
		IsFadedOut = true;
	}

	[System.Serializable]
	public class RendererOpacityFadeout {
		public bool Sprite = true;
		public bool Mesh = true;
		public bool ParticleSystem = false;
		public bool Line = true;
		public bool Trail = true;

		public bool IsAffected ( Renderer renderer ) {
			return	( renderer is SpriteRenderer && Sprite ) ||
				 ( renderer is MeshRenderer && Mesh ) ||
				 ( renderer is ParticleSystemRenderer && ParticleSystem ) ||
				 ( renderer is LineRenderer && Line ) ||
				 ( renderer is TrailRenderer && Trail );
		}
	}
}
