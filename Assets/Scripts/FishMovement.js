﻿#pragma strict 
import System.Collections.Generic;

 var pauseTime : float = 1.0;
 var moveSpeed = 1.5;

private var targetPosition : Vector3;

private var eagleGO : GameObject;
private var ringProjector : AnimatedProjector;

private var points = new List.<Vector3>(); 

 function Start() {

	// Get movement coordinates from fishnodes
 	 var fishNodes :GameObject[] = GameObject.FindGameObjectsWithTag('FishNode');

 	 for (var go : GameObject in fishNodes)  { 
		var pos:Vector3 = go.transform.position;
		points.Add(pos); 
	}

	eagleGO = GameObject.Find(Constants.GO_EAGLE_NAME);
	ringProjector = gameObject.GetComponentInChildren(AnimatedProjector) as AnimatedProjector;

	// Start movement
     MoveAndPause();


 }

 function LateUpdate() {

	// Set indicator ring size based on distance from eagle
	var dist:float = Vector3.Distance(gameObject.transform.position, eagleGO.transform.position);

	if (dist > 260) {
		
		ringProjector.DefaultSize = 120;
		ringProjector.DefaultColor.a = 1.0f;
	} else {
		// Once closer than 200 units, start shrinking ring.
		ringProjector.DefaultSize = Utils.Map(dist, 40, 260, 5, 120);
		ringProjector.DefaultColor.a = Utils.Map(dist, 50.0, 250, 0.0, 1.0);
	}

 }
 
 function MoveAndPause() {

     while (true) {

     	// Decide next travel destination
         targetPosition = NextTargetPosition();

         // Face fish towards new target
         transform.LookAt(targetPosition);

         while (Vector3.Distance(transform.position, targetPosition) > moveSpeed * 2) {
             transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * moveSpeed);
             yield;
         }

     	yield WaitForSeconds(pauseTime);

     }

 }

 function NextTargetPosition():Vector3 {
	points.Sort(function(c1:Vector3, c2:Vector3){
		return Vector3.Distance(this.transform.position, c1).CompareTo((Vector3.Distance(this.transform.position, c2)));   
	});
		
 	// Choose one of the closest nodes. (the closest is the one it is on)
 	var rIndex:int = Random.Range(1, 3);
 	return points[rIndex];

 }
