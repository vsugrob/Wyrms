using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class DevCoroutineMessageHandler : MonoBehaviour {
	void Start () {
		SendMessage ( "MessageHandler" );
	}

	IEnumerator MessageHandler () {
		print ( "Time.fixedTime: " + Time.fixedTime );
		yield return	new WaitForSeconds ( 1.1f );
		print ( "Time.fixedTime: " + Time.fixedTime );
	}
}
