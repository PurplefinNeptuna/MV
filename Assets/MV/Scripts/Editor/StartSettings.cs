using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad()]
public class StartSettings : Editor {
	static StartSettings() {

	}

	[MenuItem("MV/Create MV Core")]
	public static void CreateMVCore() {
		AddCinemachineBrain();
		CreateVCam();
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

	private static void CreateVCam() {
		GameObject vCamGO = new GameObject("PlayerVCam", typeof(CinemachineVirtualCamera), typeof(CinemachineFramingTransposer));
		CinemachineVirtualCamera vCam = vCamGO.GetComponent<CinemachineVirtualCamera>();
		GameObject owner = vCam.GetComponentOwner().gameObject;
		owner.AddComponent(typeof(CinemachineFramingTransposer));
		vCam.InvalidateComponentPipeline();
	}
}
