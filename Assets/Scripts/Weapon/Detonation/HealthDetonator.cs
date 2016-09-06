using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HealthDetonator : MonoBehaviour {
	public float DetonationDelay = 0;

	public bool IsDetonated { get; private set; }
	private float detonationMessageTimestamp = float.NaN;

	void FixedUpdate () {
		if ( !enabled || float.IsNaN ( detonationMessageTimestamp ) || IsDetonated )
			return;

		ProcessDetonationDelay ();
	}

	void OnDamageAccounted ( DamageAccountedMessageData data ) {
		if ( !enabled || !float.IsNaN ( detonationMessageTimestamp ) || IsDetonated )
			return;

		if ( data.NewHealth <= 0 && data.OldHealth > 0 ) {
			detonationMessageTimestamp = Time.fixedTime;
			ProcessDetonationDelay ();
		}
	}

	private void ProcessDetonationDelay () {
		float timeElapsed = Time.fixedTime - detonationMessageTimestamp;

		if ( timeElapsed >= DetonationDelay ) {
			var messageData = new DetonateMessageData ( transform.position, this );
			BroadcastMessage ( DetonateMessageData.MessageName, messageData, SendMessageOptions.DontRequireReceiver );
			IsDetonated = true;
		}
	}
}
