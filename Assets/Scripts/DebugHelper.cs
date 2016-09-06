using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class DebugHelper {
	public static bool UseGizmos = false;

	public static void DrawCircle (
		Vector3 center, float radius, Color color,
		Quaternion rotation,
		float duration = 0, bool depthTest = false,
		int numSegments = 50
	) {
		float dAngle = Mathf.PI * 2 / numSegments;
		var prevPoint = rotation * ( Vector3.right * radius ) + center;
		var firstPoint = prevPoint;
		Vector3 curPoint = firstPoint;
		float angle = dAngle;

		for ( int i = 1 ; i < numSegments ; i++, angle += dAngle ) {
			curPoint = rotation *
				( new Vector3 ( Mathf.Cos ( angle ), Mathf.Sin ( angle ), 0 ) * radius )
				+ center;
			
			DrawLine ( prevPoint, curPoint, color, duration, depthTest );
			prevPoint = curPoint;
		}

		DrawLine ( curPoint, firstPoint, color, duration, depthTest );
	}

	public static void DrawCircle (
		Vector3 center, float radius, Color color,
		int numSegments = 50
	) {
		DrawCircle ( center, radius, color, Quaternion.identity, 0, false, numSegments );
	}

	public static void DrawLine ( Vector3 start, Vector3 end, Color color, float duration = 0, bool depthTest = false ) {
		if ( UseGizmos ) {
			Gizmos.color = color;
			Gizmos.DrawLine ( start, end );
		} else
			Debug.DrawLine ( start, end, color, duration, depthTest );
	}

	public static void DrawRay ( Vector3 start, Vector3 dir, Color color, float duration = 0, bool depthTest = false ) {
		if ( UseGizmos ) {
			Gizmos.color = color;
			Gizmos.DrawRay ( start, dir );
		} else
			Debug.DrawRay ( start, dir, color, duration, depthTest );
	}

	private const float WingLenth = 0.25f;
	private const float WingAngle = 22.5f;

	public static void DrawArrow (
		Vector3 start, Vector3 end, Color color,
		float duration = 0, bool depthTest = false
	) {
		DrawLine ( start, end, color, duration, depthTest );

		var mainDir = ( end - start ).normalized * WingLenth;
		var wingDir = Quaternion.Euler ( 0, 0, WingAngle ) * mainDir;
		DrawRay ( end, -wingDir, color, duration, depthTest );

		wingDir = Quaternion.Euler ( 0, 0, -WingAngle ) * mainDir;
		DrawRay ( end, -wingDir, color, duration, depthTest );
	}
}
