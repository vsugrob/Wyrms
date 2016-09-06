using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomPropertyDrawer ( typeof ( StateMaterial ) )]
public class StateMaterialDrawer : PropertyDrawer {
	public override void OnGUI ( Rect pos, SerializedProperty property, GUIContent label ) {
		EditorGUI.BeginProperty ( pos, label, property );

		pos = EditorGUI.PrefixLabel ( pos, GUIUtility.GetControlID ( FocusType.Passive ), label );
		int prevIndentLevel = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		pos = PropertyFieldWithShortLabel (
			new Rect ( pos.x, pos.y, 50, pos.height ),
			property.FindPropertyRelative ( "Friction" )
		);

		pos.x += 10;
		pos = PropertyFieldWithShortLabel (
			new Rect ( pos.x, pos.y, 50, pos.height ),
			property.FindPropertyRelative ( "Bounciness" )
		);

		EditorGUI.indentLevel = prevIndentLevel;
		EditorGUI.EndProperty ();
	}

	private Rect PropertyFieldWithShortLabel ( Rect pos, SerializedProperty property, GUIContent label = null ) {
		if ( label == null )
			label = new GUIContent ( property.name );

		var size = GUI.skin.label.CalcSize ( label );
		EditorGUIUtility.labelWidth = size.x;

		EditorGUI.PropertyField (
			new Rect ( pos.x, pos.y, pos.width + size.x, pos.height ),
			property,
			label
		);

		pos.x += pos.width + size.x;

		return	pos;
	}
}
