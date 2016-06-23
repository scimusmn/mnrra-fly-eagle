﻿#pragma strict
import System.Collections.Generic;

public var wingSpanAngle : float;
public var wingSpanYaw : float;
public var wingSpanPitch : float;

public var maxRotation : float = 70.0;
public var targetRollRotation : Quaternion;
private var rotationSmooth : float = 5.0;

private var rb : Rigidbody;

public var baseSpeed : float = 1.0;
public var warpSpeed : float = 30.0;
public var rotateSpeed : float = 1.5;
public var pitchMax : float = 5.0;
public var rollMax : float = 60.0;
public var yawMax : float = 60.0;

private var speedBoost : float = 0.0;
private var dampSpeedBoost : float = 0.0;
private var altitudeBoost : float = 0.0;
private var dampAltitudeBoost : float = 0.0;

private var birdAni : BirdAnimation;
private var followCam : CameraFollow;

private var leftWing : Transform;
private var rightWing : Transform;
private var flapHistory : List.<float> = new List.<float>();
private var prevFlapState : float;
private var hasFlapped : boolean = false;

function Start () {

	rb = this.gameObject.GetComponent(Rigidbody);
	birdAni = this.gameObject.GetComponent(BirdAnimation);
	followCam = Camera.main.GetComponent(CameraFollow);

	leftWing = transform.Find("LeftWingParent");
	rightWing = transform.Find("RightWingParent");

}

function Update () {

	// Forward movement
	if (Input.GetKey(KeyCode.W)){
      transform.Translate( 0, 0, warpSpeed);
    } else {
    	transform.Translate( 0, 0, baseSpeed);
    }

    var h = wingSpanAngle;
    var v = wingSpanPitch;
    var y = wingSpanYaw;

	// Do rotation
	transform.Rotate(0, h * rotateSpeed, 0);

	// Up/Down (Pitch) control
	transform.localEulerAngles.x = -v * pitchMax;

	// Left/Right (Roll) control
	transform.localEulerAngles.z = -h * rollMax;

    // Left/Right (Yaw) control
    //transform.localEulerAngles.y = y * yawMax;

    // Update camera height based on Pitch.
	followCam.boostFollowHeight = Mathf.Clamp(-Utils.Map(wingSpanPitch, -1, 1, -0.45, 0.45), -0.3, 0.3);

	// If flap boosting, apply here.
	if (speedBoost > 0.0 || dampSpeedBoost > 0.001) {

        // Smooth speed increase
	    dampSpeedBoost = Mathf.Lerp(dampSpeedBoost, speedBoost, 0.075);

	    // Smooth altitude increase (decays faster)
	    dampAltitudeBoost = Mathf.Lerp(dampAltitudeBoost, altitudeBoost, 0.6);

	    // Apply to transform
	    transform.Translate( 0, dampAltitudeBoost, dampSpeedBoost);

        // Update camera to fall back when boosting.
	    followCam.boostFollowDistance = dampSpeedBoost * 2.6;

	    // Camera lowers slightly when boosting
	    followCam.boostFollowHeight = dampSpeedBoost * 1.5;

	    //TEMP - raise camtarget when fast
	    transform.Find("CamTarget").transform.localPosition.y = Utils.Map(dampSpeedBoost, 0, 2, 3.75, 15.0);

        // Reduce the boost speed over time.
	    speedBoost -= 0.001;
	    if (speedBoost < 0.0) {
	        speedBoost = 0.0;
	    } else if (speedBoost > 1.0) {
	        // Ceiling
	        speedBoost = 1.0;
	    }

	    // Reduce the boost altitude over time. (decays faster)
	    altitudeBoost -= 0.02;
	    if (altitudeBoost < 0.0) {
	        altitudeBoost = 0.0;
	    } else if (altitudeBoost > 1.5) {
	        // Ceiling
	        altitudeBoost = 1.5;
	    }

	}

}

public function Flap() {

	// Make first flap most powerful
	if (speedBoost == 0.0) {
		speedBoost += 0.2f;
		altitudeBoost += 0.05f;
	} else {
		speedBoost += 0.15f;
		altitudeBoost += 0.2f;
		altitudeBoost *= 1.2f; // Altitude momentum
	}

}

