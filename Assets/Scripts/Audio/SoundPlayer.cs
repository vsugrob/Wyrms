using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class SoundPlayer : MonoBehaviour {
	[Tooltip (
		"Preconfigured AudioSource that will be used by PlaySpecificClip () method" +
		" in order to play audio clips which has no matching AudioSource in hierarchy."
	)]
	public AudioSource DefaultSource;

	void Start () {
#if UNITY_EDITOR
		if ( !Application.isPlaying && DefaultSource == null ) {
			var sources = GetComponents <AudioSource> ()
				.Where ( src => src.clip == null );

			if ( sources.Any () )
				DefaultSource = sources.First ();
		}
#endif
	}

	// TODO: remove. Unused now.
	void PlayFirstFound () {
		var sources = GetComponentsInChildren <AudioSource> ();

		if ( sources.Any () ) {
			var audioSource = sources.First ();
			audioSource.Play ();
		}
	}

	public void PlaySpecificClip ( AudioClip clip, bool restart = true, bool loop = false, float volume = float.NaN ) {
		if ( clip == null )
			return;

		// TODO: cache AudioSources corresponding to specific AudioClips.
		var sources = GetComponentsInChildren <AudioSource> ();
		var clipSources = sources.Where ( src => src.clip == clip );
		AudioSource audioSource;

		if ( clipSources.Any () )
			audioSource = clipSources.First ();
		else {
			if ( DefaultSource == null ) {
				DefaultSource = gameObject.AddComponent <AudioSource> ();
				DefaultSource.playOnAwake = false;
			}

			audioSource = DefaultSource;

			if ( audioSource.clip == null || !audioSource.isPlaying || ( restart && audioSource.clip != clip ) )
				audioSource.clip = clip;
		}

		audioSource.loop = loop;

		if ( restart || !audioSource.isPlaying ) {
			if ( !float.IsNaN ( volume ) )
				audioSource.volume = volume;

			audioSource.Play ();
		}
	}

	public void PlaySpecificClipOneShot ( AudioClip clip ) {
		PlaySpecificClip ( clip, restart : true, loop : false );
	}

	public void PlayVariation ( AudioClip [] variations, bool restart = true, bool loop = false, float volume = float.NaN ) {
		if ( variations == null || variations.Length == 0 )
			return;

		var clip = variations [Random.Range ( 0, variations.Length )];
		PlaySpecificClip ( clip, restart, loop, volume );
	}
}
