using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent ( typeof ( SoundPlayer ) )]
public class CollisionSound : MonoBehaviour {
	public float MaxVolumeRelativeVelocity = 2;
	public float MinRelativeVelocity = 0.2f;
	public AudioClip [] SoundVariations;

	private SoundPlayer soundPlayer;

	void Start () {}

	void Awake () {
		soundPlayer = GetComponent <SoundPlayer> ();
	}

	void OnCollisionEnter2D ( Collision2D collision ) {
		float velocity = collision.relativeVelocity.magnitude;

		if ( velocity >= MinRelativeVelocity ) {
			float volume = ( velocity - MinRelativeVelocity ) / ( MaxVolumeRelativeVelocity - MinRelativeVelocity );
			volume = Mathf.Clamp01 ( volume );
			soundPlayer.PlayVariation ( SoundVariations, volume : volume );
		}
	}
}
