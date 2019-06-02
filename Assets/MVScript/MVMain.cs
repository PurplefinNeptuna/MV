using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameScript))]
[RequireComponent(typeof(WorldManager))]
[RequireComponent(typeof(RoomManager))]
[RequireComponent(typeof(MusicManager))]
[RequireComponent(typeof(SoundManager))]
public class MVMain : MonoBehaviour {
	public static GameScript Core;
	public static WorldManager World;
	public static RoomManager Room;
	public static MusicManager Music;
	public static SoundManager Sound;
	public static ProjectileManager Projectile;
	public static EnemyManager Enemy;
	public static DifficultyManager Difficulty;

	private void Awake() {
		Enemy = new EnemyManager();
		Difficulty = new DifficultyManager();
		Projectile = new ProjectileManager();
	}
}
