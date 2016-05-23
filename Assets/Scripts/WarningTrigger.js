#pragma strict
import UnityEngine.UI;

public var warningMsg : String = 'Adjust course to stay near river.';

@Header ("Rotate forward direction towards safe zone (blue arrow)")

private var uiWarning : GameObject;
private var uiWarningText : Text;

private var isShowing = false;

function Awake () {

	uiWarning = GameObject.Find('UI-Warning');
	uiWarningText = uiWarning.GetComponentInChildren(Text);

}

function Start () {

	showWarning(false);

}

function Update () {

}

function OnTriggerEnter(other:Collider) {

	if (other.gameObject.name == Constants.GO_EAGLE_NAME) {
		
		showWarning(true);

	}

}

function OnTriggerStay(other:Collider) {

	// If other is facing in opposite direction of
	// trigger, assume they are leaving as instructed
	if (other.gameObject.name == Constants.GO_EAGLE_NAME) {

		// Dot represents the difference between facing directions
		// 1 = exact same direction, -1 = exact opposite direction
		var dot : float = Vector3.Dot(this.transform.forward, other.transform.forward);

		if (dot >= 0.5f && isShowing == true) {
			// Is facing away from out-of-bounds. Hide warning.
			showWarning(false);
		} 
		if (dot < 0.5f && isShowing == false) {
			// Is (again) facing towards out-of-bounds. Show warning.
			showWarning(true);
		}

	}

}

function OnTriggerExit(other:Collider) {

	if (other.gameObject.name == Constants.GO_EAGLE_NAME) {

		showWarning(false);

	}

}

function showWarning(doShow:boolean) {

	if (doShow == true) {
		uiWarningText.text = warningMsg;
		uiWarning.SetActive(true);
		isShowing = true;
	} else {
		uiWarning.SetActive(false);
		isShowing = false;
	}

}