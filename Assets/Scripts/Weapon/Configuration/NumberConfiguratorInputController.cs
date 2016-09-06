using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent ( typeof ( NumberConfigurator ) )]
public class NumberConfiguratorInputController : MonoBehaviour {
	private NumberConfigurator configurator;

	void Start () {
		configurator = GetComponent <NumberConfigurator> ();

		if ( !InputReceiverSwitch.CheckInputActiveInHierarchy ( this ) )
			this.enabled = false;
	}

	void Update () {
		for ( int i = 0 ; i <= 9 ; i++ ) {
			var keyCode = KeyCode.Alpha0 + i;

			if ( Input.GetKeyDown ( keyCode ) )
				configurator.NumberInput = i;
		}
	}

	void OnEnableReceiveInput ( bool enable ) {
		this.enabled = enable;
	}

	void OnDisable () {
		configurator.NumberInput = float.NaN;
	}
}
