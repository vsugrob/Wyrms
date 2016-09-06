using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TurnOrderComponent : MonoBehaviour {
	public int Order;

	public static int GetOrderIndex ( GameObject gameObject ) {
		var component = gameObject.GetComponent <TurnOrderComponent> ();

		return	component != null ? component.Order : -1;
	}

	public static int GetOrderIndex ( Component component ) {
		return	GetOrderIndex ( component.gameObject );
	}

	public static bool TryGetOrderIndex ( GameObject gameObject, out int index ) {
		var component = gameObject.GetComponent <TurnOrderComponent> ();

		if ( component == null ) {
			index = -1;

			return	false;
		} else {
			index = component.Order;

			return	true;
		}
	}

	public static bool TryGetOrderIndex ( Component component, out int index ) {
		return	TryGetOrderIndex ( component.gameObject, out index );
	}

	public static bool IsOrdered ( GameObject gameObject ) {
		return	gameObject.GetComponent <TurnOrderComponent> () != null;
	}

	public static bool IsOrdered ( Component component ) {
		return	IsOrdered ( component.gameObject );
	}
}
