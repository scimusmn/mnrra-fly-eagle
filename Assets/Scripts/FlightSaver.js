
import System;
import System.IO;
import UnityEngine.SceneManagement;
 
var filePath = "Assets/Scripts/SavedFlights/SavedFlight_0.txt";
var recordFlight = false;

private var recordedInputData: List.<String> = new List.<String>();
private var recordedFlapData: List.<String> = new List.<String>();

private var playbackInputIndex:int = 0;
private var playbackFlapIndex:int = 0;
private var loadedInputData: List.<Vector3> = new List.<Vector3>();
private var loadedFlapData: List.<Vector2> = new List.<Vector2>();

private var isResetting:boolean = false;
private var sceneFader:SceneFader;

function Awake() {
	sceneFader = FindObjectOfType(SceneFader);
}
 
function Start() {

	if (recordFlight == false) {
		ReadFile();
	}

}

function Update () {

	// Press 'P' to write recorded flight data to file.
	// Why P? Because S was taken.
	if (recordFlight == true && Input.GetKeyDown(KeyCode.P)){ 
    	WriteFile();
    }

}

function recordInputData(wingsRoll : float, wingsYaw : float, wingsPitch : float) {

	var inputStr:String = '' + wingsRoll + ',' + wingsYaw + ',' + wingsPitch + ''; 
	recordedInputData.Add(inputStr);

}

function recordFlapData(wingLeftAngle:float, wingRightAngle:float) {

	var flapStr:String = '' + wingLeftAngle + ',' + wingRightAngle + ''; 
	recordedFlapData.Add(flapStr);

}

function getNextInputData():Vector3 {

	if (playbackInputIndex < loadedInputData.Count){

		playbackInputIndex ++;
		return loadedInputData[playbackInputIndex-1];

	} else {

		Debug.Log("Reached end of INPUT playback data.");

		if (!isResetting) {
			var currentSceneName = SceneManager.GetActiveScene().name;
			sceneFader.EndScene(currentSceneName);
			isResetting = true;
		}

		return loadedInputData[loadedInputData.Count-1];

	}

}


function getNextFlapData():Vector2 {

	if (playbackFlapIndex < loadedFlapData.Count){

		playbackFlapIndex ++;
		return loadedFlapData[playbackFlapIndex-1];

	} else {

		Debug.Log("Reached end of FLAP playback data.");

		if (!isResetting) {
			var currentSceneName = SceneManager.GetActiveScene().name;
			sceneFader.EndScene(currentSceneName);
			isResetting = true;
		}

		return loadedFlapData[loadedFlapData.Count-1];

	}

}

function WriteFile() {
   
    if (File.Exists(filePath)) {
	    Debug.Log("AVOIDING OVERWRITE : " + filePath+" already exists. Delete or rename before recording.");
	    return;
	}

    var sr = File.CreateText(filePath); // Returns Stream Writer

    sr.WriteLine ("---> INPUT STATES");

    // Write input records
    for(var i:int = 0; i < recordedInputData.Count; i++) {
        sr.WriteLine ('INPUT-STATE,' + recordedInputData[i]);
    }

    sr.WriteLine ("---> FLAP STATES");

    // Write flap records
    for(var j:int = 0; j < recordedFlapData.Count; j++) {
        sr.WriteLine ('FLAP-STATE,' + recordedFlapData[j]);
    }

    sr.WriteLine ("---> END");

    sr.Close();

}

function ReadFile() {
   
    if(File.Exists(filePath)){
        var sr = File.OpenText(filePath);
        var line = sr.ReadLine();

        print('FlightSaver: File read.');

        while(line != null) {
//            Debug.Log(line); // prints each line of the file

            if (line.Contains("INPUT-STATE")) {

            	// Parse input data
            	var inputs:String[] = line.Split(","[0]);
				var inputV3:Vector3 = new Vector3( parseFloat(inputs[1]), parseFloat(inputs[2]), parseFloat(inputs[3]) );
            	loadedInputData.Add(inputV3);

            } else if (line.Contains("FLAP-STATE")) {

            	// Parse flap data
            	var flaps:String[] = line.Split(","[0]);
            	var flapV2:Vector2 = new Vector2( parseFloat(flaps[1]), parseFloat(flaps[2]) );
            	loadedFlapData.Add(flapV2);

            }

            line = sr.ReadLine();

        }  

    } else {

        Debug.Log("Could not Open the file: " + filePath + " for reading.");
        return;

    }

}