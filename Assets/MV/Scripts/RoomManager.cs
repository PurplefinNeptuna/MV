using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MV {
	/// <summary>
	/// Class for managing active and available rooms
	/// </summary>
	public class RoomManager : MonoBehaviour {
		//public static RoomManager main;
		public Dictionary<string, RoomData> rooms;
		public string activeRoomName = "Room0";
		public RoomData activeRoomData;
		public string spawnFrom = "O0";

		private void Awake() {
			if (MVMain.Room == null) {
				MVMain.Room = this;
			}
			else if (MVMain.Room != this) {
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

		/// <summary>
		/// Load the activeRoom data
		/// </summary>
		public void LoadRoomData() {
			if (rooms.ContainsKey(activeRoomName))
				activeRoomData = rooms[activeRoomName];
			else
				activeRoomData = rooms.First().Value;
		}

		/// <summary>
		/// Get local chunk position based from tile position
		/// </summary>
		/// <param name="pos">tile position</param>
		/// <returns>position of chunk in room</returns>
		public Vector2Int GetLocalChunkPos(Vector3Int pos) {
			return MVUtility.TilePosToLocalChunkPos(pos.x, pos.y, activeRoomData.chunkTopLeft, new Vector2Int(24, 16));
		}
	}
}