/*
 *
 * Update current inputs from external controllers.
 * Inputs should be normalized from -1 to 1.
 *
 */
public function UpdateInputs(wingsRoll : float, wingsYaw : float, wingsPitch : float) {

  wingSpanAngle = wingsRoll;
  wingSpanYaw = wingsYaw;
  wingSpanPitch = wingsPitch;

  // Map angle to object rotation
  targetRollRotation = Quaternion.Euler (0, 0, Utils.Map(wingSpanAngle, -1, 1, -maxRotation, maxRotation));

}

public function UpdateFlapState(wingLeftAngle:float, wingRightAngle:float) {

    // Average both wing angles (birds never flap one wing)
    var avgAngle = (wingLeftAngle + (-1 * wingRightAngle)) / 2;

    // Clamp wing angle
    avgAngle = Mathf.Clamp(avgAngle, -55.0, 41.0);

    if(avgAngle < -5.0){
    	// shift wing up
    	leftWing.transform.localPosition.y = 2.0 + Utils.Map(avgAngle, -55, -5.0, 0.15, 0.0);
    	rightWing.transform.localPosition.y = 2.0 + Utils.Map(avgAngle, -55, -5.0, 0.15, 0.0);
    	// shift wing in
    	leftWing.transform.localPosition.x = -0.5 + Utils.Map(avgAngle, -55, -5.0, 0.25, 0.0);
    	rightWing.transform.localPosition.x = 0.5 - Utils.Map(avgAngle, -55, -5.0, 0.25, 0.0);
    }else{
    	leftWing.transform.localPosition.y = 2.0;
    	rightWing.transform.localPosition.y = 2.0;
    	leftWing.transform.localPosition.x = -0.5;
    	rightWing.transform.localPosition.x = 0.5;
    }

    leftWing.transform.localEulerAngles.z = avgAngle;
    rightWing.transform.localEulerAngles.z = avgAngle * -1;

    // Flap detection
    // If we detect 5 frames of fast downward movement
    // trigger a "flap boost" and don't allow another
    // until upward movement completes.
    if (updateFlapDetection(avgAngle) == true) {
        Flap();
    }

}

private function updateFlapDetection(newFlapState:float) {
    
    // Difference between current and previous state
    var stateDifference : float = 0.0;
    if (flapHistory.Count > 0) {
        stateDifference = newFlapState - prevFlapState;
        if (Mathf.Abs(stateDifference) < 0.2) {
            // Zero out small changes
            stateDifference = 0.0;
        }
    }

    // Note - Negative stateDifference = upward motion
    // Note - Positive stateDifference = downward motion

    // Remember current flap state
    prevFlapState = newFlapState;

    // Add new state to history
    flapHistory.Add(stateDifference);

    // Remove oldest flap state
    while (flapHistory.Count > 5) {
        flapHistory.RemoveAt(0);
    }

    // Get total sum of flap history
    var flapSum :float = 0.0;
    for(var i:int = 0; i < flapHistory.Count; i++) {
        flapSum += flapHistory[i];
    }

    // Any positive sum (upward motion) unlocks flap
    if (flapSum < 0.0) {
        hasFlapped = false;
    }

    // If history has downward sum over threshold,
    // and hasn't already triggered flap.
    if (hasFlapped == false && flapSum > 10.0) {
        // A flap has taken place.
        print('-> FLAP! /  ' + flapSum);
        hasFlapped = true;
        return true;
    } else {
        return false;
    }

}

// Called when no user is found
// or kinect data is not useful.
// Assume user isn't attempting
// to input real data. (e.g., walking away)
public function noInputUpdate() {

    // When not recieiving input, 
    // lerp values back to a zeroed state.
    var curInputs : Vector3 = Vector3(wingSpanAngle, wingSpanYaw, wingSpanPitch);
    var lerpedInputs = Vector3.Lerp(curInputs, Vector3.zero, 0.01);
    
    var lerpedFlap : float = Mathf.Lerp(prevFlapState, 0.0, 0.1);

    this.UpdateInputs( lerpedInputs.x, lerpedInputs.y, lerpedInputs.z );
    this.UpdateFlapState( lerpedFlap, -lerpedFlap );

}