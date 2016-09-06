using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class ForceFieldBase : MonoBehaviour {
	public ForceKind Kind = ForceKind.Wind;
	public ForceMode Mode = ForceMode.Force;
	public float Amount;

	// TODO: remove? Currently I don't think that any object will need this sort of information.
	protected bool SendForceMessage ( GameObject gameObject, Vector2 force, Vector2 point ) {
		return	true;
		//var data = new ForceMessageData ( this, force, point );
		//gameObject.SendMessage ( ForceMessageData.MessageName, data, SendMessageOptions.DontRequireReceiver );

		//return	data.ApplyForceToRigidbody;
	}
}

public class ForceMessageData {
	public const string MessageName = "OnForceFieldImpact";
	public ForceFieldBase ForceField;
	public Vector2 Force;
	public Vector2 Point;
	public bool ApplyForceToRigidbody = true;

	public ForceMessageData ( ForceFieldBase forceField, Vector2 force, Vector2 point ) {
		this.ForceField = forceField;
		this.Force = force;
		this.Point = point;
	}
}
