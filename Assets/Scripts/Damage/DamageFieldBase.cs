using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class DamageFieldBase : MonoBehaviour {
	public DamageKind Kind = DamageKind.Shockwave;
	public float Amount;

	protected bool SendDamageMessage ( GameObject gameObject, float damage, List <SurfaceContact> contacts ) {
		var data = new DamageMessageData ( this, damage, Kind, contacts );
		gameObject.SendMessage ( DamageMessageData.MessageName, data, SendMessageOptions.DontRequireReceiver );
		
		return	data.ApplyDamageEffect;
	}
}

public class DamageMessageData {
	public const string MessageName = "OnDamage";
	public Object Inflictor;
	public float Damage;
	public DamageKind Kind;
	public List <SurfaceContact> Contacts;
	public bool ApplyDamageEffect = true;
	
	public DamageMessageData ( Object inflictor, float damage, DamageKind kind, List <SurfaceContact> contacts ) {
		this.Inflictor = inflictor;
		this.Damage = damage;
		this.Kind = kind;
		this.Contacts = contacts;
	}
}
