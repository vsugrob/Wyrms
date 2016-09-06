using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryUser : MonoBehaviour {
	public const string OnSetItemActiveMessageName = "OnSetItemActive";
	public const string OnSwitchItemResponseMessageName = "OnSwitchItemResponse";
	public Inventory Inventory;
	public bool CarriedItemIsActive = true;
	public float ItemActivationDelay = 0.2f;
	public bool DelaySwitchTillActive = false;
	public bool DisplaySwitchNotification = true;
	[Multiline]
	public string SwitchMessage = "<color=#804020ff>Item: <b>{LongName}</b></color>";
	public string SwitchMessageSlotId = "InventoryUserSwitch";
	public bool DisplaySwitchDisallowedNotification = true;
	[Multiline]
	public string SwitchDisallowedMessage = "<color=#b04020ff>Item switching disallowed till the next turn.</color>";
	public string SwitchDisallowedMessageSlotId = "InventoryUserSwitchDisallowed";

	public GameObject CurrentItemPrefab { get; private set; }
	private GameObject NextItemPrefab { get; set; }
	private bool itemActivationInitialized = false;
	private bool prevCarriedItemIsActive = false;
	private float carriedItemActivatedTimestamp = float.NegativeInfinity;
	private bool waitingForActivationDelay = false;

	private bool waitingForResponse = false;

	public bool SwitchingPermitted {
		get {
			var permissionReqest = new SwitchItemPermissionRequest ();
			BroadcastMessage ( SwitchItemPermissionRequest.MessageName, permissionReqest, SendMessageOptions.DontRequireReceiver );

			return	!permissionReqest.PermissionDenied;
		}
	}

	void Start () {
		SwitchToNextItem ();
	}

	void Update () {
		// <DEBUG switch item input.>
		if ( Input.GetKeyDown ( KeyCode.Q ) && Inventory != null && InputReceiverSwitch.CheckInputActiveInHierarchy ( this ) ) {
			SwitchToNextItem ();
		}
		// </DEBUG switch item input.>
	}

	void FixedUpdate () {
		// Delay everything else when waiting for switch-out response.
		if ( waitingForResponse ) {
			if ( transform.childCount == 0 ) {
				// Item was destroyed while we waited a response message from it.
				waitingForResponse = false;
				PerformSwitch ();
			} else
				return;
		}

		// Item activation/deactivation.
		if ( CarriedItemIsActive != prevCarriedItemIsActive || !itemActivationInitialized ) {
			if ( CarriedItemIsActive ) {
				carriedItemActivatedTimestamp = Time.fixedTime;
				waitingForActivationDelay = true;
			} else
				SetItemActive ( false );

			prevCarriedItemIsActive = CarriedItemIsActive;
			itemActivationInitialized = true;
		}

		if ( CarriedItemIsActive && waitingForActivationDelay && Time.fixedTime - carriedItemActivatedTimestamp >= ItemActivationDelay ) {
			SetItemActive ( true );
			waitingForActivationDelay = false;
		}

		/* TODO: develop switching to nothing. Currently it is not possible
		 * because NextItemPrefab set to null signals that no switch was requested. */
		// Initiate switch message sequence if needed.
		if ( NextItemPrefab != CurrentItemPrefab && NextItemPrefab != null ) {
			if ( !CarriedItemIsActive && DelaySwitchTillActive )
				return;

			if ( SwitchingPermitted ) {
				var switchRequest = new SwitchItemRequest ( this );
				BroadcastMessage ( SwitchItemRequest.MessageName, switchRequest, SendMessageOptions.DontRequireReceiver );

				if ( !switchRequest.WaitForResponse ) {
					// No need to wait, switch immediately.
					PerformSwitch ();
				} else
					waitingForResponse = true;
			} else
				NextItemPrefab = null;
		}
	}

	void OnSwitchItemResponse () {
		waitingForResponse = false;
		PerformSwitch ();
	}

	private void SetItemActive ( bool active ) {
		BroadcastMessage ( OnSetItemActiveMessageName, active, SendMessageOptions.DontRequireReceiver );
	}

	private void PerformSwitch () {
		if ( NextItemPrefab != null ) {
			// Destroy old item.
			for ( int i = 0 ; i < transform.childCount ; i++ ) {
				var childTf = transform.GetChild ( i );
				Destroy ( childTf.gameObject );
			}

			// Create new item and position it nicely.
			var newItem = Instantiate ( NextItemPrefab ) as GameObject;
			var localPosition = newItem.transform.position;
			newItem.transform.parent = transform;
			newItem.transform.localPosition = localPosition;
			newItem.transform.localScale = Vector3.one;
			newItem.transform.localRotation = Quaternion.identity;

			if ( !CarriedItemIsActive || Time.fixedTime - carriedItemActivatedTimestamp < ItemActivationDelay ) {
				SetItemActive ( false );
			}

			CurrentItemPrefab = NextItemPrefab;
			NextItemPrefab = null;
		}
	}

	public void SwitchToNextItem () {
		if ( !SwitchingPermitted ) {
			if ( DisplaySwitchDisallowedNotification )
				GuiHelper.NotificationPanel.AddMessage ( SwitchDisallowedMessage, SwitchDisallowedMessageSlotId );

			return;
		}

		int currentItemIndex = -1;
		var items = Inventory.Items;
		var desiredItemPrefab = NextItemPrefab != null ? NextItemPrefab : CurrentItemPrefab;

		if ( desiredItemPrefab != null )
			currentItemIndex = items.FindIndex ( item => item.Prefab == desiredItemPrefab );

		if ( items.Count > 0 ) {
			currentItemIndex++;

			if ( currentItemIndex >= items.Count )
				currentItemIndex = 0;

			NextItemPrefab = items [currentItemIndex].Prefab;

			// Do not show notification on game start.
			bool isInitSwitch = CurrentItemPrefab == null;

			if ( DisplaySwitchNotification && !isInitSwitch ) {
				var namedValues = new Dictionary <string, object> ();
				namedValues ["LongName"] = NextItemPrefab.name;
				GuiHelper.NotificationPanel.AddMessage ( SwitchMessage, namedValues, SwitchMessageSlotId );
			}
		}
	}
}

public class SwitchItemPermissionRequest {
	public const string MessageName = "OnSwitchItemPermissionRequest";
	private bool permissionDenied = false;
	public bool PermissionDenied {
		get { return	permissionDenied; }
		set {
			if ( value == true )
				permissionDenied = true;
		}
	}
}

public class SwitchItemRequest {
	public const string MessageName = "OnSwitchItemRequest";
	public MonoBehaviour Sender;
	public bool WaitForResponse;

	public SwitchItemRequest ( MonoBehaviour sender ) {
		this.Sender = sender;
	}
}
