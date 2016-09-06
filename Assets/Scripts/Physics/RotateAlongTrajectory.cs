using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class RotateAlongTrajectory : MonoBehaviour {
	public Transform TransformToRotate;
	public UpdateHandlerMethod UpdateHandler;

	void Start () {
#if UNITY_EDITOR
		if ( !Application.isPlaying && TransformToRotate == null )
			TransformToRotate = transform;
#endif
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

		if ( TransformToRotate != null && rigidbody2D != null ) {
			// TODO: implement heading-dependent rotation like it is implemented in CharacterMovement.LateUpdate ().
			TransformToRotate.rotation = Common.RotateAlongDirection ( rigidbody2D.velocity );
			/* Statement above makes rotation of VisualGameObject non-deterministic
			 * from the fixed update routine point of view. Do not forget about this! */
		}
	}
}
