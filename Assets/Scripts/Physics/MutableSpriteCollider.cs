//#define MUTABLE_COLLIDER_STATS
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MutableSpriteCollider : MonoBehaviour {
	public int CellSize = 256;
	public bool CloneTexture = true;
	public float AlphaThreshold = 0.5f;
	public int MipLevel = 0;
	public bool ClockWise = true;
	public bool UseEdgeCollider = true;
	public ReductionSettings EdgeColliderReduction = new ReductionSettings ();
	public bool UsePolyCollider = true;
	public ReductionSettings PolyColliderReduction = new ReductionSettings () {
		MinDistance = 0.075f,
		MinTriangleArea = 0.0025f
	};

	private Dictionary <Collider2D, SpriteRenderer> colliderSpriteMap = new Dictionary <Collider2D, SpriteRenderer> ();
	
	void Start () {
		var spriteRenderer = GetComponent <SpriteRenderer> ();
		spriteRenderer.enabled = false;

		var initialPolyCollider = GetComponent <PolygonCollider2D> ();

		if ( initialPolyCollider != null )
			Destroy ( initialPolyCollider );

		// TODO: i don't remember why we clone texture here. Is this really needed?
		if ( CloneTexture )
			RasterHelper.CloneTextureAndSprite ( spriteRenderer );

		var initialSprite = spriteRenderer.sprite;
		spriteRenderer.sprite = null;	// Prevent newly added polygon colliders from autotrace.
		var initialTexture = initialSprite.texture;
		float pixelsPerUnit = RasterHelper.GetPixelsPerUnit ( initialSprite );

		/* TODO: unite small cells on the border with those which are inside.
		 * UPD: no, don't unite. Make border cell and adjacent cell even sized,
		 * thus guarantees that size of any cell is smaller than CellSize. */

		for ( int y = 0 ; y < initialTexture.height ; y += CellSize ) {
			int cellHeight = Mathf.Min ( initialTexture.height - y, CellSize );

			for ( int x = 0 ; x < initialTexture.width ; x += CellSize ) {
				int cellWidth = Mathf.Min ( initialTexture.width - x, CellSize );
				var pixels = initialTexture.GetPixels ( x, y, cellWidth, cellHeight );
				var cellTexture = new Texture2D ( cellWidth, cellHeight );
				cellTexture.name = string.Format (
					"{0} [y: {1}, x: {2}]",
					initialTexture.name, y, x
				);
				cellTexture.filterMode = FilterMode.Trilinear;
				cellTexture.wrapMode = TextureWrapMode.Clamp;
				cellTexture.SetPixels ( pixels );
				cellTexture.Apply ();

				var pixelRect = new Rect ( x, y, cellWidth, cellHeight );
				var cellPivot = RasterHelper.CalculateRelativePivot ( initialSprite, pixelRect );

				var cellSprite = Sprite.Create (
					cellTexture,
					new Rect ( 0, 0, cellWidth, cellHeight ),
					cellPivot,
					pixelsPerUnit
				);

				cellSprite.name = string.Format (
					"{0} [y: {1}, x: {2}]",
					initialSprite.name, y, x
				);

				var cellGameObject = new GameObject (
					string.Format (
						"{0} [y: {1}, x: {2}]",
						name, y, x
					)
				);
				var cellSpriteRenderer = cellGameObject.AddComponent <SpriteRenderer> ();
				cellSpriteRenderer.sprite = cellSprite;
				cellSpriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
				cellSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder;
				
				var cellTf = cellGameObject.transform;
				cellTf.parent = this.transform;
				cellTf.localPosition = Vector3.zero;
				cellTf.localRotation = Quaternion.identity;
				cellTf.localScale = Vector3.one;

				if ( UsePolyCollider ) {
					var cellPolyCollider = gameObject.AddComponent <PolygonCollider2D> ();
					cellPolyCollider.pathCount = 0;
					colliderSpriteMap [cellPolyCollider] = cellSpriteRenderer;
				}

				UpdateSpriteColliders ( cellSpriteRenderer );
			}
		}
		
		spriteRenderer.sprite = initialSprite;
	}

	// TODO: make it FillCircle ()
	public void CutCircle ( Vector2 center, float radius, IEnumerable <Collider2D> affectedColliders = null ) {
		if ( affectedColliders == null )
			affectedColliders = Physics2D.OverlapCircleAll ( center, radius );

		var affectedSpriteRenderers = new HashSet <SpriteRenderer> ();

		foreach ( var affectedCollider in affectedColliders ) {
			SpriteRenderer spriteRenderer;

			if ( colliderSpriteMap.TryGetValue ( affectedCollider, out spriteRenderer ) )
				affectedSpriteRenderers.Add ( spriteRenderer );
		}

		foreach ( var spriteRenderer in affectedSpriteRenderers ) {
			if ( RasterHelper.CutCircle ( spriteRenderer, center, radius ) )
				UpdateSpriteColliders ( spriteRenderer );
		}
	}

	private void UpdateSpriteColliders ( SpriteRenderer spriteRenderer ) {
		var spriteColliders = colliderSpriteMap
			.Where ( kv => kv.Value == spriteRenderer )
			.Select ( kv => kv.Key )
			.ToArray ();

		var polyPath = BuildPolyPath ( spriteRenderer );

		if ( polyPath.PolysWithHoles.Count != 0 ) {
			float reductionStartTime = Time.realtimeSinceStartup;
			int edgePointCountBeforeOpt = polyPath.TotalPointCount;
			ReducePath ( polyPath, EdgeColliderReduction );
			int edgePointCountAfterOpt = polyPath.TotalPointCount;
			float reductionTimeElapsed = Time.realtimeSinceStartup - reductionStartTime;

			var polys = polyPath.Polygons.Select ( poly => poly ).ToArray ();
			float polyColliderTimeElapsed = 0, edgeColliderTimeElapsed = 0;

			if ( UseEdgeCollider ) {
				float edgeColliderTimeStart = Time.realtimeSinceStartup;
				var edgeColliders = spriteColliders.OfType <EdgeCollider2D> ().ToArray ();

				// Destroy all of the edge colliders since they will be rebuilt from scratch.
				foreach ( var edgeCollider in edgeColliders ) {
					colliderSpriteMap.Remove ( edgeCollider );
					Destroy ( edgeCollider );
				}

				for ( int i = 0 ; i < polys.Length ; i++ ) {
					var edgeCollider = gameObject.AddComponent <EdgeCollider2D> ();
					var vertexList = polys [i].Vertices;
					var vertices = vertexList.Concat ( new [] { vertexList [0] } ).ToArray ();
					edgeCollider.points = vertices;
					colliderSpriteMap [edgeCollider] = spriteRenderer;
				}

				edgeColliderTimeElapsed = Time.realtimeSinceStartup - edgeColliderTimeStart;
			}

			int polyPointCountBeforeOpt = polyPath.TotalPointCount;

			if ( UsePolyCollider ) {
				ReducePath ( polyPath, PolyColliderReduction, EdgeColliderReduction );

				var polyColliders = spriteColliders.OfType <PolygonCollider2D> ();
				PolygonCollider2D polyCollider;

				if ( polyColliders.Any () ) {
					// We expect exactly one PolygonCollider2D to be mapped to SpriteRenderer.
					polyCollider = polyColliders.First ();
				} else {
					/* There might be no poly collider associated with given sprite renderer,
					 * which might happen when UsePolyCollider setting was just set to true. */
					polyCollider = gameObject.AddComponent <PolygonCollider2D> ();
					polyCollider.pathCount = 0;
					colliderSpriteMap [polyCollider] = spriteRenderer;
				}

				float polyColliderTimeStart = Time.realtimeSinceStartup;
				polyCollider.pathCount = polys.Length;

				for ( int i = 0 ; i < polys.Length ; i++ ) {
					var vertices = polys [i].Vertices.ToArray ();
					polyCollider.SetPath ( i, vertices );
				}
				polyColliderTimeElapsed = Time.realtimeSinceStartup - polyColliderTimeStart;
			}

			int polyPointCountAfterOpt = polyPath.TotalPointCount;

#if MUTABLE_COLLIDER_STATS
			print (
				"Timing -- Poly collider: " + polyColliderTimeElapsed +
				", Edge collider: " + edgeColliderTimeElapsed +
				", Reduction: " + reductionTimeElapsed +
				string.Format (
					", Edge collider reduction: {0}/{1} ({2:P})",
					edgePointCountAfterOpt, edgePointCountBeforeOpt, 1.0f - ( float ) edgePointCountAfterOpt / edgePointCountBeforeOpt
				) +
				string.Format (
					", Poly collider reduction: {0}/{1} ({2:P})",
					polyPointCountAfterOpt, polyPointCountBeforeOpt, 1.0f - ( float ) polyPointCountAfterOpt / polyPointCountBeforeOpt
				)
			);
#endif
		} else {
			// Unload cell.
			Destroy ( spriteRenderer.sprite.texture );
			Destroy ( spriteRenderer.gameObject );

			foreach ( var spriteCollider in spriteColliders ) {
				colliderSpriteMap.Remove ( spriteCollider );
				Destroy ( spriteCollider );
			}
		}
	}

	private PolygonPath BuildPolyPath ( SpriteRenderer spriteRenderer ) {
		var sprite = spriteRenderer.sprite;

		float buildPathStartTime = Time.realtimeSinceStartup;
		float msStartTime = Time.realtimeSinceStartup;
		var ms = new MarchingSquares (
			sprite.texture,
			alphaThreshold : AlphaThreshold,
			clockWise : ClockWise,
			mipLevel : MipLevel
		);
		float msTimeElapsed = Time.realtimeSinceStartup - msStartTime;

		float traceStartTime = Time.realtimeSinceStartup;
		var polyPath = ms.TraceContours ();
		float traceTimeElapsed = Time.realtimeSinceStartup - traceStartTime;

		float pixelsPerUnit = RasterHelper.GetPixelsPerUnit ( sprite );
		float scale = ( 1 << MipLevel ) / pixelsPerUnit;
		var pivot = RasterHelper.GetPivot ( sprite );
		var offset = -Vector2.Scale ( sprite.bounds.size, pivot );

		float transformStartTime = Time.realtimeSinceStartup;
		polyPath.Scale ( scale );
		polyPath.Translate ( offset );
		float transformTimeElapsed = Time.realtimeSinceStartup - transformStartTime;

#if MUTABLE_COLLIDER_STATS
		float buildPathTimeElapsed = Time.realtimeSinceStartup - buildPathStartTime;
		
		print ( string.Format (
			"Build path timing -- Trace: {0}, Transform: {1}, Get pixels: {2}, Total: {3}",
			traceTimeElapsed, transformTimeElapsed, msTimeElapsed, buildPathTimeElapsed
		) );
#endif

		return	polyPath;
	}

	private void ReducePath ( PolygonPath polyPath, ReductionSettings settings, ReductionSettings toughestSettings = null ) {
		if ( settings.ReduceByMinDistance &&
			( toughestSettings == null || !toughestSettings.ReduceByMinDistance || settings.MinDistance > toughestSettings.MinDistance )
		) {
			polyPath.ReduceByMinDistance ( settings.MinDistance, settings.MinVertexCount );
		}

		if ( settings.ReduceByMinTriangleArea &&
			( toughestSettings == null || !toughestSettings.ReduceByMinTriangleArea || settings.MinTriangleArea > toughestSettings.MinTriangleArea )
		) {
			float worldScale = Common.WorldScale ( transform );
			float globalScaleSq = worldScale * worldScale;

			polyPath.ReduceByMinTriangleArea ( settings.MinTriangleArea / globalScaleSq, settings.MinVertexCount );
		}

		if ( settings.ReduceCodirected &&
			( toughestSettings == null || !toughestSettings.ReduceCodirected || settings.MinAngle > toughestSettings.MinAngle )
		) {
			polyPath.ReduceCodirected ( settings.MinAngle * Mathf.Deg2Rad, settings.MinVertexCount );
		}
	}

	[System.Serializable]
	public class ReductionSettings {
		public bool ReduceByMinDistance = true;
		public float MinDistance = 0.05f;
		public bool ReduceByMinTriangleArea = true;
		public float MinTriangleArea = 0.00125f;
		public bool ReduceCodirected = false;
		public float MinAngle = 5;
		public int MinVertexCount = 8;
	}
}
