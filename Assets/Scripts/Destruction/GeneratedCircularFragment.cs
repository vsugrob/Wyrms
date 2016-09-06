using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
[RequireComponent ( typeof ( MeshFilter ) )]
public class GeneratedCircularFragment : MonoBehaviour {
	public const string DefaultShaderName = "Sprites/Default";
	public int NumVertices = 12;
	public float Radius = 0.1f;
	public float RadiusRandomDeviation = 0.2f;
	public float VertexRadiusJitter = 0.3f;
	public float VertexAngleJitter = 0.3f;
	public bool RandomizeOnStart = true;
	public bool CreateCircleCollider = true;
	public bool CreatePolygonCollider = false;
	public PhysicsMaterial2D PhysicsMaterial;
	public bool ScaleTextureCoords = true;
	public float PixelsPerUnit = 100;

	[SerializeField, HideInInspector]
	private Vector3 [] generatedVertices;
	[SerializeField, HideInInspector]
	private Mesh generatedMesh;
	[SerializeField, HideInInspector]
	private CircleCollider2D generatedCircleCollider;
	[SerializeField, HideInInspector]
	private PolygonCollider2D generatedPolygonCollider;
	[SerializeField, HideInInspector]
	private bool polyColliderSynched;
	[SerializeField, HideInInspector]
	private float averageRadius;

	void Awake () {
		if ( !Application.isPlaying && generatedMesh != null ) {
			var components = FindObjectsOfType <GeneratedCircularFragment> ()
				.Where ( component => component.generatedMesh == this.generatedMesh );

			if ( components.Count () > 1 )
				generatedMesh = null;
		}
	}

	void Start () {
		if ( ( Application.isPlaying && RandomizeOnStart ) || generatedMesh == null )
			Generate ();
	}

	void Update () {
		if ( !Application.isPlaying )
			UpdateColliders ();
	}

	void OnDestroy () {
		DestroyGeneratedMesh ();
	}

	private void DestroyGeneratedMesh () {
		if ( generatedMesh != null )
			Common.Destroy ( generatedMesh );
	}

	private void Generate () {
		DestroyGeneratedMesh ();

		var meshFilter = GetComponent <MeshFilter> ();
		var mesh = GenerateMesh ();
		meshFilter.mesh = mesh;
		generatedMesh = mesh;

		UpdateColliders ();
	}

	private void UpdateColliders () {
		if ( CreateCircleCollider ) {
			if ( generatedCircleCollider == null ) {
				var circleCollider = GetComponent <CircleCollider2D> ();

				if ( circleCollider == null )
					circleCollider = gameObject.AddComponent <CircleCollider2D> ();

				generatedCircleCollider = circleCollider;
			}

			generatedCircleCollider.radius = averageRadius;
			generatedCircleCollider.sharedMaterial = PhysicsMaterial;
		} else if ( generatedCircleCollider != null )
			Common.Destroy ( generatedCircleCollider );

		if ( CreatePolygonCollider ) {
			if ( !polyColliderSynched ) {
				if ( generatedPolygonCollider == null ) {
					var polygonCollider = GetComponent <PolygonCollider2D> ();

					if ( polygonCollider == null )
						polygonCollider = gameObject.AddComponent <PolygonCollider2D> ();

					generatedPolygonCollider = polygonCollider;
				}

				var colliderVertices = new Vector2 [NumVertices];

				for ( int i = 0 ; i < NumVertices ; i++ ) {
					colliderVertices [i] = generatedVertices [i];
				}

				generatedPolygonCollider.pathCount = 1;
				generatedPolygonCollider.SetPath ( 0, colliderVertices );
				generatedPolygonCollider.sharedMaterial = PhysicsMaterial;
				polyColliderSynched = true;
			}
		} else if ( generatedPolygonCollider != null )
			Common.Destroy ( generatedPolygonCollider );
	}

	[ContextMenu ( "Generate" )]
	private void GenerateContextMenu () {
		Generate ();
	}

	private Mesh GenerateMesh () {
		float deviatedRadius = Radius * ( 1 + Random.value * RadiusRandomDeviation );
		generatedVertices = new Vector3 [NumVertices + 1];
		float dAngle = Mathf.PI * 2 / NumVertices;
		float angle = 0;
		averageRadius = 0;

		for ( int i = 0 ; i < NumVertices ; i++, angle += dAngle ) {
			float displacementFactor = 1 + Random.Range ( -VertexRadiusJitter, VertexRadiusJitter );
			float vertexRadius = displacementFactor * deviatedRadius;
			float displacedAngle = angle + Random.Range ( -VertexAngleJitter, VertexAngleJitter ) * dAngle;
			var v = new Vector3 (
				Mathf.Cos ( displacedAngle ) * vertexRadius,
				Mathf.Sin ( displacedAngle ) * vertexRadius,
				0
			);

			generatedVertices [i] = v;
			averageRadius += vertexRadius;
		}

		generatedVertices [NumVertices] = Vector3.zero;
		polyColliderSynched = false;
		averageRadius /= NumVertices;

		var triangles = new int [NumVertices * 3];

		for ( int i = NumVertices - 1, j = 0, k = 0 ; j < NumVertices ; i = j, j++ ) {
			triangles [k++] = j;
			triangles [k++] = i;
			triangles [k++] = NumVertices;
		}

		float maxRadius = deviatedRadius * ( 1 + VertexRadiusJitter );
		var minExtent = new Vector2 ( -maxRadius, -maxRadius );
		float maxDiameterInv = 1 / ( maxRadius * 2 );
		var uv = new Vector2 [generatedVertices.Length];

		for ( int i = 0 ; i < NumVertices ; i++ ) {
			var v = generatedVertices [i];
			var texCoord = ( ( Vector2 ) v - minExtent ) * maxDiameterInv;
			uv [i] = texCoord;
		}

		uv [NumVertices] = new Vector2 ( 0.5f, 0.5f );

		if ( ScaleTextureCoords ) {
			var renderer = this.renderer;

			if ( renderer != null ) {
				var material = renderer.sharedMaterial;

				if ( material != null ) {
					var texture = material.mainTexture;

					if ( texture != null ) {
						float worldScale = transform.TransformVector ( Vector3.right ).magnitude;

						var texSize = new Vector2 ( texture.width, texture.height );
						texSize *= worldScale / PixelsPerUnit;

						float maxDiameterWorld = maxRadius * 2 * worldScale;

						var uvScale = new Vector2 (
							maxDiameterWorld / texSize.x,
							maxDiameterWorld / texSize.y
						);

						var translateRange = Vector2.one - uvScale;
						var uvOffset = new Vector2 (
							translateRange.x * Random.value,
							translateRange.y * Random.value
						);

						for ( int i = 0 ; i < uv.Length ; i++ ) {
							uv [i].Scale ( uvScale );
							uv [i] += uvOffset;
						}
					}
				}
			}
		}

		var normals = new Vector3 [generatedVertices.Length];

		for ( int i = 0 ; i < normals.Length ; i++ ) {
			normals [i] = -Vector3.forward;
		}

		var mesh = new Mesh ();
		mesh.name = "FragmentMesh (Generated, Id: " + mesh.GetInstanceID () + ")";
		mesh.vertices = generatedVertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		mesh.normals = normals;

		return	mesh;
	}
}
