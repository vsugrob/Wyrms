using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InputReceiverSwitch : MonoBehaviour {
	const string OnEnableReceiveInputMessageName = "OnEnableReceiveInput";
	private bool receiveInput = true;
	public bool ReceiveInput {
		get {
			return	receiveInput;
		}
		set {
			receiveInput = value;
			BroadcastEnableReceive ( gameObject, value );
		}
	}

	public static void SetReceiveInput ( GameObject rootObject, bool receive ) {
		var inputSwitch = rootObject.GetComponent <InputReceiverSwitch> ();

		if ( inputSwitch != null )
			inputSwitch.ReceiveInput = receive;
		else
			BroadcastEnableReceive ( rootObject, receive );
	}

	private static void BroadcastEnableReceive ( GameObject rootObject, bool receive ) {
		rootObject.BroadcastMessage ( OnEnableReceiveInputMessageName, receive, SendMessageOptions.DontRequireReceiver );
	}

	public static bool CheckInputActiveInHierarchy ( MonoBehaviour behaviour ) {
		var inputSwitch = behaviour.GetComponentInParent <InputReceiverSwitch> ();
		// TODO: check nested switches?

		if ( inputSwitch == null )
			return	true;
		else
			return	inputSwitch.receiveInput;
	}
}
