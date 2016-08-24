#pragma strict
import System.Collections.Generic;
import UnityEngine.SceneManagement;

@Header ("Must link to UI Image")
/*
	Usage: Attach to GameObject (Main Camera works well)
	Create a black (or another color) Image in a UI canvas.
	Drag Image to link in inspector.
	From outside script call EndScene() with name of
	next scene you'd like to fade into.
*/

public var FadeImg: Image;
public var fadeSpeed:float = 1.5f;
public var sceneStarting:boolean = true;


function Awake() {
    FadeImg.rectTransform.localScale = new Vector2(Screen.width, Screen.height);
}

function Update() {
    // If the scene is starting...
    if (sceneStarting) {
        StartScene();
    }
}

function FadeToClear() {
	// Lerp the colour of the image between itself and transparent.
    FadeImg.color = Color.Lerp(FadeImg.color, Color.clear, fadeSpeed * Time.deltaTime);
}


function FadeToBlack() {
    // Lerp the colour of the image between itself and black.
    FadeImg.color = Color.Lerp(FadeImg.color, Color.black, fadeSpeed * Time.deltaTime);
}


function StartScene() {

    // Fade the texture to clear.
    FadeToClear();

    // If the texture is almost clear...
    if (FadeImg.color.a <= 0.005f) {

        // ... set the colour to clear and disable the RawImage.
        FadeImg.color = Color.clear;
        FadeImg.enabled = false;

        // The scene is no longer starting.
        sceneStarting = false;

    }
}


public function EndScene(SceneName:String) {

	// TN - Attempt to unload resources
	Resources.UnloadUnusedAssets();
	print('Unloading Unused Assets...');

    // Make sure the RawImage is enabled.
    FadeImg.enabled = true;
    sceneStarting = false;
    StartCoroutine('EndSceneRoutine', SceneName);
        
}

public function EndSceneRoutine(SceneName: String):IEnumerator {

	do {

		FadeToBlack();

		// If the screen is almost black...
		if (FadeImg.color.a >= 0.95f) {
		    SceneManager.LoadScene(SceneName);
		    yield;
		} else {
		    yield;
		}
				    
    } while(true);

}