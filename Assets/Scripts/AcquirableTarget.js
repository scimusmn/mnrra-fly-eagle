#pragma strict

private var tg:TargetManager;

function Awake () {

	tg = FindObjectOfType(TargetManager);

}

function OnTriggerEnter(other:Collider) {
	
	if (other.gameObject.name == Constants.GO_EAGLE_NAME) {
		
		tg.AcquireTarget(this.gameObject);

	}

}