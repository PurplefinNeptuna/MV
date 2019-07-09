using UnityEditor;
using UnityEngine;

// Create a new type of Settings Asset.
public class MVSettings : ScriptableObject {
	public const string k_MyCustomSettingsPath = "Assets/MV/Settings/MVSettings.asset";

	[SerializeField]
	private int m_Number;

	[SerializeField]
	private string m_SomeString;

	internal static MVSettings GetOrCreateSettings() {
		var settings = AssetDatabase.LoadAssetAtPath<MVSettings>(k_MyCustomSettingsPath);
		if (settings == null) {
			settings = ScriptableObject.CreateInstance<MVSettings>();
			AssetDatabase.CreateAsset(settings, k_MyCustomSettingsPath);
			AssetDatabase.SaveAssets();
		}
		return settings;
	}

	internal static SerializedObject GetSerializedSettings() {
		return new SerializedObject(GetOrCreateSettings());
	}
}
