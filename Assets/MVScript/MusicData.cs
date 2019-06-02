using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMusic", menuName = "MVFramework/Create New Music Data")]
public class MusicData : ScriptableObject {
	public AudioClip music;
	[Range(0f, 1f)]
	public float volume;
}
