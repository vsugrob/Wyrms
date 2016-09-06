using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TimerDetonator : MonoBehaviour {
	public string DelayParameterName = "Delay";
	public float Delay = 3;
	public float DelayRandomDeviation = 0;

	public float InstanceDelay { get; private set; }
	public bool IsDetonated { get; private set; }
	private float createdTimestamp;
	public float TimeElapsed { get { return	Time.fixedTime - createdTimestamp; } }
	public float TimeLeft { get { return	InstanceDelay - TimeElapsed; } }

	// TODO: make base class FixedMonoBehaviour : MonoBehaviour.
	bool isFixedStarted;

	void FixedStart () {
		createdTimestamp = Time.fixedTime;
		InstanceDelay = Delay + Random.Range ( -DelayRandomDeviation, DelayRandomDeviation );
	}
	
	void FixedUpdate () {
		if ( !isFixedStarted ) {
			FixedStart ();
			isFixedStarted = true;
		}

		if ( IsDetonated )
			return;

		if ( TimeElapsed >= InstanceDelay ) {
			var messageData = new DetonateMessageData ( transform.position, this );
			BroadcastMessage ( DetonateMessageData.MessageName, messageData, SendMessageOptions.DontRequireReceiver );
			IsDetonated = true;
		}
	}

	void OnConfigurationReceived ( WeaponConfiguration conf ) {
		float value;

		if ( conf.TryGetFloat ( DelayParameterName, out value ) )
			this.Delay = value;
	}

	void OnLifetimeRequest ( LifetimeRequest request ) {
		request.MinLifetime = TimeLeft;
	}
}