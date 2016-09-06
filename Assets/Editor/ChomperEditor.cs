using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor ( typeof ( Chomper ) )]
public class ChomperEditor : Editor {
	void OnSceneGUI () {
		Undo.RecordObject ( target, "Chomper change" );

		Handles.color = new Color ( 0.25f, 0.5f, 1 );
		var chomper = target as Chomper;
		chomper.Radius = Handles.RadiusHandle (
			Quaternion.identity,
			chomper.Origin,
			chomper.Radius
		);

		var style = new GUIStyle ();
		style.richText = true;
		Handles.Label (
			chomper.transform.position + Vector3.down * chomper.Radius,
			"<color=#" + Handles.color.ToHex () + ">Chomp Radius</color>",
			style
		);
		
		if ( GUI.changed )
			EditorUtility.SetDirty ( target );
	}
}
