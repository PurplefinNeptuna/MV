using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MV {
	public class RoomMaker : EditorWindow {

		private SerializedObject settings;
		private string roomName = "Room Name";
		private string roomBasePath = "Assets/MV/Resources/Prefabs/RoomBase.prefab";
		private string scriptablePath;
		private string assetPath;
		private string changedString = "None";
		private bool[, ] toggled;
		private bool[, ] oldToggled;
		private List<int> changed;
		private bool nonrectangle;
		private Vector2Int minIdx = Vector2Int.zero;
		private Vector2Int maxIdx = Vector2Int.zero;
		private Vector2 center = Vector2.zero;
		private Vector2 bgCenter = Vector2.zero;
		private Vector2Int bgChunkSize = Vector2Int.zero;
		private Vector2 bgSize = Vector2.zero;
		private GameObject roomBase;
		private MusicData roomMusic;
		private StageData roomStage;
		private Sprite roomBackground;
		private Rect targetSize;
		private bool generateWall;
		private TileBase wallTile;
		private Color roomColor = Color.white;

		[MenuItem("MV/CreateRoom")]
		public static void CreateRoom() {
			GetWindow<RoomMaker>("Room Creator");
		}

		private void OnEnable() {
			settings = MVSettings.GetSerializedSettings();
			scriptablePath = (string) MVEditorUtils.GetValue(settings.FindProperty("roomDataPath"));
			assetPath = (string) MVEditorUtils.GetValue(settings.FindProperty("roomPrefabPath"));
			roomBase = PrefabUtility.LoadPrefabContents(roomBasePath);
			roomMusic = Resources.Load<MusicData>("MusicData/level1");
			roomStage = Resources.Load<StageData>("StageData/Default");
			roomBackground = Resources.Load<Sprite>("Sprites/bg");
			targetSize = new Rect(0, 0, 24, 16);
			toggled = new bool[5, 5];
			oldToggled = new bool[5, 5];
			changed = new List<int>();
		}

		private void OnGUI() {
			changed.Clear();
			if (roomName == "")
				roomName = "Room Name";

			GUILayout.Label("Room Properties", EditorStyles.boldLabel);
			roomName = EditorGUILayout.TextField("Room Name", roomName, GUILayout.MaxWidth(300f), GUILayout.MinWidth(300f));
			roomMusic = (MusicData) EditorGUILayout.ObjectField("Room Music", roomMusic, typeof(MusicData), false, GUILayout.MaxWidth(300f), GUILayout.MinWidth(300f));
			roomStage = (StageData) EditorGUILayout.ObjectField("Room Stage", roomStage, typeof(StageData), false, GUILayout.MaxWidth(300f), GUILayout.MinWidth(300f));
			roomBackground = (Sprite) EditorGUILayout.ObjectField("Room Background", roomBackground, typeof(Sprite), false, GUILayout.MaxWidth(300f), GUILayout.MinWidth(300f));
			roomColor = EditorGUILayout.ColorField("Room Color", roomColor, GUILayout.MaxWidth(300f), GUILayout.MinWidth(300f));
			generateWall = EditorGUILayout.Toggle("Generate Wall", generateWall, GUILayout.MaxWidth(300f), GUILayout.MinWidth(300f));
			if (generateWall)
				wallTile = (TileBase) EditorGUILayout.ObjectField("Wall Tile", wallTile, typeof(TileBase), false, GUILayout.MaxWidth(300f), GUILayout.MinWidth(300f));

			GUILayout.Label("Room Shape", EditorStyles.boldLabel);
			GUILayout.BeginVertical(GUILayout.MaxWidth(120f), GUILayout.MinWidth(120f));
			for (int i = 0; i < toggled.GetLength(0); i++) {
				GUILayout.BeginHorizontal(GUILayout.MaxWidth(120f), GUILayout.MinWidth(120f));
				for (int j = 0; j < toggled.GetLength(1); j++) {
					toggled[i, j] = EditorGUILayout.Toggle(toggled[i, j]);
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			for (int i = 0; i < toggled.GetLength(0); i++) {
				for (int j = 0; j < toggled.GetLength(1); j++) {
					if (toggled[i, j] != oldToggled[i, j]) {
						changed.Add(i * 5 + j);
					}
				}
			}
			if (changed.Count > 0) {
				changedString = "";
			}
			for (int i = 0; i < changed.Count; i++) {
				changedString += "[" + changed[i] + "] ";
			}
			GUILayout.Label("Last Edited: " + changedString, EditorStyles.label);
			oldToggled = (bool[, ]) toggled.Clone();

			nonrectangle = CheckNonSquare();
			center = new Vector2(minIdx.x + maxIdx.x, minIdx.y + maxIdx.y);
			center /= 2;
			bgChunkSize.x = (maxIdx.x - minIdx.x + 1);
			bgChunkSize.y = (minIdx.y - maxIdx.y + 1);
			bgSize = new Vector2(targetSize.width, targetSize.height);
			bgCenter = bgSize;
			bgSize.x *= bgChunkSize.x;
			bgSize.y *= bgChunkSize.y;
			bgCenter.x *= center.x;
			bgCenter.y *= center.y;

			GUILayout.BeginHorizontal(GUILayout.MinWidth(300f), GUILayout.MaxWidth(300f));
			GUILayout.Label("MinPos: " + minIdx, EditorStyles.label, GUILayout.Width(150f));
			GUILayout.Label("MaxPos: " + maxIdx, EditorStyles.label, GUILayout.Width(150f));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.MinWidth(300f), GUILayout.MaxWidth(300f));
			GUILayout.Label("Center: " + center, EditorStyles.label, GUILayout.Width(150f));
			GUILayout.Label("RCenter: " + bgCenter, EditorStyles.label, GUILayout.Width(150f));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.MinWidth(300f), GUILayout.MaxWidth(300f));
			GUILayout.Label("Size: " + bgChunkSize, EditorStyles.label, GUILayout.Width(150f));
			GUILayout.Label("RSize: " + Vector2Int.RoundToInt(bgSize), EditorStyles.label, GUILayout.Width(150f));
			GUILayout.EndHorizontal();

			using(new EditorGUI.DisabledGroupScope(nonrectangle)) {
				if (GUILayout.Button(nonrectangle ? "Non Rectangle Room" : "Create Room", GUILayout.MaxWidth(300f), GUILayout.MinWidth(300f))) {
					string assetResultPath = assetPath + roomName + ".prefab";
					if (AssetDatabase.LoadAssetAtPath(assetResultPath, typeof(GameObject))) {
						if (EditorUtility.DisplayDialog(
								"Are you sure?",
								"Room \"" + roomName + "\" already exists. Do you want to overwrite it?",
								"Yes",
								"No")) {
							BuildRoom(assetResultPath);
						}
					}
					else {
						BuildRoom(assetResultPath);
					}
				}
			}
		}

		private bool CheckNonSquare() {
			bool found = false;
			for (int i = 0; i < toggled.GetLength(0) && !found; i++) {
				for (int j = 0; j < toggled.GetLength(1) && !found; j++) {
					if (toggled[i, j]) {
						minIdx = new Vector2Int(j - 2, -(i - 2));
						found = true;
					}
				}
			}

			found = false;
			for (int i = toggled.GetLength(0) - 1; i >= 0 && !found; i--) {
				for (int j = toggled.GetLength(1) - 1; j >= 0 && !found; j--) {
					if (toggled[i, j]) {
						maxIdx = new Vector2Int(j - 2, -(i - 2));
						found = true;
					}
				}
			}

			Vector2Int nowIdx;
			for (int i = 0; i < toggled.GetLength(0); i++) {
				for (int j = 0; j < toggled.GetLength(1); j++) {
					nowIdx = new Vector2Int(j - 2, -(i - 2));
					if (toggled[i, j] ^ MVUtility.BetweenVector2Int(nowIdx, minIdx, maxIdx)) {
						return true;
					}
				}
			}

			return false;
		}

		private void BuildRoom(string path) {
			RoomData roomData = ScriptableObject.CreateInstance<RoomData>();
			string scriptableResultPath = scriptablePath + roomName + ".asset";
			GameObject bg = roomBase.transform.GetChild(3).gameObject;
			GameObject roomStructure = roomBase.transform.GetChild(0).gameObject;
			SpriteRenderer bgRender = bg.GetComponent<SpriteRenderer>();
			Tilemap groundMap = roomBase.transform.GetChild(1).GetComponent<Tilemap>();
			RectInt mapRect;

			mapRect = new RectInt(Vector2Int.RoundToInt(bgCenter - bgSize * .5f), Vector2Int.RoundToInt(bgSize));

			bgRender.size = bgSize;
			bgRender.sprite = roomBackground;
			bgRender.color = roomColor;
			bg.transform.position = bgCenter;
			roomStructure.transform.position = bgCenter;
			roomStructure.transform.localScale = new Vector3(bgChunkSize.x, bgChunkSize.y, 1);
			PolygonCollider2D pCollider = roomStructure.AddComponent<PolygonCollider2D>();
			if (generateWall)
				BuildWall(groundMap, wallTile, mapRect);

			GameObject result = PrefabUtility.SaveAsPrefabAsset(roomBase, path);
			roomData.Room = result;
			roomData.BGM_Name = roomMusic.name;
			roomData.stage = roomStage.name;
			roomData.chunkSize = new Vector2Int(bgChunkSize.y, bgChunkSize.x);
			roomData.chunkTopLeft = minIdx;
			AssetDatabase.CreateAsset(roomData, scriptableResultPath);
			AssetDatabase.SaveAssets();
			Debug.Log("Room " + roomName + " Created!");
		}

		private void BuildWall(Tilemap map, TileBase tile, RectInt rectBound) {
			foreach (Vector2Int pos in rectBound.allPositionsWithin) {
				if (pos.x == rectBound.xMin || pos.x == rectBound.xMax - 1 || pos.y == rectBound.yMin || pos.y == rectBound.yMax - 1)
					map.SetTile(new Vector3Int(pos.x, pos.y, 0), tile);
			}
		}
	}
}
