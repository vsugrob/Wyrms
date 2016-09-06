using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class HealthComponent : MonoBehaviour {
	public float Health = 100;
	public bool AllowNegativeValues = true;
	public bool ChangeImmediately = true;
	public bool PauseChangesDuringTurn = false;
	public float MinChangeToAccount = 0;

	public Queue <DamageMessageData> DamageQueue { get; private set; }
	public bool HasPendingChanges { get { return	DamageQueue.Count > 0; } }
	public float ActualHealth {
		get {
			return	DamageQueue.Aggregate ( Health, ( h, damageData ) => h - damageData.Damage );
		}
	}
	public float TotalDamageReceived { get; private set; }

	void Awake () {
		DamageQueue = new Queue <DamageMessageData> ();
	}

	void Start () {}

	void OnDamage ( DamageMessageData data ) {
		if ( !enabled )
			return;

		if ( Mathf.Abs ( data.Damage ) < MinChangeToAccount )
			return;

		print ( "OnDamage, " + this + " received damage: " + data.Damage + ", inflictor: " + data.Inflictor );

		if ( ChangeImmediately )
			ProcessQueueItem ( data );
		else
			DamageQueue.Enqueue ( data );
	}

	public bool FlushDelayedDamage () {
		bool queueHadItems = DamageQueue.Count > 0;

		while ( DamageQueue.Count > 0 ) {
			var damageData = DamageQueue.Dequeue ();
			ProcessQueueItem ( damageData );
		}

		return	queueHadItems;
	}

	public void PauseChanges () {
		ChangeImmediately = false;
	}

	public bool ResumeChanges () {
		ChangeImmediately = true;

		return	FlushDelayedDamage ();
	}

	public static void PauseChangesForAll () {
		var objectsToDelay = FindObjectsOfType <HealthComponent> ()
			.Where ( c => c.PauseChangesDuringTurn );

		foreach ( var objToDelay in objectsToDelay ) {
			objToDelay.PauseChanges ();
		}
	}

	public static void ResumeChangesForAll () {
		var objectsToDelay = FindObjectsOfType <HealthComponent> ()
			.Where ( c => c.PauseChangesDuringTurn );

		foreach ( var objToDelay in objectsToDelay ) {
			objToDelay.ResumeChanges ();
		}
	}

	private void ProcessQueueItem ( DamageMessageData damageData ) {
		float oldHealth = Health;
		Health -= damageData.Damage;
		TotalDamageReceived += damageData.Damage;

		if ( !AllowNegativeValues && Health < 0 )
			Health = 0;

		var messageData = new DamageAccountedMessageData ( damageData, oldHealth, Health );
		SendMessage ( DamageAccountedMessageData.MessageName, messageData, SendMessageOptions.DontRequireReceiver );
	}

	public static bool IsAliveOrHasNoHealthComponent ( GameObject gameObject, bool checkActualHealth = false ) {
		var component = gameObject.GetComponent <HealthComponent> ();

		if ( component == null )
			return	true;

		if ( checkActualHealth )
			return	component.ActualHealth > 0;
		else
			return	component.Health > 0;
	}
}

public class DamageAccountedMessageData {
	public const string MessageName = "OnDamageAccounted";
	public DamageMessageData DamageData;
	public float OldHealth;
	public float NewHealth;
	
	public DamageAccountedMessageData ( DamageMessageData damageData, float oldHealth, float newHealth ) {
		this.DamageData = damageData;
		this.OldHealth = oldHealth;
		this.NewHealth = newHealth;
	}
}