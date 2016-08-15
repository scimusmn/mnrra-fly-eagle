#pragma strict

function Start () {

	HideAll();
	     
}

function Show(alertId:String) {
	HideAll();
	var alertGO:GameObject = transform.Find(alertId).gameObject;
	alertGO.SetActive(true);

}

function Show(alertId:String, duration:float) {

	var alertGO:GameObject = transform.Find(alertId).gameObject;
	alertGO.SetActive(true);

	Invoke('HideAll', duration);

}

function Hide(alertId:String) {

	var alertGO:GameObject = transform.Find(alertId).gameObject;
	alertGO.SetActive(false);

}

function HideAll() {

	var children:int = transform.childCount;

	for (var i:int = 0; i < children; i++) {
		transform.GetChild(i).gameObject.SetActive(false);
	}

}