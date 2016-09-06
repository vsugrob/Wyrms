using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ForceMaterial : ScriptableObject {
	public float Gravity;
	public float Magnetism;
	public float Shockwave;
	public float Wind;
	public float Love;

	public ForceMaterial (
		float gravity = 1,
		float magnetism = 0,
		float shockwave = 1,
		float wind = 0,
		float love = 1
	) {
		this.Gravity = gravity;
		this.Magnetism = magnetism;
		this.Shockwave = shockwave;
		this.Wind = wind;
		this.Love = love;
	}

	public float GetInfluence ( ForceKind forceKind ) {
		switch ( forceKind ) {
		case ForceKind.Gravity: return	Gravity;
		case ForceKind.Magnetism: return	Magnetism;
		case ForceKind.Shockwave: return	Shockwave;
		case ForceKind.Wind: return	Wind;
		case ForceKind.Love: return	Love;
		default:
			throw new System.ArgumentOutOfRangeException ( "forceKind" );
		}
	}

	public ForceMaterial Clone () {
		return	new ForceMaterial (
			Gravity, Magnetism, Shockwave, Wind, Love
		);
	}
}
