using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using MV;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad()]
public class StartSettings : Editor {
	static StartSettings() {

	}

	[MenuItem("MV/Create MV Core")]
	public static void CreateMVCore() {
		AddCinemachineBrain();
		GameObject core = CreateCore();
		CreateVCam(core);
		CreateSoundManager(core);
		CreateMusicManager(core);
	}

	private static void CreateMusicManager(GameObject parent) {
		GameObject music = new GameObject("MV Music", typeof(AudioSource), typeof(MusicManager));
		AudioSource m = music.GetComponent<AudioSource>();
		m.playOnAwake = false;
		m.loop = true;
		m.bypassListenerEffects = true;
		m.bypassReverbZones = true;
		m.bypassEffects = true;
		music.transform.SetParent(parent.transform);
		music.transform.position = Vector3.zero;
	}

	private static void CreateSoundManager(GameObject parent) {
		GameObject sound = new GameObject("MV Sound", typeof(AudioSource), typeof(AudioSource), typeof(SoundManager));
		AudioSource[] s = sound.GetComponents<AudioSource>();
		foreach (var si in s) {
			si.playOnAwake = false;
			si.loop = false;
			si.bypassListenerEffects = true;
			si.bypassReverbZones = true;
			si.bypassEffects = true;
		}
		sound.transform.SetParent(parent.transform);
		sound.transform.position = Vector3.zero;
	}

	private static GameObject CreateCore() {
		GameObject core = new GameObject("MV Core", typeof(MVMain));
		core.transform.position = Vector3.zero;
		return core;
	}

	private static void AddCinemachineBrain() {
		CinemachineBrain found = UnityEngine.Component.FindObjectOfType<CinemachineBrain>();
		if (found == null) {
			Camera mainCamera = Camera.main;
			if (mainCamera != null) {
				mainCamera.gameObject.AddComponent<CinemachineBrain>();
			}
		}
	}

	private static void CreateVCam(GameObject parent) {
		GameObject vCamGO = new GameObject("MV VCam", typeof(CinemachineVirtualCamera));
		CinemachineVirtualCamera vCam = vCamGO.GetComponent<CinemachineVirtualCamera>();
		GameObject owner = vCam.GetComponentOwner().gameObject;
		owner.AddComponent(typeof(CinemachineFramingTransposer));
		vCam.InvalidateComponentPipeline();
		vCamGO.transform.SetParent(parent.transform);
		vCamGO.transform.position = Vector3.zero;
		parent.GetComponent<GameScript>().VCamPlayer = vCamGO;
	}
}
