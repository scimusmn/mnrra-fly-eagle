#pragma strict

var anim : Animator;
var toSoarHash : int;
var flapHash : int;
var flapCountHash : int;
var soarStateHash : int;
var flapStateHash : int;

var camScript : CameraFollow;
var birdFlight : BirdFlight;

function Start () {

	anim = GetComponent("Animator");

	toSoarHash = Animator.StringToHash("toSoarTrigger");
	flapCountHash = Animator.StringToHash('flapCount');

	soarStateHash = Animator.StringToHash("BirdLayer.aSoar");
	flapStateHash = Animator.StringToHash("BirdLayer.aFlap");

	camScript = Camera.main.GetComponent(CameraFollow);
	birdFlight = this.gameObject.GetComponent(BirdFlight);

	Invoke("CameraTransitionToSoar", 0.1f);

}

function Update () {

	// ANIMATION
	//var stateInfo : AnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);

//	if(Input.GetKeyDown(KeyCode.Space) && stateInfo.nameHash == soarStateHash) {
//		anim.SetTrigger(flapHash);
//	} 
//
//	if(Input.GetKeyUp(KeyCode.Space) && stateInfo.nameHash == flapStateHash) {
//		anim.SetTrigger(soarHash);
//	}

}

function CameraTransitionToSoar() {
	
	camScript.doLookAt = true;
	camScript.doFollow = true;
	camScript.TransitionFollowDistance(2.2f, 4.0f);

}
