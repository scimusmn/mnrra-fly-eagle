#pragma strict
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

private var forcePullUp : float = 0.0;
var allTerrains:Terrain[];

private var soundManager:SoundManager;
private var flightSaver:FlightSaver;

function Awake () {

	flightSaver = GameObject.Find('ManagerPF').GetComponent('FlightSaver');

}

function Start () {

	rb = this.gameObject.GetComponent(Rigidbody);
	birdAni = this.gameObject.GetComponent(BirdAnimation);
	followCam = Camera.main.GetComponent(CameraFollow);

	leftWing = transform.Find("LeftWingParent");
	rightWing = transform.Find("RightWingParent");

	allTerrains = Terrain.activeTerrains;

	soundManager = FindObjectOfType(SoundManager);
	soundManager.startLoop(1, 1); // Wind
	soundManager.startLoop(2, 2); // Water

}

function Update () {

	// Forward movement
	if (Input.GetKey(KeyCode.W)){ // Warp
    	transform.Translate( 0, 0, warpSpeed);
    } else if (Input.GetKey(KeyCode.S)) { // Slow
    	transform.Translate( 0, 0, baseSpeed/25);
    	speedBoost = 0.0;
    } else { // Normal
    	transform.Translate( 0, 0, baseSpeed);
    }

    // Keep on top of water.
    if (forcePullUp != 0.0 && wingSpanPitch <= 0.0) {
    	transform.position.y = forcePullUp;
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
	followCam.boostFollowHeight = Mathf.Clamp(-Utils.Map(wingSpanPitch, -1, 1, -0.77, 0.77), -0.7, 0.7);

	// Boost adjust when pointed downwards or upwards
	if (wingSpanPitch < -0.25) {
		var diveBoost: float = Utils.Map(wingSpanPitch, -0.25, -1.0, 0.002, 0.018);
		speedBoost += diveBoost;
	} else if (wingSpanPitch > 0.2) {
		var pullBoost: float = Utils.Map(wingSpanPitch, 0.2, 1.0, 0.005, 0.015);
		speedBoost -= pullBoost;
	}

	// Reduce boost on sharp turns
	var absSpanAngle : float = Mathf.Abs(wingSpanAngle);
	if (absSpanAngle > 0.35) {
		var turnBoost: float = Utils.Map(absSpanAngle, 0.35, 1.0, 0.001, 0.01);
		speedBoost -= turnBoost;
	}

	// If flap boosting, apply here.
	if (speedBoost > 0.0 || dampSpeedBoost > 0.001) {
		
        // Smooth speed increase
	    dampSpeedBoost = Mathf.Lerp(dampSpeedBoost, speedBoost, 0.1);

	    // Smooth altitude increase (decays faster)
	    dampAltitudeBoost = Mathf.Lerp(dampAltitudeBoost, altitudeBoost, 0.6);

	    // Apply to transform
	    transform.Translate( 0, dampAltitudeBoost, dampSpeedBoost);

        // Update camera to fall back when boosting.
	    followCam.boostFollowDistance = dampSpeedBoost * 0.75;

	}

	// Reduce the boost speed over time.
	// Clamp if necessary
    speedBoost -= 0.0055;
    if (speedBoost < 0.0) {
        speedBoost = 0.0;
    } else if (speedBoost > 2.25) {
        // Ceiling
        speedBoost = 2.25;
    }

    // Reduce the boost altitude over time. (decays faster)
    // Clamp if necessary
    altitudeBoost -= 0.02;
    if (altitudeBoost < 0.0) {
        altitudeBoost = 0.0;
    } else if (altitudeBoost > 1.25) {
        // Ceiling
        altitudeBoost = 1.25;
    }

	// Update wind and water volumes based on altitude
	var altitudeVol: float = Utils.Map(transform.position.y, 0, 700, 0.0, 1); // Wind volume
	soundManager.setLoopVolume(1, altitudeVol);
	var waterVol: float = Utils.Map(transform.position.y, 0, 100, 1.0, 0.0); // River volume
	soundManager.setLoopVolume(2, waterVol);

}

function LateUpdate () {

	keepAboveTerrain();

}

public function Flap() {

	// Make first flap most powerful
	if (speedBoost == 0.0) {
		speedBoost += 0.5f;
		altitudeBoost += 0.1f;
	} else {
		speedBoost += 0.25f;
		altitudeBoost += 0.08f;
	}

	// Play flap sound effect
	soundManager.play(0, Random.Range(0.15, 0.3));

}

/*
 *
 * Update current inputs from external controllers.
 * Inputs should be normalized from -1 to 1.
 *
 */
public function UpdateInputs(wingsRoll : float, wingsYaw : float, wingsPitch : float) {

	// Record inputs for later playback
	if (flightSaver.recordFlight == true){
		flightSaver.recordInputData(wingsRoll, wingsYaw, wingsPitch);
	}

	wingSpanAngle = wingsRoll;
	wingSpanYaw = wingsYaw;
	wingSpanPitch = wingsPitch;

	// Map angle to object rotation
	targetRollRotation = Quaternion.Euler (0, 0, Utils.Map(wingSpanAngle, -1, 1, -maxRotation, maxRotation));

}

public function UpdateFlapState(wingLeftAngle:float, wingRightAngle:float) {

	// Record inputs for playback
	if (flightSaver.recordFlight == true){
		flightSaver.recordFlapData(wingLeftAngle, wingRightAngle);
	}

    // Average both wing angles (birds never flap one wing)
    var avgAngle = (wingLeftAngle + (-1 * wingRightAngle)) / 2;

    // Clamp wing angle
    avgAngle = Mathf.Clamp(avgAngle, -55.0, 41.0);

    if(avgAngle < -5.0){
    	// shift wing up
    	leftWing.transform.localPosition.y = 1.96 + Utils.Map(avgAngle, -55, -5.0, 0.15, 0.0);
    	rightWing.transform.localPosition.y = 1.96 + Utils.Map(avgAngle, -55, -5.0, 0.15, 0.0);
    	// shift wing in
    	leftWing.transform.localPosition.x = -0.5 + Utils.Map(avgAngle, -55, -5.0, 0.3, 0.0);
    	rightWing.transform.localPosition.x = 0.5 - Utils.Map(avgAngle, -55, -5.0, 0.3, 0.0);
    }else{
    	leftWing.transform.localPosition.y = 1.96;
    	rightWing.transform.localPosition.y = 1.96;
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
        //print('-> FLAP! /  ' + flapSum);
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

// This flys the eagle around,
// occassionally dipping. 
// Only meant for a screensaver mode.
public function aiUpdate() {

	var inputState:Vector3 = flightSaver.getNextInputData();
	var flapState:Vector2 = flightSaver.getNextFlapData();

	this.UpdateInputs( inputState.x, inputState.y, inputState.z );
    this.UpdateFlapState( flapState.x, flapState.y );

}

public function ForcePullUp(toAltitude : float) {

    forcePullUp = toAltitude;

}

public function StopPullUp() {

    forcePullUp = 0.0;

}

private function keepAboveTerrain():boolean{

	var eaglePos : Vector2 = Vector2(transform.position.x, transform.position.z);

	var terrain:Terrain;
	var tPos:Vector3;
	var tSize:Vector3;
	var tRect:Rect;

	// Find terrain under eagle
	for(var i:int = 0; i < allTerrains.length; i++) {

		terrain = allTerrains[i];
		tPos = terrain.GetPosition();
		tSize = terrain.terrainData.size;
        tRect = Rect (tPos.x, tPos.z, tSize.x, tSize.z);

        if (tRect.Contains(eaglePos) == true) {

	        // If below terrain, snap to terrain height
			var curTerrainHeight : float = terrain.SampleHeight(transform.position);
			if (transform.position.y < curTerrainHeight) {
				transform.position.y = curTerrainHeight;
			}

			// Exit
			return true;

        }
    }

    // No terrain was found.
    return false;

}