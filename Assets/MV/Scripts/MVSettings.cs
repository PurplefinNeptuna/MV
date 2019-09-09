using System;
using UnityEditor;
using UnityEngine;

namespace MV {
	// Create a new type of Settings Asset.
	[Serializable]
	public class MVSettings : ScriptableObject {
		public const string k_MyCustomSettingsPath = "Assets/MV/Settings/MVSettings.asset";
		public string enemyPrefabPath = "Prefabs/Enemies";
		public string projectilePrefabPath = "Prefabs/Projectiles";
		public string soundPath = "Sounds";
		public string musicDataPath = "MusicData";
		public string stageDataPath = "StageData";
		public string roomPrefabPath = "Assets/Resources/Rooms";
		public string roomDataPath = "Assets/Resources/RoomData";

		internal static MVSettings GetOrCreateSettings() {
			var settings = AssetDatabase.LoadAssetAtPath<MVSettings>(k_MyCustomSettingsPath);
			if (settings == null) {
				settings = ScriptableObject.CreateInstance<MVSettings>();
				AssetDatabase.CreateAsset(settings, k_MyCustomSettingsPath);
				AssetDatabase.SaveAssets();
			}
			return settings;
		}

		public static MVSettings GetSettingsInEditor() {
			return GetOrCreateSettings();
		}

		public static MVSettings GetSettings() {
			var settings = AssetDatabase.LoadAssetAtPath<MVSettings>(k_MyCustomSettingsPath);
			return settings;
		}
	}
}
