using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WheelInputController : MonoBehaviour {
	public string TorqueAxis = "Torque";
	private WheelController wheelController;

	void Awake () {
		wheelController = GetComponent <WheelController> ();
	}
	
	void Update () {
		wheelController.DesiredMotorTorque = Input.GetAxis ( TorqueAxis );
	}
}
