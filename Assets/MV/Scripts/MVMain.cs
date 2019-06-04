using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MV {

	[ExecOrder(-95)]
	[RequireComponent(typeof(GameScript))]
	[RequireComponent(typeof(WorldManager))]
	[RequireComponent(typeof(RoomManager))]
	public class MVMain : MonoBehaviour {
		public static GameScript Core;
		public static WorldManager World;
		public static RoomManager Room;
		public static MusicManager Music;
		public static SoundManager Sound;
		public static ProjectileManager Projectile;
		public static EnemyManager Enemy;
		public static DifficultyManager Difficulty;

		private static MVMain _main_;

		private void Awake() {
			if (_main_ == null) {
				_main_ = this;
			}
			else if (_main_ != this) {
				Destroy(gameObject);
			}

			Enemy = new EnemyManager();
			Difficulty = new DifficultyManager();
			Projectile = new ProjectileManager();
		}
	}
}
