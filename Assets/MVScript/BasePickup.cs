using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MV {

	/// <summary>
	/// Base class for pickups
	/// </summary>
	public class BasePickup : BaseObject {

		private Collider2D[] overlapPlayer = new Collider2D[4];
		private bool alreadyGrounded = false;
		protected bool showOnMap = false;
		protected bool showOnDestroyOnly = false;
		protected bool oneTimeOnly = false;
		protected bool forcedDestroy = false;
		public string defaultName = "";

		private void Awake() {
			SetDefault();
		}

		protected override void POStart() {
			if (oneTimeOnly && MVMain.World.GetMarker(MVMain.Room.activeRoomName, MVMain.Core.GetLocalChunkPos(rb2d.position)) == defaultName) {
				Destroy(gameObject);
			}
		}

		private void Update() {
			if (grounded) {
				if (!alreadyGrounded) {
					affectedByGravity = false;
					gameObject.GetComponent<Collider2D>().isTrigger = true;
					rb2d.Sleep();
				}
				velocity = Vector2.zero;
			}
			if (!MVMain.Core.haltGame)
				PickupUpdate(Time.deltaTime);
		}

		protected override void POFixedUpdate() {
			if (!MVMain.Core.dead) {
				int count = rb2d.OverlapCollider(MVMain.Core.playerContact, overlapPlayer);
				if (count > 0) {
					if (showOnMap && !showOnDestroyOnly) {
						Vector2Int chunkPos = MVMain.Core.GetLocalChunkPos(rb2d.position);
						Color color = GetComponent<SpriteRenderer>().color;
						MVMain.World.SetMarker(defaultName, MVMain.Room.activeRoomName, chunkPos, color);
						if (MVMain.World.map != null) {
							//UIManager.main.ForceRefreshMinimap();
						}
					}
					OnHitPlayer(MVMain.Core.playerPlayer);
				}
			}
		}

		private void OnDestroy() {
			if (showOnMap && showOnDestroyOnly && forcedDestroy) {
				Vector2Int chunkPos = MVMain.Core.GetLocalChunkPos(rb2d.position);
				Color color = GetComponent<SpriteRenderer>().color;
				MVMain.World.SetMarker(defaultName, MVMain.Room.activeRoomName, chunkPos, color);
				if (MVMain.World.map != null) {
					//UIManager.main.ForceRefreshMinimap();
				}
			}
		}

		/// <summary>
		/// Run when player get the item
		/// </summary>
		/// <param name="player">The player</param>
		public virtual void OnHitPlayer(Player player) {}

		/// <summary>
		/// Set default properties here
		/// </summary>
		public virtual void SetDefault() {}

		public virtual void PickupUpdate(float deltaTime) {}
	}
}
