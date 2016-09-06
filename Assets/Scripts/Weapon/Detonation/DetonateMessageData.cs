using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DetonateMessageData {
	public const string MessageName = "OnDetonate";
	public Vector2 Position;
	public MonoBehaviour Detonator;

	public DetonateMessageData ( Vector2 position, MonoBehaviour detonator ) {
		this.Position = position;
		this.Detonator = detonator;
	}
}
