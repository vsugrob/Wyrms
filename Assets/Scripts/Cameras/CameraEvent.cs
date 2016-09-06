using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CameraEvent : MonoBehaviour {
	public int Priority;
	public bool GiveControlToNewest = false;
	public float Duration = float.PositiveInfinity;
	public string EventName;
	public string ExclusiveGroupName;
	public CameraFocusKind FocusKind = CameraFocusKind.BringIntoView;
	public float TransitionHardness = 0.05f;
	public float ViewportRadius = 0.4f;

	private float enabledTimestamp = float.PositiveInfinity;
	public float EnabledTimestamp { get { return	enabledTimestamp; } }
	public float TimeElapsed { get { return	Time.unscaledTime - enabledTimestamp; } }
	public bool IsActive { get { return	TimeElapsed < Duration; } }

	// TODO: use some sort of priority queue instead of HashSet, just make sure that elements are distinct.
	private static HashSet <CameraEvent> allEvents = new HashSet <CameraEvent> ();
	public static IEnumerable <CameraEvent> AllEvents {
		get {
			foreach ( var ev in allEvents ) {
				yield return	ev;
			}
		}
	}
	public static IEnumerable <CameraEvent> ActiveInHierarchyEvents {
		get {
			foreach ( var ev in allEvents ) {
				if ( ev.gameObject.activeInHierarchy )
					yield return	ev;
			}
		}
	}
	public static IEnumerable <CameraEvent> MostNotableEvents {
		get {
			return	FilterMostNotableEvents ( ActiveInHierarchyEvents, maxPriorityDiff : 0 );
		}
	}

	void Update () {
		if ( !IsActive )
			this.enabled = false;
	}

	void OnEnable () {
		enabledTimestamp = Time.unscaledTime;
		allEvents.Add ( this );

		if ( !string.IsNullOrEmpty ( ExclusiveGroupName ) ) {
			foreach ( var ev in allEvents.ToArray () ) {
				if ( ev != this && ev.ExclusiveGroupName == ExclusiveGroupName )
					ev.enabled = false;
			}
		}
	}

	void OnDisable () {
		allEvents.Remove ( this );
	}

	/// <summary>
	/// Reset timer even when event was enabled already.
	/// </summary>
	public void Restart () {
		enabledTimestamp = Time.unscaledTime;
		this.enabled = true;
	}

	public static IEnumerable <CameraEvent> FilterMostNotableEvents (
		IEnumerable <CameraEvent> events,
		int maxPriorityDiff = 0
	) {
		if ( events.Any () ) {
			int highestPriority = events.Max ( ev => ev.Priority );

			return	events.Where ( ev => highestPriority - ev.Priority <= maxPriorityDiff );
		} else
			return	Enumerable.Empty <CameraEvent> ();
	}
}

// TODO: deprecate this since CenterView can be simulated with ViewportRadius set to 0.
public enum CameraFocusKind {
	/// <summary>
	/// Move camera so that the object is within viewing frustum of the camera.
	/// It doesn't matter where on the screen object is placed.
	/// </summary>
	BringIntoView,
	/// <summary>
	/// Make camera centered on the object.
	/// </summary>
	CenterView
}