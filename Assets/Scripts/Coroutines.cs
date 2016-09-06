using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public static class Coroutines {
	// TODO: tests shows that builtin class WaitForSeconds provie same result.
	public static Coroutine WaitForFixedSeconds ( this MonoBehaviour behaviour, float duration ) {
		return	behaviour.StartCoroutine ( WaitForFixedSecondsCoroutine ( duration ) );
	}

	// TODO: tests shows that builtin class WaitForSeconds provie same result.
	private static IEnumerator WaitForFixedSecondsCoroutine ( float duration ) {
		float startTimestamp = Time.fixedTime;

		while ( Time.fixedTime - startTimestamp < duration ) {
			yield return	new WaitForFixedUpdate ();
		}
	}

	public static Coroutine WaitForCondition (
		this MonoBehaviour behaviour, System.Func <bool> condition,
		float maxWaitTime = float.PositiveInfinity, float pauseBetweenChecks = 0.25f
	) {
		return	behaviour.StartCoroutine (
			WaitForConditionCoroutine (
				behaviour, condition,
				maxWaitTime, pauseBetweenChecks
			)
		);
	}

	private static IEnumerator WaitForConditionCoroutine (
		MonoBehaviour behaviour, System.Func <bool> condition,
		float maxWaitTime, float pauseBetweenChecks
	) {
		float startTimestamp = Time.fixedTime;

		while ( Time.fixedTime - startTimestamp < maxWaitTime && !condition () ) {
			yield return	behaviour.WaitForFixedSeconds ( pauseBetweenChecks );
		}
	}

	public static Coroutine InvokeFixed ( this MonoBehaviour behaviour, System.Action action, float delay ) {
		return	behaviour.StartCoroutine ( InvokeFixedCoroutine ( behaviour, action, delay ) );
	}

	private static IEnumerator InvokeFixedCoroutine ( MonoBehaviour behaviour, System.Action action, float delay ) {
		yield return	behaviour.WaitForFixedSeconds ( delay );
		action ();
	}
}
