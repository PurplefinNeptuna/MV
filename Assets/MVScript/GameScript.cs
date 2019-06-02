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

public class GameScript : MonoBehaviour {
	public static GameScript main;
	public Camera GameMainCamera;
	public Canvas GameUICanvas;
	public GameScriptData data;
	public bool testMode = false;
	public GameObject testRoom;
	public GameObject playerPrefab;
	public GameObject player;
	[HideInInspector]
	public Player playerPlayer;
	public Vector2Int playerLocalChunkPos;
	public string lastRoom;
	public string lastSpawn;
	public int lastMaxHealth;
	public int score = 0;
	public int finalScore = 0;
	public Timer gameTimer;
	public float startTime = 0;
	public bool dead = false;
	public bool win = false;
	public GameObject VCamPlayer;
	public GameObject VBossBattle;
	public GameObject room;
	public GameObject healthDrop;
	public ContactFilter2D playerContact;
	public Grid grid;
	public Dictionary<Vector3Int, WorldTile> tiles;
	public List<WorldTile> teleporter;
	public Text scoreText;
	public GameObject gameOverPanel;
	public GameObject winPanel;
	public LayerMask playerLayer;
	public LayerMask playerCoreLayer;
	public LayerMask enemyLayer;
	public LayerMask groundLayer;
	public TileBase trapperTile;
	public bool bossBattle;
	public BaseEnemy bossEnemy;
	public bool haltGame = false;
	private bool playerTrapped;
	private List<Vector3Int> trapperTiles = new List<Vector3Int>();
	private Vector3 playerPosDelta;
	private ChunkData[, ] savedMap;

	private void Awake() {
		if (main == null) {
			main = this;
		}
		else if (main != this) {
			Destroy(gameObject);
		}
	}

	private void Start() {
		playerLayer = LayerMask.GetMask("Player");
		playerCoreLayer = LayerMask.GetMask("PlayerCore");
		enemyLayer = LayerMask.GetMask("Enemy", "EnemyMimic", "EnemyThrough");
		groundLayer = LayerMask.GetMask("Ground");
		playerContact.useTriggers = false;
		playerContact.SetLayerMask(playerLayer);
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
		finalScore = Mathf.RoundToInt((float) score * DifficultyManager.difficulty);
	}

	private void LateUpdate() {
		if ((bossBattle && !VBossBattle.activeSelf) || (!bossBattle && !VCamPlayer.activeSelf)) {
			StartCoroutine(SwitchCamera(bossBattle));
		}
	}

	public void LoadRoom(TeleporterData.TeleporterType transitionMode) {
		if (!testMode) {
			room = Instantiate<GameObject>(RoomManager.main.activeRoomData.Room, Vector3.zero, Quaternion.identity);
			room.name = RoomManager.main.activeRoomData.name;
		}
		else {
			room = testRoom;
		}

		grid = room.GetComponentInChildren<Grid>();
		RoomManager.main.activeRoomData.GetRoomTiles(out tiles, out teleporter);

		if (!testMode) {
			SpawnPlayer(transitionMode);
		}

		SetCamera();
		WorldManager.main.RevealChunk(RoomManager.main.activeRoomName, playerLocalChunkPos);
		MusicManager.Play(RoomManager.main.activeRoomData.BGM_Name);
	}

	private void SpawnPlayer(TeleporterData.TeleporterType transitionMode) {
		WorldTile spawnTile = teleporter.FirstOrDefault(x => x.name == RoomManager.main.spawnFrom);
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

	private void SetCamera() {
		CinemachineVirtualCamera playerVC = VCamPlayer.GetComponent<CinemachineVirtualCamera>();
		CinemachineVirtualCamera bossVC = VBossBattle.GetComponent<CinemachineVirtualCamera>();
		if (playerVC.Follow == null) {
			playerVC.Follow = player.transform;
		}
		if (bossVC.Follow == null) {
			bossVC.Follow = player.transform;
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
		CinemachineConfiner BossCamConfiner = VBossBattle.GetComponent<CinemachineConfiner>();

		PlayerCamConfiner.m_BoundingShape2D = bound;
		BossCamConfiner.m_BoundingShape2D = bound;

		int numVcams = CinemachineCore.Instance.VirtualCameraCount;
		for (int i = 0; i < numVcams; ++i) {
			CinemachineCore.Instance.GetVirtualCamera(i).OnTargetObjectWarped(player.transform, playerPosDelta);
		}
	}

	public void GameOver() {
		SaveTimer();
		gameOverPanel.SetActive(true);
		dead = true;
	}

	private void Retry() {
		gameOverPanel.SetActive(false);
		player = Instantiate<GameObject>(playerPrefab, Vector3.zero, Quaternion.identity);
		player.name = playerPrefab.name;
		player.transform.SetAsFirstSibling();
		playerPlayer = player.GetComponent<Player>();

		//LOAD MINIMAP HERE
		LoadAllData();

		dead = false;
	}

	public void Win() {
		scoreText.text = "score: " + finalScore;
		winPanel.SetActive(true);
		dead = true;
		win = true;
	}

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
			RoomManager.main.activeRoomName = source.roomTarget;
			RoomManager.main.spawnFrom = source.teleportTarget;
			RoomManager.main.LoadRoomData();
			Destroy(room);
			LoadRoom(source.transitionMode);
		}
	}

