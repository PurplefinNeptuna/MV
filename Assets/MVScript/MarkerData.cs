using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MV {
	[CreateAssetMenu(fileName = "NewMarker", menuName = "MVFramework/Create New Marker Data")]
	public class MarkerData : ScriptableObject {
		public Sprite sprite;
	}
}
