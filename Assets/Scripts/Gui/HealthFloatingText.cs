using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class HealthFloatingText : MonoBehaviour {
	public float ChangeSpeed = 40;
	public float MaxChangeDuration = 3;
	public FloatingText FloatingText;
	public Color DamageTextColor = Color.red;
	public Color HealTextColor = new Color32 ( 100, 255, 0, 255 );
	public float DamageTextDuration = 3;
	public Vector2 DamageTextStartOffset = new Vector2 ( 0.5f, 0.5f );
	public Vector2 DamageTextEndOffset = new Vector2 ( 0.5f, 1.5f );
	public GUIStyle DamageTextStyle;
	public GUIStyle HealTextStyle;

	private HealthComponent healthComponent;

	private float prevValue;
	private float curValue;

	void Awake () {
		healthComponent = GetComponent <HealthComponent> ();
		prevValue = curValue = healthComponent.Health;

		if ( FloatingText == null ) {
			FloatingText = gameObject.AddComponent <FloatingText> ();
			FloatingText.InheritOwnerColor = true;
		}
	}

	void Update () {
		if ( curValue != healthComponent.Health ) {
			if ( Application.isEditor && !Application.isPlaying )
				curValue = healthComponent.Health;
			else {
				float dHealth = Mathf.Abs ( healthComponent.Health - curValue );
				float duration = dHealth / this.ChangeSpeed;
				float changeSpeed;

				if ( duration > MaxChangeDuration )
					changeSpeed = Mathf.Max ( dHealth / MaxChangeDuration, this.ChangeSpeed * 10 );
				else
					changeSpeed = this.ChangeSpeed;

				curValue = Mathf.MoveTowards ( curValue, healthComponent.Health, changeSpeed * Time.deltaTime );
			}
		}
		
		if ( prevValue != healthComponent.Health ) {
			if ( Application.isPlaying ) {
				int healthChange = ToIntHealth ( healthComponent.Health ) - ToIntHealth ( prevValue );
				
				if ( healthChange != 0 ) {
					var damageText = gameObject.AddComponent <FloatingText> ();
					damageText.Text = ToDamageString ( healthChange );
					damageText.Duration = DamageTextDuration;
					damageText.IsMoving = true;
					damageText.Offset = DamageTextStartOffset;
					damageText.EndOffset = DamageTextEndOffset;

					if ( healthChange < 0 ) {
						damageText.Color = DamageTextColor;
						damageText.Style = DamageTextStyle;
					} else {
						damageText.Color = HealTextColor;
						damageText.Style = HealTextStyle;
					}
				}
			}

			prevValue = healthComponent.Health;
		}

		if ( FloatingText != null ) {
			string newText = ToIntHealth ( curValue ).ToString ();

			if ( FloatingText.Text != newText )
				FloatingText.Text = newText;
		}
	}

	private static int ToIntHealth ( float health ) {
		return	Mathf.CeilToInt ( health );
	}

	private static string ToDamageString ( float damage ) {
		int intDamage = ToIntHealth ( damage );
		string str = intDamage.ToString ();

		if ( intDamage > 0 )
			str = "+" + str;

		return	str;
	}
}
