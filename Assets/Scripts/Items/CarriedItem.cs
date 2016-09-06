using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CarriedItem : MonoBehaviour {
	public bool IsActive = true;
	public AnimatorNameList AnimatorNames = new AnimatorNameList () {
		HiddenState = "Hidden",
		AppearTrigger = "AppearTrigger",
		DisappearTrigger = "DisappearTrigger"
	};

	private Animator animator;
	
	private bool isInitialized = false;
	private bool prevIsActive = false;
	public bool IsSwitchingOut { get; private set; }
	private MonoBehaviour SwitchOutRequestor;

	void Start () {
		animator = GetComponentInChildren <Animator> ();
		
		if ( IsActive )
			animator.SetTrigger ( AnimatorNames.AppearTrigger );
	}

	void FixedUpdate () {
		// TODO: uncomment and investigate causes of infinite waiting for HiddenState.
		//if ( IsSwitchingOut && IsActive ) {
		//	// Forbid reactivation while switching out.
		//	IsActive = false;
		//}

		if ( IsActive != prevIsActive || !isInitialized ) {
			if ( IsActive )
				animator.SetTrigger ( AnimatorNames.AppearTrigger );
			else if ( !isInitialized )
				animator.Play ( AnimatorNames.HiddenState );
			else
				animator.SetTrigger ( AnimatorNames.DisappearTrigger );

			prevIsActive = IsActive;
			isInitialized = true;
		}

		if ( IsSwitchingOut && animator.InState ( AnimatorNames.HiddenState ) && SwitchOutRequestor != null ) {
			SwitchOutRequestor.SendMessage ( InventoryUser.OnSwitchItemResponseMessageName, SendMessageOptions.DontRequireReceiver );
			Destroy ( gameObject );

			return;
		}
	}

	void OnSetItemActive ( bool active ) {
		IsActive = active;
	}

	void OnSwitchItemRequest ( SwitchItemRequest request ) {
		if ( !enabled )
			return;

		request.WaitForResponse = true;
		IsActive = false;

		// Forbid to reactivate this item.
		IsSwitchingOut = true;
		SwitchOutRequestor = request.Sender;
	}

	[System.Serializable]
	public struct AnimatorNameList {
		public string HiddenState;
		public string AppearTrigger;
		public string DisappearTrigger;
	}
}
