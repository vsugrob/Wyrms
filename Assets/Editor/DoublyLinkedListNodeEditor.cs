using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor ( typeof ( DoublyLinkedListNode ) )]
public class DoublyLinkedListNodeEditor : ExposedPropertiesEditor {
	public override void OnInspectorGUI () {
		if ( target == null )
			return;

		var node = target as DoublyLinkedListNode;
		var recordedObjects = new List <UnityEngine.Object> () { node };

		if ( node.Prev != null )
			recordedObjects.Add ( node.Prev );

		if ( node.Next != null )
			recordedObjects.Add ( node.Next );

		Undo.RecordObjects ( recordedObjects.ToArray (), node.name + " property change" );

		this.DrawDefaultInspector ();
		ExposeProperties.Expose ( propertyFields, recordUndo : false );
	}
}
