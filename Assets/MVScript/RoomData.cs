using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class TeleporterData {
	public enum TeleporterType {
		Vertical,
		Horizontal,
		Direct
	}
	public string teleporter;
	public string targetRoom;
	public string targetTeleporter;
	public Vector2Int chunkPos;
	public TeleporterType teleporterType;
	[Tooltip("Arah teleporter\nPositive -> Kanan/bawah\nNegative -> Kiri/atas")]
	public Direction teleporterDirection;
}

[Serializable]
public class ReceiverData {
	public string name;
	public Vector2Int chunkPos;
}

//[CreateAssetMenu(fileName = "NewRoom", menuName = "MVFramework/Create New Room Data")]
public class RoomData : ScriptableObject {
	[Header("Generated Data")]
	public GameObject Room;
	public string BGM_Name;
	public string stage = "Default";
	public Vector2Int chunkSize;
	public Vector2Int chunkTopLeft;
	[Header("Editable Data")]
	public List<TeleporterData> teleporters;
	[Header("Generated Data")]
	public List<ReceiverData> receivers;

	public void GetRoomTiles(out Dictionary<Vector3Int, WorldTile> tiles, out List<WorldTile> teleporter) {
		tiles = new Dictionary<Vector3Int, WorldTile>();
		teleporter = new List<WorldTile>();

		Grid grid = Room.GetComponentInChildren<Grid>();
		foreach (Tilemap map in grid.GetComponentsInChildren<Tilemap>()) {
			foreach (Vector3Int pos in map.cellBounds.allPositionsWithin) {
				Vector3Int localplace = new Vector3Int(pos.x, pos.y, pos.z);

				if (!map.HasTile(localplace))
					continue;
				if (tiles.ContainsKey(localplace))
					tiles.Remove(localplace);
				WorldTile tile = new WorldTile() {
					localPlace = localplace,
						worldLocation = new Vector3(localplace.x + .5f, localplace.y + .5f, localplace.z),
						tileBase = map.GetTile(localplace),
						tilemapMember = map,
				};

				tile.name = tile.tileBase.name;

				if (map.name == "Teleporter") {
					if (tile.name[0] == 'I' && tile.name.Length == 2 && char.IsDigit(tile.name[1])) {
						TeleporterData teleporterData = teleporters.FirstOrDefault(x => x.teleporter == tile.name);
						if (teleporterData != null) {
							tile.roomTarget = teleporterData.targetRoom;
							tile.teleportTarget = teleporterData.targetTeleporter;
							tile.transitionMode = teleporterData.teleporterType;
						}
					}

					teleporter.Add(tile);
				}

				tiles.Add(tile.localPlace, tile);
			}
		}
	}

	public void RecalculateData() {
		receivers = new List<ReceiverData>();
		Grid grid = Room.GetComponentInChildren<Grid>();
		foreach (Tilemap map in grid.GetComponentsInChildren<Tilemap>()) {
			if (map.name == "Teleporter") {
				foreach (Vector3Int pos in map.cellBounds.allPositionsWithin) {
					Vector3Int localplace = new Vector3Int(pos.x, pos.y, pos.z);

					if (!map.HasTile(localplace))
						continue;

					string tileName = map.GetTile(pos).name;
					Vector2Int tileChunkPos = MVUtility.TilePosToLocalChunkPos(pos.x, pos.y, chunkTopLeft, new Vector2Int(24, 16));

					if (tileName[0] == 'O' && tileName.Length == 2 && char.IsDigit(tileName[1])) {
						//Receiver
						ReceiverData receiverData = new ReceiverData {
							name = tileName,
								chunkPos = tileChunkPos
						};
						receivers.Add(receiverData);
					}
					else if (tileName[0] == 'I' && tileName.Length == 2 && char.IsDigit(tileName[1])) {
						//Teleporter
						TeleporterData teleporter = teleporters.FirstOrDefault(x => x.teleporter == tileName);
						if (teleporter == null) {
							continue;
						}
						//atas
						if (tileChunkPos.x < 0) {
							teleporter.teleporterType = TeleporterData.TeleporterType.Vertical;
							teleporter.teleporterDirection = Direction.Up;
							teleporter.chunkPos = tileChunkPos + Vector2Int.right;
						}
						//bawah
						else if (tileChunkPos.x >= chunkSize.x) {
							teleporter.teleporterType = TeleporterData.TeleporterType.Vertical;
							teleporter.teleporterDirection = Direction.Down;
							teleporter.chunkPos = tileChunkPos + Vector2Int.left;
						}
						//kiri
						else if (tileChunkPos.y < 0) {
							teleporter.teleporterType = TeleporterData.TeleporterType.Horizontal;
							teleporter.teleporterDirection = Direction.Left;
							teleporter.chunkPos = tileChunkPos + Vector2Int.up;
						}
						//kanan
						else if (tileChunkPos.y >= chunkSize.y) {
							teleporter.teleporterType = TeleporterData.TeleporterType.Horizontal;
							teleporter.teleporterDirection = Direction.Right;
							teleporter.chunkPos = tileChunkPos + Vector2Int.down;
						}
						//bug :v
						else {
							teleporter.teleporterType = TeleporterData.TeleporterType.Direct;
							teleporter.teleporterDirection = Direction.Up;
							teleporter.chunkPos = tileChunkPos;
						}
					}
				}
			}
		}

	}
}
