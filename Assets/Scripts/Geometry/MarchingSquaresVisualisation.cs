using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MarchingSquaresVisualisation : MonoBehaviour {
	public float AlphaThreshold = 0.5f;
	private float prevAlphaThreshold = float.NaN;
	public int MipLevel = 0;
	private int prevMipLevel;
	public bool ClockWise = true;
	private bool prevClockWise;
	public bool ReduceByMinDistance = true;
	private bool prevReduceByMinDistance;
	public float MinDistance = 0.05f;
	private float prevMinDistance;
	public bool ReduceByMinTriangleArea = true;
	private bool prevReduceByMinTriangleArea;
	public float MinTriangleArea = 0.00125f;
	private float prevMinTriangleArea;
	public bool ReduceCodirected = false;
	private bool prevReduceCodirected;
	public float MinAngle = 5;
	private float prevMinAngle;
	public int MinVertexCount = 4;
	private int prevMinVertexCount;

	private float prevWorldScale = 1;

	private PolygonPath polyPath;

	void Update () {
		float worldScale = Common.WorldScale ( transform );

		if ( AlphaThreshold != prevAlphaThreshold ||
			MipLevel != prevMipLevel ||
			ClockWise != prevClockWise ||
			ReduceByMinDistance != prevReduceByMinDistance ||
			MinDistance != prevMinDistance ||
			ReduceByMinTriangleArea != prevReduceByMinTriangleArea ||
			MinTriangleArea != prevMinTriangleArea ||
			ReduceCodirected != prevReduceCodirected ||
			MinAngle != prevMinAngle ||
			MinVertexCount != prevMinVertexCount ||
			worldScale != prevWorldScale
		) {
			PerformTrace ();
			prevAlphaThreshold = AlphaThreshold;
			prevMipLevel = MipLevel;
			prevClockWise = ClockWise;
			prevReduceByMinDistance = ReduceByMinDistance;
			prevMinDistance = MinDistance;
			prevReduceByMinTriangleArea = ReduceByMinTriangleArea;
			prevMinTriangleArea = MinTriangleArea;
			prevReduceCodirected = ReduceCodirected;
			prevMinAngle = MinAngle;
			prevMinVertexCount = MinVertexCount;
			prevWorldScale = worldScale;
		}

		polyPath.DebugDraw ( Color.green, Color.red, Color.yellow, transform );
	}

	private void PerformTrace () {
		var spriteRenderer = GetComponent<SpriteRenderer> ();
		var sprite = spriteRenderer.sprite;

		var ms = new MarchingSquares (
			sprite.texture,
			alphaThreshold : AlphaThreshold,
			clockWise : ClockWise,
			mipLevel : MipLevel
		);
		polyPath = ms.TraceContours ();
		float pixelsPerUnit = RasterHelper.GetPixelsPerUnit ( sprite );
		float scale = ( 1 << MipLevel ) / pixelsPerUnit;
		var pivot = RasterHelper.GetPivot ( sprite );
		var offset = -Vector2.Scale ( sprite.bounds.size, pivot );

		polyPath.Scale ( scale );
		polyPath.Translate ( offset );

		int pointCountBeforeOpt = polyPath.TotalPointCount;
		float worldScale = Common.WorldScale ( spriteRenderer.transform );

		if ( ReduceByMinDistance )
			polyPath.ReduceByMinDistance ( MinDistance, MinVertexCount );

		if ( ReduceByMinTriangleArea ) {
			float globalScaleSq = worldScale * worldScale;

			polyPath.ReduceByMinTriangleArea ( MinTriangleArea / globalScaleSq, MinVertexCount );
		}

		if ( ReduceCodirected )
			polyPath.ReduceCodirected ( MinAngle * Mathf.Deg2Rad, MinVertexCount );

		int pointCountAfterOpt = polyPath.TotalPointCount;
		print ( string.Format (
			"Reduction: {0}/{1} ({2:P})",
			pointCountAfterOpt, pointCountBeforeOpt, 1.0f - ( float ) pointCountAfterOpt / pointCountBeforeOpt
		) );
	}
}
