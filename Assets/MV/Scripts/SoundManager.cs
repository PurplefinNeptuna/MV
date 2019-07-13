using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MV {
	/// <summary>
	/// Class for playing sounds
	/// </summary>
	[ExecOrder(-80)]
	public class SoundManager : MonoBehaviour {

		#region Static

		private void Awake() {
			if (MVMain.Sound != this) {
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
			sounds = Resources.LoadAll<AudioClip>(MVMain.settings.soundPath).ToList();
			foreach (AudioClip clip in sounds) {
				soundDic.Add(clip.name, clip);
			}
		}

		/// <summary>
		/// Play new sound
		/// </summary>
		/// <param name="soundName">name of sound file</param>
		/// <param name="pitch">pitch</param>
		/// <param name="audioID">channel (currently only 2 channel exist)</param>
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
}
