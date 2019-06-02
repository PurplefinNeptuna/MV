using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MV {

	public class BaseEnemyBuff {
		public string name;
		public BaseEnemy host;
		public float timeleft;

		public Tuple<bool, bool> Tick(float deltaTime) {
			timeleft -= deltaTime;
			bool alive = (!MVUtility.Leq0(timeleft));
			return new Tuple<bool, bool>(BuffTick(deltaTime), alive);
		}

		public virtual bool BuffTick(float deltaTime) {
			return true;
		}

		public virtual void SetDefault() {

		}
	}
}
