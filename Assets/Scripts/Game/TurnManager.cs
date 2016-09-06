using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class TurnManager : MonoBehaviour {
	// TODO: change "OnPrepareToFinish..." to "OnFinish..." and "OnPrepareToStart..." to "OnStart..." ?
	public const string OnPrepareToFinishTurnMessagName = "OnPrepareToFinishTurn";
	public const string OnPrepareToStartTurnMessagName = "OnPrepareToStartTurn";
	public bool PauseHealthChangesDuringTurn = true;
	public float TransientObjectsMaxWaitTime = 10;
	public float MaxDramaDuration = 40;

	private static TurnManager singletonInstance;
	public static TurnManager Singleton {
		get {
			if ( singletonInstance == null ) {
				singletonInstance = FindObjectOfType <TurnManager> ();

				if ( singletonInstance == null ) {
					var containerGameObject = new GameObject ( "Turn Manager" );
					singletonInstance = containerGameObject.AddComponent <TurnManager> ();
				}
			}

			return	singletonInstance;
		}
	}

	public TurningPlayerInfo TurningPlayer = new TurningPlayerInfo ();
	private Dictionary <Player, TurningUnitInfo> turningUnitsByPlayer = new Dictionary <Player, TurningUnitInfo> ();

	public Player CurrentPlayer { get { return	TurningPlayer.Entity; } }
	public PlayerOwnedObject CurrentUnit {
		get {
			var currentPlayer = this.CurrentPlayer;

			if ( currentPlayer == null )
				return	null;

			TurningUnitInfo turningUnit;

			if ( turningUnitsByPlayer.TryGetValue ( currentPlayer, out turningUnit ) )
				return	turningUnit.Entity;
			else
				return	null;
		}
	}
	public IEnumerable <PlayerOwnedObject> CurrentPlayerOwnedObjects {
		get {
			var currentPlayer = this.CurrentPlayer;

			if ( currentPlayer != null ) {
				foreach ( var ownedObject in currentPlayer.OwnedObjects )
					yield return	ownedObject;
			}
		}
	}

	private bool finishTurnRequested = false;
	private float finishTurnDelayEndTimestamp;
	private bool finishTurnSequencePlaying = false;
	public bool FinishingTurn { get { return	finishTurnRequested || finishTurnSequencePlaying; } }

	private Dictionary <GameObject, int> attentionSeekers = new Dictionary <GameObject, int> ();
	public GameObject AttentionTarget { get; private set; }

	void Start () {
		ReleaseControlOnAllPlayers ();
		TransferControlToNextPlayer ();
	}

	void Update () {
		// <DEBUG>
		if ( Input.GetKeyDown ( KeyCode.N ) ) {
			TransferControlToNextPlayer ();
		}
		// </DEBUG>
	}

	void FixedUpdate () {
		if ( finishTurnSequencePlaying )
			return;

		if ( ( finishTurnRequested && Time.fixedTime >= finishTurnDelayEndTimestamp ) ||
			 ( !FinishingTurn && CurrentUnit == null )
		) {
			// Withdraw control from current player.
			var currentPlayer = CurrentPlayer;

			if ( currentPlayer != null )
				currentPlayer.ReleaseUserControl ();

			finishTurnRequested = false;
			finishTurnSequencePlaying = true;
			StartCoroutine ( FinishTurnSequence () );
		} else if ( !FinishingTurn && CurrentUnit == null )
			TransferControlToNextPlayer ();
	}

	private IEnumerator FinishTurnSequence () {
		// Wait till short-living projectiles and other temporary objects settle down.
		yield return	StartCoroutine ( WaitForTransientObjects () );

		/* Cycle through all objects requiring attention (e.g. dying characters).
		 * Let them perform some actions (e.g. dying animation) one by one. */
		GameObject rockstar;
		var rockstarPos = Vector2.zero;
		float dramaStartTimestamp = Time.fixedTime;
		bool patienceHasEnded = false;

		while ( TryGetClosestAttentionSeeker ( rockstarPos, out rockstar ) ) {
			rockstarPos = rockstar.transform.position;

			// Watch delightful performance provided by rockstar.
			var messageData = TurnManagerAttentionMessageData.SendMessage ( rockstar );

			if ( !patienceHasEnded ) {
				// Let camera fetch information abount currently attended object.
				AttentionTarget = rockstar;

				// Wait until it ends.
				do {
					float dramaTimeElapsed = Time.fixedTime - dramaStartTimestamp;
					patienceHasEnded = dramaTimeElapsed >= MaxDramaDuration;

					if ( patienceHasEnded || messageData.PerformanceIsOver )
						break;

					yield return	this.WaitForFixedSeconds ( 0.25f );
				} while ( rockstar != null );	// Object could be destroyed.

				AttentionTarget = null;
			}

			attentionSeekers.Remove ( rockstar );
		}

		TransferControlToNextPlayer ();
		finishTurnSequencePlaying = false;
	}

	private IEnumerator WaitForTransientObjects () {
		float transientWaitStartTimestamp = Time.fixedTime;

		while ( true ) {
			float waitTimeElapsed = Time.fixedTime - transientWaitStartTimestamp;
			float maxWaitTime = TransientObjectsMaxWaitTime - waitTimeElapsed;

			var transientObjects = LifetimeRequest
				.Query ( CurrentPlayerOwnedObjects.Select ( ownedObject => ownedObject.gameObject ) )
				.Where ( objLifetime => objLifetime.MinLifetime <= maxWaitTime && objLifetime.MinLifetime > 0 );

			if ( transientObjects.Any () )
				yield return	this.WaitForFixedSeconds ( 0.25f );
			else
				break;
		}
	}

	public void RequestFinishCurrentTurn ( PlayerOwnedObject requestingUnit, float delay = 3 ) {
		var currentUnit = CurrentUnit;

		if ( currentUnit == null || requestingUnit != currentUnit )
			return;

		float delayEndTimestamp = Time.fixedTime + delay;

		if ( finishTurnRequested ) {
			// There might be several consequent requests, adhere to the toughest one.
			if ( delayEndTimestamp < finishTurnDelayEndTimestamp )
				finishTurnDelayEndTimestamp = delayEndTimestamp;

			return;
		} else if ( finishTurnSequencePlaying )
			return;

		finishTurnRequested = true;
		finishTurnDelayEndTimestamp = delayEndTimestamp;
		currentUnit.BroadcastMessage ( OnPrepareToFinishTurnMessagName, SendMessageOptions.DontRequireReceiver );
	}

	public void DrawAttention ( GameObject dramaQueen, int priority = AttentionPriority.Default ) {
		int existingPriority;

		if ( !attentionSeekers.TryGetValue ( dramaQueen, out existingPriority ) || priority > existingPriority )
			attentionSeekers [dramaQueen] = priority;
	}

	private bool TryGetClosestAttentionSeeker ( Vector2 origin, out GameObject attentionSeeker ) {
		var nonDestroyedList = attentionSeekers.Where ( kv => kv.Key != null && kv.Key.activeInHierarchy );

		if ( nonDestroyedList.Any () ) {
			int maxPriority = nonDestroyedList.Max ( kv => kv.Value );
			var samePriorityObjects = nonDestroyedList.Where ( kv => kv.Value == maxPriority );
			var closestSeekerAndPriority = samePriorityObjects
				.WithMin ( kv => Common.DistanceSq ( origin, kv.Key.transform.position ) );

			attentionSeeker = closestSeekerAndPriority.Key;

			return	true;
		}

		attentionSeeker = null;

		return	false;
	}

	private void ReleaseControlOnAllPlayers () {
		foreach ( var player in Player.AllPlayers ) {
			player.ReleaseUserControl ();
		}
	}

	public void TransferControlToNextPlayer () {
		if ( PauseHealthChangesDuringTurn )
			HealthComponent.PauseChangesForAll ();

		if ( TurningPlayer.Entity != null )
			TurningPlayer.Entity.ReleaseUserControl ();

		Player firstPickedPlayer = null;
		Player currentPlayer;
		bool firstRound = true;

		do {
			currentPlayer = TurningPlayer.Entity = GetNextPlayerInTurnQueue ( out TurningPlayer.TurnIndex );

			if ( currentPlayer == null )
				return;

			if ( firstPickedPlayer == null )
				firstPickedPlayer = currentPlayer;
			else
				firstRound = false;

			TurningUnitInfo turningUnit;

			if ( !turningUnitsByPlayer.TryGetValue ( currentPlayer, out turningUnit ) ) {
				turningUnit = new TurningUnitInfo ();
				turningUnitsByPlayer [currentPlayer] = turningUnit;
			}

			var currentUnit = turningUnit.Entity = GetNextUnitInTurnQueue ( turningUnit, out turningUnit.TurnIndex );

			if ( currentUnit != null ) {
				InputReceiverSwitch.SetReceiveInput ( currentUnit.gameObject, true );
				currentUnit.BroadcastMessage ( OnPrepareToStartTurnMessagName, SendMessageOptions.DontRequireReceiver );

				return;
			}
		} while ( firstRound || currentPlayer != firstPickedPlayer );

		// If this line was reached then there were no "turnable" units.
	}

	public Player GetNextPlayerInTurnQueue ( out int turnIndex ) {
		return	TurningPlayer.GetNextObjectInQueue ( Player.AllPlayers, out turnIndex );
	}

	public PlayerOwnedObject GetNextUnitInTurnQueue ( TurningUnitInfo turningUnit, out int turnIndex ) {
		var aliveUnits = TurningPlayer.Entity.OwnedObjects
			.Where ( obj => HealthComponent.IsAliveOrHasNoHealthComponent ( obj.gameObject ) );

		return	turningUnit.GetNextObjectInQueue ( aliveUnits, out turnIndex );
	}

	[System.Serializable]
	public class TurningPlayerInfo : TurningEntityInfo <Player> {}

	[System.Serializable]
	public class TurningUnitInfo : TurningEntityInfo <PlayerOwnedObject> {}

	[System.Serializable]
	public class TurningEntityInfo <TComponent> where TComponent : MonoBehaviour {
		public TComponent Entity;
		public int TurnIndex = -1;

		public TComponent GetNextObjectInQueue ( IEnumerable <TComponent> queue, out int turnIndex ) {
			queue = queue
				.Where ( obj => TurnOrderComponent.IsOrdered ( obj ) )
				.OrderBy ( obj => TurnOrderComponent.GetOrderIndex ( obj ) );

			foreach ( var obj in queue ) {
				if ( obj == Entity )
					continue;

				turnIndex = TurnOrderComponent.GetOrderIndex ( obj );

				if ( turnIndex >= TurnIndex )
					return	obj;
			}

			if ( queue.Any () ) {
				var lonelyOne = queue.First ();
				turnIndex = TurnOrderComponent.GetOrderIndex ( lonelyOne );

				return	lonelyOne;
			} else {
				turnIndex = -1;

				return	null;
			}
		}
	}
}

public class TurnManagerAttentionMessageData {
	public const string MessageName = "OnTurnManagerAttention";
	public bool PerformanceIsOver;

	public static TurnManagerAttentionMessageData SendMessage ( GameObject gameObject ) {
		var data = new TurnManagerAttentionMessageData ();
		gameObject.SendMessage ( MessageName, data, SendMessageOptions.RequireReceiver );

		return	data;
	}
}

public static class AttentionPriority {
	public const int ClosingSpeech = -1;
	public const int Default = 0;
	public const int HealthChanged = 50;
	public const int Death = 100;
}