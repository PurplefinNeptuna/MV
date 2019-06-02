using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MV {
	/// <summary>
	/// Base class for Projectiles, has lightweight pysics calculation and use circle bounds
	/// </summary>
	public class BaseProjectile : MonoBehaviour {
		public float size;
		public int damage;
		public float gravityModifier = 1f;
		public bool affectedByGravity = true;

		private bool _friendly;
		public LayerMask target;

		public bool localHalt;
		public bool localHaltPhysics;

		public bool Friendly {
			get {
				return _friendly;
			}
			set {
				_friendly = value;
				if (value) {
					target = MVMain.Core.enemyLayer;
					gameObject.layer = LayerMask.NameToLayer("Projectile");
				}
				else {
					target = MVMain.Core.playerCoreLayer;
					gameObject.layer = LayerMask.NameToLayer("EnemyProjectile");
				}
			}
		}

		public bool sourceXFlip = false;
		public Vector2 spawnerPos;
		private GameObject _source;
		public GameObject Source {
			get {
				return _source;
			}
			set {
				_source = value;
				sourceXFlip = value.GetComponent<SpriteRenderer>().flipX || (value.transform.localScale.x == -1);
				spawnerPos = value.transform.position;
			}
		}
		public bool stayAlive = false;
		public float lifeTime;

		protected SpriteRenderer spriteRenderer;

		public Vector2 velocity;
		public Vector2 defaultDirection;
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

		public float defaultSpeed;
		private Collider2D[] colliderHitBuffer = new Collider2D[16];

		public float Speed {
			get {
				return velocity.magnitude;
			}
			set {
				if (velocity.normalized == Vector2.zero)
					velocity = value * Vector2.up;
				else
					velocity = value * velocity.normalized;
			}
		}

		public RaycastHit2D? groundHitRaycast;

		protected float deltaTime;

		private void Awake() {
			spriteRenderer = GetComponent<SpriteRenderer>();
			groundHitRaycast = null;
			SetDefault();
		}

		private void Update() {
			if (MVMain.Core.haltGame)
				return;

			deltaTime = Time.deltaTime;
			if (!stayAlive) {
				lifeTime -= Time.deltaTime;
				if (MVUtility.Leq0(lifeTime))
					Destroy(gameObject);
			}
			if (_source == null) {
				Destroy(gameObject);
			}
			if (!localHalt)
				AI();
		}

		private void FixedUpdate() {
			if (localHalt || localHaltPhysics || MVMain.Core.haltGame)
				return;

			float fixedDT = Time.fixedDeltaTime;
			if (affectedByGravity)
				velocity += gravityModifier * Physics2D.gravity * fixedDT;

			Vector2 deltaPos = velocity * fixedDT;

			transform.position += (Vector3) deltaPos;

			int count = Physics2D.OverlapCircleNonAlloc(transform.position, size, colliderHitBuffer, target);
			for (int i = 0; i < count; i++) {
				if (Friendly) {
					OnHitEnemy(colliderHitBuffer[i].gameObject);
				}
				else {
					OnHitPlayer(MVMain.Core.playerPlayer);
				}
			}

			RaycastHit2D ground2 = Physics2D.CircleCast(transform.position, size, velocity.normalized, velocity.magnitude * fixedDT, MVMain.Core.groundLayer);
			if (ground2.collider != null) {
				groundHitRaycast = ground2;
				OnHitGround(ground2.normal);
			}
		}

		/// <summary>
		/// Set default properties here
		/// </summary>
		public virtual void SetDefault() {

		}

		/// <summary>
		/// Run AI logic once each Update tick
		/// </summary>
		public virtual void AI() {

		}

		/// <summary>
		/// Run when projectile hit Enemy
		/// </summary>
		/// <param name="target">Target</param>
		public virtual void OnHitEnemy(GameObject target) {}

		/// <summary>
		/// Run when projectile hit player
		/// </summary>
		/// <param name="player">The player</param>
		public virtual void OnHitPlayer(Player player) {}

		/// <summary>
		/// Run when projectile hit ground tiles
		/// </summary>
		/// <param name="normal">Normal vector of collision</param>
		public virtual void OnHitGround(Vector2 normal) {}
	}
}
