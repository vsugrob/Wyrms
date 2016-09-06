using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class ForceReceiver : MonoBehaviour {
	public ForceMaterial Material;

	public static ForceMaterial GetMaterial ( GameObject gameObject ) {
		var receiver = gameObject.GetComponent <ForceReceiver> ();

		if ( receiver == null || receiver.Material == null )
			return	DefaultSettings.Singleton.ForceReceiverMaterialOrDefault;

		return	receiver.Material;
	}

	public static float GetInfluence ( GameObject gameObject, ForceKind forceKind ) {
		var material = GetMaterial ( gameObject );

		return	material.GetInfluence ( forceKind );
	}
}