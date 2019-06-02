using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class of nearly all physiscal objects
/// </summary>
public class BaseObject : MonoBehaviour {
	[HideInInspector]
	public float minGroundNormalY = .65f;
	[HideInInspector]
	public Vector2 currentNormal;
	[HideInInspector]
	public float gravityModifier = 1f;
	[HideInInspector]
	public bool affectedByGravity = true;
	[HideInInspector]
	public LayerMask mask;
	protected bool grounded;
	protected ContactFilter2D contactFilter;
	protected Rigidbody2D rb2d;
	protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
	protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);
	[HideInInspector]
	public Vector2 velocity = Vector2.zero;
	protected Vector2 groundNormal = Vector2.up;
	protected const float minMoveDist = 0.001f;
	protected const float shellRadius = 0.01f;
	protected float fixedDeltaTime;
	public bool localHaltPhysics = false;

	void OnEnable() {
		rb2d = GetComponent<Rigidbody2D>();
		POOnEnable();
	}

	void Start() {
		contactFilter.useTriggers = false;
		mask = Physics2D.GetLayerCollisionMask(gameObject.layer);
		contactFilter.SetLayerMask(mask);
		contactFilter.useLayerMask = true;
		POStart();
	}

	void FixedUpdate() {
		if (localHaltPhysics || GameScript.main.haltGame)
			return;

		fixedDeltaTime = Time.fixedDeltaTime;
		if (affectedByGravity)
			velocity += gravityModifier * Physics2D.gravity * fixedDeltaTime;

		Vector2 deltaPos = velocity * fixedDeltaTime;

		if (!grounded) {
			groundNormal = Vector2.up;
			currentNormal = Vector2.up;
		}
		Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);
		grounded = false;

		Vector2 move = moveAlongGround * deltaPos.x;

		Movement(move, false);
		move = Vector2.up * deltaPos.y;
		Movement(move, true);
		POFixedUpdate();
	}

	void Movement(Vector2 move, bool yMovement) {
		float distance = move.magnitude;

		if (distance > minMoveDist) {
			int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);

			hitBufferList.Clear();
			for (int i = 0; i < count; i++) {
				hitBufferList.Add(hitBuffer[i]);
			}

			for (int i = 0; i < hitBufferList.Count; i++) {
				int thisHitLayer = hitBufferList[i].collider.gameObject.layer;

				currentNormal = hitBufferList[i].normal;
				if (currentNormal.y > minGroundNormalY) {
					grounded = true;
					if (yMovement) {
						groundNormal = currentNormal;
					}
				}

				float projection = Vector2.Dot(velocity, currentNormal);
				if (projection < 0) {
					velocity = velocity - projection * currentNormal;
				}

				float modifiedDistance = hitBufferList[i].distance - shellRadius;
				distance = modifiedDistance < distance ? modifiedDistance : distance;
			}
		}
		rb2d.position = rb2d.position + move.normalized * distance;
	}

	/// <summary>
	/// Run in FixedUpdate
	/// </summary>
	protected virtual void POFixedUpdate() {}

	/// <summary>
	/// Run in Start
	/// </summary>
	protected virtual void POStart() {}

	/// <summary>
	/// Run in OnEnable
	/// </summary>
	protected virtual void POOnEnable() {}
}
