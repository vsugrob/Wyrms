using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DamageMaterial : ScriptableObject {
	public float Shockwave;
	public float Collision;
	public float Bullet;
	public float Electricity;
	public float Fire;
	public float Cold;
	public float Acid;
	/// <summary>
	/// Heals everything on its way.
	/// </summary>
	public float Love;

	public DamageMaterial (
		float shockwave = 1,
		float collision = 1,
		float bullet = 1,
		float electricity = 1,
		float fire = 1,
		float cold = 1,
		float acid = 1,
		float love = 1
	) {
		this.Shockwave = shockwave;
		this.Collision = collision;
		this.Bullet = bullet;
		this.Electricity = electricity;
		this.Fire = fire;
		this.Cold = cold;
		this.Acid = acid;
		this.Love = love;
	}

	public float GetInfluence ( DamageKind damageKind ) {
		switch ( damageKind ) {
		case DamageKind.Shockwave: return	Shockwave;
		case DamageKind.Collision: return	Collision;
		case DamageKind.Bullet: return	Bullet;
		case DamageKind.Electricity: return	Electricity;
		case DamageKind.Fire: return	Fire;
		case DamageKind.Cold: return	Cold;
		case DamageKind.Acid: return	Acid;
		case DamageKind.Love: return	Love;
		default:
			throw new System.ArgumentOutOfRangeException ( "damageKind" );
		}
	}

	public DamageMaterial Clone () {
		return	new DamageMaterial (
			Shockwave, Bullet, Electricity, Fire, Cold, Acid, Love
		);
	}
}