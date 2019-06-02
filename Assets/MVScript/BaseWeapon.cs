using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MV {
	/// <summary>
	/// Serialized weapon class for save/load purposes
	/// </summary>
	[Serializable]
	public class SerializedWeapon {
		public string name;
		public int level;
		public SerializedWeapon(BaseWeapon bwep) {
			name = bwep.name;
			level = bwep.level;
		}
	}

	/// <summary>
	/// Base class wor weaponry
	/// </summary>
	public class BaseWeapon {
		public string name;
		public bool fired;
		public float distance;
		public int level;
		public bool chargeUse;

		public Player Player {
			get {
				return MVMain.Core.playerPlayer;
			}
		}

		public virtual int Damage {
			get;
			set;
		}

		public virtual float WeaponUsage {
			get;
			set;
		}

		/// <summary>
		/// when player start press attack button
		/// </summary>
		public virtual void StartUse() {}

		/// <summary>
		/// when player hold attack button
		/// </summary>
		public virtual void HoldUse() {}

		/// <summary>
		/// when player release attack button
		/// </summary>
		public virtual void EndUse() {}

		/// <summary>
		/// set default parameter when created
		/// </summary>
		public virtual void SetDefault() {}

		/// <summary>
		/// update weapon state;
		/// </summary>
		/// <param name="deltaTime">Time since last update</param>
		public virtual void Update(float deltaTime) {

		}
	}
}
