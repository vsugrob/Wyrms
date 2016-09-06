using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class ParallaxDisplacement : MonoBehaviour {
	public Transform Viewer;
	public float DisplacementRatio = 2f;

	private Vector3 originalPosition;

	void Start () {
		originalPosition = transform.position;
	}

	void Update () {
		Vector3 newPos;

		if ( Viewer != null ) {
			var displ = ( Vector2 ) Viewer.position - ( Vector2 ) originalPosition;
			displ /= DisplacementRatio;

			newPos = originalPosition + ( Vector3 ) displ;
		} else
			newPos = originalPosition;

		if ( transform.position != newPos )
			transform.position = newPos;
	}
}
