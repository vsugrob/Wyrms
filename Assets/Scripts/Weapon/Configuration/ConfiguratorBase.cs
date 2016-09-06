using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class ConfiguratorBase : MonoBehaviour {
	public bool AlwaysActive = false;
	public string [] ActiveItemStates = new [] { "Ready" };
	public bool AcceptTransitionState = false;
	private Animator animator;
	protected WeaponConfiguration configuration;

	public bool IsInActiveState {
		get {
			if ( AlwaysActive )
				return	true;

			/* TODO: why am I forced to re-request animator?
			 * Why if I use animator instance requested and stored in Start () method,
			 * it reports false when state.IsName () called?
			 * Are they different instances? Investigate this. */
			animator = GetComponentInChildren <Animator> ();

			if ( animator != null ) {
				if ( !AcceptTransitionState && animator.IsInTransition ( 0 ) )
					return	false;

				var state = animator.GetCurrentAnimatorStateInfo ( 0 );
				
				return	ActiveItemStates.Any ( stateName => state.IsName ( stateName ) );
			}

			return	false;
		}
	}

	protected virtual void Awake () {
		animator = GetComponentInChildren <Animator> ();
	}

	protected virtual void Start () {
		configuration = GetComponentInParent <WeaponConfiguration> ();
	}

	protected void StoreParameter ( string name, object value, bool persist ) {
		var parameter = new ConfigurationParameter ( name, value, persist );
		ConfigurationParameter.SendMessage ( gameObject, parameter );
		configuration.StoreParameter ( parameter );
	}
}
