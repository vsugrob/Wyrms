using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AddInstantForce : MonoBehaviour {
	public float Amount = 100;
	public ForceMode Mode = ForceMode.Force;
	public Vector2 Direction = Vector2.right;
	public Transform DirectionSource;
	public float TorqueAmount = 0;

	void Start () {
		if ( rigidbody2D != null ) {
			if ( DirectionSource != null )
				Direction = DirectionSource.position - transform.position;
			
			var force = Direction.normalized * Amount;
			rigidbody2D.AddForce ( force, Mode );
			rigidbody2D.AddTorque ( TorqueAmount, ForceMode2D.Force );
		}
	}
}
