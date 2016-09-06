using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class FloatingText : MonoBehaviour {
	public Color Color = Color.green;
	public bool InheritOwnerColor = false;
	public Color OutlineColor = Color.black;
	public bool DrawOutline = false;
	[Multiline]
	public string Text;
	public Vector2 Offset = new Vector2 ( 0, 0.5f );
	public float Duration = float.PositiveInfinity;
	public bool IsMoving = false;
	public Vector2 EndOffset = new Vector2 ( 0, 1.5f );
	public float StartOpacity = 1;
	public float EndOpacity = 0;
	public bool Centered = true;
	public GUIStyle Style;

	public Vector2 Origin { get { return	( Vector2 ) transform.position + Offset; } }
	public Vector2 EndOrigin { get { return	( Vector2 ) transform.position + EndOffset; } }
	private GUIContent Content { get { return	GetColoredContent ( Color ); } }
	public Rect ScreenRect {
		get {
			var content = this.Content;
			var rect = CalcScreenRect ( content );

			return	rect;
		}
	}

	void Start () {
		FetchPlayerColor ();
	}

	void Update () {
		if ( !Application.isPlaying ) {
			FetchPlayerColor ();

			return;
		}

		if ( float.IsInfinity ( Duration ) )
			return;

		if ( Duration <= 0 ) {
			Destroy ( this );
			
			return;
		}

		if ( IsMoving ) {
			var dOffset = EndOffset - Offset;
			float velocity = dOffset.magnitude / Duration;
			Offset = Vector2.MoveTowards ( Offset, EndOffset, velocity * Time.deltaTime );
		}

		if ( Color.a != EndOpacity ) {
			float dOpacity = EndOpacity - Color.a;
			float velocity = Mathf.Abs ( dOpacity / Duration );
			Color.a = Mathf.MoveTowards ( Color.a, EndOpacity, velocity * Time.deltaTime );
		}

		Duration -= Time.deltaTime;
	}

	private static Vector2 [] radialOffsets = new [] {
		new Vector2 ( 1, 0 ),
		new Vector2 ( 1, 1 ),
		new Vector2 ( 0, 1 ),
		new Vector2 ( -1, 1 ),
		new Vector2 ( -1, 0 ),
		new Vector2 ( -1, -1 ),
		new Vector2 ( 0, -1 ),
		new Vector2 ( 1, -1 ),
	};

	void OnGUI () {
		if ( Style == null ) {
			Style = new GUIStyle ( GUI.skin.label );
			Style.name = gameObject.name + "'s style";
			Style.richText = true;
			Style.fontSize = 12;
		}

		if ( DrawOutline ) {
			var outlineContent = GetColoredContent ( OutlineColor );
			var outlineRect = CalcScreenRect ( outlineContent );

			for ( int i = 0 ; i < radialOffsets.Length ; i++ ) {
				var displacedRect = outlineRect;
				displacedRect.position += radialOffsets [i];
				GUI.Label ( displacedRect, outlineContent, Style );
			}
		}

		var content = this.Content;
		var rect = CalcScreenRect ( content );
		GUI.Label ( rect, content, Style );
	}

	public GUIContent GetColoredContent ( Color color ) {
		string richText = string.Format (
			"<color=#{0}>{1}</color>",
			color.ToHex (),
			Text
		);

		return	new GUIContent ( richText );
	}

	private Rect CalcScreenRect ( GUIContent content ) {
		var mainCamera = Camera.main;

		if ( mainCamera != null ) {
			var screenPos = ( Vector2 ) mainCamera.WorldToScreenPoint ( Origin );
			screenPos.y = mainCamera.pixelHeight - screenPos.y;

			var size = Style.CalcSize ( content );

			if ( Centered )
				screenPos -= size / 2;

			return	new Rect ( screenPos.x, screenPos.y, size.x, size.y );
		} else
			return	new Rect ();
	}

	private void FetchPlayerColor () {
		if ( InheritOwnerColor ) {
			var owningPlayer = this.GetOwningPlayer ();

			if ( owningPlayer != null && this.Color != owningPlayer.Color )
				this.Color = owningPlayer.Color;
		}
	}
}
