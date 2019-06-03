using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MV {
	/// <summary>
	/// Class for tile data
	/// </summary>
	[Serializable]
	public class WorldTile {
		public Vector3Int localPlace;

		public Vector3 worldLocation;

		public TileBase tileBase;

		public Tilemap tilemapMember;

		public string name;

		public string roomTarget;

		public string teleportTarget;

		public TeleporterData.TeleporterType transitionMode;
	}
}
