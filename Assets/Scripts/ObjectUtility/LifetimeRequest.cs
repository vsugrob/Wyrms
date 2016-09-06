using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LifetimeRequest {
	public const string MessageName = "OnLifetimeRequest";
	private float minLifetime = float.PositiveInfinity;
	public float MinLifetime {
		get { return	minLifetime; }
		set {
			if ( value < minLifetime )
				minLifetime = value;
		}
	}

	public static LifetimeRequest Query ( GameObject gameObject ) {
		var request = new LifetimeRequest ();
		gameObject.SendMessage ( MessageName, request, SendMessageOptions.DontRequireReceiver );

		return	request;
	}

	public static IEnumerable <ObjectLifetime> Query ( IEnumerable <GameObject> gameObjects ) {
		foreach ( var gameObject in gameObjects ) {
			var response = Query ( gameObject );

			yield return	new ObjectLifetime ( gameObject, response );
		}
	}

	public class ObjectLifetime {
		public GameObject GameObject;
		public LifetimeRequest Response;
		public float MinLifetime { get { return	Response.MinLifetime; } }

		public ObjectLifetime ( GameObject gameObject, LifetimeRequest response ) {
			this.GameObject = gameObject;
			this.Response = response;
		}
	}
}
