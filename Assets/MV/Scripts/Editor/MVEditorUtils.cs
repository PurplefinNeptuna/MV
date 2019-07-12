using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

public static class MVEditorUtils {
	public static object GetValue(this SerializedProperty property) {
		object obj = property.serializedObject.targetObject;

		FieldInfo field = null;
		foreach (var path in property.propertyPath.Split('.')) {
			var type = obj.GetType();
			field = type.GetField(path);
			obj = field.GetValue(obj);
		}
		return obj;
	}
}
