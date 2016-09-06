using UnityEditor;
using UnityEngine;

public class ExposedPropertiesEditor : Editor {
	protected PropertyField [] propertyFields;

	public virtual void OnEnable () {
		propertyFields = ExposeProperties.GetProperties ( target );
	}

	public override void OnInspectorGUI () {
		if ( target == null )
			return;

		this.DrawDefaultInspector ();
		ExposeProperties.Expose ( propertyFields, recordUndo : true );
	}
}