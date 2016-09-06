using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PolygonWithHoles {
	public Polygon Outer { get; set; }
	public List <Polygon> Holes { get; set; }
	public int TotalPointCount {
		get {
			return	Outer.Vertices.Count + Holes.Sum ( hole => hole.Vertices.Count );
		}
	}
	public IEnumerable <Polygon> Polygons { get { return	new [] { Outer }.Concat ( Holes ); } }

	public PolygonWithHoles ( Polygon outer = null, List <Polygon> holes = null ) {
		this.Outer = outer ?? new Polygon ();
		this.Holes = holes ?? new List <Polygon> ();
	}

	public void Translate ( Vector2 t ) {
		Outer.Translate ( t );

		foreach ( var hole in Holes ) {
			hole.Translate ( t );
		}
	}

	public void Scale ( float s ) {
		Outer.Scale ( s );

		foreach ( var hole in Holes ) {
			hole.Scale ( s );
		}
	}

	public void ReduceByMinDistance ( float minDistance, int minVertexCount = 3 ) {
		Outer.ReduceByMinDistance ( minDistance, minVertexCount );

		foreach ( var hole in Holes ) {
			hole.ReduceByMinDistance ( minDistance, minVertexCount );
		}
	}

	public void ReduceCodirected ( float minAngle, int minVertexCount = 3 ) {
		Outer.ReduceCodirected ( minAngle, minVertexCount );

		foreach ( var hole in Holes ) {
			hole.ReduceCodirected ( minAngle, minVertexCount );
		}
	}

	public void ReduceByMinTriangleArea ( float minArea, int minVertexCount = 3 ) {
		Outer.ReduceByMinTriangleArea ( minArea, minVertexCount );

		foreach ( var hole in Holes ) {
			hole.ReduceByMinTriangleArea ( minArea, minVertexCount );
		}
	}

	public void DebugDraw (
		Color edgeColor, Color holeColor, Color vertexColor,
		Transform transform,
		float duration = 0, bool depthTest = false
	) {
		Outer.DebugDraw ( edgeColor, vertexColor, transform, duration, depthTest );

		foreach ( var hole in Holes ) {
			hole.DebugDraw ( holeColor, vertexColor, transform, duration, depthTest );
		}
	}
}
