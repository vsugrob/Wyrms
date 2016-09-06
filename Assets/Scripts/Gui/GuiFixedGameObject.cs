using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GuiFixedGameObject : MonoBehaviour {
	public Vector2 ViewportPosition;
	public Vector2 PanelAnchor;
	public float PixelsPerUnit = 50;

	public Vector2 PanelAnchorWorldPos {
		get { return	transform.TransformPoint ( PanelAnchor ); }
		set { PanelAnchor = transform.InverseTransformPoint ( value ); }
	}
	private float CameraWidthWorld {
		get {
			var mainCamera = Camera.main;

			if ( mainCamera != null ) {
				var brWorld = mainCamera.ViewportToWorldPoint ( Vector3.right );
				var blWorld = mainCamera.ViewportToWorldPoint ( Vector3.zero );
				float cameraWidthWorld = Vector3.Distance ( brWorld, blWorld );

				return	cameraWidthWorld;
			} else
				return	0;
		}
	}

	private float CameraPixelsPerUnit {
		get {
			var mainCamera = Camera.main;

			if ( mainCamera != null )
				return	Camera.main.pixelWidth / CameraWidthWorld;
			else
				return	0;
		}
	}
	
	void LateUpdate () {
		var mainCamera = Camera.main;

		if ( mainCamera == null )
			return;

		float cameraPpu = CameraPixelsPerUnit;
		float scale = PixelsPerUnit / cameraPpu;

		if ( float.IsInfinity ( scale ) || float.IsNaN ( scale ) )
			scale = 1;

		transform.localScale = new Vector3 ( scale, scale, scale );

		var worldPosition = mainCamera.ViewportToWorldPoint ( ViewportPosition );
		var posDelta = ( Vector2 ) worldPosition - PanelAnchorWorldPos;
		transform.position += ( Vector3 ) posDelta;
	}
}
