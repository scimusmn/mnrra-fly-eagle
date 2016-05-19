#pragma strict

import UnityEngine.SceneManagement;

private var sceneFader:SceneFader;

function Awake() {
	sceneFader = FindObjectOfType(SceneFader);
}

function OnTriggerEnter(other:Collider) {

	if (other.gameObject.name == Constants.GO_EAGLE_NAME) {

		fullReset();

	}

}

function fullReset() {

	var currentSceneName = SceneManager.GetActiveScene().name;
	sceneFader.EndScene(currentSceneName);

}