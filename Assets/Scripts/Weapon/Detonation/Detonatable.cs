using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class Detonatable : MonoBehaviour {
	public GameObject ExplosionPrefab;
	public bool InheritRotation = true;
	public GameObject VisualToHide;
	public ObjectTermination TerminationSettings = new ObjectTermination ();

	public bool IsDestroyed { get; private set; }

	void Start () {
#if UNITY_EDITOR
		if ( !Application.isPlaying && VisualToHide == null )
			VisualToHide = gameObject;
#endif
	}

	void OnDetonate ( DetonateMessageData messageData ) {
		if ( !enabled || IsDestroyed )
			return;

		Explode ( messageData.Position );
	}

	public bool Explode () {
		return	Explode ( transform.position );
	}

	public bool Explode ( Vector2 spawnPosition ) {
		if ( IsDestroyed )
			return	false;

		if ( ExplosionPrefab != null ) {
			var rotation = InheritRotation ? transform.rotation : Quaternion.identity;
			var explosion = Instantiate ( ExplosionPrefab, spawnPosition, rotation ) as GameObject;
			explosion.transform.parent = transform.parent;	// TODO: spawn explosion on layer dedicated to explosions!
			PlayerOwnedObject.InheritOwner ( this.gameObject, explosion );
		}

		TerminationSettings.Terminate ( gameObject );
		IsDestroyed = true;

		/* Hide visual part of the object starting from appropriate root.
		 * This allows to hide projectile and at the same time to keep showing particle systems. */
		var objectToHide = VisualToHide != null ? VisualToHide : gameObject;
		Common.SetVisibility ( objectToHide, false );

		return	true;
	}
}
