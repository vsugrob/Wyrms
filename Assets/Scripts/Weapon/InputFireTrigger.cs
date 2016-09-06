using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InputFireTrigger : MonoBehaviour {
	public string FireInputAxis = "Fire1";

	void Start () {
		if ( !InputReceiverSwitch.CheckInputActiveInHierarchy ( this ) )
			this.enabled = false;
	}

	void Update () {
		if ( Input.GetAxis ( FireInputAxis ) != 0 )
			SendMessage ( CarriedWeapon.OnFireTriggeredMessageName, SendMessageOptions.RequireReceiver );
	}

	void OnEnableReceiveInput ( bool enable ) {
		this.enabled = enable;
	}
}
