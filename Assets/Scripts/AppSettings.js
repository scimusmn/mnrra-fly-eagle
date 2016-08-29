#pragma strict

var fullResetTime : float = 7200; // 2 hrs in secs

function Awake () {

    // Attempt to unload resources
	Resources.UnloadUnusedAssets();
	System.GC.Collect();
	print('AWAKE - Unloading Unused Resources...');

	ForceFrameRate();

}

function ForceFrameRate() {
	
	QualitySettings.vSyncCount = 0;  // VSync must be disabled or disable in quality manually 
    Application.targetFrameRate = 60;

}

function OnDestroy () {

	fullResetCheck();

    // Unload resources
	Resources.UnloadUnusedAssets();
	System.GC.Collect();

}

function fullResetCheck() {

	// If no kinect user present ,
	// and game has been running for 
	// longer than 2 hours, quit application.
	if (Time.realtimeSinceStartup > fullResetTime) {

		var manager = KinectManager.Instance;

		if(manager && manager.IsInitialized()){
			
			var userId = manager.GetPrimaryUserID();

			if (!userId || userId <= 0) {

				Application.Quit();

			}
		}

	}

}