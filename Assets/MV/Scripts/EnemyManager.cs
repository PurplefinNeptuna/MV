using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MV {
	/// <summary>
	/// Class for scripted enemy spawn
	/// </summary>
	public class EnemyManager {
		/// <summary>
		/// Spawn new enemy
		/// </summary>
		/// <param name="worldPos">position in 2D space</param>
		/// <param name="enemyName">name of enemy's prefab</param>
		/// <param name="behaviourName">class name for enemy's AI</param>
		/// <param name="layer">layer for collision detection</param>
		/// <returns>GameObject of spawned enemy</returns>
		public GameObject Spawn(Vector2 worldPos, string enemyName = "Spike", string behaviourName = "Spike", string layer = "Enemy") {
			Type type;
			GameObject prefab = Resources.Load<GameObject>(MVMain.settings.GetEnemyPath() + enemyName);
			try {
				type = Type.GetType(behaviourName);
			}
			catch {
				//type = typeof(Spike);
				return null;
			}
			GameObject result = GameObject.Instantiate<GameObject>(prefab, worldPos, Quaternion.identity, MVMain.Core.room.transform);
			result.AddComponent(type);
			result.layer = LayerMask.NameToLayer(layer);
			return result;
		}
	}
}
