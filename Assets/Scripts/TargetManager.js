#pragma strict

import Greyman;

// NOTE - Must place OffScreenIndicator folder into 'Assets/Plugins/'
// Link OffscreenIndicator class with at least one indicator already created.

public var targetObject:GameObject;
public var offScreenIndicator:OffScreenIndicator;

function Start () {

	if (!targetObject) {
		Debug.LogError('TargetGenerator.js: Set targetObject', targetObject);
	}

	if (!offScreenIndicator) {
		Debug.LogError('TargetGenerator.js: Set offScreenIndicator', offScreenIndicator);
	}

	//Temp- Add first target
	Invoke('AddTarget', 7);

}

function Update () {
	if (Input.GetKeyUp(KeyCode.Q)){
		AddTarget();
	}

	if (offScreenIndicator) {
		print(offScreenIndicator.indicators.length);
	}

}

public function AddTarget() {

	var newObj:GameObject = GameObject.Instantiate(targetObject);

	// random position
	newObj.transform.localPosition = Vector3(Random.Range(-1000, 1000), 16.5, Random.Range(-1000, 1000));

	// add ArrowIndicator
	var indicatorId = 0; // ID (index) of indicator to use from OffScreenIndicator's "Indicators" List.
	offScreenIndicator.AddIndicator(newObj.transform, indicatorId);

}

public function RemoveTarget(objToRemove:GameObject) {

	print('RemoveTarget: ' + objToRemove.name);
	offScreenIndicator.RemoveIndicator(objToRemove.transform);
	Destroy(objToRemove);

	// Spawn fresh target after 8 seconds
	Invoke('AddTarget', 7);

}