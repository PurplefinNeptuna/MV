using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MV {
	/// <summary>
	/// Class for spawning projectiles
	/// </summary>
	public class ProjectileManager {

		public static int projectileCount = 0;

		/// <summary>
		/// Spawn new projectile
		/// </summary>
		/// <param name="worldPos">position in 2D space</param>
		/// <param name="speed">speed</param>
		/// <param name="direction">direction</param>
		/// <param name="friendly">friendly to player?</param>
		/// <param name="source">source of projectile</param>
		/// <param name="damage">damage</param>
		/// <param name="projectileName">name of the projectile prefab</param>
		/// <param name="behaviourName">name of the projectile AI class</param>
		/// <param name="color">projectile color</param>
		/// <param name="sound">projectile sound when spawning</param>
		/// <returns>GameObject of spawned projectile</returns>
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
}
