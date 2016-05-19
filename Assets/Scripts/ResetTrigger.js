#pragma strict

import UnityEngine.SceneManagement;

private var targetName : String = 'Eagle';

private var useFade : boolean = true;

function OnTriggerEnter(other:Collider) {

	if (other.gameObject.name == targetName) {

		fullReset();

	}

}

function fullReset() {

//	if (useFade == true) {
//
//		var fader:SceneFader = GameObject.Find('Fader').GetComponent(SceneFader);
//		var fadeTime : float = fader.fadeTime;
//
//		// Start fade out.
//		fader.fadeIn = false;
//
//		// Wait for fade-out to complete
//		yield WaitForSeconds(fadeTime);
//
//	}
//
//	var currentScene = SceneManager.GetActiveScene();
//	SceneManager.LoadScene(currentScene.name);

	var sceneFader:SceneFader = Camera.main.GetComponent(SceneFader);

	var currentSceneName = SceneManager.GetActiveScene().name;
	sceneFader.EndScene(currentSceneName);

}