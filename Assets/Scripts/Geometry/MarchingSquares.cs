using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MarchingSquares {
	public float AlphaThreshold { get; set; }
	private int width;
	private int height;
	private int numPixels;
	private Color [] pixels;
	private Dictionary <int, Polygon> contoursByPoints = new Dictionary <int, Polygon> ();

	private static readonly Direction [] ccwDirMap;
	private static readonly Direction [] cwDirMap;

	private bool clockWise;
	private Direction [] dirMap;

	static MarchingSquares () {
		ccwDirMap = new Direction [16];
		ccwDirMap [0] = Direction.None;		// Illegal
		ccwDirMap [1] = Direction.Up;
		ccwDirMap [2] = Direction.Right;
		ccwDirMap [3] = Direction.Right;
		ccwDirMap [4] = Direction.Left;
		ccwDirMap [5] = Direction.Up;
		ccwDirMap [6] = Direction.None;		// Ambiguous
		ccwDirMap [7] = Direction.Right;
		ccwDirMap [8] = Direction.Down;
		ccwDirMap [9] = Direction.None;		// Ambiguous
		ccwDirMap [10] = Direction.Down;
		ccwDirMap [11] = Direction.Down;
		ccwDirMap [12] = Direction.Left;
		ccwDirMap [13] = Direction.Up;
		ccwDirMap [14] = Direction.Left;
		ccwDirMap [15] = Direction.None;	// Illegal

		cwDirMap = new Direction [16];
		cwDirMap [0] = Direction.None;		// Illegal
		cwDirMap [1] = Direction.Left;
		cwDirMap [2] = Direction.Up;
		cwDirMap [3] = Direction.Left;
		cwDirMap [4] = Direction.Down;
		cwDirMap [5] = Direction.Down;
		cwDirMap [6] = Direction.None;		// Ambiguous
		cwDirMap [7] = Direction.Down;
		cwDirMap [8] = Direction.Right;
		cwDirMap [9] = Direction.None;		// Ambiguous
		cwDirMap [10] = Direction.Up;
		cwDirMap [11] = Direction.Left;
		cwDirMap [12] = Direction.Right;
		cwDirMap [13] = Direction.Right;
		cwDirMap [14] = Direction.Up;
		cwDirMap [15] = Direction.None;	// Illegal
	}

	public MarchingSquares ( Color [] pixels, int width, int height, float alphaThreshold = 0.5f, bool yDownXRightClockWise = true ) {
		this.width = width;
		this.height = height;
		this.numPixels = pixels.Length;
		this.pixels = pixels;
		this.AlphaThreshold = alphaThreshold;
		this.clockWise = yDownXRightClockWise;
		this.dirMap = yDownXRightClockWise ? cwDirMap : ccwDirMap;
	}

	public MarchingSquares (
		Texture2D texture,
		int x = 0, int y = 0, int width = -1, int height = -1,
		float alphaThreshold = 0.5f,
		bool clockWise = true,
		int mipLevel = 0
	) {
		this.width = width < 0 ? Mathf.Max ( 1, texture.width >> mipLevel ) : width;
		this.height = height < 0 ? Mathf.Max ( 1, texture.height >> mipLevel ) : height;
		this.pixels = texture.GetPixels ( x, y, this.width, this.height, mipLevel );
		this.numPixels = this.pixels.Length;
		this.AlphaThreshold = alphaThreshold;
		this.clockWise = clockWise;
		this.dirMap = clockWise ? cwDirMap : ccwDirMap;
	}

	public bool IsSolid ( int x, int y ) {
		if ( x < 0 || y < 0 || x >= width || y >= height )
			return	false;

		var pixel = pixels [y * width + x];

		return	pixel.a >= AlphaThreshold;
	}

	public bool IsSolidByIndex ( int index ) {
		if ( index < 0 || index >= numPixels )
			return	false;

		var pixel = pixels [index];

		return	pixel.a >= AlphaThreshold;
	}

	public PolygonPath TraceContours () {
		float traceStart = Time.realtimeSinceStartup;

		var polyPath = new PolygonPath ();
		var polysWithHoles = polyPath.PolysWithHoles;
		contoursByPoints = new Dictionary <int, Polygon> ();
		int i = 0;

		for ( int y = 0 ; y < height ; y++ ) {
			bool prevWasSolid = false;
			PolygonWithHoles container = null;

			for ( int x = 0 ; x < width ; x++, i++ ) {
				bool curIsSolid = IsSolidByIndex ( i );

				if ( prevWasSolid != curIsSolid ) {
					var contour = GetContourByPoint ( x, y );

					if ( contour == null ) {
						if ( curIsSolid ) {
							// This is the beginning of new outer contour.
							contour = TraceContour ( x, y );
							container = new PolygonWithHoles ( contour );
							contour.Container = container;
							polysWithHoles.Add ( container );
						} else {
							// This is a new hole. It must be traced and linked with current container.
							contour = TraceContour ( x, y );
							contour.Container = container;
							container.Holes.Add ( contour );
							container = null;
						}
					} else {
						if ( curIsSolid ) {
							/* We've entered a contour.
							 * When contour is an outer -> store contour container in container var.
							 * When contour is a hole -> do the same thing. */

							container = contour.Container;
						} else {
							/* We've exited a contour.
							 * When contour is an outer -> set container to null.
							 * When contour is a hole -> do the same thing. */

							container = null;
						}
					}
				}

				prevWasSolid = curIsSolid;
			}
		}

		float traceTime = Time.realtimeSinceStartup - traceStart;

		// DEBUG
		if ( false ) {
			Debug.Log (
				"MarchingSquares.TraceContours ()" +
				" time: " + traceTime +
				", width: " + width +
				", height: " + height +
				", numPixels: " + numPixels +
				", pixels per sec: " + ( numPixels / traceTime ).ToString ( "F" )
			);
		}

		return	polyPath;
	}

	private Polygon TraceContour ( int sx, int sy ) {
		var pts = new List <Vector2> ();
		var poly = new Polygon ( pts );
		int x = sx, y = sy;
		var dir = Direction.None;
		var prevDir = Direction.None;
		int prevPattern = 0;	// DEBUG
		int pattern = 0;			// DEBUG

		do {
			//int pattern = GetPattern ( x, y );

			// DEBUG
			prevPattern = pattern;
			pattern = GetPattern ( x, y );

			if ( pattern == 0 || pattern == 15 ) {
				throw new System.InvalidOperationException (
					"Marching squares algorithm ran into invalid state " +
					" when pattern=" + pattern + ". Values 0 and 15 are not valid." +
					" This might be caused either by wrong arguments sx=" + sx + " and sy=" + sy +
					" or unstable function that checks whether point is solid." +
					" prevPattern: " + prevPattern + ", dir: " + dir		// DEBUG
				);
			} else if ( pattern == 6 ) {
				if ( clockWise )
					dir = prevDir == Direction.Right ? Direction.Up : Direction.Down;
				else
					dir = prevDir == Direction.Up ? Direction.Right : Direction.Left;
			} else if ( pattern == 9 ) {
				if ( clockWise )
					dir = prevDir == Direction.Up ? Direction.Left : Direction.Right;
				else
					dir = prevDir == Direction.Right ? Direction.Down : Direction.Up;
			} else
				dir = dirMap [pattern];

			SetContourByPoint ( x, y, poly );

			if ( dir != prevDir )
				pts.Add ( new Vector2 ( x, y ) );

			prevDir = dir;

			if ( dir == Direction.Right )
				x++;
			else if ( dir == Direction.Up )
				y--;
			else if ( dir == Direction.Left )
				x--;
			else /*if ( dir == Direction.Down )*/
				y++;
		} while ( x != sx || y != sy );

		return	poly;
	}

	private int GetPattern ( int x, int y ) {
		var pattern = 0;
		
		if ( IsSolid ( x - 1, y - 1 ) )
			pattern |= 1;

		if ( IsSolid ( x, y - 1 ) )
			pattern |= 2;

		if ( IsSolid ( x - 1, y ) )
			pattern |= 4;

		if ( IsSolid ( x, y ) )
			pattern |= 8;

		return	pattern;
	}

	private Polygon GetContourByPoint ( int x, int y ) {
		Polygon poly;
		contoursByPoints.TryGetValue ( x & 0xffff | ( y << 16 ), out poly );
		
		return	poly;
	}

	private void SetContourByPoint ( int x, int y, Polygon contour ) {
		contoursByPoints [x & 0xffff | ( y << 16 )] = contour;
	}
}
