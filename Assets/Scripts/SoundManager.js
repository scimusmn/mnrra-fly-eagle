#pragma strict
import System.Collections.Generic;

var sounds : AudioClip[];

private var audioSource : AudioSource;
private var loopAudioSource : AudioSource;

function Awake () {

	audioSource = gameObject.AddComponent (AudioSource);
	loopAudioSource = gameObject.AddComponent (AudioSource);

}

function Start () {

	if (!audioSource) {
		Debug.LogError('Did not find AudioSource for SoundManager');
	}

	if (!loopAudioSource) {
		Debug.LogError('Did not find loopAudioSource for SoundManager');
	}

	// Settings for sound effect source
	audioSource.loop = false;
	audioSource.spatialBlend = 0.0;

	// Settings for looping sounds source
	loopAudioSource.loop = true;
	loopAudioSource.spatialBlend = 0.0;

}

function play(id:int):void {

	audioSource.GetComponent.<AudioSource>().PlayOneShot(sounds[id]);

}

function startLoop(id:int):void{

	if (!sounds[id]){
		print('sound ' + id + 'wasnt loaded');
	 	return;
	}
	loopAudioSource.clip = sounds[id];
	loopAudioSource.volume = 0.25;
	loopAudioSource.Play();

}

function setLoopVolume(vol:float) :void{

	loopAudioSource.volume = vol;

}

