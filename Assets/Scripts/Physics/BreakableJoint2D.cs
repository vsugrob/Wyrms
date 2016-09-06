using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class BreakableJoint2D : MonoBehaviour {
	public Joint2D Joint;
	public float BreakForce = float.PositiveInfinity;
	public float BreakTorque = float.PositiveInfinity;
	public JointBreakBehaviour BreakBehaviour = JointBreakBehaviour.Disable;

	public float DebugPrint_MinForce = float.PositiveInfinity;

	void Start () {
#if UNITY_EDITOR
		if ( !Application.isPlaying && Joint == null )
			Joint = GetComponent <Joint2D> ();
#endif
	}

	void FixedUpdate () {
		// Destroy itself when monitored joint was destroyed.
		if ( Joint == null ) {
			Destroy ( this );

			return;
		} else if ( !Joint.enabled )
			return;

		Vector2 reactionForce;
		float reactionTorque;
		
		if ( Joint is DistanceJoint2D ) {
			var specificJoint = Joint as DistanceJoint2D;
			reactionForce = specificJoint.GetReactionForce ( Time.fixedDeltaTime );
			reactionTorque = specificJoint.GetReactionTorque ( Time.fixedDeltaTime );
		} else if ( Joint is HingeJoint2D ) {
			var specificJoint = Joint as HingeJoint2D;
			reactionForce = specificJoint.GetReactionForce ( Time.fixedDeltaTime );
			reactionTorque = specificJoint.GetReactionTorque ( Time.fixedDeltaTime );
		} else if ( Joint is SliderJoint2D ) {
			var specificJoint = Joint as SliderJoint2D;
			reactionForce = Vector2.zero;
			reactionTorque = specificJoint.GetMotorForce ( Time.fixedDeltaTime );
		} else if ( Joint is SpringJoint2D ) {
			var specificJoint = Joint as SpringJoint2D;
			reactionForce = specificJoint.GetReactionForce ( Time.fixedDeltaTime );
			reactionTorque = specificJoint.GetReactionTorque ( Time.fixedDeltaTime );
		} else if ( Joint is WheelJoint2D ) {
			var specificJoint = Joint as WheelJoint2D;
			reactionForce = Vector2.zero;
			reactionTorque = specificJoint.GetMotorTorque ( Time.fixedDeltaTime );
		} else {
			Debug.LogWarning ( "Unsupported joint type: " + Joint.GetType ().Name, this );
			Destroy ( this );

			return;
		}

		if ( reactionForce.magnitude >= DebugPrint_MinForce ) {
			print (
				"reactionForce: " + reactionForce.magnitude +
				", reactionTorque: " + reactionTorque
			);
		}

		if ( reactionForce.magnitude > BreakForce || reactionTorque > BreakTorque ) {
			var messageData = new BrokenJointData () {
				BreakableJoint = this,
				Force = reactionForce,
				Torque = reactionTorque
			};

			SendMessage ( BrokenJointData.MessageName, messageData, SendMessageOptions.DontRequireReceiver );

			if ( BreakBehaviour == JointBreakBehaviour.Disable )
				Joint.enabled = false;
			else if ( BreakBehaviour == JointBreakBehaviour.Destroy )
				Destroy ( Joint );
		}
	}
}

public struct BrokenJointData {
	public const string MessageName = "OnJointBreak2D";
	public BreakableJoint2D BreakableJoint;
	public Vector2 Force;
	public float Torque;
}

public enum JointBreakBehaviour {
	Disable,
	Destroy,
	NoAction
}