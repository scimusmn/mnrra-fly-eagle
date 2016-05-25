#pragma strict

import Greyman;

// NOTE - Must place OffScreenIndicator folder into 'Assets/Plugins/'
// Link OffscreenIndicator class with at least one indicator already created.

public var targetObject:GameObject;
public var offScreenArrow:OffScreenIndicator;

function Start () {

	if (!targetObject) {
		Debug.LogError('TargetGenerator.js: Set targetObject', targetObject);
	}

	if (!offScreenArrow) {
		Debug.LogError('TargetGenerator.js: Set offScreenArrow', offScreenArrow);
	}

	//Temp- Add first target
	Invoke('AddTarget', 7);

}

function Update () {
	if (Input.GetKeyUp(KeyCode.Q)){
		AddTarget();
	}
}

public function AddTarget() {

	var newObj:GameObject = GameObject.Instantiate(targetObject);

	// random position
	newObj.transform.localPosition = Vector3(Random.Range(-1000, 1000), 65.0, Random.Range(-1000, 1000));

	// add ArrowIndicator
	var indicatorId = 0; // ID (index) of indicator to use from OffScreenIndicator's "Indicators" List.
	offScreenArrow.AddIndicator(newObj.transform, indicatorId);

}

public function RemoveTarget(objToRemove:GameObject) {

	print('RemoveTarget: ' + objToRemove.name);
	offScreenArrow.RemoveIndicator(objToRemove.transform);
	Destroy(objToRemove);

	// Spawn fresh target after 8 seconds
	Invoke('AddTarget', 7);

}