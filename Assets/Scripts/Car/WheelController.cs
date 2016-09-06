using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WheelController : MonoBehaviour {
	private WheelJoint2D wheelJoint;
	public float MaxSpeed = 360 * 1;
	public float MaxMotorTorque = 10;
	[HideInInspector]
	public float DesiredMotorTorque;

	void Awake () {
		wheelJoint = GetComponent <WheelJoint2D> ();
	}
	
	void FixedUpdate () {
		float absDesiredTorque = Mathf.Clamp01 ( Mathf.Abs ( DesiredMotorTorque ) );
		float sign = Mathf.Sign ( DesiredMotorTorque );
		
		wheelJoint.motor = new JointMotor2D () {
			maxMotorTorque = MaxMotorTorque * absDesiredTorque,
			motorSpeed = MaxSpeed * sign
		};
	}
}
