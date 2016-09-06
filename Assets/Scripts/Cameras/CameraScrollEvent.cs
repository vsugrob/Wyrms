using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class CameraScrollEvent : MonoBehaviour {
	public CameraEvent ScrollEvent;
	public Transform CameraTransform;
	public float Sensitivity = 1;
	public string ViewScrollXAxis = "View Scroll X";
	public string ViewScrollYAxis = "View Scroll Y";

	public bool Changed { get; set; }

	void Awake () {
		if ( !Application.isPlaying && ScrollEvent == null ) {
			ScrollEvent = gameObject.AddComponent <CameraEvent> ();
			ScrollEvent.Priority = 500;
			ScrollEvent.Duration = 3;
			ScrollEvent.EventName = "Camera Scrolled";
			ScrollEvent.ExclusiveGroupName = "Exclusive Attention";
			ScrollEvent.FocusKind = CameraFocusKind.CenterView;
			ScrollEvent.TransitionHardness = 0.1f;
			ScrollEvent.enabled = false;
		}
	}

	void Update () {
		if ( !Application.isPlaying )
			return;

		if ( Screen.lockCursor ) {
			float axisX = Input.GetAxis ( ViewScrollXAxis );
			float axisY = Input.GetAxis ( ViewScrollYAxis );

			if ( axisX != 0 || axisY != 0 ) {
				var mousePosDelta = new Vector2 ( axisX, axisY );
				mousePosDelta *= Sensitivity;
				transform.position += ( Vector3 ) mousePosDelta;
				ScrollEvent.Restart ();
				Changed = true;
			}

			if ( Input.GetKeyDown ( KeyCode.Escape ) )
				Screen.lockCursor = false;
		} else {
			if ( Input.GetMouseButtonDown ( 0 ) ) {
				Screen.lockCursor = true;
				print ( "Screen.lockCursor: " + Screen.lockCursor );
			}
		}

		if ( !ScrollEvent.enabled )
			transform.position = CameraTransform.position;
	}
}
