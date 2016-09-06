using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EventDrivenCamera : MonoBehaviour {
	private static readonly Vector2 viewportCenter = new Vector2 ( 0.5f, 0.5f );
	private CameraEvent curEvent = null;

	void LateUpdate () {
		var events = CameraEvent.MostNotableEvents;

		if ( events.Any () ) {
			var camPos = transform.position;
			curEvent = events.WithMin ( ev => Common.DistanceSq ( ev.transform.position, camPos ) );

			if ( curEvent.GiveControlToNewest )
				curEvent = events.WithMax ( ev => ev.EnabledTimestamp );
		}

		if ( curEvent != null ) {
			var eventPos = curEvent.transform.position;

			if ( curEvent.FocusKind == CameraFocusKind.BringIntoView ) {
				var vpPos = ( Vector2 ) camera.WorldToViewportPoint ( eventPos );
				var vToPos = vpPos - viewportCenter;
				var distance = vToPos.magnitude;

				if ( distance > curEvent.ViewportRadius ) {
					var centerPos = vpPos - vToPos.normalized * curEvent.ViewportRadius;
					centerPos = camera.ViewportToWorldPoint ( centerPos );
					CenterView ( centerPos, curEvent.TransitionHardness );
				}
			} else if ( curEvent.FocusKind == CameraFocusKind.CenterView )
				CenterView ( eventPos, curEvent.TransitionHardness );
		}
	}

	private void CenterView ( Vector2 targetPos, float transitionHardness = 0.05f ) {
		const float ReferenceDeltaTime = 1f / 80;
		float deltaTimeRatio = Time.deltaTime / ReferenceDeltaTime;

		var camTf = camera.transform;
		var pos = Vector2.Lerp (
			camTf.position,
			targetPos,
			transitionHardness * deltaTimeRatio
		);

		camTf.position = new Vector3 ( pos.x, pos.y, camTf.position.z );
	}
}
