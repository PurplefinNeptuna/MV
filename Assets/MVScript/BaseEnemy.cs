using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for enemies
/// </summary>
public class BaseEnemy : BaseObject {
	[HideInInspector]
	public Player targetPlayer;
	[HideInInspector]
	public string defaultName;
	[HideInInspector]
	public float defaultSpeed;
	[HideInInspector]
	public float attackPower;
	public int health;
	[HideInInspector]
	public int maxHealth;
	[HideInInspector]
	public bool immortal;
	[HideInInspector]
	public int damage;
	[HideInInspector]
	public bool noFlip = false;
	[HideInInspector]
	public bool dropHeart;
	[HideInInspector]
	public bool noContactDamage;
	[HideInInspector]
	public int score;
	[HideInInspector]
	public bool boss;
	[HideInInspector]
	public Dictionary<string, BaseEnemyBuff> buffs;
	private List<string> deadBuff;
	private bool freezeAI;

	protected string deathSound = "die1";
	protected bool useCustomHit;
	protected bool useCustomDeath;
	protected float deltaTime;
	protected bool playerInSight;

	public float Speed {
		get {
			return velocity.magnitude;
		}
		set {
			defaultSpeed = value;
			if (velocity.normalized == Vector2.zero)
				velocity = value * Vector2.up;
			else
				velocity = value * velocity.normalized;
		}
	}

	public Vector2 Direction {
		get {
			return velocity.normalized;
		}
		set {
			if (velocity.magnitude == 0f)
				velocity = 1 * value;
			else
				velocity = velocity.magnitude * value;
		}
	}

	public bool SpriteFlipX {
		get {
			//return spriteRenderer.flipX;
			return transform.localScale.x == -1;
		}
	}

	protected SpriteRenderer spriteRenderer;
	private Collider2D[] overlapPlayer = new Collider2D[4];

	private void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		buffs = new Dictionary<string, BaseEnemyBuff>();
		if (targetPlayer == null) {
			targetPlayer = GameScript.main.playerPlayer;
		}
		SetDefault();
	}

	protected override void POStart() {
		if (boss && WorldManager.main.GetMarker(RoomManager.main.activeRoomName, GameScript.main.GetLocalChunkPos(rb2d.position)) == defaultName) {
			Destroy(gameObject);
		}
	}

	protected override void POOnEnable() {
		if (boss) {
			GameScript.main.bossBattle = true;
			GameScript.main.bossEnemy = this;
		}
		maxHealth = health;

		if (defaultName == null || defaultName == "") {
			defaultName = gameObject.name;
		}
		else {
			gameObject.name = defaultName;
		}
	}

	protected override void POFixedUpdate() {
		if (!GameScript.main.dead)
			FixedAI();
		if (!GameScript.main.dead && !noContactDamage) {
			int count = rb2d.OverlapCollider(GameScript.main.playerContact, overlapPlayer);
			if (count > 0) {
				OnHitPlayer(targetPlayer);
				Vector2 dir = targetPlayer.GetPosition() - rb2d.position;
				targetPlayer.GetHit(dir, damage, false, attackPower);
			}
		}
	}

	private void Update() {
		if (health <= 0) {
			if (useCustomDeath)
				CustomDeath();
			else
				Destroy(gameObject);
		}

		if (!GameScript.main.haltGame) {
			#region buff
			deadBuff = new List<string>();
			freezeAI = false;
			foreach (var item in buffs) {
				var resBuff = item.Value.Tick(deltaTime);
				if (!resBuff.Item1) {
					freezeAI = true;
				}
				if (!resBuff.Item2) {
					deadBuff.Add(item.Key);
				}
			}

			foreach (var item in deadBuff) {
				buffs.Remove(item);
			}

			if (freezeAI != localHaltPhysics)
				localHaltPhysics = freezeAI;
			#endregion
		}

		if (targetPlayer == null && GameScript.main.player != null) {
			targetPlayer = GameScript.main.playerPlayer;
		}
		else if (targetPlayer == null) {
			Destroy(gameObject);
		}

		deltaTime = Time.deltaTime;
		if (!GameScript.main.dead) {
			Vector2 playerDir = targetPlayer.GetPosition() - rb2d.position;
			RaycastHit2D groundHit = Physics2D.Raycast(rb2d.position, playerDir.normalized, playerDir.magnitude, GameScript.main.groundLayer);
			playerInSight = (groundHit.collider == null);
			if (!freezeAI && !GameScript.main.haltGame) AI();
		}

		bool flipSprite = (SpriteFlipX ? (velocity.x > 0.01f) : (velocity.x < -0.01f));
		if (flipSprite && !noFlip)
			transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
	}

	public void GetHit(int damage, Player source) {
		if (!immortal || source.godMode) {
			int newDamage = Mathf.RoundToInt(Random.Range(.8f, 1.2f) * (float) damage);
			if (useCustomHit)
				CustomGetHit(newDamage, source);
			else
				DefaultGetHit(newDamage);
		}
		else {
			SoundManager.Play("deny1", 1.5f, 1);
		}
	}

	public void GetBuff(string name) {
		if (!buffs.ContainsKey(name)) {
			BaseEnemyBuff newBuff = GameUtility.CreateEnemyBuff(name, this);
			buffs.Add(newBuff.name, newBuff);
		}
	}

	protected void DefaultGetHit(int damage) {
		health -= damage;
	}

	/// <summary>
	/// Put enemy AI logic here, run once each update ticks
	/// </summary>
	public virtual void AI() {}

	/// <summary>
	/// Used for AI logic which needed to run every physics frame
	/// </summary>
	public virtual void FixedAI() {}

	/// <summary>
	/// Set some default properties here
	/// </summary>
	public virtual void SetDefault() {}

	/// <summary>
	/// Run logic when enemy's body touch player's
	/// </summary>
	/// <param name="player">The player</param>
	public virtual void OnHitPlayer(Player player) {}

	/// <summary>
	/// Do custom event when get hit by player, need set useCustomHit = true
	/// </summary>
	/// <param name="damage">damage</param>
	/// <param name="source">source of damage</param>
	public virtual void CustomGetHit(int damage, Player source) {}

	/// <summary>
	/// Do custom event when died, need set useCustomDeath = true
	/// Need to destroy GameObject manually with this event
	/// </summary>
	public virtual void CustomDeath() {}

	private void OnDestroy() {
		if (health <= 0) {
			SoundManager.Play(deathSound);
			if (dropHeart) {
				float chance = Random.Range(0f, 1f);
				if (chance * 3 <= 1f || boss) {
					Instantiate<GameObject>(GameScript.main.healthDrop, rb2d.position, Quaternion.identity, GameScript.main.room.transform);
				}
			}
			GameScript.main.score += score;
			if (boss) {
				Vector2Int chunkPos = GameScript.main.GetLocalChunkPos(rb2d.position);
				Color color = GetComponent<SpriteRenderer>().color;
				WorldManager.main.SetMarker(defaultName, RoomManager.main.activeRoomName, chunkPos, color);
				//UIManager.main.ForceRefreshMinimap();
			}
		}
		if (boss) {
			GameScript.main.bossBattle = false;
			GameScript.main.bossEnemy = null;
		}
	}
}
