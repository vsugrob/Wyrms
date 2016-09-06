using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Polygon {
	public List <Vector2> Vertices { get; set; }
	public PolygonWithHoles Container { get; set; }

	public Polygon () {
		this.Vertices = new List <Vector2> ();
	}

	public Polygon ( List <Vector2> vertices ) {
		this.Vertices = vertices;
	}

	public void Translate ( Vector2 t ) {
		for ( int i = 0 ; i < Vertices.Count ; i++ ) {
			Vertices [i] += t;
		}
	}

	public void Scale ( float s ) {
		for ( int i = 0 ; i < Vertices.Count ; i++ ) {
			Vertices [i] *= s;
		}
	}

	public void ReduceByMinDistance ( float minDistance, int minVertexCount = 3 ) {
		if ( Vertices.Count <= minVertexCount || Vertices.Count < 2 )
			return;

		float minDistSq = minDistance * minDistance;

		for ( int i = Vertices.Count - 1, j = 0 ; j < Vertices.Count ; i = j, j++ ) {
			float iLenSq = GetPrevEdgeLenSq ( i );

			while ( j < Vertices.Count && Common.DistanceSq ( Vertices [i], Vertices [j] ) < minDistSq ) {
				float jLenSq = GetNextEdgeLenSq ( j );

				if ( jLenSq <= iLenSq ) {
					Vertices.RemoveAt ( j );

					if ( i >= Vertices.Count )
						i = Vertices.Count - 1;
				} else {
					Vertices.RemoveAt ( i );

					if ( i >= Vertices.Count )
						i = Vertices.Count - 1;

					iLenSq = GetPrevEdgeLenSq ( i );
				}

				if ( Vertices.Count <= minVertexCount )
					return;
			}
		}
	}

	private float GetPrevEdgeLenSq ( int idx ) {
		int idxPrev = idx - 1;

		if ( idxPrev < 0 )
			idxPrev = Vertices.Count - 1;

		return	Common.DistanceSq ( Vertices [idxPrev], Vertices [idx] );
	}

	private float GetNextEdgeLenSq ( int idx ) {
		int idxNext = idx + 1;

		if ( idxNext >= Vertices.Count )
			idxNext = 0;

		return	Common.DistanceSq ( Vertices [idx], Vertices [idxNext] );
	}

	public void ReduceCodirected ( float minAngle, int minVertexCount = 3 ) {
		if ( Vertices.Count <= minVertexCount || Vertices.Count < 3 )
			return;

		minAngle = minAngle * Mathf.Rad2Deg;
		
		for ( int i = Vertices.Count - 1, j = 0, k = 1 ; k < Vertices.Count ; i = j, j = k, k++ ) {
			var p0 = Vertices [i];
			
			while ( k < Vertices.Count && Vector2.Angle ( Vertices [j] - p0, Vertices [k] - Vertices [j] ) < minAngle ) {
				Vertices.RemoveAt ( j );

				if ( Vertices.Count <= minVertexCount )
					return;
			}
		}
	}

	public void ReduceByMinTriangleArea ( float minArea, int minVertexCount = 3 ) {
		if ( Vertices.Count <= minVertexCount || Vertices.Count < 3 )
			return;

		minArea = minArea * 2;	// Area of corresponding parallelogram.

		for ( int i = Vertices.Count - 1, j = 0, k = 1 ; k < Vertices.Count ; i = j, j = k, k++ ) {
			var p0 = Vertices [i];
			
			while ( k < Vertices.Count && Mathf.Abs ( Common.Cross ( Vertices [j] - p0, Vertices [k] - Vertices [j] ) ) < minArea ) {
				Vertices.RemoveAt ( j );

				if ( Vertices.Count <= minVertexCount )
					return;
			}
		}
	}

	public void DebugDraw (
		Color edgeColor, Color vertexColor,
		Transform transform,
		float duration = 0, bool depthTest = false
	) {
		if ( Vertices.Count != 0 ) {
			var p0 = ( Vector2 ) transform.TransformPoint ( Vertices [Vertices.Count - 1] );

			for ( int i = 0 ; i < Vertices.Count ; i++ ) {
				var p1 = ( Vector2 ) transform.TransformPoint ( Vertices [i] );
				Debug.DrawLine ( p0, p1, edgeColor, duration, depthTest );
				DebugHelper.DrawCircle ( p0, 0.01f, vertexColor, 6 );

				p0 = p1;
			}
		}
	}
}