	private IEnumerator SwitchCamera(bool boss) {
		yield return new WaitForSeconds(1f);
		int ppuScale = boss ? 40 : 64;
		int resX = boss ? 960 : 1024;
		int resY = boss ? 540 : 576;
		Camera camera = Camera.main;
		Rect crect = camera.rect;
		PixelPerfectCamera PPC = camera.GetComponent<PixelPerfectCamera>();
		CinemachinePixelPerfect CPP = camera.GetComponent<CinemachinePixelPerfect>();
		CinemachineBrain CB = camera.GetComponent<CinemachineBrain>();
		CPP.enabled = false;
		PPC.enabled = false;
		camera.rect = crect;
		VBossBattle.SetActive(boss);
		VCamPlayer.SetActive(!boss);
		PPC.assetsPPU = ppuScale;
		PPC.refResolutionX = resX;
		PPC.refResolutionY = resY;
		yield return new WaitForSeconds(CB.m_DefaultBlend.m_Time);
		PPC.enabled = true;
		CPP.enabled = true;
	}

	private IEnumerator SetLookAhead(CinemachineFramingTransposer camFT, float time) {
		yield return new WaitForSeconds(.2f);
		camFT.m_LookaheadTime = time;
	}

	private void TrapPlayerInRoom(bool boss) {
		MusicManager.Play(boss ? "boss1" : RoomManager.main.activeRoomData.BGM_Name);
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

	private void UpdatePlayerChunk() {
		Vector2Int oldPlayerChunkPos = new Vector2Int(playerLocalChunkPos.x, playerLocalChunkPos.y);
		playerLocalChunkPos = GetLocalChunkPos(playerPlayer.GetPosition());
		playerLocalChunkPos.x = Mathf.Clamp(playerLocalChunkPos.x, 0, RoomManager.main.activeRoomData.chunkSize.x - 1);
		playerLocalChunkPos.y = Mathf.Clamp(playerLocalChunkPos.y, 0, RoomManager.main.activeRoomData.chunkSize.y - 1);
		if (oldPlayerChunkPos != playerLocalChunkPos) {
			WorldManager.main.RevealChunk(RoomManager.main.activeRoomName, playerLocalChunkPos);
		}
	}

	public Vector2Int GetLocalChunkPos(Vector3 pos) {
		Vector3Int gridPos = grid.WorldToCell(pos);
		return RoomManager.main.GetLocalChunkPos(gridPos);
	}

	private void OnDestroy() {
		SaveTimer();
	}

	private void OnApplicationQuit() {
		SaveTimer();
	}

	public void SaveMinimapData() {
		savedMap = new ChunkData[WorldManager.main.mapSize.x, WorldManager.main.mapSize.y];
		for (int i = 0; i < WorldManager.main.mapSize.x; i++) {
			for (int j = 0; j < WorldManager.main.mapSize.y; j++) {
				savedMap[i, j] = new ChunkData(WorldManager.main.map[i, j]);
			}
		}
	}

	private void LoadMinimapData() {
		WorldManager.main.map = new ChunkData[WorldManager.main.mapSize.x, WorldManager.main.mapSize.y];
		for (int i = 0; i < WorldManager.main.mapSize.x; i++) {
			for (int j = 0; j < WorldManager.main.mapSize.y; j++) {
				WorldManager.main.map[i, j] = savedMap[i, j];
			}
		}
	}

	public void SaveAllData(string spawner = "") {

		if (spawner == "") {
			spawner = RoomManager.main.spawnFrom;
		}

		lastRoom = RoomManager.main.activeRoomName;
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

		Vector2Int mapSize = WorldManager.main.mapSize;
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

		playerPlayer.weapons = data.weapons.Select(x => GameUtility.CreateWeapon(x.name, x.level)).ToList();

		playerPlayer.maxHealth = lastMaxHealth;
		playerPlayer.health = lastMaxHealth;

		RoomManager.main.activeRoomName = lastRoom;
		RoomManager.main.spawnFrom = lastSpawn;
		RoomManager.main.LoadRoomData();
		if (room != null)
			Destroy(room);
		LoadRoom(TeleporterData.TeleporterType.Direct);
		return true;
	}

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
