using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class Common {
	public static float DistanceSq ( Vector2 p1, Vector2 p2 ) {
		float dx = p2.x - p1.x;
		float dy = p2.y - p1.y;

		return	dx * dx + dy * dy;
	}

	public static Vector2 RightOrthogonal ( Vector2 v ) {
		return	new Vector2 ( v.y, -v.x );
	}

	public static Vector2 LeftOrthogonal ( Vector2 v ) {
		return	new Vector2 ( -v.y, v.x );
	}

	public static float Cross ( Vector2 v1, Vector2 v2 ) {
		return v1.x * v2.y - v1.y * v2.x;
	}

	public static Vector2 Project ( Vector2 vector, Vector2 normal ) {
		float projLen = Vector2.Dot ( vector, normal );
		
		return	projLen * normal;
	}

	public static float SignedAngle ( Vector2 from, Vector2 to ) {
		float angle = Vector2.Angle ( from, to );
		float cross = Common.Cross ( from, to );

		if ( cross < 0 )
			angle = -angle;

		return	angle;
	}

	public static float AngleRad ( this Vector2 v ) {
		return	Mathf.Atan2 ( v.y, v.x );
	}

	public static float AngleDeg ( this Vector2 v ) {
		return	v.AngleRad () * Mathf.Rad2Deg;
	}

	public static Quaternion RotateAlongDirection ( Vector2 dir ) {
		float angle = SignedAngle ( Vector2.right, dir );

		return	Quaternion.Euler ( 0, 0, angle );
	}

	public static Vector2 RotateTowards ( Vector2 from, Vector2 to, float maxDegreesDelta ) {
		var startRotation = RotateAlongDirection ( from );
		var endRotation = RotateAlongDirection ( to );
		var curRotation = Quaternion.RotateTowards ( startRotation, endRotation, maxDegreesDelta );
		var curVector = curRotation * Vector2.right;

		return	curVector;
	}

	public static Vector2 RandomPointOnUnitCircle () {
		float angle = Random.Range ( 0, Mathf.PI * 2 );
		var v = new Vector2 ( Mathf.Cos ( angle ), Mathf.Sin ( angle ) );

		return	v;
	}

	public static float WorldScale ( Transform transform ) {
		var worldScaleVector = transform.TransformPoint ( Vector3.right ) - transform.position;

		return	worldScaleVector.magnitude;
	}

	public static void SetVisibility ( GameObject gameObject, bool visible ) {
		var renderers = gameObject.GetComponentsInChildren <Renderer> ();

		foreach ( var r in renderers ) {
			r.enabled = visible;
		}
	}

	public static CircleCollider2D Clone ( CircleCollider2D source, GameObject destinationGameObject ) {
		var clone = destinationGameObject.AddComponent <CircleCollider2D> ();
		clone.sharedMaterial = source.sharedMaterial;
		clone.isTrigger = source.isTrigger;
		clone.radius = source.radius;
		clone.center = source.center;

		return	clone;
	}

	public static BoxCollider2D Clone ( BoxCollider2D source, GameObject destinationGameObject ) {
		var clone = destinationGameObject.AddComponent <BoxCollider2D> ();
		clone.sharedMaterial = source.sharedMaterial;
		clone.isTrigger = source.isTrigger;
		clone.size = source.size;
		clone.center = source.center;

		return	clone;
	}

	public static Collider2D Clone ( Collider2D source, GameObject destinationGameObject ) {
		if ( source is CircleCollider2D )
			return	Clone ( source as CircleCollider2D, destinationGameObject );
		else if ( source is BoxCollider2D )
			return	Clone ( source as BoxCollider2D, destinationGameObject );
		else {
			throw new System.ArgumentException (
				"Unsupported type: " + source.GetType ().FullName,
				"source"
			);
		}
	}

	public static bool InState ( this Animator animator, string stateName, int layerIndex = 0 ) {
		var state = animator.GetCurrentAnimatorStateInfo ( layerIndex );

		return	state.IsName ( stateName );
	}

	public static void SetTransform ( Transform valueReceiver, Transform valueSource ) {
		valueReceiver.position = valueSource.position;
		valueReceiver.localScale = valueSource.localScale;
		valueReceiver.rotation = valueSource.rotation;
	}

	private static Regex htmlTagRegex = new Regex ( @"</?\w+>" );

	public static string StripTags ( string html ) {
		return	htmlTagRegex.Replace ( html, "" );
	}

	public static string ExpandString ( string str, IEnumerable <KeyValuePair <string, object>> namedValues ) {
		int i = 0;

		// TODO: replace "{" and "}" with "{{" and "}}" in non-expanded parts of the string.

		foreach ( var namedValue in namedValues ) {
			/* Preserve format part of string, i.e. substitute
			 * "{Health:F2}" with "{0:F2}" */
			str = str.Replace ( "{" + namedValue.Key, "{" + i );
			i++;
		}

		var values = namedValues.Select ( namedValue => namedValue.Value ).ToArray ();
		str = string.Format ( str, values );

		return	str;
	}

	public static string ToHex ( this Color32 color ) {
		string hex = color.r.ToString ( "x2" ) +
					 color.g.ToString ( "x2" ) +
					 color.b.ToString ( "x2" ) +
					 color.a.ToString ( "x2" );

		return	hex;
	}

	public static string ToHex ( this Color color ) {
		return	ToHex ( ( Color32 ) color );
	}

	public static void Destroy ( UnityEngine.Object obj ) {
		if ( Application.isPlaying )
			Object.Destroy ( obj );
		else
			Object.DestroyImmediate ( obj );
	}
}
