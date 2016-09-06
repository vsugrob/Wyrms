using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class GuiHelper {
	private const string NotificationPanelName = "Gui Notifications";
	private static GuiNotificationPanel notificationPanel = null;

	public static GuiNotificationPanel NotificationPanel {
		get {
			if ( notificationPanel == null ) {
				var panelGameObject = GameObject.Find ( NotificationPanelName );

				if ( panelGameObject == null ) {
					panelGameObject = new GameObject ( NotificationPanelName );
					notificationPanel = panelGameObject.AddComponent <GuiNotificationPanel> ();
				} else
					notificationPanel = panelGameObject.GetComponent <GuiNotificationPanel> ();
			}

			return	notificationPanel;
		}
	}
}
