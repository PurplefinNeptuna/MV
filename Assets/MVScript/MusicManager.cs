using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MV {
	/// <summary>
	/// Class for playing background music
	/// </summary>
	public class MusicManager : MonoBehaviour {

		#region static

		private void Awake() {
			if (MVMain.Music == null) {
				MVMain.Music = this;
			}
			else if (MVMain.Music != this) {
				Destroy(gameObject);
			}
		}
		#endregion

		private AudioSource audioSource;

		private void Start() {
			audioSource = GetComponent<AudioSource>();
		}

		/// <summary>
		/// Play the music
		/// </summary>
		/// <param name="musicName">Name in MusicData</param>
		public void Play(string musicName) {
			if (musicName != audioSource.clip?.name) {
				MusicData music = Resources.Load<MusicData>("MusicData/" + musicName);
				if (music == null)
					return;

				AudioClip clip = music?.music;
				audioSource.Stop();
				audioSource.volume = music.volume;
				audioSource.clip = clip;
				audioSource.Play();
			}
		}
	}
}
