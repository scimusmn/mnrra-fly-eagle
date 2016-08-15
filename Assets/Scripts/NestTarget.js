#pragma strict

private var eagleGO : GameObject;
private var homeIndicator : Indicator;

function Start () {
	eagleGO = GameObject.Find(Constants.GO_EAGLE_NAME);

	var mgr : GameObject = GameObject.Find(Constants.GO_MANAGER_NAME);
	var ind:OffScreenIndicator = mgr.GetComponent(OffScreenIndicator);

	homeIndicator = ind.indicators[0];

}

function Update () {

}

function LateUpdate() {

	// Set indicator arrow height based on distance from eagle
	var dist:float = Vector3.Distance(gameObject.transform.position, eagleGO.transform.position);
	homeIndicator.targetOffset.y = (dist/20) + 5;

 }