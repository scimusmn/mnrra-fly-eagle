#pragma strict

public var target : Transform;
public var followDistance = 10.0;
public var boostFollowDistance = 0.0;
public var followHeight = 5.0;
public var boostFollowHeight = 0.0;
public var followHeightDamping = 2.0;
public var rotationDamping = 1.0;

public var doLookAt = false;
public var doFollow = false;

function Start () {

	if (!target) {
		Debug.LogWarning("Please assign target to follow.");
	}

	// Make the rigid body not change rotation
	if (GetComponent.<Rigidbody>()){
		GetComponent.<Rigidbody>().freezeRotation = true;
	}

}


function LateUpdate () {
	if (doFollow == true) {
		// Current offsets based off target's current position/angle.
		var goalRotationAngle = target.eulerAngles.y;
		var goalfollowHeight = target.position.y + followHeight + boostFollowHeight;
			
		var currentRotationAngle = transform.eulerAngles.y;
		var currentfollowHeight = transform.position.y;

	    // This incorporates a jitter fix found here:
	    // http://forum.unity3d.com/threads/how-to-smooth-damp-towards-a-moving-target-without-causing-jitter-in-the-movement.130920/
		var betterDampTime = (1 - Mathf.Exp( -20 * Time.deltaTime ));
		
		// Damp the rotation around the y-axis
		currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, goalRotationAngle, rotationDamping * betterDampTime);

		// Damp the followHeight
		currentfollowHeight = Mathf.Lerp (currentfollowHeight, goalfollowHeight, followHeightDamping * betterDampTime);

		// Convert the angle into a rotation
		var currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);
		
		// Set the position of the camera on the x-z plane to:
		// followDistance meters behind the target
		transform.position = target.position;
		transform.position -= currentRotation * Vector3.forward * (followDistance + boostFollowDistance);

		// Set the followHeight of the camera
		transform.position.y = currentfollowHeight;
	}

	// Rotate camera to center target in view
	if (doLookAt == true) {
		transform.LookAt (target);
	}

}

public function TransitionFollowDistance(to:float, time:float) {

	var elapsedTime : float = 0;
	var vectorDiff = target.position - transform.position;
	vectorDiff.y = 0;
	var startingDist:float = vectorDiff.magnitude;

	while (elapsedTime < time) {
		var perc : float = (elapsedTime / time);
		followDistance = Mathf.SmoothStep(startingDist, to, perc);
		elapsedTime += Time.deltaTime;
		yield;
	}

}
