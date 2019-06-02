using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	#region Static

	private void Awake() {
		if (MVMain.Sound == null) {
			MVMain.Sound = this;
		}
		else if (MVMain.Sound != this) {
			Destroy(gameObject);
		}
	}
	#endregion

	private Queue<Tuple<string, float, int>> soundQueue;
	private HashSet<string> soundList;
	private AudioSource[] audioSource;
	private List<AudioClip> sounds;
	private Dictionary<string, AudioClip> soundDic;

	private void Start() {
		soundQueue = new Queue<Tuple<string, float, int>>();
		soundList = new HashSet<string>();
		audioSource = GetComponents<AudioSource>();
		soundDic = new Dictionary<string, AudioClip>();
		sounds = Resources.LoadAll<AudioClip>("Sounds").ToList();
		foreach (AudioClip clip in sounds) {
			soundDic.Add(clip.name, clip);
		}
	}

	public void Play(string soundName, float pitch = 1f, int audioID = 0) {
		bool soundExist = sounds.Any(x => x.name == soundName);
		if (!soundList.Contains(soundName) && soundExist) {
			soundList.Add(soundName);
			soundQueue.Enqueue(new Tuple<string, float, int>(soundName, pitch, audioID));
		}
	}

	private void LateUpdate() {
		while (soundQueue.Count > 0) {
			var sound = soundQueue.Dequeue();
			audioSource[sound.Item3].pitch = sound.Item2;
			audioSource[sound.Item3].PlayOneShot(soundDic[sound.Item1], .3f);
		}
		soundList.Clear();
	}
}
