using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MV {

	[ExecOrder(-95)]
	[ExecuteAlways]
	[RequireComponent(typeof(GameScript))]
	[RequireComponent(typeof(WorldManager))]
	[RequireComponent(typeof(RoomManager))]
	public class MVMain : MonoBehaviour {
		public static GameScript Core {
			get {
				return _main_.GetComponentInChildren<GameScript>();
			}
		}
		public static WorldManager World {
			get {
				return _main_.GetComponentInChildren<WorldManager>();
			}
		}
		public static RoomManager Room {
			get {
				return _main_.GetComponentInChildren<RoomManager>();
			}
		}
		public static MusicManager Music {
			get {
				return _main_.GetComponentInChildren<MusicManager>();
			}
		}
		public static SoundManager Sound {
			get {
				return _main_.GetComponentInChildren<SoundManager>();
			}
		}
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
