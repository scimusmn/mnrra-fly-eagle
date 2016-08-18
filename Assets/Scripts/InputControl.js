﻿#pragma strict

import UnityEngine.SceneManagement;

public var targetObj : GameObject;
private var birdFlight : BirdFlight;
public var kinectInput: boolean = false;

// Collect indices of the joints we're interested in...
private var iHandLeft:int = parseInt(KinectInterop.JointType.HandLeft);
private var iHandRight:int = parseInt(KinectInterop.JointType.HandRight);
private var iWristLeft:int = parseInt(KinectInterop.JointType.WristLeft);
private var iWristRight:int = parseInt(KinectInterop.JointType.WristRight);
private var iElbowLeft:int = parseInt(KinectInterop.JointType.ElbowLeft);
private var iElbowRight:int = parseInt(KinectInterop.JointType.ElbowRight);
private var iShoulderLeft:int = parseInt(KinectInterop.JointType.ShoulderLeft);
private var iShoulderRight:int = parseInt(KinectInterop.JointType.ShoulderRight);
private var iShoulderCenter:int = parseInt(KinectInterop.JointType.SpineShoulder);
private var iSpine:int = parseInt(KinectInterop.JointType.SpineMid);
private var iHead:int = parseInt(KinectInterop.JointType.Head);
private var iHipCenter:int = parseInt(KinectInterop.JointType.SpineBase);

// Variables for storing positions of joints
private var posWristLeft: Vector3;
private var posWristRight: Vector3;
private var posShoulderLeft: Vector3;
private var posShoulderRight: Vector3;
private var posHead: Vector3;
private var posHipCenter: Vector3;

private var sceneFader:SceneFader;
private var screensaverMode:boolean = false;
private var tPoseCount:int = 0;
private var tPoseThreshold:int = 300; // (60fps*5) frames t-pose must be held to abort screensaver

// Mouse control variables
private var mouseScrollWingAngle: float = 0.0;

function Start () {

    birdFlight = targetObj.GetComponent(BirdFlight);

    // Set defaults
    birdFlight.UpdateInputs(0.0, 0.0, 0.0);

    // Once scene starts, check if user is active.
    // If not, assume there is no one using, 
    // begin "screensaver" AI flight.
    CheckForScreensaverMode();

}

function Update () {

    if(kinectInput == false || Input.GetMouseButton(0)) {

        // Get normalized mouse position between -1f and 1f.
        var mouseRatioX : float = Utils.Map(Input.mousePosition.x, 0, Screen.width, -1, 1);
        var mouseRatioY : float = Utils.Map(Input.mousePosition.y, 0, Screen.height, -1, 1);

        birdFlight.UpdateInputs( mouseRatioX, 0.0, mouseRatioY );
  	
        mouseScrollWingAngle += Input.GetAxis("Mouse ScrollWheel");
        var mouseWingAngle : float = Utils.Map(mouseScrollWingAngle, -1.0, 1.0, 50, -50);

        birdFlight.UpdateFlapState(mouseWingAngle, -mouseWingAngle);

    } else {

        kinectUpdate();

    }

}

