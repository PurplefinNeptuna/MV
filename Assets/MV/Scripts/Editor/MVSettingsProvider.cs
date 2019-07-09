using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Create MyCustomSettingsProvider by deriving from SettingsProvider:
class MVSettingsProvider : SettingsProvider {
	private SerializedObject m_CustomSettings;

	class Styles {
		public static GUIContent number = new GUIContent("My Number");
		public static GUIContent someString = new GUIContent("Some string");
	}

	const string k_MyCustomSettingsPath = "Assets/MV/Settings/MVSettings.asset";
	public MVSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) {
		label = "MV Project Settings";
	}

	public static bool IsSettingsAvailable() {
		return File.Exists(k_MyCustomSettingsPath);
	}

	public override void OnActivate(string searchContext, VisualElement rootElement) {
		// This function is called when the user clicks on the MyCustom element in the Settings window.
		m_CustomSettings = MVSettings.GetSerializedSettings();
	}

	public override void OnGUI(string searchContext) {
		// Use IMGUI to display UI:
		EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("m_Number"), Styles.number);
		EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("m_SomeString"), Styles.someString);

		m_CustomSettings.ApplyModifiedProperties();
	}

	// Register the SettingsProvider
	[SettingsProvider]
	public static SettingsProvider CreateMyCustomSettingsProvider() {
		if (!IsSettingsAvailable()) {
			AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MVSettings>(), k_MyCustomSettingsPath);
			AssetDatabase.SaveAssets();
		}
		if (IsSettingsAvailable()) {
			var provider = new MVSettingsProvider("Project/MVSettingsProvider", SettingsScope.Project);

			// Automatically extract all keywords from the Styles.
			provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
			return provider;
		}

		// Settings Asset doesn't exist yet; no need to display anything in the Settings window.
		return null;
	}
}
