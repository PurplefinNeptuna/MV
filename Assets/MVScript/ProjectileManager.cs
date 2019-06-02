using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager {

	public static int projectileCount = 0;

	public GameObject Spawn(Vector2 worldPos, float speed, Vector2 direction, bool friendly, GameObject source, int damage = 5, string projectileName = "Bullet", string behaviourName = "DefaultBullet", Color? color = null, string sound = null) {
		Type type;
		Color targetColor;
		GameObject prefab = Resources.Load<GameObject>("Prefabs/Projectiles/" + projectileName);
		try {
			type = Type.GetType(behaviourName);
		}
		catch {
			//type = typeof(DefaultBullet);
			return null;
		}

		if (color == null) {
			if (friendly)
				targetColor = Color.white;
			else
				targetColor = Color.red;
		}
		else {
			targetColor = (Color) color;
		}

		direction.Normalize();
		projectileCount++;
		GameObject result = GameObject.Instantiate<GameObject>(prefab, worldPos, Quaternion.identity, MVMain.Core.room.transform);
		result.GetComponent<SpriteRenderer>().color = targetColor;
		result.AddComponent(type);
		result.transform.rotation = MVUtility.TopDownRotationFromDirection(direction);
		BaseProjectile projectile = result.GetComponent(type) as BaseProjectile;
		projectile.Source = source;
		projectile.Friendly = friendly;
		projectile.defaultSpeed = speed;
		projectile.defaultDirection = direction;
		projectile.Speed = speed;
		projectile.Direction = direction;
		projectile.damage = damage;

		if (sound != null)
			MVMain.Sound.Play(sound);

		return result;
	}
}
