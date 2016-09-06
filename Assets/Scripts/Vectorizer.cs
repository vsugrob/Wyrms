using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class Vectorizer {
	private static GameObject dummyGameObject = null;

	public static Vector2 [][] Trace ( Texture2D texture, Rect pixelRect, Vector2 pivot, float pixelsPerUnit = 100 ) {
		if ( dummyGameObject == null ) {
			dummyGameObject = new GameObject ( "Dummy object for vectorization" );
			dummyGameObject.AddComponent <SpriteRenderer> ();
			dummyGameObject.SetActive ( false );
		}

		var texSize = new Vector2 ( texture.width, texture.height ) / pixelsPerUnit;
		var texBottomLeft = -Vector2.Scale ( texSize, pivot );

		var rectOffset = pixelRect.min / pixelsPerUnit;
		var rectBottomLeft = texBottomLeft + rectOffset;
		var dummySpritePivot = new Vector2 (
			-rectBottomLeft.x * pixelsPerUnit / pixelRect.width,
			-rectBottomLeft.y * pixelsPerUnit / pixelRect.height
		);
		
		var dummySprite = Sprite.Create (
			texture,
			pixelRect,
			dummySpritePivot,
			pixelsPerUnit
		);

		var spriteRenderer = dummyGameObject.GetComponent <SpriteRenderer> ();
		spriteRenderer.sprite = dummySprite;

		var polyCollider = dummyGameObject.GetComponent <PolygonCollider2D> ();
		
		if ( polyCollider != null )
			GameObject.Destroy ( polyCollider );
		
		//float traceStart = Time.realtimeSinceStartup;				// DEBUG
		polyCollider = dummyGameObject.AddComponent <PolygonCollider2D> ();
		//float traceTime = Time.realtimeSinceStartup - traceStart;	// DEBUG
		//Debug.Log ( "Vectorizer.Trace (), trace time: " + traceTime );			// DEBUG

		if ( polyCollider.pathCount == 1 && polyCollider.GetTotalPointCount () == 5 ) {
			// Suspicion for default collider data - pentagon.
			/* TODO: anyway, this vectorizer will be replaced with my own implementation,
			 * so don't bother detecting pentagon and just return empty data. */
			
			return	new Vector2 [0][];
		}

		var paths = new Vector2 [polyCollider.pathCount][];

		for ( int i = 0 ; i < polyCollider.pathCount ; i++ ) {
			var path = polyCollider.GetPath ( i );
			paths [i] = path;
		}

		return	paths;
	}

	public static Vector2 [][] Trace ( Sprite sprite, Rect pixelRect ) {
		return	Trace (
			sprite.texture,
			pixelRect,
			RasterHelper.GetPivot ( sprite ),
			RasterHelper.GetPixelsPerUnit ( sprite )
		);
	}

	public static Vector2 [][] Trace ( Sprite sprite ) {
		var texture = sprite.texture;
		var pixelRect = new Rect ( 0, 0, texture.width, texture.height );

		return	Trace (
			sprite.texture,
			pixelRect,
			RasterHelper.GetPivot ( sprite ),
			RasterHelper.GetPixelsPerUnit ( sprite )
		);
	}
}
