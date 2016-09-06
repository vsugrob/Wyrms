using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ForcemeterConfigurator : ConfiguratorBase {
	public string ParameterName = "NormalizedForce";
	public bool Persist = false;
	public float ForceGainDuration = 1.5f;
	public bool HideWhenFinished = true;

	public float NormalizedForce;

	[HideInInspector]
	public bool GainInput;

	private Forcemeter forcemeter;
	public bool IsGainingForce { get; private set; }

	protected override void Awake () {
		base.Awake ();
		forcemeter = GetComponentInChildren <Forcemeter> ();
	}

	protected override void Start () {
		base.Start ();
		configuration.RestoreFloat ( ParameterName, ref NormalizedForce );
	}

	void FixedUpdate () {
		if ( !IsInActiveState ) {
			Cancel ();

			return;
		}

		bool finished = false;

		if ( GainInput ) {
			if ( IsGainingForce ) {
				NormalizedForce += Time.fixedDeltaTime / ForceGainDuration;

				if ( NormalizedForce >= 1 ) {
					NormalizedForce = 1;
					finished = true;
				}
			} else {
				IsGainingForce = true;
				NormalizedForce = 0;
				forcemeter.Begin ();
			}
		} else if ( IsGainingForce )
			finished = true;

		forcemeter.SetProgress ( NormalizedForce );

		if ( finished ) {
			IsGainingForce = false;

			if ( HideWhenFinished )
				forcemeter.End ();

			StoreParameter ( ParameterName, NormalizedForce, Persist );
		}
	}

	void OnDisable () {
		Cancel ();
	}

	public void Cancel () {
		IsGainingForce = false;
		forcemeter.End ();
	}
}