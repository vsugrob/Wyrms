using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent ( typeof ( RotationConfigurator ) )]
public class RotationConfiguratorInputController : MonoBehaviour {
	public string RotateAxis = "Vertical";

	private RotationConfigurator configurator;

	void Start () {
		configurator = GetComponent <RotationConfigurator> ();

		if ( !InputReceiverSwitch.CheckInputActiveInHierarchy ( this ) )
			this.enabled = false;
	}

	void Update () {
		configurator.RotationInput = Input.GetAxis ( RotateAxis );
	}

	void OnEnableReceiveInput ( bool enable ) {
		this.enabled = enable;
	}

	void OnDisable () {
		configurator.RotationInput = 0;
	}
}
