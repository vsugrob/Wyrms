using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GuiNotificationPanel : MonoBehaviour {
	public Rect Position = new Rect ( 100, 20, 600, 800 );

	private List <Message> messages = new List <Message> ();
	private int nextSlotIdx = 0;

	void FixedUpdate () {
		foreach ( var message in messages.ToArray () ) {
			if ( message.IsExpired )
				messages.Remove ( message );
		}
	}

	void OnGUI () {
		string composedText = string.Join ( "\n", messages.Select ( m => m.Text ).ToArray () );

		var style = new GUIStyle () {
			richText = true
		};
		GUI.Label ( Position, composedText, style );
	}

	private bool TryGetMessageBySlot ( string slotId, out Message message ) {
		var slotMessages = messages.Where ( m => m.SlotId == slotId );

		if ( slotMessages.Any () ) {
			message = slotMessages.First ();

			return	true;
		} else {
			message = null;

			return	false;
		}
	}

	public void AddMessage ( string text, string slotId, float duration = float.NaN ) {
		if ( string.IsNullOrEmpty ( slotId ) )
			slotId = "__autoId#" + nextSlotIdx++;

		Message message;

		if ( TryGetMessageBySlot ( slotId, out message ) )
			message.SetText ( text, duration );
		else {
			message = new Message ( text, slotId, duration );
			messages.Add ( message );
		}
	}

	public void AddMessage ( string text, float duration = float.NaN ) {
		AddMessage ( text, ( string ) null, duration );
	}

	public void AddMessage ( string text, Dictionary <string, object> valuesToExpand, string slotId = null, float duration = float.NaN ) {
		text = Common.ExpandString ( text, valuesToExpand );
		AddMessage ( text, slotId, duration );
	}

	private class Message {
		public string Text;
		public string SlotId;
		public float Duration;
		public bool IsExpired { get { return	Time.fixedTime - createdTimestamp >= Duration; } }
		private float createdTimestamp;

		private const float CharactersPerSecond = 10;
		private const float MinAutoDuration = 3;
		private const float MaxAutoDuration = 20;

		public Message ( string text, string slotId = null, float duration = float.NaN ) {
			this.SlotId = slotId;
			SetText ( text, duration );
		}

		public static float CalculateDuration ( string text ) {
			text = Common.StripTags ( text );
			float duration = text.Length / CharactersPerSecond;
			duration = Mathf.Clamp ( duration, MinAutoDuration, MaxAutoDuration );

			return	duration;
		}

		public void SetText ( string text, float duration = float.NaN, bool resetTimestamp = true ) {
			this.Text = text;
			this.Duration = float.IsNaN ( duration ) ? CalculateDuration ( text ) : duration;

			if ( resetTimestamp )
				this.createdTimestamp = Time.fixedTime;
		}
	}
}
