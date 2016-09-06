using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ReparentAndStartAnimation : MonoBehaviour {
	public GameObject ObjectToReparent;
	private Animator animator;

	void Start () {
		animator = GetComponent <Animator> ();
		StartCoroutine ( Reparent () );
	}

	System.Collections.IEnumerator Reparent () {
		yield return new WaitForSeconds ( 1 );
		ObjectToReparent.transform.parent = transform;
		//var state = animator.GetCurrentAnimatorStateInfo ( 0 );
		//print ( "Current state length: " + state.length );

		// These two actions necessary to "rebind" animated properties to object hierarchy. Too bad :(
		gameObject.SetActive ( false );
		gameObject.SetActive ( true );
		animator.Play ( "Scale Pulse" );
	}
}
