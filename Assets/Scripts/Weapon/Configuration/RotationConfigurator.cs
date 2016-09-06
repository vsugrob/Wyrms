using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RotationConfigurator : ConfiguratorBase {
	public string ParameterName = "Angle";
	public bool Persist = false;
	public float Angle = 0;
	public float RotationSpeed = 90;
	public float MinAngle = -90;
	public float MaxAngle = 90;
	public Transform Target;

	[HideInInspector]
	public float RotationInput;

	private float TargetAngle {
		get {
			if ( Target == null )
				return	0;

			float angle = Target.localEulerAngles.z;

			if ( angle > 180 )
				angle -= 360;

			return	angle;
		}
	}

	protected override void Start () {
		base.Start ();

#if UNITY_EDITOR
		if ( Application.isEditor && Target == null )
			Target = transform;
#endif

		configuration.RestoreFloat ( ParameterName, ref Angle );
	}

	void FixedUpdate () {
		if ( !IsInActiveState )
			return;

		if ( Target == null )
			return;

		if ( RotationInput != 0 || Angle != TargetAngle ) {
			float rotationAmount = Mathf.Clamp ( RotationInput, -1, 1 );
			var euler = Target.localEulerAngles;

			if ( Angle > 180 )
				Angle -= 360;

			Angle += rotationAmount * RotationSpeed * Time.fixedDeltaTime;
			Angle = Mathf.Clamp ( Angle, MinAngle, MaxAngle );
			Target.localEulerAngles = new Vector3 ( euler.x, euler.y, Angle );

			StoreParameter ( ParameterName, Angle, Persist );
		}
	}
}
