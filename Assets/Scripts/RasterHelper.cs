using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class RasterHelper {
	private static readonly Color transparent = new Color ( 1, 1, 1, 0 );

	// TODO: make it FillCircle ( ..., Color color )
	public static bool CutCircle ( Texture2D texture, int cx, int cy, int radius ) {
		float timeStart = Time.realtimeSinceStartup;

		int lx = Mathf.Max ( 0, cx - radius ),
			by = Mathf.Max ( 0, cy - radius ),
			rx = Mathf.Min ( texture.width, cx + radius ),
			ty = Mathf.Min ( texture.height, cy + radius ),
			rSq = radius * radius;

		if ( rx < lx || ty < by )
			return	false;

		for ( int y = by ; y < ty ; y++ ) {
			for ( int x = lx ; x < rx ; x++ ) {
				int dx = x - cx,
					dy = y - cy,
					dSq = dx * dx + dy * dy;

				if ( dSq <= rSq )
					texture.SetPixel ( x, y, transparent );
			}
		}

		texture.Apply ();
		float timeElapsed = Time.realtimeSinceStartup - timeStart;

		if ( false ) {
			Debug.Log ( "RasterHelper.CutCircle () time: " + timeElapsed );
		}

		return	true;
	}

	// TODO: make it FillCircle ()
	public static bool CutCircle ( SpriteRenderer spriteRenderer, Vector2 center, float radius ) {
		var tf = spriteRenderer.transform;
		center = tf.InverseTransformPoint ( center );
		var localScaleVector = tf.InverseTransformPoint ( tf.position + Vector3.right );
		var worldToLocalScale = localScaleVector.magnitude;
		radius *= worldToLocalScale;
		
		var sprite = spriteRenderer.sprite;
		int pixelX, pixelY;
		RasterHelper.SpriteCoordsToPixelCoords ( sprite, center, out pixelX, out pixelY );
		float pixelsPerUnit = RasterHelper.GetPixelsPerUnit ( sprite );
		int pixelRadius = Mathf.RoundToInt ( radius * pixelsPerUnit );

		return	RasterHelper.CutCircle ( sprite.texture, pixelX, pixelY, pixelRadius );
	}

	public static void SpriteCoordsToPixelCoords ( Sprite sprite, Vector2 localPos, out int pixelX, out int pixelY ) {
		var bounds = sprite.bounds;
		var p = localPos - ( Vector2 ) bounds.min;
		p.x = p.x / bounds.size.x;
		p.y = p.y / bounds.size.y;

		var rect = sprite.textureRect;
		p.x = rect.xMin + rect.width * p.x;
		p.y = rect.yMin + rect.height * p.y;

		pixelX = Mathf.RoundToInt ( p.x );
		pixelY = Mathf.RoundToInt ( p.y );
	}

	public static float GetPixelsPerUnit ( Sprite sprite ) {
		var bounds = sprite.bounds;
		var rect = sprite.textureRect;

		return	rect.width / bounds.size.x;
	}

	public static Vector2 GetPivot ( Sprite sprite ) {
		var bounds = sprite.bounds;
		var min = bounds.min;
		var size = bounds.size;
		var pivot = new Vector2 (
			-min.x / size.x,
			-min.y / size.y
		);

		return	pivot;
	}

	public static Vector2 CalculateRelativePivot ( Sprite initialSprite, Rect pixelRect ) {
		float pixelsPerUnit = GetPixelsPerUnit ( initialSprite );

		var rectOffset = pixelRect.min / pixelsPerUnit;
		var rectBottomLeft = ( Vector2 ) initialSprite.bounds.min + rectOffset;
		var newPivot = new Vector2 (
			-rectBottomLeft.x * pixelsPerUnit / pixelRect.width,
			-rectBottomLeft.y * pixelsPerUnit / pixelRect.height
		);

		return	newPivot;
	}

	public static void CloneTextureAndSprite ( SpriteRenderer spriteRenderer ) {
		var sprite = spriteRenderer.sprite;
		var texClone = Object.Instantiate ( sprite.texture ) as Texture2D;
		spriteRenderer.sprite = Sprite.Create (
			texClone,
			sprite.rect,
			RasterHelper.GetPivot ( sprite ),
			RasterHelper.GetPixelsPerUnit ( sprite )
		);
		spriteRenderer.sprite.name = sprite.name + "(Clone)";
	}
}