function kinectUpdate() {

    var manager = KinectManager.Instance;

    if(manager && manager.IsInitialized()){
        // Get 1st player
        var userId = manager.GetPrimaryUserID();

        if (!userId || userId <= 0) {
            // No players available...
            // TODO: Reset values. After X seconds,
            // assume user as exited and reset game.
            birdFlight.noInputUpdate();
            return;
        }

        manager.DetectGesture(userId, KinectGestures.Gestures.Tpose);

    } else {

		if (screensaverMode == true){
			birdFlight.aiUpdate();
		} else {
			birdFlight.noInputUpdate();
		}
        return;

    }

    // Are both wrists being tracked?
    if (manager.IsJointTracked(userId, iWristLeft) && manager.IsJointTracked(userId, iWristRight)) {

        // Calculate all the useful angles

        // Get positions of any joints we're interested in.
        posWristLeft = manager.GetJointPosition(userId, iWristLeft);
        posWristRight = manager.GetJointPosition(userId, iWristRight);
        posShoulderLeft = manager.GetJointPosition(userId, iShoulderLeft);
        posShoulderRight = manager.GetJointPosition(userId, iShoulderRight);
        posHead = manager.GetJointPosition(userId, iHead);
        posHipCenter = manager.GetJointPosition(userId, iHipCenter);

        // Get angle between left and right wrists
        var vectorWrists : Vector3 = posWristRight - posWristLeft;

        // Get angle between center of head and hips
        var vectorHeadToHips : Vector2 = new Vector2(posHead.z, posHead.y) - new Vector2(posHipCenter.z, posHipCenter.y);

        var rollAngle = Mathf.Atan2(vectorWrists.y, vectorWrists.x) * Mathf.Rad2Deg;
        var yawAngle = Mathf.Atan2(vectorWrists.z, vectorWrists.x) * Mathf.Rad2Deg;
        var pitchAngle = Mathf.Atan2(vectorHeadToHips.y, vectorHeadToHips.x) * Mathf.Rad2Deg;

        // Get angle between shoulders and wrists for flap movement
        var vLeftWristToShoulder : Vector2 = new Vector2(posShoulderLeft.x, posShoulderLeft.y) - new Vector2(posWristLeft.x, posWristLeft.y);
        var wingLeftAngle : float = Mathf.Atan2(vLeftWristToShoulder.y, vLeftWristToShoulder.x) * Mathf.Rad2Deg;

        var vRightWristToShoulder : Vector2 = new Vector2(posWristRight.x, posWristRight.y) - new Vector2(posShoulderRight.x, posShoulderRight.y);
        var wingRightAngle : float = Mathf.Atan2(vRightWristToShoulder.y, vRightWristToShoulder.x) * Mathf.Rad2Deg;

        // Normalize degrees to -1 ~ 1 range.
        var normRoll :float = Utils.Map(rollAngle, -60, 60, 1.0, -1.0);
        var normYaw :float = Utils.Map(yawAngle, -70, 70, -1.0, 1.0);
        var normPitch :float = Utils.Map(pitchAngle, 63, 103, 1.0, -1.0);

        if (screensaverMode == false) {

        	// Is user facing camera? 
	        // (Check by ensuring wrists/shoulders are on expected sides)
	        if (posWristRight.x - posWristLeft.x < 0.2 || 
	            posShoulderRight.x - posShoulderLeft.x < 0.1) {
	            birdFlight.noInputUpdate();
	            return;
	        }

			// Send update to bird controller
	        birdFlight.UpdateInputs( normRoll, normYaw, normPitch );
	        birdFlight.UpdateFlapState(wingLeftAngle, wingRightAngle);

        } else {
        	
			// During screensaver, we silently watch for a held t-pose
	        if (wingRightAngle > -15 && wingRightAngle < 15 &&
	        	wingLeftAngle > -15 && wingLeftAngle < 15 ) {
	        	tPoseTick();
	        } else {
	        	tPoseReset();
	        }

	        birdFlight.aiUpdate();

        }

    } else {

		tPoseReset();
        birdFlight.noInputUpdate();
        return;

    }

}

function CheckForScreensaverMode() {

	yield WaitForSeconds(1);

	var manager = KinectManager.Instance;

	if(manager && manager.IsInitialized()){
        var userId = manager.GetPrimaryUserID();
        if (!userId || userId <= 0) {
            // No players available...
            print('Kinect active but No user active. Start screensaver mode');
            toggleScreensaverMode(true);
            return;
        }

    } else {

        print('Warning: Kinect not active.');
//        toggleScreensaverMode(true);

    }

}	

function tPoseTick() {

	if (screensaverMode == true) {

		print('Performing T-Pose during screensaver');
		tPoseCount ++;

		if (tPoseCount > tPoseThreshold) {

			print('Valid user recognized. Aborting screensaver.');
			toggleScreensaverMode(false);

		}

	}

}

function tPoseReset() {
	if (screensaverMode == true && tPoseCount != 0) {
//		print("Cancel TPose during screensaver");
		tPoseCount = 0;
	}

}

function toggleScreensaverMode(active:boolean){

	if (active == true && screensaverMode == false) {
		// ENTER screensaver mode
		// from here out bird will update with AI
		print('Screensaver mode enabled.');

	} else if (active == false && screensaverMode == true) {
		// EXIT screensaver mode
		// Simply restart scene now that user is ready.
		var currentSceneName = SceneManager.GetActiveScene().name;
		sceneFader.EndScene(currentSceneName);
	}

	screensaverMode = active;

}