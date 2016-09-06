using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NumberConfigurator : ConfiguratorBase {
	public string ParameterName = "Number";
	public bool Persist = false;
	public float Value = 3;
	public float MinValue = 1;
	public float MaxValue = 5;
	public bool DisplayNotification = true;
	[Multiline]
	public string NotificationMessage = "<color=#804020ff>Value: <b>{Value}</b></color>";
	public string MessageSlotId = "NumberConfiguratorValue";

	[HideInInspector]
	public float NumberInput;

	protected override void Awake () {
		NumberInput = float.NaN;
	}

	protected override void Start () {
		base.Start ();
		configuration.RestoreFloat ( ParameterName, ref Value );
	}
	
	void FixedUpdate () {
		if ( !IsInActiveState )
			return;

		if ( !float.IsNaN ( NumberInput ) ) {
			Value = Mathf.Clamp (
				NumberInput,
				MinValue, MaxValue
			);
			NumberInput = float.NaN;
			
			if ( DisplayNotification ) {
				var namedValues = new Dictionary <string, object> ();
				namedValues ["Value"] = Value;
				namedValues ["MinValue"] = MinValue;
				namedValues ["MaxValue"] = MaxValue;
				GuiHelper.NotificationPanel.AddMessage ( NotificationMessage, namedValues, MessageSlotId );
			}

			StoreParameter ( ParameterName, Value, Persist );
		}
	}
}
