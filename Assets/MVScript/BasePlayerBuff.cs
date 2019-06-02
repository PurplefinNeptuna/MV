using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MV {
	public class BasePlayerBuff {
		public Player host;
		public float timeleft;
		public string name;

		/// <summary>
		/// Tick for Player, override BuffTick for buff effects
		/// </summary>
		/// <param name="deltaTime">time since last tick</param>
		/// <returns>continuity and buff alive status</returns>
		public Tuple<bool, bool> Tick(float deltaTime) {
			timeleft -= deltaTime;
			bool alive = (!MVUtility.Leq0(timeleft));
			return new Tuple<bool, bool>(BuffTick(deltaTime), alive);
		}

		/// <summary>
		/// Method for giving effect per ticks
		/// </summary>
		/// <param name="deltaTime">time since last tick</param>
		/// <returns>set false for freezing Player Input</returns>
		public virtual bool BuffTick(float deltaTime) {
			return true;
		}

		/// <summary>
		/// Set default properties for BasePlayerBuff
		/// </summary>
		public virtual void SetDefault() {

		}
	}
}
