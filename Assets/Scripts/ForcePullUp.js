#pragma strict

private var myAltitude : float;

function Start() {

	myAltitude = transform.position.y + (transform.localScale.y / 2);

}

function OnTriggerEnter(other:Collider) {
	
	if (other.gameObject.name == Constants.GO_EAGLE_NAME) {
		
		other.gameObject.GetComponent(BirdFlight).ForcePullUp(myAltitude);

	}

}

function OnTriggerStay(other: Collider) {

	if (other.gameObject.name == Constants.GO_EAGLE_NAME) {

		other.gameObject.GetComponent(BirdFlight).ForcePullUp(myAltitude);

	}

}

function OnTriggerExit(other: Collider) {

	if (other.gameObject.name == Constants.GO_EAGLE_NAME) {

		other.gameObject.GetComponent(BirdFlight).StopPullUp();
		
	}

}
