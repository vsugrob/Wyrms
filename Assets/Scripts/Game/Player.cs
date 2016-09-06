using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Player : MonoBehaviour {
	private static List <Player> allPlayers = new List <Player> ();
	public static IEnumerable <Player> AllPlayers {
		get {
			foreach ( var obj in allPlayers ) {
				yield return	obj;
			}
		}
	}

	// TODO: implement character coloring.
	public Color Color = Color.green;
	public GameObject HeadstonePrefab;

	public IEnumerable <PlayerOwnedObject> OwnedObjects {
		get {
			foreach ( var obj in PlayerOwnedObject.AllOwnableObjects ) {
				if ( obj.Owner == this )
					yield return	obj;
			}
		}
	}

	void OnEnable () {
		allPlayers.Add ( this );
	}

	void OnDisable () {
		allPlayers.Remove ( this );
	}

	public void ReleaseUserControl () {
		foreach ( var obj in OwnedObjects ) {
			InputReceiverSwitch.SetReceiveInput ( obj.gameObject, false );
		}
	}
}
