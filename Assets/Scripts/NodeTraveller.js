﻿#pragma strict
import System.Collections.Generic;

var nodeTag:String = '';
var nodePause : float = 0.0;
var loopPause : float = 0.0;
var moveSpeed : float = 1.5;
private var smoothRotation : boolean = false; // TODO - not ready yet

var movementStyle:MovementStyle;
enum MovementStyle { localRandom, globalRandom, fullLoop, respawnLoop, pingPong }

private var nodeIndex:int = -1;
private var targetPosition : Vector3;
private var nextTargetPosition : Vector3;
private var points = new List.<Vector3>();
private var direction :int = 1;

function Start () {

	// Get movement coordinates from fishnodes
	if (!nodeTag || nodeTag == '') {
		Debug.LogError('NodeTraveller.js: Set nodeTag to find nodes');
	}

	// Find nodes
	var nodes :GameObject[] = GameObject.FindGameObjectsWithTag(nodeTag);

	// Sort nodes alphanumerically
	// To control order of movement, name node objects accordingly (e.g. node_1, node_2, node_3,...)
	nodes.Sort(nodes, function(g1,g2) String.Compare(g1.name, g2.name));

	// For Debug - print node order
//	for(var o in nodes){
//		Debug.Log(o.name);
//	}

	for (var go : GameObject in nodes) {
		var pos:Vector3 = go.transform.position;
		points.Add(pos);
	}

	// Start movement
	MoveAndPause();

}

function Update () {

	// TODO - needs to smooth between current direction and direction needed for next turn
	if (smoothRotation == true) {
		// Smooth towards current movement direction
		transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation( targetPosition - transform.position ), Time.deltaTime );
	}

}

function MoveAndPause() {

	while (true) {

		// Decide next travel destination
		targetPosition = NextTargetPosition();

		if (movementStyle == MovementStyle.respawnLoop && nodeIndex == 1) {
			yield WaitForSeconds(loopPause);
		}
//		if (movementStyle == MovementStyle.pingPong && nodeIndex == 1){
//			yield WaitForSeconds(loopPause);
//		}

		// Face towards new target
		// TODO - smooth out rotation so there isn't a "snap" after each node

		if (smoothRotation == false) {
			transform.LookAt(targetPosition);
		}
		 
		while (transform.position != targetPosition) {
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * moveSpeed);
			yield;
		}

		yield WaitForSeconds(nodePause);

	}

 }

function NextTargetPosition():Vector3 {

	if (movementStyle == MovementStyle.localRandom) {

		// LOCAL RANDOM
		// Choose randomly between two closest nodes (excluding current node)
		points.Sort(function(c1:Vector3, c2:Vector3){
			return Vector3.Distance(this.transform.position, c1).CompareTo((Vector3.Distance(this.transform.position, c2)));   
		});

		nodeIndex = Random.Range(1, 3);

	} else if (movementStyle == MovementStyle.globalRandom) {

		// GLOBAL RANDOM
		// Choose randomly between ALL nodes (excluding current node)
		points.Sort(function(c1:Vector3, c2:Vector3){
			return Vector3.Distance(this.transform.position, c1).CompareTo((Vector3.Distance(this.transform.position, c2)));   
		});

		nodeIndex = Random.Range(1, points.Count);

	} else if (movementStyle == MovementStyle.fullLoop) {

		// FULL LOOP
		// Go to next target in line.
		nodeIndex++;

		// If on last, travel back to first
		if (nodeIndex >= points.Count){
			nodeIndex = 0;
		}

	} else if (movementStyle == MovementStyle.respawnLoop) {

		// RESPAWN LOOP
		// Go to next target in line.
		nodeIndex++;

		// If on last, respawn at first
		if (nodeIndex >= points.Count){

			transform.position = points[0];
			nodeIndex = 1;

		}

	} else if (movementStyle == MovementStyle.pingPong) {

		// PING PONG
		// Go to next target in line (depending on direction).
		nodeIndex += direction;

		// If on last or first, turn around
		if (nodeIndex == points.Count || nodeIndex == -1){

			direction *= -1;
			nodeIndex += (direction * 2);

		}	

	}

	return points[nodeIndex];

}
