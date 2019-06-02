using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	#region Static
	public static SoundManager main;

	private void Awake() {
		if (main == null) {
			main = this;
		}
		else if (main != this) {
			Destroy(gameObject);
		}
	}

	public static void Play(string soundName, float pitch = 1f, int audioSourceID = 0) {
		main?.PlaySound(soundName, pitch, audioSourceID);
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

	private void PlaySound(string soundName, float pitch, int audioID) {
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
