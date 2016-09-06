using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class Forcemeter : MonoBehaviour {
	public AudioSource AudioSource;
	public float StartPitch = 0.5f;
	public float EndPitch = 2.5f;
	public GlobalWind GlobalWind;

	private Animator animator;
	private Transform headingTransform;

	void Awake () {
		if ( !Application.isPlaying )
			AudioSource = gameObject.GetComponent <AudioSource> ();

		animator = GetComponentInChildren <Animator> ();

		// Find heading transform.
		headingTransform = transform.FindChild ( "Heading Transform" );

		if ( headingTransform == null ) {
			if ( transform.childCount != 0 )
				headingTransform = transform.GetChild ( 0 );
			else
				headingTransform = transform;
		}
	}

	void Start () {
		if ( !Application.isPlaying ) {
			if ( AudioSource != null )
				AudioSource.playOnAwake = false;

			return;
		}

		if ( GlobalWind != null ) {
			// Will be controlled by force field.
			Begin ();
		} else {
			// Will be controlled manually.
			Common.SetVisibility ( gameObject, false );
		}
	}

	void FixedUpdate () {
		if ( GlobalWind != null && GlobalWind.ForceField != null ) {
			var forceField = GlobalWind.ForceField;
			float normalizedProgress = forceField.Amount / GlobalWind.MaxAmount;
			SetProgress ( normalizedProgress, forceField.Direction );
		}
	}

	public void Begin () {
		if ( AudioSource != null )
			AudioSource.Play ();

		SetProgress ( 0 );
		Common.SetVisibility ( gameObject, true );
	}

	public void End () {
		if ( AudioSource != null )
			AudioSource.Stop ();

		Common.SetVisibility ( gameObject, false );
	}

	public void SetProgress ( float normalizedValue, Vector2 direction ) {
		int sign = System.Math.Sign ( normalizedValue );

		if ( sign < 0 )
			normalizedValue = -normalizedValue;

		transform.localRotation = Common.RotateAlongDirection ( direction );

		if ( sign != 0 )
			headingTransform.localScale = new Vector3 ( sign, 1, 1 );

		if ( AudioSource != null )
			AudioSource.pitch = Mathf.Lerp ( StartPitch, EndPitch, normalizedValue );

		animator.Play ( "GainForce", -1, normalizedValue );
	}

	public void SetProgress ( float normalizedValue ) {
		SetProgress ( normalizedValue, Vector2.right );
	}
}
