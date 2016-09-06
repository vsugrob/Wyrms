using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor ( typeof ( GuiFixedGameObject ) )]
public class GuiFixedGameObjectEditor : Editor {
	void OnSceneGUI () {
		Undo.RecordObject ( target, "GUI fixed game object change" );

		var guiFixedGameObject = target as GuiFixedGameObject;
		guiFixedGameObject.PanelAnchorWorldPos = Handles.PositionHandle (
			guiFixedGameObject.PanelAnchorWorldPos,
			Quaternion.identity
		);

		var style = new GUIStyle ();
		style.richText = true;
		Handles.Label (
			( Vector3 ) guiFixedGameObject.PanelAnchorWorldPos + Vector3.down * 0.1f,
			"<color=gray>Panel Anchor</color>",
			style
		);

		if ( GUI.changed )
			EditorUtility.SetDirty ( target );
	}
}
