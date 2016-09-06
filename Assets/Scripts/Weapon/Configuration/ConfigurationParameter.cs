using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConfigurationParameter {
	public const string MessageName = "OnParameterConfigured";
	public string Name;
	public object Value;
	public bool Persist;

	public ConfigurationParameter ( string name, object value, bool persist ) {
		this.Name = name;
		this.Value = value;
		this.Persist = persist;
	}

	public static void SendMessage ( GameObject gameObject, ConfigurationParameter parameter ) {
		gameObject.SendMessage ( MessageName, parameter, SendMessageOptions.DontRequireReceiver );
	}

	public override string ToString () {
		return	Name + ": " + Value;
	}
}
