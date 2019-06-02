using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour {
	public static RoomManager main;
	public Dictionary<string, RoomData> rooms;
	public string activeRoomName = "Room0";
	public RoomData activeRoomData;
	public string spawnFrom = "O0";

	private void Awake() {
		if (main == null) {
			main = this;
		}
		else if (main != this) {
			Destroy(gameObject);
		}

		rooms = Resources.LoadAll<RoomData>("RoomsData").ToDictionary(x => x.name, x => x);

		if (activeRoomName == null || activeRoomName == "") {
			activeRoomName = rooms.First().Value.name;
		}

		if (spawnFrom == null || spawnFrom == "") {
			spawnFrom = "O0";
		}

		LoadRoomData();
	}

	private void Start() {
		if (Application.isEditor) {
			foreach (var room in rooms) {
				room.Value.RecalculateData();
			}
		}
	}

	public void LoadRoomData() {
		if (rooms.ContainsKey(activeRoomName))
			activeRoomData = rooms[activeRoomName];
		else
			activeRoomData = rooms.First().Value;
	}

	public Vector2Int GetLocalChunkPos(Vector3Int pos) {
		return GameUtility.TilePosToLocalChunkPos(pos.x, pos.y, activeRoomData.chunkTopLeft, new Vector2Int(24, 16));
	}
}
