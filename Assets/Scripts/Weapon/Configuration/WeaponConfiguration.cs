using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// TODO: change or remove word "Weapon" from name?
public class WeaponConfiguration : MonoBehaviour {
	public const string OnConfigurationReceivedMessageName = "OnConfigurationReceived";
	private Dictionary <string, ConfigurationParameter> parameters = new Dictionary <string, ConfigurationParameter> ();
	public Dictionary <string, ConfigurationParameter> Parameters { get { return	parameters; } }

	public void StoreParameter ( ConfigurationParameter parameter ) {
		parameters [parameter.Name] = parameter;
	}

	public bool TryGetFloat ( string name, out float value ) {
		ConfigurationParameter parameter;

		if ( Parameters.TryGetValue ( name, out parameter ) ) {
			if ( parameter.Value is float ) {
				value = ( float ) parameter.Value;

				return	true;
			}
		}

		value = 0;

		return	false;
	}

	public bool RestoreFloat ( string name, ref float value ) {
		ConfigurationParameter parameter;

		if ( Parameters.TryGetValue ( name, out parameter ) ) {
			if ( parameter.Persist && parameter.Value is float ) {
				value = ( float ) parameter.Value;

				return	true;
			}
		}

		return	false;
	}

	public void SendConfiguration ( GameObject gameObject ) {
		gameObject.SendMessage ( OnConfigurationReceivedMessageName, this, SendMessageOptions.DontRequireReceiver );
	}
}