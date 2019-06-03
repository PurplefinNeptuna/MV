using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MV {
	/// <summary>
	/// Class for player
	/// </summary>
	public class Player : BaseObject {

		private float deltaTime;
		public bool godMode = false;
		public float maxSpeed = 8f;
		public float jumpTakeOffSpd = 13f;
		public float knockBackXSpeed = .5f;
		public float knockBackYSpeed = 4f;
		public float knockBackResistance = 1f;
		public float knockBackDirection = 1f;
		public float invincibleTimeMax = 1.5f;
		private float invincibleTime = 0f;
		public bool invincible = false;
		private bool blinking = false;
		public float blinkRateMax = .2f;
		private float blinkRate;
		private Color defaultColor = Color.white;
		public int maxHealth = 100;
		public int health;

		public bool jumpBack = false;
		private bool inJumpBack = false;

		private SpriteRenderer spriteRenderer;
		public bool SpriteFlipX {
			get {
				//return spriteRenderer.flipX;
				return transform.localScale.x == -1;
			}
		}

		public List<BaseWeapon> weapons;
		public int activeWeaponIndex = 0;

		public float weaponPower = 0f;
		public float weaponPowerMax = 100f;
		public float weaponRechargeSpeed = 125f;
		private float weaponRechargeDelay = .5f;
		private float weaponRechargeDelayMax = .5f;
		public bool weaponRechargeDelayStart = false;
		public bool weaponCanRecharge = true;

		public float weaponAttackMovingPenaltyMax = .15f;
		public float weaponAttackMovingPenalty;

		public BaseWeapon ActiveWeapon {
			get {
				return weapons[activeWeaponIndex];
			}
		}

		public Vector2 WeaponDirection {
			get {
				return SpriteFlipX ? Vector2.left : Vector2.right;
			}
		}

		private List<string> deadBuff;
		private Dictionary<string, BasePlayerBuff> buffs;
		public bool freezeInput;

		public Vector2 VelocityFromEffects;

		// public Animator animator;

		private void Awake() {
			health = maxHealth;
			spriteRenderer = GetComponent<SpriteRenderer>();
			// animator = GetComponent<Animator>();
			weapons = new List<BaseWeapon>();
			buffs = new Dictionary<string, BasePlayerBuff>();
			weaponAttackMovingPenalty = 0f;
		}

		private void Update() {
			if (MVMain.Core.haltGame) {
				// animator.speed = 0;
				return;
			}
			else {
				// animator.speed = 1;
			}

			VelocityFromEffects = Vector2.zero;

			deltaTime = Time.deltaTime;
			if (health <= 0) {
				health = 0;
				Dead();
			}

			freezeInput = false;
			if (weapons.Count > 0) {
				foreach (BaseWeapon wep in weapons) {
					wep.Update(deltaTime);
				}
			}

			#region buff
			deadBuff = new List<string>();
			foreach (var item in buffs) {
				var resBuff = item.Value.Tick(deltaTime);
				if (!resBuff.Item1) {
					freezeInput = true;
				}
				if (!resBuff.Item2) {
					deadBuff.Add(item.Key);
				}
			}

			foreach (var item in deadBuff) {
				buffs.Remove(item);
			}
			#endregion

			if (!MVUtility.Leq0(weaponAttackMovingPenalty)) {
				weaponAttackMovingPenalty -= deltaTime;
			}

			if (invincibleTime > 0f) {
				invincibleTime -= deltaTime;
			}
			else {
				invincibleTime = 0f;
				if (invincible) spriteRenderer.color = defaultColor;
				invincible = false;
				blinking = false;
			}

			if (velocity.y <= 0f) {
				inJumpBack = false;
				jumpBack = false;
			}
			else if (jumpBack) {
				inJumpBack = true;
			}

			PlayerController();

			activeWeaponIndex = (weapons.Count > 0) ? activeWeaponIndex % weapons.Count : 0;
			/*if (!GameUtility.Geq(weaponPower, (weapons.Count > 0) ? ActiveWeapon.WeaponUsage : 50f)) {
				weaponCanRecharge = true;
			}
			else*/
			if (Mathf.Approximately(weaponPower, weaponPowerMax)) {
				weaponCanRecharge = false;
			}

			if (!weaponCanRecharge && weaponRechargeDelayStart) {
				weaponRechargeDelay -= deltaTime;
				if (MVUtility.Leq0(weaponRechargeDelay)) {
					weaponRechargeDelay = weaponRechargeDelayMax;
					weaponRechargeDelayStart = false;
					weaponCanRecharge = true;
				}
			}
			else if (weaponCanRecharge) {
				weaponRechargeDelay = weaponRechargeDelayMax;
				weaponRechargeDelayStart = false;
				weaponPower += weaponRechargeSpeed * deltaTime;
			}
			weaponPower = Mathf.Clamp(weaponPower, 0f, weaponPowerMax);
			if (weapons.Count > 0) {
				PlayerShoot();
			}

			if (blinking) {
				if (blinkRate > blinkRateMax / 2f) {
					spriteRenderer.color = new Color(1, 1, 1, .5f);
				}
				else {
					spriteRenderer.color = defaultColor;
				}
				blinkRate -= deltaTime;
				if (blinkRate <= 0) {
					blinkRate = blinkRateMax;
				}
			}
			velocity += VelocityFromEffects;
		}

		/// <summary>
		/// Method for giving player knockback effects
		/// </summary>
		/// <param name="direction">Vector2 of forces</param>
		/// <param name="attackMultiplier">power of knockback</param>
		public void Knockback(Vector2 direction, float attackMultiplier = 1f) {
			knockBackDirection = Mathf.Sign(direction.x);
			jumpBack = true;
			velocity.y = knockBackYSpeed * knockBackResistance * attackMultiplier;
		}

		/// <summary>
		/// Method for giving player damage
		/// </summary>
		/// <param name="direction">direction of attack</param>
		/// <param name="damage">damage</param>
		/// <param name="ignoreInvincible">is the attack ignoring the invincible status?</param>
		/// <param name="knockbackMultiplier">knockback power</param>
		/// <param name="playSound">should it playing hurt sound?</param>
		/// <param name="drawNumber">should it draw damage number?(currently unused)</param>
		public void GetHit(Vector2 direction, int damage, bool ignoreInvincible = false, float knockbackMultiplier = 1f, bool playSound = true, bool drawNumber = false) {
			if (godMode)
				return;
			if (!invincible || ignoreInvincible) {
				if (playSound)
					MVMain.Sound.Play("hit1", 1.5f, 1);

				int damageF = Mathf.RoundToInt((float) damage * MVMain.Difficulty.difficulty);
				if (damage > 0)
					damageF = Mathf.Max(1, damageF);

				health -= damageF;
				if (health < 0)
					health = 0;

				if (!ignoreInvincible) {
					invincible = true;
					invincibleTime = invincibleTimeMax;
					blinking = true;
					blinkRate = blinkRateMax;
				}

				if (drawNumber) {
					//GameUtility.SpawnPopupText("-" + damageF.ToString(), rb2d.position + Random.insideUnitCircle * 0.25f, Color.red);
				}

				if (!MVUtility.Leq0(knockbackMultiplier))
					Knockback(direction, knockbackMultiplier);
			}
		}

		/// <summary>
		/// Method for healing player
		/// </summary>
		/// <param name="amount">amount of heal</param>
		public void GetHeal(int amount) {
			health = Mathf.Min(health + amount, maxHealth);
			//GameUtility.SpawnPopupText(amount.ToString(), rb2d.position + Random.insideUnitCircle * 0.25f, Color.green);
		}

		/// <summary>
		/// Method for giving player buff
		/// </summary>
		/// <param name="name">name of buff</param>
		public void GetBuff(string name) {
			if (!buffs.ContainsKey(name)) {
				BasePlayerBuff newBuff = MVUtility.CreatePlayerBuff(name, this);
				buffs.Add(newBuff.name, newBuff);
			}
		}

		/// <summary>
		/// Method for removing player after dead, and forcing game over
		/// </summary>
		public void Dead() {
			MVMain.Core.GameOver();
			Destroy(gameObject);
		}

		private void OnTriggerStay2D(Collider2D collision) {
			Vector3Int pos = MVMain.Core.grid.WorldToCell(rb2d.position);
			WorldTile teleporter = MVMain.Core.teleporter.FirstOrDefault(x => x.localPlace == pos);
			if (teleporter != null) {
				if (teleporter.name[0] == 'I') {
					MVMain.Core.Teleport(teleporter);
				}
			}
		}

		/// <summary>
		/// Set position of player
		/// </summary>
		/// <param name="pos">position in 3D space</param>
		public void SetPosition(Vector3 pos) {
			rb2d.position = pos;
		}

		/// <summary>
		/// Get player position
		/// </summary>
		/// <returns>position in 2D space</returns>
		public Vector2 GetPosition() {
			return rb2d.position;
		}

		/// <summary>
		/// Control of player
		/// </summary>
		private void PlayerController() {
			float xMove = 0f;
			if (!inJumpBack || grounded) {
				if (MVUtility.Leq0(weaponAttackMovingPenalty)) {
					xMove = freezeInput? 0 : Input.GetAxis("Horizontal");
				}
				else if (freezeInput) xMove = 0;
			}
			else
				xMove = knockBackXSpeed * knockBackResistance * knockBackDirection;

			if (Input.GetButtonDown("Jump") && grounded && !freezeInput) {
				MVMain.Sound.Play("jump1");
				// animator.SetBool("StartJumpBool", true);
				velocity.y = jumpTakeOffSpd + (godMode ? 5f : 0f);
			}
			else if (Input.GetButtonUp("Jump") || freezeInput) {
				// animator.SetBool("StartJumpBool", false);
				if (velocity.y > 0) {
					velocity.y *= .5f;
				}
			}
			else if (Input.GetButton("Jump")) {
				// animator.SetBool("StartJumpBool", false);
			}

			if (weapons.Count > 0 && !ActiveWeapon.fired && !freezeInput) {
				if (Input.GetButtonDown("Scroll+")) {
					activeWeaponIndex++;
					if (activeWeaponIndex >= weapons.Count)
						activeWeaponIndex -= weapons.Count;
				}
				else if (Input.GetButtonDown("Scroll-")) {
					activeWeaponIndex--;
					if (activeWeaponIndex < 0)
						activeWeaponIndex += weapons.Count;
				}
			}

			velocity.x = xMove * (maxSpeed + (godMode ? 5f : 0f));
			bool flipSprite = (SpriteFlipX ? (xMove > 0.01f) : (xMove < -0.01f));
			if (flipSprite && !inJumpBack)
				transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

			// animator.SetFloat("Speed", Mathf.Abs(velocity.x));
			// animator.SetFloat("VelY", velocity.y);
			// animator.SetBool("Grounded", grounded);
		}

		/// <summary>
		/// Control player weapon using behaviour
		/// </summary>
		private void PlayerShoot() {
			if (Input.GetButtonDown("Fire1") && !freezeInput && MVUtility.Geq(weaponPower, ActiveWeapon.WeaponUsage)) {
				weaponRechargeDelay = weaponRechargeDelayMax;
				weaponRechargeDelayStart = false;
				weaponCanRecharge = false;
				ActiveWeapon.StartUse();
				weaponAttackMovingPenalty = weaponAttackMovingPenaltyMax;
				// animator.SetTrigger("Attack");
				// animator.SetBool("HoldFire", true);
			}
			else if (Input.GetButtonUp("Fire1") || freezeInput) {
				weaponRechargeDelayStart = true;
				ActiveWeapon.EndUse();
				// animator.SetBool("HoldFire", false);
			}
			else if (Input.GetButton("Fire1") && !freezeInput) {
				if (ActiveWeapon.chargeUse && MVUtility.Geq(weaponPower, ActiveWeapon.WeaponUsage) && ActiveWeapon.fired) {
					weaponRechargeDelay = weaponRechargeDelayMax;
					weaponRechargeDelayStart = false;
					weaponCanRecharge = false;
					weaponAttackMovingPenalty = weaponAttackMovingPenaltyMax;
				}
				else
					weaponRechargeDelayStart = true;

				ActiveWeapon.HoldUse();
			}
		}

		/// <summary>
		/// Check if player has the given weapon
		/// </summary>
		/// <param name="name">name of the weapon</param>
		public bool HasWeapon(string name) {
			if (weapons.Count == 0)
				return false;

			if (weapons.FirstOrDefault(x => x.name == name) != null)
				return true;

			return false;
		}

		/// <summary>
		/// Get player's weapon data
		/// </summary>
		/// <param name="name">name of weapon</param>
		/// <returns>weapon data</returns>
		public BaseWeapon GetWeapon(string name) {
			if (!HasWeapon(name))
				return null;

			return weapons.FirstOrDefault(x => x.name == name);
		}

		/// <summary>
		/// Get currently active weapon's level
		/// </summary>
		public int GetWeaponLevel() {
			if (weapons.Count == 0)
				return -1;
			return ActiveWeapon.level;
		}

		/// <summary>
		/// Get the level of given weapon
		/// </summary>
		/// <param name="name">weapon name</param>
		public int GetWeaponLevel(string name) {
			if (!HasWeapon(name))
				return -1;
			return GetWeapon(name).level;
		}

		/// <summary>
		/// Set color of player
		/// </summary>
		public void SetColor(Color color) {
			spriteRenderer.color = color;
			defaultColor = color;
		}
	}
}
