using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProjectileSpawner : MonoBehaviour {
	public string ForceParameterName = "NormalizedForce";
	public float MainDirectionAngle = 0;
	public float ArcHalfAngle = 0;
	public bool AngleIsAbsolute = false;
	public int NumObjectsToSpawn = 1;
	public bool CollideWithEachOther = false;
	public ProjectileSettings Projectile = new ProjectileSettings ();
	public bool SpawnOnStart = false;
	public bool SpawnOnShooting = true;
	public bool SpawnOnDetonation = false;
	public AudioClip [] SpawnSounds;

	private WeaponConfiguration configuration;
	private SoundPlayer soundPlayer;

	private Vector2 SpawnerAimDirection {
		get {
			// This takes negative scale values into account.
			return	( Vector2 ) transform.localToWorldMatrix.MultiplyVector ( Vector2.right ).normalized;
		}
	}

	void Awake () {
		soundPlayer = GetComponentInChildren <SoundPlayer> ();
	}

	void Start () {
		/* This is the right place for querying this component because at the time
		 * of Awake () this object is usually not yet parented. */
		configuration = GetComponentInParent <WeaponConfiguration> ();

		if ( SpawnOnStart )
			SpawnObjects ();
	}

	void OnPerformSingleShot () {
		if ( enabled && SpawnOnShooting )
			SpawnObjects ();
	}

	void OnDetonate ( DetonateMessageData messageData ) {
		if ( enabled && SpawnOnDetonation )
			SpawnObjects ();
	}

	private void SpawnObjects () {
		float normalizedForce = 0;
		// TODO: configuration must be transferred across projectile objects chain.
		if ( configuration != null )
			configuration.TryGetFloat ( ForceParameterName, out normalizedForce );

		float forceAmount = Mathf.Lerp (
			Projectile.MinForceAmount,
			Projectile.MaxForceAmount,
			normalizedForce
		);

		var spawnerBody = GetComponentInParent <Rigidbody2D> ();
		var spawnerGameObject = spawnerBody != null ? spawnerBody.gameObject : gameObject;

		float absMainDirAngle = MainDirectionAngle;

		if ( !AngleIsAbsolute ) {
			float spawnerAimAngle = Common.SignedAngle ( Vector2.right, SpawnerAimDirection );
			absMainDirAngle += spawnerAimAngle;
		}

		float halfAngle = ArcHalfAngle * Mathf.Deg2Rad;
		float angle = absMainDirAngle * Mathf.Deg2Rad - halfAngle;
		float dAngle = halfAngle * 2 / ( NumObjectsToSpawn - 1 );

		var spawnedObjects = new List <GameObject> ();

		for ( int i = 0 ; i < NumObjectsToSpawn ; i++, angle += dAngle ) {
			float deviatedAngle = angle + Random.value * Projectile.AngleRandomDeviation * ArcHalfAngle;
			var dir = new Vector2 ( Mathf.Cos ( deviatedAngle ), Mathf.Sin ( deviatedAngle ) );
			var spawnPos = ( Vector2 ) transform.position + dir * Projectile.SpawnDistance;
			var rotation = Quaternion.identity;

			if ( Projectile.RotateAlongLaunchDir )
				rotation = Common.RotateAlongDirection ( dir );

			var projGameObject = Instantiate ( Projectile.Prefab, spawnPos, rotation ) as GameObject;
			spawnedObjects.Add ( projGameObject );
			// TODO: spawn projectiles on layer dedicated to projectiles!
			PlayerOwnedObject.InheritOwner ( this.gameObject, projGameObject );
			
			var projBodies = projGameObject.GetComponentsInChildren <Rigidbody2D> ();

			foreach ( var projBody in projBodies ) {
				if ( projBody != null ) {
					float deviatedForceAmount = forceAmount + Random.value * Projectile.ForceRandomDeviation * Projectile.MaxForceAmount;

					if ( deviatedForceAmount < 0 )
						deviatedForceAmount = 0;

					var force = dir * deviatedForceAmount;
					projBody.AddForce ( force, Projectile.ForceMode );

					float angularVel = Projectile.AngularVelocity;

					if ( Projectile.AngularVelocitySignDependsOnDir )
						angularVel *= Mathf.Sign ( dir.x );

					if ( Projectile.AngularVelocityDependsOnForce )
						angularVel *= normalizedForce;

					projBody.angularVelocity = angularVel;

					if ( Projectile.InheritSpawnerVelocity && spawnerBody != null ) {
						var spawnerVelocity = spawnerBody.GetPointVelocity ( projGameObject.transform.position );
						projBody.velocity += spawnerVelocity;
					}
				}
			}

			if ( !Projectile.CollideWithSpawner )
				PhysicsHelper.IgnoreCollision ( spawnerGameObject, projGameObject );
			// TODO: configuration must be transferred across projectile objects chain.
			if ( configuration != null )
				configuration.SendConfiguration ( projGameObject );
		}

		if ( !CollideWithEachOther ) {
			foreach ( var obj1 in spawnedObjects ) {
				foreach ( var obj2 in spawnedObjects ) {
					if ( obj1 == obj2 )
						continue;

					PhysicsHelper.IgnoreCollision ( obj1, obj2 );
				}
			}
		}

		if ( soundPlayer != null )
			soundPlayer.PlayVariation ( SpawnSounds );
	}

	[System.Serializable]
	public class ProjectileSettings {
		public bool CollideWithSpawner = false;
		public float MinForceAmount = 0;
		public float MaxForceAmount = 10;
		public ForceMode ForceMode = ForceMode.VelocityChange;
		public bool RotateAlongLaunchDir = true;
		[Tooltip ( "Percentage of MaxForceAmount, from 0 to 1" )]
		public float ForceRandomDeviation = 0;
		[Tooltip ( "Percentage of ArcHalfAngle, from 0 to 1" )]
		public float AngleRandomDeviation = 0;
		public float SpawnDistance = 0.15f;
		public float AngularVelocity = 0;
		public bool AngularVelocitySignDependsOnDir = true;
		public bool AngularVelocityDependsOnForce = true;
		public bool InheritSpawnerVelocity = true;
		public GameObject Prefab;
	}
}
