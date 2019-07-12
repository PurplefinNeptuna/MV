using UnityEditor;
using UnityEngine;

namespace MV {
	// Create a new type of Settings Asset.
	public class MVSettings : ScriptableObject {
		public const string k_MyCustomSettingsPath = "Assets/MV/Settings/MVSettings.asset";

		[SerializeField]
		private string enemyPrefabPath = "Prefabs/Enemies";

		[SerializeField]
		private string projectilePrefabPath = "Prefabs/Projectiles";

		[SerializeField]
		private string roomPrefabPath = "Rooms";

		[SerializeField]
		private string roomDataPath = "RoomData";

		internal static MVSettings GetOrCreateSettings() {
			var settings = AssetDatabase.LoadAssetAtPath<MVSettings>(k_MyCustomSettingsPath);
			if (settings == null) {
				settings = ScriptableObject.CreateInstance<MVSettings>();
				AssetDatabase.CreateAsset(settings, k_MyCustomSettingsPath);
				AssetDatabase.SaveAssets();
			}
			return settings;
		}

		public static SerializedObject GetSerializedSettings() {
			return new SerializedObject(GetOrCreateSettings());
		}

		public static MVSettings GetSettings() {
			var settings = AssetDatabase.LoadAssetAtPath<MVSettings>(k_MyCustomSettingsPath);
			return settings;
		}

		public string GetEnemyPath() {
			return enemyPrefabPath;
		}

		public string GetProjectilPath() {
			return projectilePrefabPath;
		}
	}
}
