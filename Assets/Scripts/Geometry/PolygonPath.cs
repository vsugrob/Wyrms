using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PolygonPath {
	public List <PolygonWithHoles> PolysWithHoles { get; set; }
	public int TotalPointCount {
		get {
			return	PolysWithHoles.Sum ( polyWithHoles => polyWithHoles.TotalPointCount );
		}
	}
	public IEnumerable <Polygon> Polygons { get { return	PolysWithHoles.SelectMany ( polyWithHoles => polyWithHoles.Polygons ); } }

	public PolygonPath ( List <PolygonWithHoles> polysWithHoles = null ) {
		this.PolysWithHoles = polysWithHoles ?? new List <PolygonWithHoles> ();
	}

	public void Translate ( Vector2 t ) {
		foreach ( var polyWithHoles in PolysWithHoles ) {
			polyWithHoles.Translate ( t );
		}
	}

	public void Scale ( float s ) {
		foreach ( var polyWithHoles in PolysWithHoles ) {
			polyWithHoles.Scale ( s );
		}
	}

	public void ReduceByMinDistance ( float minDistance, int minVertexCount = 3 ) {
		foreach ( var polyWithHoles in PolysWithHoles ) {
			polyWithHoles.ReduceByMinDistance ( minDistance, minVertexCount );
		}
	}

	public void ReduceCodirected ( float minAngle, int minVertexCount = 3 ) {
		foreach ( var polyWithHoles in PolysWithHoles ) {
			polyWithHoles.ReduceCodirected ( minAngle, minVertexCount );
		}
	}
	
	public void ReduceByMinTriangleArea ( float minArea, int minVertexCount = 3 ) {
		foreach ( var polyWithHoles in PolysWithHoles ) {
			polyWithHoles.ReduceByMinTriangleArea ( minArea, minVertexCount );
		}
	}

	public void DebugDraw (
		Color edgeColor, Color holeColor, Color vertexColor,
		Transform transform,
		float duration = 0, bool depthTest = false
	) {
		foreach ( var polyWithHoles in PolysWithHoles ) {
			polyWithHoles.DebugDraw ( edgeColor, holeColor, vertexColor, transform, duration, depthTest );
		}
	}
}
