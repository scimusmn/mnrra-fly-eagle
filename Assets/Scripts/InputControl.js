#pragma strict

import UnityEngine.SceneManagement;
import System;

public var targetObj : GameObject;
private var birdFlight : BirdFlight;
public var kinectInput: boolean = false;
public var hideCursor: boolean = true;

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
private var ssChecks:int = 0;
private var hasSeenUser:boolean = false;
private var screensaverUI:GameObject;
private var tPoseCount:int = 0;
private var tPoseThreshold:int = 30; // frames t-pose must be held to abort screensaver

private var inactivityCount:int = 0;
private var inactivityThreshold:int = 1500; // (60fps * 25secs) frames before active scene is aborted for screensaver
private var destroyKinectOnScene:boolean = true;

// Mouse control variables
private var mouseScrollWingAngle: float = 0.0;

function Awake() {
	sceneFader = FindObjectOfType(SceneFader);
	screensaverUI = GameObject.Find('UI-StartGame');
	screensaverUI.active = false;
}

function Start () {

    birdFlight = targetObj.GetComponent(BirdFlight);

    // Set defaults
    birdFlight.UpdateInputs(0.0, 0.0, 0.0);

    // Once scene starts, check if user is active.
    // If not, assume there is no one using, 
    // begin "screensaver" AI flight.
    var nowTime:String = System.DateTime.Now.ToString("hh:mm:ss"); 
    var nowDate:String = System.DateTime.Now.ToString("MM/dd/yyyy");
    Debug.Log('----] InputControls Start() ' + nowDate + ' | ' + nowTime + ' [-----');
    Invoke('CheckForScreensaverMode', 4);
    Invoke('CheckForScreensaverMode', 6);
    Invoke('CheckForScreensaverMode', 8);
    Invoke('CheckForScreensaverMode', 10);

    // Show/hide cursor
    if (hideCursor == true) {
    	// Hide the cursor
		Cursor.visible = false;

    }

    if (kinectInput == true && hideCursor == true) {

		// If cursor is invisible
		// assume in long-term installation mode
		// if kinect isn't available after 10 secs,
		// something is wrong, force restart.
		Invoke('CheckForHardwareAccess', 10);

    }



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
            if (screensaverMode == true){
				birdFlight.aiUpdate();
			} else {
				inactivityTick();
				birdFlight.noInputUpdate();
			}
            return;
        }

        manager.DetectGesture(userId, KinectGestures.Gestures.Tpose);

    } else {
    	// Kinect not available...
		if (screensaverMode == true){
			birdFlight.aiUpdate();
		} else {
			birdFlight.noInputUpdate();
		}
        return;

    }

    // Kinect and User data is available...
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

        // Uncomment to test for normal pitch angle
        // print('Input pitch angle: ' + pitchAngle);

        // Normalize degrees to -1 ~ 1 range.
        var normRoll :float = Utils.Map(rollAngle, -60, 60, 1.0, -1.0);
        var normYaw :float = Utils.Map(yawAngle, -70, 70, -1.0, 1.0);
        var normPitch :float = Utils.Map(pitchAngle, 68, 108, 1.0, -1.0);

        inactivityReset();

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
        if (screensaverMode == true){
			birdFlight.aiUpdate();
		} else {
			birdFlight.noInputUpdate();
		}
        return;

    }

}

function CheckForHardwareAccess() {
	var manager = KinectManager.Instance;

	if(manager && manager.IsInitialized()){

	} else {
		Debug.LogError('ERROR - kinect hardware still not available after 10 secs');
		var currentSceneName = SceneManager.GetActiveScene().name;
		sceneFader.EndScene(currentSceneName);
	}

}

function CheckForScreensaverMode() {

	if (hasSeenUser == true) {
		return;
	}

	// TEMP - logging to debug occasional kinect issues.
	var manager = KinectManager.Instance;
	if (!manager) {
		Debug.Log('CheckForScreensaverMode t['+Mathf.Round(Time.timeSinceLevelLoad)+']. manager: null.');
	} else {
		Debug.Log('CheckForScreensaverMode t['+Mathf.Round(Time.timeSinceLevelLoad)+']. manager: true, IsInitialized:' + manager.IsInitialized() + ', GetUsersClrTex:' + manager.GetUsersClrTex());
		if (!manager.GetRawDepthMap()) {
			Debug.Log('GetRawDepthMap: false ');
		} else {
			Debug.Log('GetRawDepthMap: true ');
		}
	}

	if(manager && manager.IsInitialized()){
        var userId = manager.GetPrimaryUserID();
        if (!userId || userId <= 0) {
            // No players available...

            if (screensaverMode == false){
            	toggleScreensaverMode(true);
            }

            // check again in X secs
            ssChecks++;
            print('No user on ss check:'+ssChecks);

            return;
        } else {
        	print('User found. Disable screensaver mode');
        	hasSeenUser = true;
        	screensaverUI.active = false;
        	screensaverMode = false;
        }

    } else {

        print('Warning: Kinect not active.');
        // Uncomment to test screensaver mode wo kinect
        toggleScreensaverMode(true);
        screensaverMode = false;
    }

}	

function tPoseTick() {

	if (screensaverMode == true) {

		print('Performing T-Pose during screensaver: ' + tPoseCount + ' / ' + tPoseThreshold);
		tPoseCount ++;

		if (tPoseCount > tPoseThreshold) {

			print('Valid user recognized. Aborting screensaver.');
			toggleScreensaverMode(false);

			// When exiting screensaver with new user
			// is the ONLY time we don't want kinect to 
			// reboot from scratch.
			destroyKinectOnScene = false;

		}

	}

}

function tPoseReset() {
	if (screensaverMode == true && tPoseCount != 0) {
//		print("Cancel TPose during screensaver");
		tPoseCount = 0;
	}

}

function inactivityTick() {

	if (screensaverMode == false) {

		//print('inactivity: ' + inactivityCount + ' / ' +inactivityThreshold);
		inactivityCount ++;

		if (inactivityCount > inactivityThreshold) {

			print('Inactivity threshold reached. Going to screensaver mode. ');

			destroyKinectOnScene = true;

			var currentSceneName = SceneManager.GetActiveScene().name;
			sceneFader.EndScene(currentSceneName);

			screensaverMode = true;

		}

	}

}

function inactivityReset() {

	inactivityCount = 0;

}

function toggleScreensaverMode(active:boolean){

	if (active == true && screensaverMode == false) {
		// ENTER screensaver mode
		// from here out bird will update with AI
		print('Screensaver mode enabled.');

		// Show screensaver GUI
		screensaverUI.active = true;


	} else if (active == false && screensaverMode == true) {
		// EXIT screensaver mode
		// Simply restart scene now that user is ready.
		print('EXIT Screensaver mode.');

		var currentSceneName = SceneManager.GetActiveScene().name;
		sceneFader.EndScene(currentSceneName);
	}

	screensaverMode = active;

}

