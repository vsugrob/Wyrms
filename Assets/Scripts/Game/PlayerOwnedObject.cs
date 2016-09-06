using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerOwnedObject : MonoBehaviour {
	private static List <PlayerOwnedObject> allOwnableObjects = new List <PlayerOwnedObject> ();
	public static IEnumerable <PlayerOwnedObject> AllOwnableObjects {
		get {
			foreach ( var obj in allOwnableObjects ) {
				yield return	obj;
			}
		}
	}

	public Player Owner;
	/* TODO: implement.
	 * This is essential part for calculation of damage statistics.
	 * OwningObject is the unit that inflicted damage. */
	public GameObject OwningObject;

	// This method accompanies PlayerOwnedObjectExt.IsOwnedBy () method overloads.
	public bool IsOwnedBy ( Player player ) {
		return	Owner == player;
	}

	void OnEnable () {
		allOwnableObjects.Add ( this );
	}

	void OnDisable () {
		allOwnableObjects.Remove ( this );
	}

	public static bool InheritOwner ( GameObject srcObject, GameObject dstObject ) {
		var srcOwnedObj = srcObject.GetComponentInParent <PlayerOwnedObject> ();

		if ( srcOwnedObj == null )
			return	false;

		var dstRootOwnedObj = dstObject.GetComponent <PlayerOwnedObject> ();

		if ( dstRootOwnedObj == null )
			dstRootOwnedObj = dstObject.AddComponent <PlayerOwnedObject> ();

		dstRootOwnedObj.Owner = srcOwnedObj.Owner;

		var dstNestedOwnedObjects = dstObject.GetComponentsInChildren <PlayerOwnedObject> ();

		foreach ( var nestedOwnedObj in dstNestedOwnedObjects ) {
			if ( nestedOwnedObj == dstRootOwnedObj )
				continue;

			nestedOwnedObj.Owner = srcOwnedObj.Owner;
		}

		return	true;
	}
}

public static class PlayerOwnedObjectExt {
	public static bool IsOwnedBy ( this GameObject obj, Player player ) {
		var ownedObj = obj.GetComponent <PlayerOwnedObject> ();

		return	ownedObj != null && ownedObj.Owner == player;
	}

	public static bool IsOwnedBy ( this MonoBehaviour component, Player player ) {
		var ownedObj = component.GetComponent <PlayerOwnedObject> ();

		return	ownedObj != null && ownedObj.Owner == player;
	}

	public static Player GetOwningPlayer ( this GameObject obj ) {
		var ownedObj = obj.GetComponentInParent <PlayerOwnedObject> ();

		return	ownedObj != null ? ownedObj.Owner : null;
	}

	public static Player GetOwningPlayer ( this Component component ) {
		var ownedObj = component.GetComponentInParent <PlayerOwnedObject> ();

		return	ownedObj != null ? ownedObj.Owner : null;
	}
}
