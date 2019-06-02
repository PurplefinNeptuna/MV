using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyManager : MonoBehaviour {

	public static float difficulty = 1f;
	public Text diffText;
	private bool canChange = true;

	void Update() {
		if (diffText != null) {
			if (Input.GetButtonDown("Vertical") || !GameUtility.Leq0(Mathf.Abs(Input.GetAxisRaw("Vertical")))) {
				float diff = .1f * Mathf.Sign(Input.GetAxis("Vertical"));
				if (canChange)
					difficulty = Mathf.Clamp(difficulty + diff, .1f, 2f);
				canChange = false;
			}
			else {
				canChange = true;
			}
			diffText.text = "difficulty: " + Mathf.RoundToInt(difficulty * 100) + " percent";
		}
	}
}
