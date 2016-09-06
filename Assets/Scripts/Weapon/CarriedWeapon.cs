using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

// TODO: review code.
[RequireComponent ( typeof ( ShootingController ) )]
[DisallowMultipleComponent]	// TODO: review all components, insert this attribute where appropriate.
public class CarriedWeapon : MonoBehaviour {
	public const string OnFireTriggeredMessageName = "OnFireTriggered";
	public float Cooldown = 1;
	public int NumberOfUsesPerTurn = 1;
	public float ResumeHealthChangesDuration = 3;
	public bool DisplayNotification = true;
	[Multiline]
	public string NotificationMessage = "<color=#804020ff>Shots remainig: <b>{NumberOfUsesLeft}/{NumberOfUsesPerTurn}</b></color>";
	public string MessageSlotId = "CarriedWeaponNumberOfUses";
	[Tooltip ( "When set to true, actual shooting will be started when PerformFire () method" +
			   " is called as animation event. When false, it will be started immediately." )]
	public bool AwaitSignalFromAnimation = true;
	public bool RequireReadyStateToShoot = true;
	public AnimatorNameList AnimatorNames = new AnimatorNameList () {
		ReadyState = "Ready",
		FireTrigger = "FireTrigger",
		CooldownParameter = "Cooldown",
		SingleShotTrigger = "SingleShotTrigger"
	};
	public WeaponFinishTurnCondition FinishTurnCondition = WeaponFinishTurnCondition.ShootingFinished;
	public float FinishTurnDelay = 3;

	private Animator animator;
	private ShootingController shootingController;
	protected WeaponConfiguration configuration;

	public int NumberOfUsesLeft { get; private set; }
	private float lastFireTimestamp = float.NegativeInfinity;
	public bool ReadyToFire { get { return	Time.fixedTime - lastFireTimestamp >= Cooldown; } }
	public bool IsShooting { get { return	shootingController.IsShooting; } }
	private IEnumerator resumeHealthChangesRoutine;

	void Awake () {
		animator = GetComponentInChildren <Animator> ();
		shootingController = GetComponent <ShootingController> ();
		NumberOfUsesLeft = NumberOfUsesPerTurn;
	}

	void Start () {
		/* This is the right place for querying this component because at the time
		 * of Awake () this object is not yet parented. */
		configuration = GetComponentInParent <WeaponConfiguration> ();
	}

	void FixedUpdate () {
		animator.SetFloat ( AnimatorNames.CooldownParameter, lastFireTimestamp + Cooldown - Time.fixedTime );
	}

	void OnFireTriggered () {
		StartShooting ();
	}

	void OnSetItemActive ( bool active ) {
		if ( !active ) {
			animator.ResetTrigger ( AnimatorNames.FireTrigger );
			shootingController.StopShooting ();
		}
	}

	public bool StartShooting () {
		if ( !enabled || !ReadyToFire )
			return	false;

		if ( RequireReadyStateToShoot && !animator.InState ( AnimatorNames.ReadyState ) )
			return	false;

		if ( NumberOfUsesLeft <= 0 )
			return	false;

		if ( AwaitSignalFromAnimation )
			animator.SetTrigger ( AnimatorNames.FireTrigger );
		else {
			// TODO: start animation by trigger. Ignore PerformFire () call from animation.
			PerformFire ();
		}

		return	true;
	}

	public void StopShooting () {
		shootingController.StopShooting ();
	}

	/// <summary>
	/// This method is to be called as animation event.
	/// </summary>
	private void PerformFire () {
		if ( !enabled || !ReadyToFire )
			return;

		shootingController.StartShooting ();

		if ( FinishTurnCondition == WeaponFinishTurnCondition.ShootingStarted )
			FinishTurn ();
	}

	void OnShootingCompleted () {
		lastFireTimestamp = Time.fixedTime;
		NumberOfUsesLeft--;

		if ( NumberOfUsesLeft > 0 ) {
			if ( resumeHealthChangesRoutine != null )
				StopCoroutine ( resumeHealthChangesRoutine );

			resumeHealthChangesRoutine = ResumeHealthChangesForAMoment ();
			StartCoroutine ( resumeHealthChangesRoutine );
		}

		if ( NumberOfUsesPerTurn > 1 && DisplayNotification ) {
			var namedValues = new Dictionary <string, object> ();
			namedValues ["NumberOfUsesLeft"] = NumberOfUsesLeft;
			namedValues ["NumberOfUsesPerTurn"] = NumberOfUsesPerTurn;
			GuiHelper.NotificationPanel.AddMessage ( NotificationMessage, namedValues, MessageSlotId );
		}

		if ( NumberOfUsesLeft <= 0 && FinishTurnCondition == WeaponFinishTurnCondition.ShootingFinished )
			FinishTurn ();
	}

	IEnumerator ResumeHealthChangesForAMoment () {
		if ( TurnManager.Singleton.PauseHealthChangesDuringTurn ) {
			HealthComponent.ResumeChangesForAll ();
			yield return	new WaitForSeconds ( ResumeHealthChangesDuration );
			HealthComponent.PauseChangesForAll ();
		}
	}

	void OnRequestSingleShot ( RequestSingleShotMessageData data ) {
		/* TODO: implement. This handler can set data.StopShooting = true when
		 * shooter is out of ammo. Return from method without triggering SingleShotTrigger
		 * when shooting must be stopped. */

		animator.SetTrigger ( AnimatorNames.SingleShotTrigger );
	}

	void OnPrepareToStartTurn () {
		NumberOfUsesLeft = NumberOfUsesPerTurn;
	}

	void OnPrepareToFinishTurn () {
		if ( TurnManager.Singleton.PauseHealthChangesDuringTurn ) {
			if ( resumeHealthChangesRoutine != null )
				StopCoroutine ( resumeHealthChangesRoutine );

			HealthComponent.PauseChangesForAll ();
		}
	}

	void OnSwitchItemPermissionRequest ( SwitchItemPermissionRequest request ) {
		if ( NumberOfUsesLeft != NumberOfUsesPerTurn )
			request.PermissionDenied = true;
	}

	private void FinishTurn () {
		var owningUnit = GetComponentInParent <PlayerOwnedObject> ();
		var turnManager = TurnManager.Singleton;
		turnManager.RequestFinishCurrentTurn ( owningUnit, FinishTurnDelay );
		turnManager.DrawAttention ( owningUnit.gameObject, AttentionPriority.ClosingSpeech );
	}

	[System.Serializable]
	public struct AnimatorNameList {
		public string ReadyState;
		public string FireTrigger;
		public string CooldownParameter;
		public string SingleShotTrigger;
	}
}

public enum WeaponFinishTurnCondition {
	ShootingFinished, ShootingStarted, None
}