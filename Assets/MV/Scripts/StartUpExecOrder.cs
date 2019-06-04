using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MV {
	public class ExecOrder : Attribute {
		public int order;
		public ExecOrder(int order) {
			this.order = order;
		}
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	public class StartUpExecOrder : Editor {
		static StartUpExecOrder() {
			foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts()) {
				if (monoScript.GetClass() != null) {
					foreach (var attr in Attribute.GetCustomAttributes(monoScript.GetClass(), typeof(ExecOrder))) {
						var newOrder = ((ExecOrder) attr).order;
						if (MonoImporter.GetExecutionOrder(monoScript) != newOrder)
							MonoImporter.SetExecutionOrder(monoScript, newOrder);
					}
				}
			}
		}
	}
#endif
}
