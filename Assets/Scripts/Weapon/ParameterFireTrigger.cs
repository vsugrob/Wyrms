using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class ParameterFireTrigger : MonoBehaviour {
	public string ParameterName;

	private float paramChangedTimestamp;
	private bool performFire = false;

	void FixedUpdate () {
		if ( performFire && Time.fixedTime > paramChangedTimestamp ) {
			performFire = false;
			SendMessage ( CarriedWeapon.OnFireTriggeredMessageName, SendMessageOptions.RequireReceiver );
		}
	}

	void OnParameterConfigured ( ConfigurationParameter parameter ) {
		if ( !enabled )
			return;

		if ( parameter.Name == ParameterName ) {
			/* Give parameter opportunity to propagate,
			 * otherwise weapon which is going to receive OnFireTriggered
			 * message might miss updated value. */
			performFire = true;
			paramChangedTimestamp = Time.fixedTime;
		}
	}
}
