using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

	#region static
	public static MusicManager main;

	private void Awake() {
		if (main == null) {
			main = this;
		}
		else if (main != this) {
			Destroy(gameObject);
		}

		DontDestroyOnLoad(gameObject);
	}

	public static void Play(string musicName) {
		main?.PlayMusic(musicName);
	}
	#endregion

	private AudioSource audioSource;

	private void Start() {
		audioSource = GetComponent<AudioSource>();
	}

	private void PlayMusic(string musicName) {
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
