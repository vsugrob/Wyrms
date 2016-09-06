using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent ( typeof ( DirectionalForceField ) )]
public class GlobalWind : MonoBehaviour {
	public float Amount = 1;
	public Vector2 Direction = Vector2.right;
	public float MaxAmount = 2;
	public float AmountChangeSpeed = 2;
	public float DirectionChangeSpeed = 180;
	public DirectionalForceField ForceField { get; private set; }
	public KeyCode DebugRandomizeKeyCode = KeyCode.P;
	public KeyCode DebugRandomizeAmountKeyCode = KeyCode.O;
	public KeyCode DebugRandomizeDirectionKeyCode = KeyCode.I;

	void Start () {
		ForceField = GetComponent <DirectionalForceField> ();
	}

	void FixedUpdate () {
		if ( ForceField.Amount != Amount )
			ForceField.Amount = Mathf.MoveTowards ( ForceField.Amount, Amount, AmountChangeSpeed * Time.fixedDeltaTime );

		ForceField.Direction = Common.RotateTowards ( ForceField.Direction, Direction, DirectionChangeSpeed * Time.fixedDeltaTime );
	}

	void Update () {
		if ( Input.GetKeyDown ( DebugRandomizeKeyCode ) )
			Randomize ();
		else if ( Input.GetKeyDown ( DebugRandomizeAmountKeyCode ) )
			RandomizeAmount ();
		else if ( Input.GetKeyDown ( DebugRandomizeDirectionKeyCode ) )
			RandomizeDirection ();
	}

	public void Randomize () {
		RandomizeAmount ();
		RandomizeDirection ();
	}

	public void RandomizeAmount () {
		Amount = Random.Range ( -MaxAmount, MaxAmount );
	}

	public void RandomizeDirection () {
		Direction = Common.RandomPointOnUnitCircle ();
	}
}
