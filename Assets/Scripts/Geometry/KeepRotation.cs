using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class KeepRotation : MonoBehaviour {
	public Transform TransformToRotate;
	public bool KeepSpecifiedAngle = false;
	public float AbsoluteAngle = 0;
	public UpdateHandlerMethod UpdateHandler;

	private Quaternion rotationValueToKeep;

	void Start () {
#if UNITY_EDITOR
		if ( !Application.isPlaying && TransformToRotate == null )
			TransformToRotate = transform;
#endif
		rotationValueToKeep = transform.rotation;
	}

	void FixedUpdate () {
		if ( UpdateHandler == UpdateHandlerMethod.FixedUpdate )
			UpdateRotation ();
	}
	
	void LateUpdate () {
		if ( UpdateHandler == UpdateHandlerMethod.LateUpdate )
			UpdateRotation ();
	}

	void Update () {
		if ( UpdateHandler == UpdateHandlerMethod.Update )
			UpdateRotation ();
	}

	private void UpdateRotation () {
		if ( !Application.isPlaying )
			return;

		if ( TransformToRotate != null ) {
			if ( KeepSpecifiedAngle )
				rotationValueToKeep = Quaternion.Euler ( 0, 0, AbsoluteAngle );

			TransformToRotate.rotation = rotationValueToKeep;
		}
	}
}
