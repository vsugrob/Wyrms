using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

public class PropertyField {
	private static Dictionary <Type, SerializedPropertyType> typeMap = new Dictionary <Type, SerializedPropertyType> ();
	public object Object { get; private set; }
	public UnityEngine.Object UnityObject { get { return	Object as UnityEngine.Object; } }
	private PropertyInfo info;
	private MethodInfo getter;
	private MethodInfo setter;

	public Type Type { get { return	info.PropertyType; } }
	public SerializedPropertyType SerializedType { get; private set; }
	public String Name { get { return ObjectNames.NicifyVariableName ( info.Name ); } }

	public PropertyField ( object obj, PropertyInfo info, SerializedPropertyType serializedType ) {
		this.Object = obj;
		this.info = info;
		this.SerializedType = serializedType;
		this.getter = this.info.GetGetMethod ();
		this.setter = this.info.GetSetMethod ();
	}

	public object Value {
		get { return getter.Invoke ( Object, null ); }
		set { setter.Invoke ( Object, new [] { value } ); }
	}

	public static bool GetPropertyType ( PropertyInfo info, out SerializedPropertyType propertyType ) {
		var type = info.PropertyType;

		if ( typeMap.TryGetValue ( type, out propertyType ) )
			return	true;
		else if ( type.IsSubclassOf ( typeof ( UnityEngine.Object ) ) ) {
			propertyType = SerializedPropertyType.ObjectReference;

			return	true;
		} else
			return	false;
	}

	static PropertyField () {
		typeMap [typeof ( int )] = SerializedPropertyType.Integer;
		typeMap [typeof ( float )] = SerializedPropertyType.Float;
		typeMap [typeof ( bool )] = SerializedPropertyType.Boolean;
		typeMap [typeof ( string )] = SerializedPropertyType.String;
		typeMap [typeof ( Vector2 )] = SerializedPropertyType.Vector2;
		typeMap [typeof ( Vector3 )] = SerializedPropertyType.Vector3;
		typeMap [typeof ( Enum )] = SerializedPropertyType.Enum;
		// TDOO: add other types from SerializedPropertyType enumeration.
	}
}
