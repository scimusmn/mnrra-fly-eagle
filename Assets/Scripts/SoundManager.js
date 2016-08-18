#pragma strict
import System.Collections.Generic;

var sounds : AudioClip[];

private var audioSource : AudioSource;
private var loop1AudioSource : AudioSource;
private var loop2AudioSource : AudioSource;

function Awake () {

	audioSource = gameObject.AddComponent (AudioSource);
	loop1AudioSource = gameObject.AddComponent (AudioSource);
	loop2AudioSource = gameObject.AddComponent (AudioSource);

}

function Start () {

	if (!audioSource) {
		Debug.LogError('Did not find AudioSource for SoundManager');
	}

	if (!loop1AudioSource) {
		Debug.LogError('Did not find loop1AudioSource for SoundManager');
	}

	// Settings for sound effect source
	audioSource.loop = false;
	audioSource.spatialBlend = 0.0;

	// Settings for looping sounds source
	loop1AudioSource.loop = true;
	loop1AudioSource.spatialBlend = 0.0;
	loop2AudioSource.loop = true;
	loop2AudioSource.spatialBlend = 0.0;

}

function play(id:int):void {
	
	play(id, 1.0);

}

function play(id:int, vol:float):void {
	
	audioSource.volume = vol;
	audioSource.GetComponent.<AudioSource>().PlayOneShot(sounds[id]);

}

function startLoop(channel:int, id:int):void{

	if (!sounds[id]){
		print('sound ' + id + 'wasnt loaded');
	 	return;
	}

	var audioSrc:AudioSource;

	if (channel == 1) {
		audioSrc = loop1AudioSource;
	} else if ( channel == 2) {
		audioSrc = loop2AudioSource;
	} else {
		print('Invalid channel. Must use 1 or 2.');
	}


	audioSrc.clip = sounds[id];
	audioSrc.volume = 0.25;
	audioSrc.Play();

}

function setLoopVolume(channel:int, vol:float) :void{

	var audioSrc:AudioSource;

	if (channel == 1) {
		audioSrc = loop1AudioSource;
	} else if ( channel == 2) {
		audioSrc = loop2AudioSource;
	} else {
		print('Invalid channel. Must use 1 or 2.');
	}

	audioSrc.volume = vol;

}
