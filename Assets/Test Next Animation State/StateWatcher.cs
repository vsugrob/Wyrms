using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StateWatcher : MonoBehaviour {
	private Animator animator;

	void Start () {
		animator = GetComponent <Animator> ();
	}

	private bool playedNextAnimation = false;

	void FixedUpdate () {
		if ( Time.fixedTime >= 0.04 && !playedNextAnimation ) {
			print ( "Playing next animation" );
			animator.Play ( "Disappear" );
			playedNextAnimation = true;

			// Conclusion: unfortunately nextState will not hold information about requested animation till next frame. 
		}

		var state = animator.GetCurrentAnimatorStateInfo ( 0 );
		var nextState = animator.GetNextAnimatorStateInfo ( 0 );

		print (
			"[" + Time.fixedTime + "]" +
			" state.nameHash: " + state.nameHash +
			", nextState.nameHash: " + nextState.nameHash
		);
	}

	private void AnimEvent () {
		print (
			"[" + Time.fixedTime + "]" +
			" AnimEvent () called"
		);

		// Conclusion: unfortunately this will be called only in the next frame after the one which requested next animation.
	}
}
