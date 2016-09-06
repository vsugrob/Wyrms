using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DefaultSettings : MonoBehaviour {
	#region Singleton
	public const string SingletonGameObjectName = "DefaultSettings";
	private static GameObject singletonGameObjectInstance = null;
	public static DefaultSettings Singleton {
		get {
			if ( singletonGameObjectInstance == null ) {
				singletonGameObjectInstance = GameObject.Find ( SingletonGameObjectName );

				if ( singletonGameObjectInstance == null ) {
					singletonGameObjectInstance = new GameObject ( SingletonGameObjectName );
					//singletonGameObjectInstance.hideFlags = HideFlags.HideInHierarchy;
				}
			}

			var component = singletonGameObjectInstance.GetComponent <DefaultSettings> ();

			if ( component == null )
				component = singletonGameObjectInstance.AddComponent <DefaultSettings> ();

			return	component;
		}
	}
	#endregion Singleton

	#region ForceReceiverMaterial
	public ForceMaterial ForceReceiverMaterial = null;
	private static ForceMaterial defaultForceReceiverMaterial = null;
	public static ForceMaterial DefaultForceReceiverMaterialSingleton {
		get {
			if ( defaultForceReceiverMaterial == null ) {
				defaultForceReceiverMaterial = new ForceMaterial (
					gravity : 1,
					magnetism : 0,
					shockwave : 1,
					wind : 0,
					love : 1
				);

				defaultForceReceiverMaterial.name = string.Format (
					"Default {0} for {1}",
					typeof ( ForceMaterial ).Name,
					typeof ( ForceReceiver ).Name
				);
			}

			return	defaultForceReceiverMaterial;
		}
	}

	public ForceMaterial ForceReceiverMaterialOrDefault {
		get {
			return	ForceReceiverMaterial != null ?
				ForceReceiverMaterial : DefaultForceReceiverMaterialSingleton;
		}
	}
	#endregion ForceReceiverMaterial

	#region ParticleSystemForceReceiverMaterial
	public ForceMaterial ParticleSystemForceReceiverMaterial = null;
	private static ForceMaterial defaultParticleSystemForceReceiverMaterial;
	public static ForceMaterial DefaultParticleSystemForceReceiverMaterialSingleton {
		get {
			if ( defaultParticleSystemForceReceiverMaterial == null ) {
				defaultParticleSystemForceReceiverMaterial = new ForceMaterial (
					gravity : 1,
					magnetism : 0.001f,
					shockwave : 0.01f,
					wind : 0.01f,
					love : 1
				);

				defaultParticleSystemForceReceiverMaterial.name = string.Format (
					"Default {0} for {1}",
					typeof ( ForceMaterial ).Name,
					typeof ( ParticleSystemForceReceiver ).Name
				);
			}

			return	defaultParticleSystemForceReceiverMaterial;
		}
	}

	public ForceMaterial ParticleSystemForceReceiverMaterialOrDefault {
		get {
			return	ParticleSystemForceReceiverMaterial != null ?
				ParticleSystemForceReceiverMaterial : DefaultParticleSystemForceReceiverMaterialSingleton;
		}
	}
	#endregion ParticleSystemForceReceiverMaterial

	#region DamageReceiverMaterial
	public DamageMaterial DamageReceiverMaterial = null;
	private static DamageMaterial defaultDamageReceiverMaterial = null;
	public static DamageMaterial DefaultDamageReceiverMaterialSingleton {
		get {
			if ( defaultDamageReceiverMaterial == null ) {
				defaultDamageReceiverMaterial = new DamageMaterial (
					shockwave : 1,
					bullet : 1,
					electricity : 1,
					fire : 1,
					cold : 1,
					acid : 1
				);

				defaultDamageReceiverMaterial.name = string.Format (
					"Default {0} for {1}",
					typeof ( DamageMaterial ).Name,
					typeof ( DamageReceiver ).Name
				);
			}

			return	defaultDamageReceiverMaterial;
		}
	}

	public DamageMaterial DamageReceiverMaterialOrDefault {
		get {
			return	DamageReceiverMaterial != null ?
				DamageReceiverMaterial : DefaultDamageReceiverMaterialSingleton;
		}
	}
	#endregion DamageReceiverMaterial
}
