using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class CharController : MonoBehaviour {
	public bool IsControlledByPlayer = true;
	public bool WeaponIsConcealed = false;
	public float LoseControlWhenDamagedAbove = 5;
	public float LowControlLossDuration = 0.2f;
	public float MediumControlLossDuration = 0.5f;
	public float HeadstoneThrowUpVelocity = 2;
	public float StandingStillVelocity = 0.05f;
	public AudioClip [] LowHitSounds;
	public AudioClip [] MediumHitSounds;
	public AudioClip [] DieSounds;
	public StatePhysicMaterials StateMaterials = new StatePhysicMaterials ();
	public WaitSettingList WaitSettings = new WaitSettingList ();
	public CameraEventList CameraEvents = new CameraEventList ();
	public AnimatorNameList AnimatorNames = new AnimatorNameList ();

	public CharacterMovement Movement { get { return	GetComponent <CharacterMovement> (); } }
	public HealthComponent HealthComponent { get { return	GetComponent <HealthComponent> (); } }
	public PlayerOwnedObject PlayerOwnedObject { get { return	GetComponent <PlayerOwnedObject> (); } }
	public Player OwningPlayer {
		get {
			var ownedObject = PlayerOwnedObject;

			if ( ownedObject == null )
				return	null;

			return	ownedObject.Owner;
		}
	}
	public bool IsShooting {
		get {
			var carriedWeapon = GetComponentInChildren <CarriedWeapon> ();

			if ( carriedWeapon == null )
				return	false;

			return	carriedWeapon.IsShooting;
		}
	}

	private Animator animator;
	private InventoryUser inventoryUser;
	private SoundPlayer soundPlayer;

	private TurnManagerAttentionMessageData turnManagerAttentionData = null;
	private bool switchIntoIdleState = false;
	private bool playingIdleState = false;
	private string idleActionStateName = null;
	private CameraEvent idleActionCameraEvent = null;

	void Awake () {
		animator = GetComponentInChildren <Animator> ();
		inventoryUser = GetComponentInChildren <InventoryUser> ();
		soundPlayer = GetComponentInChildren <SoundPlayer> ();
	}

	void FixedUpdate () {
		if ( animator.InState ( AnimatorNames.Dead ) ) {
			ProcessDeadState ();
			gameObject.SetActive ( false );
		} else {
			if ( HealthComponent.ActualHealth <= 0 ) {
				// Don't interrupt other kind of idle action.
				if ( idleActionStateName == null ) {
					switchIntoIdleState = true;
					idleActionStateName = AnimatorNames.NearlyDead;
				}
			} else if ( idleActionStateName == AnimatorNames.NearlyDead ) {
				// Drop idle action when healed.
				switchIntoIdleState = false;
				idleActionStateName = null;
			}
		}
	}

	void OnEnableReceiveInput ( bool enable ) {
		/* When "enable" is false character still can be controlled
		 * by AI player. */

		/* TODO: following is a temporary convenience stub.
		 * In future TurnManager must instruct scene objects to lose
		 * player control via some message. I'm not sure that OnPrepareToFinishTurn message
		 * is adequate for this purpose, so probably it'll be worthy to invent some other kind of message. */
		if ( !enable )
			IsControlledByPlayer = false;
	}

	void OnPrepareToStartTurn () {
		IsControlledByPlayer = true;
		WeaponIsConcealed = false;
		CameraEvents.ReceivedTurnControl.Restart ();
	}

	IEnumerator OnPrepareToFinishTurn () {
		IsControlledByPlayer = false;

		// Wait till shooting completed, then conceal the weapon.
		yield return	this.WaitForCondition ( () => !this.IsShooting );
		WeaponIsConcealed = true;
	}

	void OnDamage ( DamageMessageData data ) {
		if ( !enabled )
			return;

		float actualHealth = HealthComponent.ActualHealth;

		if ( data.Damage >= LoseControlWhenDamagedAbove || actualHealth <= 0 ) {
			Movement.IncreaseControlLossTimer ( MediumControlLossDuration );
			soundPlayer.PlayVariation ( MediumHitSounds );
			TurnManager.Singleton.RequestFinishCurrentTurn ( PlayerOwnedObject, 0 );

			if ( actualHealth <= 0 )
				TurnManager.Singleton.DrawAttention ( gameObject, AttentionPriority.Death );

			CameraEvents.LostControl.Restart ();
		} else {
			Movement.IncreaseControlLossTimer ( LowControlLossDuration );
			soundPlayer.PlayVariation ( LowHitSounds );
		}

		TurnManager.Singleton.DrawAttention ( gameObject, AttentionPriority.HealthChanged );
	}

	void OnDamageAccounted ( DamageAccountedMessageData data ) {
		if ( data.NewHealth <= 0 && data.OldHealth > 0 ) {
			TurnManager.Singleton.RequestFinishCurrentTurn ( PlayerOwnedObject, 0 );
			TurnManager.Singleton.DrawAttention ( gameObject, AttentionPriority.Death );
		}

		CameraEvents.HealthAccounted.Restart ();
	}

	private void ProcessDeadState () {
		// Respond to turn manager if it's waiting.
		if ( turnManagerAttentionData != null ) {
			TurnManager.Singleton.InvokeFixed ( () => {
				turnManagerAttentionData.PerformanceIsOver = true;
				turnManagerAttentionData = null;
			}, WaitSettings.DeadStateExtraTime );
		}

		var owner = OwningPlayer;
		GameObject headstonePrefab = null;

		if ( owner != null )
			headstonePrefab = owner.HeadstonePrefab;

		if ( headstonePrefab != null ) {
			var headstone = Instantiate ( headstonePrefab, transform.position, Quaternion.identity ) as GameObject;
			headstone.transform.parent = transform.parent;
			headstone.name += " of " + this.name;
			var body = headstone.rigidbody2D;

			if ( body != null ) {
				/* Inherit character velocity.
				 * This is essential since character could stand on moving platform. */
				body.velocity = rigidbody2D.velocity;
				body.AddForce ( Vector2.up * HeadstoneThrowUpVelocity, ForceMode.VelocityChange );
			}

			/* Make this game object to be child of headstone.
			 * Character will be placed at the position of its headstone
			 * if it's happen to be resurrected. */
			transform.parent = headstone.transform;
		}
	}

	IEnumerator OnTurnManagerAttention ( TurnManagerAttentionMessageData data ) {
		var healthComp = HealthComponent;
		bool playClosingSpeech = false;
		turnManagerAttentionData = data;

		if ( healthComp.HasPendingChanges ) {
			// Give camera some time to focus on attended object.
			yield return	new WaitForSeconds ( WaitSettings.TimeBeforeResumingHealthChanges );
			healthComp.ResumeChanges ();

			// Wait for end of changes.
			yield return	new WaitForSeconds ( WaitSettings.HealthChangesWaitTime );

			if ( healthComp.Health <= 0 )
				Die ();
			else
				playClosingSpeech = true;
		} else {
			if ( healthComp.Health <= 0 )
				Die ();
			else
				playClosingSpeech = true;
		}

		if ( playClosingSpeech ) {
			// Play closing speech only for currently turning unit.
			if ( TurnManager.Singleton.CurrentUnit == this.PlayerOwnedObject ) {
				/* Play closing speech animation.
				 * Specific speech and animation can be played
				 * depending on results of the turn: good shot
				 * can be concluded with "Hell yeah!" emotion,
				 * whereas shot that hurts teammate will be
				 * ended up with guilty look. */
				switchIntoIdleState = true;
				idleActionStateName = AnimatorNames.ClosingSpeech;
				idleActionCameraEvent = CameraEvents.ClosingSpeech;

				/* Wait some time for character to be grounded and not moving
				 * relative to the platform it's standing on. */
				yield return	WaitTillStandingStill ();

				// Give CharacterMovement some time to initiate idle action.
				yield return	new WaitForSeconds ( WaitSettings.TimeToInitiateIdleAction );

				yield return	this.WaitForCondition (
					() => !animator.InState ( idleActionStateName ),
					WaitSettings.MaxIdleActionDuration
				);
				idleActionStateName = null;
				idleActionCameraEvent = null;
				playingIdleState = false;

				// Character could die during the speech.
				if ( healthComp.Health <= 0 )
					Die ();
				else
					data.PerformanceIsOver = true;
			} else
				data.PerformanceIsOver = true;
		}
	}

	private Coroutine WaitTillStandingStill () {
		var movement = this.Movement;

		return	this.WaitForCondition (
			() => movement.IsStandingStill ( StandingStillVelocity ),
			WaitSettings.StandingStillWaitTime
		);
	}

	private void Die () {
		StartCoroutine ( WaitForStandingStillAndDie () );
	}

	private IEnumerator WaitForStandingStillAndDie () {
		yield return	WaitTillStandingStill ();

		Movement.enabled = false;
		inventoryUser.CarriedItemIsActive = false;
		InterruptIdleAction ();

		if ( HealthComponent.Health <= 0 ) {
			SetState ( "Die", StateMaterials.Die );
			soundPlayer.PlayVariation ( DieSounds );
			CameraEvents.Death.Restart ();
		} else {
			// Lucky character was healed during wait period.
			if ( turnManagerAttentionData != null )
				turnManagerAttentionData.PerformanceIsOver = true;
		}
	}

	public bool PerformIdleAction () {
		if ( idleActionStateName != null ) {
			if ( switchIntoIdleState ) {
				animator.Play ( idleActionStateName );

				if ( idleActionCameraEvent != null )
					idleActionCameraEvent.Restart ();

				switchIntoIdleState = false;
				playingIdleState = true;
			}

			return	true;
		} else
			return	false;
	}

	public void InterruptIdleAction () {
		if ( idleActionStateName != null && playingIdleState ) {
			/* TODO: stop idle action sounds?
			 * UPD: currently in most situations idle action sound
			 * interrupted automatically by playing another sound
			 * on same AudioSource. */
			idleActionStateName = null;
			idleActionCameraEvent = null;
			playingIdleState = false;
		}
	}

	public void SetState ( string animationName, StateMaterial state ) {
		animator.Play ( animationName );
		state.SetupValues ( collider2D );
	}

	[System.Serializable]
	public class StatePhysicMaterials {
		public StateMaterial Die = new StateMaterial ( CharacterMovement.StatePhysicMaterials.DefaultStandFriction, 0 );
	}

	[System.Serializable]
	public class CameraEventList {
		public CameraEvent Death;
		public CameraEvent ReceivedTurnControl;
		public CameraEvent ClosingSpeech;
		public CameraEvent HealthAccounted;
		public CameraEvent LostControl;
	}

	[System.Serializable]
	public class WaitSettingList {
		public float StandingStillWaitTime = 5;
		public float TimeToInitiateIdleAction = 0.25f;
		public float MaxIdleActionDuration = 10;
		public float TimeBeforeResumingHealthChanges = 1;
		public float HealthChangesWaitTime = 2;
		public float DeadStateExtraTime = 2;
	}
	
	[System.Serializable]
	public class AnimatorNameList {
		public string ClosingSpeech = "ClosingSpeech";
		public string Dead = "Dead";
		public string NearlyDead = "NearlyDead";
	}
}
