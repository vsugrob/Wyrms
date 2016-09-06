using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DelayedDestroy : MonoBehaviour {
	public float Delay = 0;

	private float createdTimestamp;

	void Start () {
		createdTimestamp = Time.fixedTime;
		Destroy ( gameObject, Delay );
	}

	void OnLifetimeRequest ( LifetimeRequest request ) {
		float timeElapsed = Time.fixedTime - createdTimestamp;
		request.MinLifetime = Delay - timeElapsed;
	}
}
