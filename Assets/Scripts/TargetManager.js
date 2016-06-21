#pragma strict

import Greyman;

// NOTE - Must place OffScreenIndicator folder into 'Assets/Plugins/'
// Link OffscreenIndicator class with at least one indicator already created.

public var nestPrefab:GameObject;
public var fishPrefab:GameObject;
public var offScreenIndicator:OffScreenIndicator;

function Start () {

	if (!nestPrefab) {
		Debug.LogError('TargetGenerator.js: Set nestPrefab', nestPrefab);
	}

	if (!offScreenIndicator) {
		Debug.LogError('TargetGenerator.js: Set offScreenIndicator', offScreenIndicator);
	}

	//Temp- Add first target
	Invoke('AddTarget', 7);

}

function Update () {

	if (Input.GetKeyUp(KeyCode.N)){
		AddTarget('nest');
	} else if (Input.GetKeyUp(KeyCode.F)){
		AddTarget('fish');
	}

	if (offScreenIndicator) {
		//print(offScreenIndicator.indicators.length);
	}

}

public function AddTarget() {
	AddTarget('fish');
}

public function AddTarget(type:String) {

	var newObj:GameObject;
	var indicatorId = 0; // ID (index) of indicator to use from OffScreenIndicator's "Indicators" List.

	if (type == 'fish') {

		newObj = GameObject.Instantiate(fishPrefab);

		// add ArrowIndicator
		indicatorId = 1;
		offScreenIndicator.AddIndicator(newObj.transform, indicatorId);

		// position at one of the fish nodes
		var fishNodes :GameObject[] = GameObject.FindGameObjectsWithTag('FishNode');
		newObj.transform.localPosition = fishNodes[Random.Range(0,fishNodes.Length)].transform.position;

	} else {

		newObj = GameObject.Instantiate(nestPrefab);

		// add ArrowIndicator
		newObj.transform.localPosition = Vector3(Random.Range(-1000, 1000), 16.5, Random.Range(-1000, 1000));
		offScreenIndicator.AddIndicator(newObj.transform, indicatorId);

	}

}

public function AcquireTarget(objToRemove:GameObject) {

	print('AcquireTarget: ' + objToRemove.name);

	// Remove target indicator
	offScreenIndicator.RemoveIndicator(objToRemove.transform);

	if (objToRemove.name.Contains('Fish')) {

		// CAUGHT FISH

		// Remove Fish
		Destroy(objToRemove);

		// TODO: Show catch animation.
		// TODO: Attach fish to eagle model

		// Target nest.
		AddTarget('nest');

	} else if (objToRemove.name.Contains('Nest')) {

		// REACHED NEST

		// Remove target indicator
		offScreenIndicator.RemoveIndicator(objToRemove.transform);

		// Spawn fish to hunt
		AddTarget('fish');

	}


}