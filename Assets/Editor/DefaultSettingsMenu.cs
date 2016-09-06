using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class DefaultSettingsMenu {
	[MenuItem ( "Wyrms/Default Settings" )]
	public static void OpenDefaultSettingsInInspector () {
		var settings = DefaultSettings.Singleton;
		Selection.activeObject = settings;

		if ( settings.ForceReceiverMaterial == null ) {
			settings.ForceReceiverMaterial = settings.ForceReceiverMaterialOrDefault;
			CustomAssets.Save (
				settings.ForceReceiverMaterial,
				"Assets/Defaults/", "ForceReceiverMaterial.asset",
				focus : false
			);
		}

		if ( settings.ParticleSystemForceReceiverMaterial == null ) {
			settings.ParticleSystemForceReceiverMaterial = settings.ParticleSystemForceReceiverMaterialOrDefault;
			CustomAssets.Save (
				settings.ParticleSystemForceReceiverMaterial,
				"Assets/Defaults/", "ParticleSystemForceReceiverMaterial.asset",
				focus : false
			);
		}

		if ( settings.DamageReceiverMaterial == null ) {
			settings.DamageReceiverMaterial = settings.DamageReceiverMaterialOrDefault;
			CustomAssets.Save (
				settings.DamageReceiverMaterial,
				"Assets/Defaults/", "DamageReceiverMaterial.asset",
				focus : false
			);
		}
	}
}
