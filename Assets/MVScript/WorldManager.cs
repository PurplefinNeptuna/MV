using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ChunkData {
	public string roomName = "";
	public string marker = "";
	public Color markerColor;
	public Color roomColor;
	public int stageID = -1;
	public bool explored = false;
	public bool[] small = new bool[4];
	public bool[] big = new bool[4];
	public bool[] diagonal = new bool[4];
	public Vector2Int localChunkPos;

	public ChunkData() {}

	public ChunkData(ChunkData node) {
		this.roomName = node.roomName;
		this.marker = node.marker;
		this.markerColor = node.markerColor;
		this.roomColor = node.roomColor;
		this.stageID = node.stageID;
		this.explored = node.explored;
		node.small.CopyTo(small, 0);
		node.big.CopyTo(big, 0);
		node.diagonal.CopyTo(diagonal, 0);
		this.localChunkPos = node.localChunkPos;
	}

	public int SInt {
		get {
			int sInt = 0;
			for (int i = 3; i >= 0; i--) {
				if (small[i]) {
					sInt++;
				}
				if (i > 0) {
					sInt = sInt << 1;
				}
			}
			return sInt;
		}
	}

	public int BInt {
		get {
			int bInt = 0;
			for (int i = 3; i >= 0; i--) {
				if (big[i]) {
					bInt++;
				}
				if (i > 0) {
					bInt = bInt << 1;
				}
			}
			return bInt;
		}
	}

	public int DInt {
		get {
			int dInt = 0;
			for (int i = 3; i >= 0; i--) {
				if (diagonal[i]) {
					dInt++;
				}
				if (i > 0) {
					dInt = dInt << 1;
				}
			}
			return dInt;
		}
	}

	public string Data {
		get {
			string result = "";
			if (explored)
				result = BInt.ToString("X1") + SInt.ToString("X1") + DInt.ToString("X1");
			else
				result = "000";
			return result;
		}
	}

	public void SetBig(Direction dir) {
		int d = 0;
		switch (dir) {
		case Direction.Up:
			d = 3;
			break;
		case Direction.Right:
			d = 2;
			break;
		case Direction.Down:
			d = 1;
			break;
		case Direction.Left:
			d = 0;
			break;
		}
		big[d] = true;
	}

	public void SetSmall(Direction dir) {
		int d = 0;
		switch (dir) {
		case Direction.Up:
			d = 3;
			break;
		case Direction.Right:
			d = 2;
			break;
		case Direction.Down:
			d = 1;
			break;
		case Direction.Left:
			d = 0;
			break;
		}
		small[d] = true;
	}
}

public class WorldManager : MonoBehaviour {
	//public static WorldManager main;

	public ChunkData[, ] map;
	public Vector2Int startScan;
	public Vector2Int mapSize;
	public Dictionary<string, Vector2Int> roomPos;

	private Queue<Tuple<string, Vector2Int>> roomQueue;
	private HashSet<string> roomQueueList;

	private void Init() {
		for (int i = 0; i < mapSize.x; i++) {
			for (int j = 0; j < mapSize.y; j++) {
				map[i, j] = new ChunkData();
			}
		}
	}

