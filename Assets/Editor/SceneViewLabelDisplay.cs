using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor ( typeof ( SceneViewLabel ) )]
public class SceneViewLabelDisplay : Editor {
	void OnSceneGUI () {
		var label = this.target as SceneViewLabel;
		var style = new GUIStyle ();
		style.richText = true;
		Handles.Label ( label.Origin, label.Text, style );

		if ( label.WriteLog && label.Text != label.PreviousText ) {
			Debug.Log ( label.name + ": " + label.Text, label );
			label.PreviousText = label.Text;
		}
	}
}
