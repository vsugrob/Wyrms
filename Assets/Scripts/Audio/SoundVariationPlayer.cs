using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent ( typeof ( SoundPlayer ) )]
public class SoundVariationPlayer : MonoBehaviour {
	public AudioClip [] Sounds;
	private SoundPlayer soundPlayer;

	void Awake () {
		soundPlayer = GetComponentInChildren <SoundPlayer> ();
	}

	void Start () {
		soundPlayer.PlayVariation ( Sounds );
	}
}
