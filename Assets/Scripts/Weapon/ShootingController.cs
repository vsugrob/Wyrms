using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// TODO: review code.
public class ShootingController : MonoBehaviour {
	public const string OnShootingCompletedMessageName = "OnShootingCompleted";
	public const string OnPerformSingleShotMessageName = "OnPerformSingleShot";
	public int NumShotsToProduce = 1;
	[Tooltip ( "Number of shots produced per second." )]
	public float Rate = 3;
	public bool IsShooting { get; private set; }
	public bool ShootOnStart = false;
	// TODO: not working. Review.
	public bool ShootOnDetonation = false;
	public AudioClip [] ShotSounds;

	private SoundPlayer soundPlayer;

	private float delayBetweenShots;
	private float nextShotTimestamp;
	public int NumShotsProduced { get; private set; }
	// TODO: rename? Seems weirdly-named...
	public float TimeLeftToShoot { get { return	( NumShotsToProduce - NumShotsProduced ) / Rate; } }

	void Awake () {
		soundPlayer = GetComponentInChildren <SoundPlayer> ();
	}

	void Start () {
		if ( ShootOnStart )
			StartShooting ();
	}

	void FixedUpdate () {
		ProcessShooting ();
	}

	void OnDetonate ( DetonateMessageData messageData ) {
		if ( enabled && ShootOnDetonation )
			StartShooting ();
	}

	public void StartShooting () {
		if ( IsShooting )
			return;

		delayBetweenShots = 1 / Rate;
		nextShotTimestamp = float.NegativeInfinity;
		NumShotsProduced = 0;
		IsShooting = true;
		ProcessShooting ();
	}

	public void StopShooting () {
		if ( IsShooting ) {
			IsShooting = false;
			SendMessage ( OnShootingCompletedMessageName, SendMessageOptions.DontRequireReceiver );
		}
	}

	private void ProcessShooting () {
		if ( !IsShooting || !enabled )
			return;

		float frameEndTimestamp = Time.fixedTime + Time.fixedDeltaTime;

		if ( float.IsNegativeInfinity ( nextShotTimestamp ) )
			nextShotTimestamp = Time.fixedTime;

		bool shootingAllowed = true;

		for ( ;
			NumShotsProduced < NumShotsToProduce && nextShotTimestamp < frameEndTimestamp && shootingAllowed ;
			NumShotsProduced++, nextShotTimestamp += delayBetweenShots
		) {
			shootingAllowed = RequestSingleShotMessageData.IssueRequest ( gameObject );

			if ( shootingAllowed ) {
				if ( soundPlayer != null )
					soundPlayer.PlayVariation ( ShotSounds );

				SendMessage ( OnPerformSingleShotMessageName, SendMessageOptions.DontRequireReceiver );
			}
		}

		if ( !shootingAllowed || NumShotsProduced >= NumShotsToProduce )
			StopShooting ();
	}

	void OnLifetimeRequest ( LifetimeRequest request ) {
		request.MinLifetime = TimeLeftToShoot;
	}
}

public class RequestSingleShotMessageData {
	public const string MessageName = "OnRequestSingleShot";
	public bool StopShooting;

	public static bool IssueRequest ( GameObject gameObject ) {
		var data = new RequestSingleShotMessageData ();
		gameObject.SendMessage ( MessageName, data, SendMessageOptions.DontRequireReceiver );

		return	!data.StopShooting;
	}
}
