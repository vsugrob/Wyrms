using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent ( typeof ( ForcemeterConfigurator ) )]
public class ForcemeterConfiguratorInputController : MonoBehaviour {
	public string GainAxis = "Fire1";

	private ForcemeterConfigurator configurator;

	void Start () {
		configurator = GetComponent <ForcemeterConfigurator> ();
		
		if ( !InputReceiverSwitch.CheckInputActiveInHierarchy ( this ) )
			this.enabled = false;
	}

	void Update () {
		configurator.GainInput = Input.GetAxis ( GainAxis ) != 0;
	}

	void OnEnableReceiveInput ( bool enable ) {
		this.enabled = enable;
	}

	void OnDisable () {
		configurator.GainInput = false;
		configurator.Cancel ();
	}
}
