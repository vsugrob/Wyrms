using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
[RequireComponent ( typeof ( SliderJoint2D ) )]
public class LineFollower : MonoBehaviour {
	public const float TargetDistanceThr = 0.05f;
	const string OnTargetPointReachedMessageName = "OnTargetPointReached";

	[SerializeField, HideInInspector]
	private Vector2 startPoint;
	[SerializeField, HideInInspector]
	private Vector2 endPoint;

	[ExposeProperty]
	public Vector2 StartPoint {
		get { return	startPoint; }
		set {
			startPoint = value;
			UpdateAngleAndLimits ();
		}
	}

	[ExposeProperty]
	public Vector2 EndPoint {
		get { return	endPoint; }
		set {
			endPoint = value;
			UpdateAngleAndLimits ();
		}
	}

	[ExposeProperty]
	public bool UseMotor {
		get { return	SliderJoint.useMotor; }
		set { SliderJoint.useMotor = value; }
	}

	[SerializeField, HideInInspector]
	private float motorSpeed = 50;

	[ExposeProperty]
	public float MotorSpeed {
		get { return	isReversed ? motorSpeed : -motorSpeed; }
		set {
			bool prevIsReversed = isReversed;

			if ( value < 0 )
				isReversed = true;
			else {
				/* We want motorSpeed to be always negative, so that
				 * the platform always move towards the limits.max gizmo. */
				value = -value;
				isReversed = false;
			}

			motorSpeed = value;

			if ( isReversed != prevIsReversed )
				UpdateAngleAndLimits ();
		}
	}

	[SerializeField, HideInInspector]
	private bool isReversed = false;

	[ExposeProperty]
	public float MaximumMotorForce {
		get { return	SliderJoint.motor.maxMotorTorque; }
		set {
			SliderJoint.motor = new JointMotor2D () {
				motorSpeed = SliderJoint.motor.motorSpeed,
				maxMotorTorque = value
			};
		}
	}

	public float SlowdownWhenDistanceBelow = 0.5f;
	public float SlowdownFactor = 0.5f;

	[ExposeProperty]
	public bool UseLimits {
		get { return	SliderJoint.useLimits; }
		set { SliderJoint.useLimits = value; }
	}

	/* ExecuteInEditMode requires more GetComponent queries
	 * than just one in Awake method. */
	private SliderJoint2D sliderJoint;
	public SliderJoint2D SliderJoint {
		get {
			if ( sliderJoint == null )
				sliderJoint = GetComponent <SliderJoint2D> ();

			return	sliderJoint;
		}
	}

	public Vector2 TargetPoint {
		get {
			if ( !isReversed )
				return	endPoint;
			else
				return	startPoint;
		}
	}

	void Awake () {
		SliderJoint.useMotor = true;
		/* Set to true, otherwise rigidbody attached to this game object
		 * not colliding with majority of the world. I don't know what's
		 * the reason behind this behaviour. */
		SliderJoint.collideConnected = true;
	}

	void Start () {
		UpdateAngleAndLimits ();
	}

	void Update () {
		if ( !Application.isPlaying )
			UpdateAngleAndLimits ();
	}

	void FixedUpdate () {
		if ( !SliderJoint.enabled )
			return;

		var curPos = ( Vector2 ) transform.position;
		float distToTarget = Vector2.Distance ( curPos, TargetPoint );

		if ( distToTarget <= TargetDistanceThr )
			SendMessage ( OnTargetPointReachedMessageName, SendMessageOptions.DontRequireReceiver );

		SetupSliderSpeed ();
	}

	private void SetupSliderSpeed () {
		if ( !UseMotor )
			return;

		var curPos = ( Vector2 ) transform.position;
		float distToStart = Vector2.Distance ( curPos, startPoint );
		float distToEnd = Vector2.Distance ( curPos, endPoint );
		float minDist = Mathf.Min ( distToStart, distToEnd );
		float speed;

		if ( minDist < SlowdownWhenDistanceBelow ) {
			float t = minDist / SlowdownWhenDistanceBelow;
			speed = Mathf.SmoothStep ( SlowdownFactor * motorSpeed, motorSpeed, t );
		} else
			speed = motorSpeed;

		if ( SliderJoint.motor.motorSpeed != speed ) {
			SliderJoint.motor = new JointMotor2D () {
				motorSpeed = speed,
				maxMotorTorque = SliderJoint.motor.maxMotorTorque
			};
		}
	}

	private Vector2 prevSrcPoint, prevDstPoint;
	private bool prevUseLimits;

	private void UpdateAngleAndLimits () {
		Vector2 dstPoint, srcPoint;

		if ( !isReversed ) {
			dstPoint = endPoint;
			srcPoint = startPoint;
		} else {
			dstPoint = startPoint;
			srcPoint = endPoint;
		}

		if ( !Application.isPlaying &&
			 prevSrcPoint == srcPoint && prevDstPoint == dstPoint &&
			 prevUseLimits == UseLimits
		) {
			return;
		} else {
			prevSrcPoint = srcPoint;
			prevDstPoint = dstPoint;
			prevUseLimits = UseLimits;
		}

		var curPos = ( Vector2 ) transform.position;
		var moveVector = dstPoint - curPos;
		var moveVectorLocal = transform.InverseTransformDirection ( moveVector );
		SliderJoint.angle = Common.SignedAngle ( Vector2.right, moveVectorLocal );

		var lineVector = dstPoint - srcPoint;
		var dir = moveVector.normalized;
		float projLen = Vector2.Dot ( lineVector, dir );
		float halfLen = projLen * 0.5f;
		SliderJoint.connectedAnchor = dstPoint - dir * halfLen;

		if ( UseLimits ) {
			SliderJoint.limits = new JointTranslationLimits2D () {
				max = halfLen,
				min = -halfLen
			};
		}
	}

	void OnDisable () {
		SliderJoint.enabled = false;
	}

	void OnEnable () {
		SliderJoint.enabled = true;
	}
}
