using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class DamageReceiver : MonoBehaviour {
	public DamageMaterial Material;

	public static DamageMaterial GetMaterial ( GameObject gameObject ) {
		var receiver = gameObject.GetComponent <DamageReceiver> ();

		if ( receiver == null || receiver.Material == null )
			return	DefaultSettings.Singleton.DamageReceiverMaterialOrDefault;

		return	receiver.Material;
	}

	public static float GetInfluence ( GameObject gameObject, DamageKind damageKind ) {
		var material = GetMaterial ( gameObject );

		return	material.GetInfluence ( damageKind );
	}

	public static void InflictDamage (
		GameObject victim,
		Object inflictor, float damage, DamageKind kind, List <SurfaceContact> contacts = null
	) {
		if ( damage == 0 )
			return;

		float influence = DamageReceiver.GetInfluence ( victim, kind );
		
		if ( influence != 0 ) {
			damage *= influence;

			var damageData = new DamageMessageData (
				inflictor,
				damage,
				DamageKind.Collision,
				contacts
			);

			victim.SendMessage ( DamageMessageData.MessageName, damageData, SendMessageOptions.DontRequireReceiver );
		}
	}
}