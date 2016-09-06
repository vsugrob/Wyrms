using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public static class RefactorMoveFieldValue {
	[MenuItem ( "Refactor/Move Field Value" )]
	public static void MoveFieldValue () {
		/* TODO: make RefactorMoveFieldValueWindow.
		 * Allow user to input class name, source field name, destination field name.
		 * Display button "Move Field Value". */

		// Let this code be here as example.
		{
			//var objects = Resources.FindObjectsOfTypeAll <CameraEvent> ();
			//var objectsToModify = objects.Where ( o => o.EventName != o.EventName ).ToArray ();
			//Undo.RecordObjects ( objectsToModify, "Move field value" );

			//foreach ( var obj in objectsToModify ) {
			//	obj.EventName = obj.EventName;
			//	EditorUtility.SetDirty ( obj );
			//}
		}

		var objects = Resources.FindObjectsOfTypeAll <MutableSpriteCollider> ();
		var objectsToModify = objects.ToArray ();
		Undo.RecordObjects ( objectsToModify, "Move field value" );

		foreach ( var obj in objectsToModify ) {
			obj.PolyColliderReduction.ReduceByMinDistance = obj.EdgeColliderReduction.ReduceByMinDistance;
			obj.PolyColliderReduction.MinDistance = obj.EdgeColliderReduction.MinDistance;
			obj.PolyColliderReduction.ReduceByMinTriangleArea = obj.EdgeColliderReduction.ReduceByMinTriangleArea;
			obj.PolyColliderReduction.MinTriangleArea = obj.EdgeColliderReduction.MinTriangleArea;
			obj.PolyColliderReduction.ReduceCodirected = obj.EdgeColliderReduction.ReduceCodirected;
			obj.PolyColliderReduction.MinAngle = obj.EdgeColliderReduction.MinAngle;
			obj.PolyColliderReduction.MinVertexCount = obj.EdgeColliderReduction.MinVertexCount;

			EditorUtility.SetDirty ( obj );
		}
	}
}
