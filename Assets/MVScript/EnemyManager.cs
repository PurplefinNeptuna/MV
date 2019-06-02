using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager {
	public static GameObject Spawn(Vector2 worldPos, string enemyName = "Spike", string behaviourName = "Spike", string layer = "Enemy") {
		Type type;
		GameObject prefab = Resources.Load<GameObject>("Prefabs/EnemyShapes/" + enemyName);
		try {
			type = Type.GetType(behaviourName);
		}
		catch {
			//type = typeof(Spike);
			return null;
		}
		GameObject result = GameObject.Instantiate<GameObject>(prefab, worldPos, Quaternion.identity, GameScript.main.room.transform);
		result.AddComponent(type);
		result.layer = LayerMask.NameToLayer(layer);
		return result;
	}
}