	private void Scan() {
		foreach (var room in MVMain.Room.rooms) {
			string debugString = room.Key + " : ";
			foreach (var receiver in room.Value.receivers) {
				debugString += receiver.name + " ";
			}
			Debug.Log(debugString);
		}
		roomQueue = new Queue<Tuple<string, Vector2Int>>();
		roomQueueList = new HashSet<string>();
		string startMap = MVMain.Room.activeRoomName;
		roomQueue.Enqueue(new Tuple<string, Vector2Int>(startMap, startScan));
		roomQueueList.Add(startMap);
		while (roomQueue.Count > 0) {
			var now = roomQueue.Dequeue();
			roomPos.Add(now.Item1, now.Item2);
			if (MVMain.Room.rooms.ContainsKey(now.Item1)) {
				RoomData roomNow = MVMain.Room.rooms[now.Item1];
				for (int i = 0; i < roomNow.chunkSize.x; i++) {
					for (int j = 0; j < roomNow.chunkSize.y; j++) {
						ChunkData nowChunk = map[now.Item2.x + i, now.Item2.y + j];
						nowChunk.roomName = now.Item1;
						nowChunk.localChunkPos = new Vector2Int(i, j);
						//nowChunk.roomColor = roomNow.mapColor;
						nowChunk.roomColor = Resources.Load<StageData>("StageData/" + roomNow.stage).color;
						bool up = false;
						bool down = false;
						bool left = false;
						bool right = false;
						//have up sibling
						if (i > 0) {
							nowChunk.SetBig(Direction.Up);
							up = true;
						}
						//have down sibling
						if (i < roomNow.chunkSize.x - 1) {
							nowChunk.SetBig(Direction.Down);
							down = true;
						}
						//have left sibling
						if (j > 0) {
							nowChunk.SetBig(Direction.Left);
							left = true;
						}
						//have right sibling
						if (j < roomNow.chunkSize.y - 1) {
							nowChunk.SetBig(Direction.Right);
							right = true;
						}
						//topleft
						if (up && left) {
							nowChunk.diagonal[3] = true;
						}
						//topright
						if (up && right) {
							nowChunk.diagonal[2] = true;
						}
						//bottomright
						if (down && right) {
							nowChunk.diagonal[1] = true;
						}
						//topleft
						if (down && left) {
							nowChunk.diagonal[0] = true;
						}
					}
				}
				for (int i = 0; i < roomNow.teleporters.Count; i++) {
					TeleporterData connector = roomNow.teleporters[i];
					ChunkData nowChunk = map[now.Item2.x + connector.chunkPos.x, now.Item2.y + connector.chunkPos.y];
					nowChunk.SetSmall(connector.teleporterDirection);

					if (!roomQueueList.Contains(connector.targetRoom)) {
						Vector2Int nextPos = now.Item2 + connector.chunkPos + MVUtility.DirectionToVector2Int(connector.teleporterDirection);
						if (MVMain.Room.rooms.ContainsKey(connector.targetRoom)) {
							RoomData nextRoom = MVMain.Room.rooms[connector.targetRoom];
							ReceiverData nextReceiver = nextRoom.receivers.FirstOrDefault(x => x.name == connector.targetTeleporter);
							nextPos -= nextReceiver.chunkPos;
							roomQueue.Enqueue(new Tuple<string, Vector2Int>(nextRoom.name, nextPos));
							roomQueueList.Add(nextRoom.name);
						}
					}
				}
			}
		}
	}

	public void GetMinimapData(out ChunkData[, ] result, Vector2Int startpos, Vector2Int size) {
		result = new ChunkData[size.x, size.y];
		for (int i = 0; i < size.x; i++) {
			for (int j = 0; j < size.y; j++) {
				if (i + startpos.x > 0 && i + startpos.x < map.GetLength(0) && j + startpos.y > 0 && j + startpos.y < map.GetLength(1))
					result[i, j] = map[i + startpos.x, j + startpos.y];
			}
		}
	}

	public void RevealChunk(string room, Vector2Int pos) {
		Vector2Int realPos = roomPos[room] + pos;
		map[realPos.x, realPos.y].explored = true;
	}

	public void SetMarker(string marker, string room, Vector2Int pos, Color? color = null) {
		Vector2Int realPos = roomPos[room] + pos;
		map[realPos.x, realPos.y].marker = marker;
		if (color == null) {
			map[realPos.x, realPos.y].markerColor = Color.white;
		}
		else {
			map[realPos.x, realPos.y].markerColor = (Color) color;
		}
	}

	public string GetMarker(string room, Vector2Int pos) {
		Vector2Int realPos = roomPos[room] + pos;
		return map[realPos.x, realPos.y].marker;
	}

	private void Awake() {
		if (MVMain.World == null) {
			MVMain.World = this;
		}
		else if (MVMain.World != this) {
			Destroy(gameObject);
		}

		map = new ChunkData[mapSize.x, mapSize.y];
		roomPos = new Dictionary<string, Vector2Int>();
		Init();
		Scan();
	}
}
