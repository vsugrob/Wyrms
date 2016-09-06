using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent ( typeof ( LineFollower ) )]
public class PathFollower : MonoBehaviour {
	public DoublyLinkedListNode PathNode;
	public float MaxDistanceFromPath = 0.2f;
	public float IntervalBetweenDistanceChecks = 0.25f;
	public bool ReverseOnEnd = true;
	// TODO: add option to reverse when platform is blocked by obstacle.
	public bool ReverseWhenBlocked = false;
	[Tooltip ( "Follower considered blocked when travelled distance per second is lower or equal to given threshold." )]
	public float BlockedDistancePerSecond = 0.1f;
	public float IntervalBetweenBlockChecks = 1.5f;

	public LineFollower LineFollower { get; private set; }
	private DoublyLinkedListNode curEdgeStartNode;
	private float lastDistanceCheckTimestamp = float.NegativeInfinity;
	public bool IsAdjustingPosition { get; private set; }

	private Vector2 prevPos;
	private float travelledDistance = 0;

	private float lastBlockCheckTimestamp;
	private float prevTravelledDistance = 0;

	void Awake () {
		LineFollower = GetComponent <LineFollower> ();
	}

	void Start () {
		prevPos = transform.position;
		lastBlockCheckTimestamp = Time.fixedTime;
	}

	void FixedUpdate () {
		if ( PathNode == null || !LineFollower.enabled )
			return;

		if ( curEdgeStartNode == null ) {
			Vector2 closestPos;
			var closestEdgeStartNode = PathNode.GetClosestEdge ( transform.position, out closestPos );
			SetCurrentEdge ( closestEdgeStartNode );
		}

		AdjustPosition ();
		UpdateTravelledDistance ();
		ProcessBlockedState ();
	}

	void OnTargetPointReached () {
		if ( IsAdjustingPosition )
			return;

		DoublyLinkedListNode nextNode;

		if ( LineFollower.MotorSpeed >= 0 )
			nextNode = curEdgeStartNode.Next;
		else
			nextNode = curEdgeStartNode.Prev;

		if ( nextNode != null )
			SetCurrentEdge ( nextNode );
		else if ( ReverseOnEnd )
			LineFollower.MotorSpeed = -LineFollower.MotorSpeed;

		// DEBUG
		//print ( "OnTargetPointReached () curEdgeStartNode: " + curEdgeStartNode.name );
	}

	private void AdjustPosition () {
		float timeSinceLastDistanceCheck = Time.fixedTime - lastDistanceCheckTimestamp;

		if ( timeSinceLastDistanceCheck >= IntervalBetweenDistanceChecks ) {
			Vector2 closestPos;
			var closestEdgeStartNode = PathNode.GetClosestEdge ( transform.position, out closestPos );
			var curPos = ( Vector2 ) transform.position;

			if ( Vector2.Distance ( curPos, closestPos ) > MaxDistanceFromPath ) {
				if ( LineFollower.MotorSpeed >= 0 ) {
					LineFollower.StartPoint = curPos;
					LineFollower.EndPoint = closestPos;
				} else {
					LineFollower.StartPoint = closestPos;
					LineFollower.EndPoint = curPos;
				}

				IsAdjustingPosition = true;
			} else {
				if ( IsAdjustingPosition )
					SetCurrentEdge ( closestEdgeStartNode );

				IsAdjustingPosition = false;
			}
		}
	}

	private void UpdateTravelledDistance () {
		var curPos = ( Vector2 ) transform.position;
		var displacement = curPos - prevPos;
		travelledDistance += displacement.magnitude;
		prevPos = curPos;
	}

	private void ProcessBlockedState () {
		if ( ReverseWhenBlocked && !IsAdjustingPosition ) {
			float timeSinceLastBlockCheck = Time.fixedTime - lastBlockCheckTimestamp;

			if ( timeSinceLastBlockCheck >= IntervalBetweenBlockChecks && timeSinceLastBlockCheck != 0 ) {
				float distance = travelledDistance - prevTravelledDistance;
				float distancePerSec = distance / timeSinceLastBlockCheck;

				if ( distancePerSec <= BlockedDistancePerSecond )
					LineFollower.MotorSpeed = -LineFollower.MotorSpeed;

				prevTravelledDistance = travelledDistance;
				lastBlockCheckTimestamp = Time.fixedTime;
			}
		}
	}

	private void SetCurrentEdge ( DoublyLinkedListNode edgeStartNode ) {
		curEdgeStartNode = edgeStartNode;
		LineFollower.StartPoint = curEdgeStartNode.Position;
		LineFollower.EndPoint = curEdgeStartNode.NextPosition;
	}

	void OnDisable () {
		LineFollower.enabled = false;
	}

	void OnEnable () {
		LineFollower.enabled = true;
	}
}
