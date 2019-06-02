using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		if (oneTimeOnly && WorldManager.main.GetMarker(RoomManager.main.activeRoomName, GameScript.main.GetLocalChunkPos(rb2d.position)) == defaultName) {
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
		if (!GameScript.main.haltGame)
			PickupUpdate(Time.deltaTime);
	}

	protected override void POFixedUpdate() {
		if (!GameScript.main.dead) {
			int count = rb2d.OverlapCollider(GameScript.main.playerContact, overlapPlayer);
			if (count > 0) {
				if (showOnMap && !showOnDestroyOnly) {
					Vector2Int chunkPos = GameScript.main.GetLocalChunkPos(rb2d.position);
					Color color = GetComponent<SpriteRenderer>().color;
					WorldManager.main.SetMarker(defaultName, RoomManager.main.activeRoomName, chunkPos, color);
					if (WorldManager.main.map != null){
						//UIManager.main.ForceRefreshMinimap();
					}
				}
				OnHitPlayer(GameScript.main.playerPlayer);
			}
		}
	}

	private void OnDestroy() {
		if (showOnMap && showOnDestroyOnly && forcedDestroy) {
			Vector2Int chunkPos = GameScript.main.GetLocalChunkPos(rb2d.position);
			Color color = GetComponent<SpriteRenderer>().color;
			WorldManager.main.SetMarker(defaultName, RoomManager.main.activeRoomName, chunkPos, color);
			if (WorldManager.main.map != null){
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
