using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class TimerDetonatorFloatingText : MonoBehaviour {
	public FloatingText FloatingText;

	private TimerDetonator detonator;

	void Awake () {
		detonator = GetComponent <TimerDetonator> ();

		if ( FloatingText == null ) {
			FloatingText = gameObject.AddComponent <FloatingText> ();
			FloatingText.InheritOwnerColor = true;
		}
	}

	void Update () {
		if ( FloatingText != null ) {
			FloatingText.enabled = detonator.enabled && !detonator.IsDetonated;

			if ( FloatingText.enabled ) {
				string str = detonator.TimeLeft.ToString ( "F2" );

				if ( FloatingText.Text != str )
					FloatingText.Text = str;
			}
		}
	}
}
