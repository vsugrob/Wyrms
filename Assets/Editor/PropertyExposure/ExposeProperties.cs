using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
 
public static class ExposeProperties {
	private static Dictionary <SerializedPropertyType, Func <PropertyField, object>> propertyDrawerFuncs =
		new Dictionary <SerializedPropertyType, Func <PropertyField, object>> ();

	static ExposeProperties () {
		propertyDrawerFuncs [SerializedPropertyType.Integer] = ( field ) => {
			return	EditorGUILayout.IntField ( field.Name, ( int ) field.Value, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.Boolean] = ( field ) => {
			return	EditorGUILayout.Toggle ( field.Name, ( bool ) field.Value, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.Float] = ( field ) => {
			return	EditorGUILayout.FloatField ( field.Name, ( float ) field.Value, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.String] = ( field ) => {
			return	EditorGUILayout.TextField ( field.Name, ( string ) field.Value, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.Color] = ( field ) => {
			return	EditorGUILayout.ColorField ( field.Name, ( Color ) field.Value, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.ObjectReference] = ( field ) => {
			return	EditorGUILayout.ObjectField ( field.Name, ( UnityEngine.Object ) field.Value, field.Type, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.LayerMask] = ( field ) => {
			// TODO: test it.
			return	EditorGUILayout.LayerField ( field.Name, ( int ) field.Value, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.Enum] = ( field ) => {
			return	EditorGUILayout.EnumPopup ( field.Name, ( Enum ) field.Value, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.Vector2] = ( field ) => {
			return	EditorGUILayout.Vector2Field ( field.Name, ( Vector2 ) field.Value, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.Vector3] = ( field ) => {
			return	EditorGUILayout.Vector3Field ( field.Name, ( Vector3 ) field.Value, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.Vector4] = ( field ) => {
			return	EditorGUILayout.Vector4Field ( field.Name, ( Vector4 ) field.Value, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.Rect] = ( field ) => {
			return	EditorGUILayout.RectField ( field.Name, ( Rect ) field.Value, new GUILayoutOption [0] );
		};
		// TODO: how to implement it?
		//propertyDrawerFuncs [SerializedPropertyType.ArraySize] = ( field ) => {
		//	return	EditorGUILayout.arr ( field.Name, ( Rect ) field.Value, new GUILayoutOption [0] );
		//};
		// TODO: how to implement it?
		//propertyDrawerFuncs [SerializedPropertyType.Character] = ( field ) => {
		//	return	EditorGUILayout.cha ( field.Name, ( Rect ) field.Value, new GUILayoutOption [0] );
		//};
		propertyDrawerFuncs [SerializedPropertyType.AnimationCurve] = ( field ) => {
			return	EditorGUILayout.CurveField ( field.Name, ( AnimationCurve ) field.Value, new GUILayoutOption [0] );
		};
		propertyDrawerFuncs [SerializedPropertyType.Bounds] = ( field ) => {
			return	EditorGUILayout.BoundsField ( field.Name, ( Bounds ) field.Value, new GUILayoutOption [0] );
		};
		// TODO: how to implement it?
		//propertyDrawerFuncs [SerializedPropertyType.Gradient] = ( field ) => {
		//	return	EditorGUILayout.gra ( field.Name, ( Bounds ) field.Value, new GUILayoutOption [0] );
		//};
		// TODO: how to implement it?
		//propertyDrawerFuncs [SerializedPropertyType.Quaternion] = ( field ) => {
		//	return	EditorGUILayout.qua ( field.Name, ( Quaternion ) field.Value, new GUILayoutOption [0] );
		//};
	}

	public static void Expose ( object obj, bool recordUndo = true ) {
		var propertyFields = ExposeProperties.GetProperties ( obj );
		Expose ( propertyFields, recordUndo );
	}

	public static void Expose ( PropertyField [] properties, bool recordUndo = true ) {
		var emptyOptions = new GUILayoutOption [0];
		EditorGUILayout.BeginVertical ( emptyOptions );
		bool undoRecorded = false;

		foreach ( PropertyField field in properties ) {
			/* Warning: unity undo system works bad with properties.
			 * It reverts values by setting fields rather than properties, which
			 * may lead to inconsistent state of object. */
			if ( recordUndo && !undoRecorded ) {
				var unityObj = field.UnityObject;

				if ( unityObj != null )
					Undo.RecordObject ( unityObj, unityObj.name + " property change" );

				undoRecorded = true;
			}

			EditorGUILayout.BeginHorizontal ( emptyOptions );

			object oldValue = field.Value;
			Func <PropertyField, object> drawerFunc;

			if ( propertyDrawerFuncs.TryGetValue ( field.SerializedType, out drawerFunc ) ) {
				object newValue = drawerFunc ( field );

				if ( !System.Object.Equals ( oldValue, newValue ) ) {
					field.Value = newValue;

					if ( field.UnityObject != null )
						EditorUtility.SetDirty ( field.UnityObject );
				}
			}

			EditorGUILayout.EndHorizontal ();
		}

		EditorGUILayout.EndVertical ();
	}
 
	public static PropertyField [] GetProperties ( object obj ) {
		var fields = new List <PropertyField> ();
		var infos = obj.GetType ().GetProperties ( BindingFlags.Public | BindingFlags.Instance );

		foreach ( PropertyInfo info in infos ) {
			if ( !( info.CanRead && info.CanWrite ) )
				continue;

			bool isExposed = info.GetCustomAttributes ( typeof ( ExposePropertyAttribute ), inherit : true ).Length > 0;

			if ( !isExposed )
				continue;

			SerializedPropertyType type;

			if ( PropertyField.GetPropertyType ( info, out type ) ) {
				var field = new PropertyField ( obj, info, type );
				fields.Add ( field );
			}
		}

		return	fields.ToArray ();
	}
}