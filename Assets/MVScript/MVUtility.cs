using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum Direction {
	Up,
	Right,
	Down,
	Left
}

/// <summary>
/// Contains many helper methods
/// </summary>
public static class MVUtility {
	/// <summary>
	/// Do complex multiplication
	/// </summary>
	public static Vector2 ComplexMult(this Vector2 aVec, Vector2 aOther) {
		return new Vector2(aVec.x * aOther.x - aVec.y * aOther.y, aVec.x * aOther.y + aVec.y * aOther.x);
	}

	/// <summary>
	/// Convert angle to Complex number
	/// </summary>
	/// <param name="aDegree">Angle in degree</param>
	/// <returns></returns>
	public static Vector2 Rotation(float aDegree) {
		float a = aDegree * Mathf.Deg2Rad;
		return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
	}

	/// <summary>
	/// Rotate aVec by aDegree counter clockwise
	/// </summary>
	public static Vector2 RotateCCW(this Vector2 aVec, float aDegree) {
		return ComplexMult(aVec, Rotation(aDegree));
	}

	/// <summary>
	/// Rotate aVec by aDegree clockwise
	/// </summary>
	public static Vector2 RotateCW(this Vector2 aVec, float aDegree) {
		return ComplexMult(aVec, Rotation(-aDegree));
	}

	/// <summary>
	/// Rotate aPoint by aDegree clockwise using Quaternion
	/// </summary>
	public static Vector2 RotateQuat(this Vector2 aPoint, float aDegree) {
		return Quaternion.Euler(0, 0, -aDegree) * aPoint;
	}

	/// <summary>
	/// Calculate the velocity needed for moving from startPos to endPos in time seconds
	/// </summary>
	public static Vector2 CalculateVelocity(Vector2 startPos, Vector2 endPos, float time) {
		return (endPos - startPos) / time;
	}

	/// <summary>
	/// Lerp color from A to B with T as progress (0-1)
	/// </summary>
	public static Color HueLerp(Color a, Color b, float t) {
		Vector3 Va, Vb, Vc;
		float Ah = 0, As = 0, Av = 0, Bh = 0, Bs = 0, Bv = 0;
		Color.RGBToHSV(a, out Ah, out As, out Av);
		Color.RGBToHSV(b, out Bh, out Bs, out Bv);
		Va = new Vector3(Ah, As, Av);
		Vb = new Vector3(Bh, Bs, Bv);
		Vc = Vector3.Lerp(Va, Vb, t);
		float alpha = Mathf.Lerp(a.a, b.a, t);
		Color result = Color.HSVToRGB(Vc.x, Vc.y, Vc.z);
		result.a = alpha;
		return result;
	}

	/// <summary>
	/// Check if A less equal than B 
	/// </summary>
	public static bool Leq(float a, float b) {
		if (Mathf.Approximately(a, b) || Mathf.Approximately(Mathf.Sign(a - b), -1f)) {
			return true;
		}
		return false;
	}

	/// <summary>
	/// Check if A greater equal than B 
	/// </summary>
	public static bool Geq(float a, float b) {
		if (Mathf.Approximately(a, b) || Mathf.Approximately(Mathf.Sign(a - b), 1f)) {
			return true;
		}
		return false;
	}

	/// <summary>
	/// Check if the float less equal than zero
	/// </summary>
	public static bool Leq0(float a) {
		if (Mathf.Approximately(a, 0f) || Mathf.Approximately(Mathf.Sign(a), -1f)) {
			return true;
		}
		return false;
	}

	/// <summary>
	/// Check if number between range (inclusive)
	/// </summary>
	public static bool BetweenInt(int val, int min, int max) {
		return (val >= min && val <= max);
	}

	/// <summary>
	/// Check if point between square (inclusive)
	/// </summary>
	public static bool BetweenVector2Int(Vector2Int val, Vector2Int min, Vector2Int max) {
		return BetweenInt(val.x, min.x, max.x) && BetweenInt(val.y, max.y, min.y);
	}

	/// <summary>
	/// Convert tile grid position to local chunk position (-y,x)
	/// </summary>
	/// <param name="tileX">tile x position</param>
	/// <param name="tileY">tile y position</param>
	/// <param name="topLeftChunk">position of top left chunk in room space</param>
	/// <param name="chunkSize">size of chunk</param>
	public static Vector2Int TilePosToLocalChunkPos(int tileX, int tileY, Vector2Int topLeftChunk, Vector2Int chunkSize) {
		int xPos = Mathf.CeilToInt(((float) tileX - chunkSize.x / 2 + 1) / (float) chunkSize.x);
		int yPos = Mathf.CeilToInt(((float) tileY - chunkSize.y / 2 + 1) / (float) chunkSize.y);
		xPos -= topLeftChunk.x;
		yPos -= topLeftChunk.y;
		return new Vector2Int(-yPos, xPos);
	}

	/// <summary>
	/// Convert Direction to vector direction in Chunk coordinate system
	/// </summary>
	public static Vector2Int DirectionToVector2Int(Direction direction) {
		switch (direction) {
		case Direction.Up:
			return Vector2Int.left;
		case Direction.Right:
			return Vector2Int.up;
		case Direction.Down:
			return Vector2Int.right;
		case Direction.Left:
			return Vector2Int.down;
		default:
			return Vector2Int.zero;
		}
	}

	/// <summary>
	/// Create new weapon
	/// </summary>
	/// <param name="name">weapon name</param>
	public static BaseWeapon CreateWeapon(string name, int level = 0) {
		Type t = Type.GetType(name, false);
		if (t == null)
			return null;
		BaseWeapon newWeapon = (BaseWeapon) Activator.CreateInstance(t);
		newWeapon.SetDefault();
		newWeapon.level = level;
		return newWeapon;
	}

	/// <summary>
	/// Rotate to direction (for top down)
	/// </summary>
	/// <param name="dir">direction of rotation</param>
	public static Quaternion TopDownRotationFromDirection(Vector2 dir) {
		return Quaternion.LookRotation(Vector3.forward, dir);
	}

	public static BaseEnemyBuff CreateEnemyBuff(string name, BaseEnemy host) {
		Type t = Type.GetType(name, false);
		if (t == null)
			return null;
		BaseEnemyBuff newEnemyBuff = (BaseEnemyBuff) Activator.CreateInstance(t);
		newEnemyBuff.host = host;
		newEnemyBuff.SetDefault();
		return newEnemyBuff;
	}

	public static BasePlayerBuff CreatePlayerBuff(string name, Player host) {
		Type t = Type.GetType(name, false);
		if (t == null)
			return null;
		BasePlayerBuff newPlayerBuff = (BasePlayerBuff) Activator.CreateInstance(t);
		newPlayerBuff.host = host;
		newPlayerBuff.SetDefault();
		return newPlayerBuff;
	}

}
