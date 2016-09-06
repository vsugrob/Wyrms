using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor ( typeof ( RadialDamageField ) )]
public class RadialDamageFieldEditor : Editor {
	void OnSceneGUI () {
		Undo.RecordObject ( target, "Damage field Change" );

		Handles.color = Color.red;
		var damageField = target as RadialDamageField;
		damageField.Radius = Handles.RadiusHandle (
			Quaternion.identity,
			damageField.Origin,
			damageField.Radius
		);

		var style = new GUIStyle ();
		style.richText = true;
		
		Handles.Label (
			damageField.transform.position + Vector3.up * damageField.Radius,
			"<color=#" + Handles.color.ToHex () + ">Damage Field Radius</color>",
			style
		);

		if ( GUI.changed )
			EditorUtility.SetDirty ( target );
	}
}
