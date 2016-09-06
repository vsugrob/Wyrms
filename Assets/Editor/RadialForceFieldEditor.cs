using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor ( typeof ( RadialForceField ) )]
public class RadialForceFieldEditor : Editor {
	void OnSceneGUI () {
		Undo.RecordObject ( target, "Force field Change" );

		Handles.color = new Color ( 1, 0.5f, 0.25f );
		var forceField = target as RadialForceField;
		forceField.Radius = Handles.RadiusHandle (
			Quaternion.identity,
			forceField.Origin,
			forceField.Radius
		);

		var style = new GUIStyle ();
		style.richText = true;
		Handles.Label (
			forceField.transform.position + Vector3.right * forceField.Radius,
			"<color=#" + Handles.color.ToHex () + ">Force Field Radius</color>",
			style
		);

		if ( GUI.changed )
			EditorUtility.SetDirty ( target );
	}
}
