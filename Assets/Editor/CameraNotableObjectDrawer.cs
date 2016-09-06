using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomPropertyDrawer ( typeof ( CameraEvent ) )]
public class CameraNotableObjectDrawer : PropertyDrawer {
	public override void OnGUI ( Rect pos, SerializedProperty property, GUIContent label ) {
		EditorGUI.BeginProperty ( pos, label, property );

		pos = EditorGUI.PrefixLabel ( pos, GUIUtility.GetControlID ( FocusType.Passive ), label );
		int prevIndentLevel = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		var objRef = property.objectReferenceValue as CameraEvent;
		string descName = null;

		if ( objRef != null && !string.IsNullOrEmpty ( objRef.EventName ) )
			descName = objRef.EventName;

		var rect = new Rect ( pos.x, pos.y, pos.width, pos.height );

		if ( descName != null )
			rect.width /= 2;

		EditorGUI.PropertyField ( rect, property, GUIContent.none );

		if ( descName != null ) {
			rect.x += rect.width;
			descName = "(\"" + descName + "\")";
			EditorGUI.LabelField ( rect, new GUIContent ( descName ) );
		}

		EditorGUI.indentLevel = prevIndentLevel;
		EditorGUI.EndProperty ();
	}
}
