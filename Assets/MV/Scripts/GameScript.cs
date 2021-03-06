using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace MV {
	/// <summary>
	/// Data for saving
	/// </summary>
	[Serializable]
	public class GameScriptData {
		public int score;
		public string lastRoom;
		public string lastSpawn;
		public int lastMaxHealth;
		public List<SerializedWeapon> weapons;
		public List<ChunkRow> savedMapRow;

		public GameScriptData() {
			savedMapRow = new List<ChunkRow>();
			weapons = new List<SerializedWeapon>();
		}
	}

	[Serializable]
	public class ChunkRow {
		public List<ChunkData> rowData;
		public ChunkRow() {
			rowData = new List<ChunkData>();
		}
	}

	[Serializable]
	public class Timer {
		public float totalPlayTime = 0;
	}

	/// <summary>
	/// Core for MV functionality
	/// </summary>
	[ExecOrder(-70)]
	public class GameScript : MonoBehaviour {
		[HideInInspector]
		public Camera GameMainCamera;
		[HideInInspector]
		public GameScriptData data;
		[HideInInspector]
		public bool testMode = false;
		[HideInInspector]
		public GameObject testRoom;
		public GameObject playerPrefab;
		[HideInInspector]
		public GameObject player;
		[HideInInspector]
		public Player playerPlayer;
		[HideInInspector]
		public Vector2Int playerLocalChunkPos;
		[HideInInspector]
		public string lastRoom;
		[HideInInspector]
		public string lastSpawn;
		[HideInInspector]
		public int lastMaxHealth;
		public int score = 0;
		[HideInInspector]
		public int finalScore = 0;
		[HideInInspector]
		public Timer gameTimer;
		[HideInInspector]
		public float startTime = 0;
		[HideInInspector]
		public bool dead = false;
		[HideInInspector]
		public bool win = false;
		[HideInInspector]
		public GameObject VCamPlayer;
		[HideInInspector]
		public GameObject room;
		public GameObject healthDrop;
		[HideInInspector]
		public ContactFilter2D playerContact;
		[HideInInspector]
		public Grid grid;
		public Dictionary<Vector3Int, WorldTile> tiles;
		[HideInInspector]
		public List<WorldTile> teleporter;
		public LayerMask playerLayer;
		public LayerMask playerCoreLayer;
		public LayerMask enemyLayer;
		public LayerMask groundLayer;
		public TileBase trapperTile;
		[HideInInspector]
		public bool bossBattle;
		[HideInInspector]
		public BaseEnemy bossEnemy;
		public bool haltGame = false;
		[HideInInspector]
		private bool playerTrapped;
		private List<Vector3Int> trapperTiles = new List<Vector3Int>();
		[HideInInspector]
		private Vector3 playerPosDelta;
		private ChunkData[, ] savedMap;

		private void Awake() {
			if (MVMain.Core != this) {
				Destroy(gameObject);
			}

			GameMainCamera = Camera.main;
		}

		private void Start() {
			playerLayer = LayerMask.GetMask("Player");
			playerCoreLayer = LayerMask.GetMask("PlayerCore");
			enemyLayer = LayerMask.GetMask("Enemy", "EnemyMimic", "EnemyThrough");
			groundLayer = LayerMask.GetMask("Ground");
			playerContact.useTriggers = false;
			playerContact.SetLayerMask(playerLayer);

			if (player == null) {
				player = Instantiate<GameObject>(playerPrefab, Vector3.zero, Quaternion.identity);
				player.name = playerPrefab.name;
				player.transform.SetAsFirstSibling();
			}
			playerPlayer = player.GetComponent<Player>();

			//SAVE MINIMAP HERE
			if (!LoadAllData())
				LoadRoom(TeleporterData.TeleporterType.Direct);
			SaveAllData();

			if (!LoadTimer()) {
				gameTimer = new Timer();
			}

			startTime = Time.time;
		}

		private void Update() {
			if (dead) {
				if (Input.GetButtonDown("Cancel")) {
					SceneManager.LoadScene(0);
				}
				else if (Input.GetButtonDown("Submit") && !win) {
					Retry();
				}
			}
			else if (Input.GetButtonDown("Submit")) {
				haltGame = haltGame?false : true;
			}
			else {
				UpdatePlayerChunk();
			}

			if ((bossBattle && !playerTrapped) || (!bossBattle && playerTrapped)) {
				TrapPlayerInRoom(bossBattle);
			}
			finalScore = Mathf.RoundToInt((float) score * MVMain.Difficulty.difficulty);
		}

		/// <summary>
		/// Load room referenced by MVMain.Room.activeroomData
		/// </summary>
		/// <param name="transitionMode">transition movement</param>
		public void LoadRoom(TeleporterData.TeleporterType transitionMode) {
			if (!testMode) {
				room = Instantiate<GameObject>(MVMain.Room.activeRoomData.Room, Vector3.zero, Quaternion.identity);
				room.name = MVMain.Room.activeRoomData.name;
			}
			else {
				room = testRoom;
			}

			grid = room.GetComponentInChildren<Grid>();
			MVMain.Room.activeRoomData.GetRoomTiles(out tiles, out teleporter);

			if (!testMode) {
				SpawnPlayer(transitionMode);
			}

			SetCamera();
			MVMain.World.RevealChunk(MVMain.Room.activeRoomName, playerLocalChunkPos);
			MVMain.Music.Play(MVMain.Room.activeRoomData.BGM_Name);
		}

		/// <summary>
		/// Teleport player to referenced teleporter position in MVMain.Room.spawnFrom
		/// </summary>
		/// <param name="transitionMode">transition movement</param>
		private void SpawnPlayer(TeleporterData.TeleporterType transitionMode) {
			WorldTile spawnTile = teleporter.FirstOrDefault(x => x.name == MVMain.Room.spawnFrom);
			if (spawnTile == null)
				return;

			Vector3 oldPlayerPos = playerPlayer.GetPosition();
			Vector3 newPlayerPos = Vector3.zero;
			switch (transitionMode) {
			case TeleporterData.TeleporterType.Vertical:
				newPlayerPos = new Vector3(oldPlayerPos.x, spawnTile.worldLocation.y, spawnTile.worldLocation.z);
				break;
			case TeleporterData.TeleporterType.Horizontal:
				newPlayerPos = new Vector3(spawnTile.worldLocation.x, oldPlayerPos.y, spawnTile.worldLocation.z);
				break;
			case TeleporterData.TeleporterType.Direct:
				newPlayerPos = spawnTile.worldLocation;
				break;
			}
			playerPosDelta = newPlayerPos - oldPlayerPos;
			playerPlayer.SetPosition(newPlayerPos);
			UpdatePlayerChunk();
		}

		/// <summary>
		/// Set Cinemachine camera settings
		/// </summary>
		private void SetCamera() {
			CinemachineVirtualCamera playerVC = VCamPlayer.GetComponent<CinemachineVirtualCamera>();
			if (playerVC.Follow == null) {
				playerVC.Follow = player.transform;
			}

			CinemachineFramingTransposer playerCamFT = VCamPlayer.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
			float oldLookAheadTime = playerCamFT.m_LookaheadTime;
			playerCamFT.m_LookaheadTime = 0;
			StartCoroutine(SetLookAhead(playerCamFT, oldLookAheadTime));

			Collider2D bound = room.transform.GetChild(0).GetComponent<CompositeCollider2D>();
			Collider2D boxBound = room.transform.GetChild(0).GetComponent<BoxCollider2D>();
			if (boxBound != null) {
				boxBound.usedByComposite = false;
				boxBound.usedByComposite = true;
			}
			else {
				bound = room.transform.GetChild(0).GetComponent<PolygonCollider2D>();
			}
			CinemachineConfiner PlayerCamConfiner = VCamPlayer.GetComponent<CinemachineConfiner>();

			PlayerCamConfiner.m_BoundingShape2D = bound;

			int numVcams = CinemachineCore.Instance.VirtualCameraCount;
			for (int i = 0; i < numVcams; ++i) {
				CinemachineCore.Instance.GetVirtualCamera(i).OnTargetObjectWarped(player.transform, playerPosDelta);
			}
		}

		/// <summary>
		/// Method for forcing dead
		/// </summary>
		public void GameOver() {
			SaveTimer();
			dead = true;
		}

		/// <summary>
		/// Method for respawn and reload last save
		/// </summary>
		private void Retry() {
			player = Instantiate<GameObject>(playerPrefab, Vector3.zero, Quaternion.identity);
			player.name = playerPrefab.name;
			player.transform.SetAsFirstSibling();
			playerPlayer = player.GetComponent<Player>();

			//LOAD MINIMAP HERE
			LoadAllData();

			dead = false;
		}

		/// <summary>
		/// Method called when winning the game
		/// </summary>
		public void Win() {
			dead = true;
			win = true;
		}

		/// <summary>
		/// Called when player come into teleporter tiles
		/// </summary>
		/// <param name="source"></param>
		public void Teleport(WorldTile source) {
			if (testMode || source.roomTarget == "" || source.roomTarget == null)
				return;

			if (source.roomTarget == room.name) {
				WorldTile target = teleporter.FirstOrDefault(x => x.name == source.teleportTarget);
				if (target != null) {
					playerPlayer.SetPosition(target.worldLocation);
				}
			}
			else {
				MVMain.Room.activeRoomName = source.roomTarget;
				MVMain.Room.spawnFrom = source.teleportTarget;
				MVMain.Room.LoadRoomData();
				Destroy(room);
				LoadRoom(source.transitionMode);
			}
		}

		/// <summary>
		/// Set Cinemachine Look Ahead level for cameras
		/// </summary>
		/// <param name="camFT">Cinemachine's camera frame</param>
		/// <param name="time">Level of look ahead in second</param>
		/// <returns></returns>
		private IEnumerator SetLookAhead(CinemachineFramingTransposer camFT, float time) {
			yield return new WaitForSeconds(.2f);
			camFT.m_LookaheadTime = time;
		}

		/// <summary>
		/// Trap/Release player in room when boss exist/not-exist
		/// </summary>
		/// <param name="boss">is the boss exist?</param>
		private void TrapPlayerInRoom(bool boss) {
			MVMain.Music.Play(boss ? "boss1" : MVMain.Room.activeRoomData.BGM_Name);
			Tilemap ground = grid.transform.Find("Ground").GetComponent<Tilemap>();
			if (boss) {
				foreach (var tile in teleporter) {
					if (tile.name[0] == 'I' && tile.name.Length == 2 && char.IsDigit(tile.name[1])) {
						Vector3Int targetPos = tile.localPlace;
						switch (tile.transitionMode) {
						case TeleporterData.TeleporterType.Horizontal:
							targetPos += (tile.localPlace.x < 0 ? Vector3Int.right : Vector3Int.left);
							ground.SetTile(targetPos, trapperTile);
							trapperTiles.Add(targetPos);
							break;
						case TeleporterData.TeleporterType.Vertical:
							targetPos += (tile.localPlace.y < 0 ? Vector3Int.up : Vector3Int.down);
							ground.SetTile(targetPos, trapperTile);
							trapperTiles.Add(targetPos);
							break;
						default:
							break;
						}
					}
				}
			}
			else {
				foreach (Vector3Int pos in trapperTiles) {
					ground.SetTile(pos, null);
				}
				trapperTiles.Clear();
			}
			playerTrapped = boss;
		}

		/// <summary>
		/// Update and reveal chunk the player reside
		/// </summary>
		private void UpdatePlayerChunk() {
			Vector2Int oldPlayerChunkPos = new Vector2Int(playerLocalChunkPos.x, playerLocalChunkPos.y);
			playerLocalChunkPos = GetLocalChunkPos(playerPlayer.GetPosition());
			playerLocalChunkPos.x = Mathf.Clamp(playerLocalChunkPos.x, 0, MVMain.Room.activeRoomData.chunkSize.x - 1);
			playerLocalChunkPos.y = Mathf.Clamp(playerLocalChunkPos.y, 0, MVMain.Room.activeRoomData.chunkSize.y - 1);
			if (oldPlayerChunkPos != playerLocalChunkPos) {
				MVMain.World.RevealChunk(MVMain.Room.activeRoomName, playerLocalChunkPos);
			}
		}

		/// <summary>
		/// get chunk position in room from 3D position
		/// </summary>
		/// <param name="pos">position</param>
		/// <returns>chunk position in room</returns>
		public Vector2Int GetLocalChunkPos(Vector3 pos) {
			Vector3Int gridPos = grid.WorldToCell(pos);
			return MVMain.Room.GetLocalChunkPos(gridPos);
		}

		private void OnDestroy() {
			SaveTimer();
		}

		private void OnApplicationQuit() {
			SaveTimer();
		}

		/// <summary>
		/// Copy minimap(world) data for saving
		/// </summary>
		public void SaveMinimapData() {
			savedMap = new ChunkData[MVMain.World.mapSize.x, MVMain.World.mapSize.y];
			for (int i = 0; i < MVMain.World.mapSize.x; i++) {
				for (int j = 0; j < MVMain.World.mapSize.y; j++) {
					savedMap[i, j] = new ChunkData(MVMain.World.map[i, j]);
				}
			}
		}

		/// <summary>
		/// Load minimap(world) data
		/// </summary>
		private void LoadMinimapData() {
			MVMain.World.map = new ChunkData[MVMain.World.mapSize.x, MVMain.World.mapSize.y];
			for (int i = 0; i < MVMain.World.mapSize.x; i++) {
				for (int j = 0; j < MVMain.World.mapSize.y; j++) {
					MVMain.World.map[i, j] = savedMap[i, j];
				}
			}
		}

		/// <summary>
		/// Save data to persistentPath
		/// </summary>
		/// <param name="spawner">last spawner(checkpoint) position</param>
		public void SaveAllData(string spawner = "") {

			if (spawner == "") {
				spawner = MVMain.Room.spawnFrom;
			}

			lastRoom = MVMain.Room.activeRoomName;
			lastSpawn = spawner;
			lastMaxHealth = playerPlayer.maxHealth;

			data = new GameScriptData {
				score = score,
					lastRoom = lastRoom,
					lastSpawn = lastSpawn,
					lastMaxHealth = lastMaxHealth,
					weapons = playerPlayer.weapons.Select(x => new SerializedWeapon(x)).ToList()
			};

			SaveMinimapData();

			for (int i = 0; i < savedMap.GetLength(0); i++) {
				ChunkRow row = new ChunkRow();
				for (int j = 0; j < savedMap.GetLength(1); j++) {
					row.rowData.Add(savedMap[i, j]);
				}
				data.savedMapRow.Add(row);
			}

			string savePath = Path.Combine(Application.persistentDataPath, "Save");
			if (!Directory.Exists(savePath)) {
				Directory.CreateDirectory(savePath);
			}
			string saveFile = Path.Combine(savePath, "main.json");
			using(StreamWriter dat = new StreamWriter(saveFile)) {
				dat.Write(JsonUtility.ToJson(data));
			}
		}

		/// <summary>
		/// load data from persistentPath
		/// </summary>
		/// <returns>true if save file exist</returns>
		private bool LoadAllData() {
			data = new GameScriptData();
			bool loadExist = false;

			string savePath = Path.Combine(Application.persistentDataPath, "Save");
			if (Directory.Exists(savePath)) {
				string saveFile = Path.Combine(savePath, "main.json");
				if (File.Exists(saveFile)) {
					using(StreamReader dat = new StreamReader(saveFile)) {
						string datJSON = dat.ReadToEnd();
						data = JsonUtility.FromJson<GameScriptData>(datJSON);
						loadExist = true;
					}
				}
			}

			if (!loadExist) {
				return false;
			}

			Vector2Int mapSize = MVMain.World.mapSize;
			savedMap = new ChunkData[mapSize.x, mapSize.y];
			for (int i = 0; i < mapSize.x; i++) {
				for (int j = 0; j < mapSize.y; j++) {
					savedMap[i, j] = data.savedMapRow[i].rowData[j];
				}
			}

			LoadMinimapData();

			score = dead ? 0 : data.score;
			lastRoom = data.lastRoom;
			lastSpawn = data.lastSpawn;
			lastMaxHealth = data.lastMaxHealth;

			playerPlayer.weapons = data.weapons.Select(x => MVUtility.CreateWeapon(x.name, x.level)).ToList();

			playerPlayer.maxHealth = lastMaxHealth;
			playerPlayer.health = lastMaxHealth;

			MVMain.Room.activeRoomName = lastRoom;
			MVMain.Room.spawnFrom = lastSpawn;
			MVMain.Room.LoadRoomData();
			if (room != null)
				Destroy(room);
			LoadRoom(TeleporterData.TeleporterType.Direct);
			return true;
		}

		/// <summary>
		/// save the timer to persistentPath
		/// </summary>
		private void SaveTimer() {
			if (Time.time == 0)
				return;
			gameTimer.totalPlayTime += Time.time - startTime;
			startTime = Time.time;

			string savePath = Path.Combine(Application.persistentDataPath, "Save");
			if (!Directory.Exists(savePath)) {
				Directory.CreateDirectory(savePath);
			}
			string saveFile = Path.Combine(savePath, "timer.json");
			using(StreamWriter dat = new StreamWriter(saveFile)) {
				dat.Write(JsonUtility.ToJson(gameTimer));
			}
		}

		/// <summary>
		/// load timer from persistentPath
		/// </summary>
		/// <returns>true if save file exist</returns>
		private bool LoadTimer() {
			bool loadExist = false;
			Timer loadedTime = new Timer();

			string savePath = Path.Combine(Application.persistentDataPath, "Save");
			if (Directory.Exists(savePath)) {
				string saveFile = Path.Combine(savePath, "timer.json");
				if (File.Exists(saveFile)) {
					using(StreamReader dat = new StreamReader(saveFile)) {
						string datJSON = dat.ReadToEnd();
						loadedTime = JsonUtility.FromJson<Timer>(datJSON);
						loadExist = true;
					}
				}
			}

			if (loadExist) {
				gameTimer = loadedTime;
			}

			return loadExist;
		}
	}
}
