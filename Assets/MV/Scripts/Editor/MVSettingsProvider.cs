using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MV {
	// Create MyCustomSettingsProvider by deriving from SettingsProvider:
	class MVSettingsProvider : SettingsProvider {
		private SerializedObject m_CustomSettings;

		class Styles {
			public static GUIContent enemyPath = new GUIContent("Enemy Prefab Path");
			public static GUIContent projPath = new GUIContent("Projectile Prefab Path");
			public static GUIContent roomGOPath = new GUIContent("Room Prefab Path");
			public static GUIContent roomDataPath = new GUIContent("Room Data Path");
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
			EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("enemyPrefabPath"), Styles.enemyPath);
			EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("projectilePrefabPath"), Styles.projPath);
			EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("roomPrefabPath"), Styles.roomGOPath);
			EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("roomDataPath"), Styles.roomDataPath);

			m_CustomSettings.ApplyModifiedProperties();
		}

		// Register the SettingsProvider
		[SettingsProvider]
		public static SettingsProvider CreateMyCustomSettingsProvider() {
			if (!IsSettingsAvailable()) {
				MVSettings.GetSerializedSettings();
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
}
