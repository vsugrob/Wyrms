using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SceneViewLabel : MonoBehaviour {
	public string Text = "";
	public Vector3 Offset;
	public bool WriteLog = true;

	[HideInInspector]
	public string PreviousText = "";

	public Vector3 Origin { get { return	transform.position + Offset; } }

	public static void SetText ( GameObject gameObject, string text ) {
		var label = gameObject.GetComponent <SceneViewLabel> ();

		if ( label == null )
			label = gameObject.AddComponent <SceneViewLabel> ();

		label.Text = text;
	}
}
