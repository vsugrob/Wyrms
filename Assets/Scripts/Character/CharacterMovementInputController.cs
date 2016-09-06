using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharacterMovementInputController : MonoBehaviour {
	public string HorizontalAxis = "Horizontal";
	public string ForwardJumpAxis = "ForwardJump";
	public string BackJumpAxis = "BackJump";

	private CharacterMovement characterMovement;

	void Awake () {
		characterMovement = GetComponent <CharacterMovement> ();
	}

	void Start () {
		if ( !InputReceiverSwitch.CheckInputActiveInHierarchy ( this ) )
			this.enabled = false;
	}

	void Update () {
		characterMovement.MoveX = Input.GetAxis ( HorizontalAxis );
		characterMovement.PerformForwardJump = Input.GetAxis ( ForwardJumpAxis ) != 0;
		characterMovement.PerformBackJump = Input.GetAxis ( BackJumpAxis ) != 0;
	}

	void OnEnableReceiveInput ( bool enable ) {
		this.enabled = enable;
	}

	void OnDisable () {
		characterMovement.MoveX = 0;
		characterMovement.MoveY = 0;
		characterMovement.PerformForwardJump = false;
		characterMovement.PerformBackJump = false;
	}
}
